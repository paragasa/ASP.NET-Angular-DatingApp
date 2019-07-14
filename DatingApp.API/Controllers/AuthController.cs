using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers {

    [Route ("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase {
        private readonly IAuthRepository _repo;

        private readonly IConfiguration _config;

        private readonly IMapper _mapper;

        public AuthController (IAuthRepository repo, IConfiguration config, IMapper mapper) {
            _mapper = mapper;
            _config = config;
            _repo = repo;
        }

        [HttpPost ("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto) {

            userForRegisterDto.Username = userForRegisterDto.Username.ToLower ();

            if (await _repo.UserExists (userForRegisterDto.Username)) {
                return BadRequest("Username already exists");
            }
        
            var userToCreate = _mapper.Map<User>(userForRegisterDto);

            var createdUser = await _repo.Register(userToCreate, userForRegisterDto.Password);
            Console.WriteLine("DEBUGGER:   "+createdUser);
            var userToReturn = _mapper.Map<UserForDetailDto>(createdUser);
            Console.WriteLine("DEBUGGER:   "+userToReturn);  
            return CreatedAtRoute("GetUser", new { Controller= "Users", id = createdUser.Id}, userToReturn);
        }

        [HttpPost ("login")]
        public async Task<IActionResult> Login (UserForLoginDto userForLoginDto) {

            //look for user existance, see if user is stored in db, need to map smaller info
            var userFromRepo = await _repo.Login(userForLoginDto.Username.ToLower (), userForLoginDto.Password);

            //return non exist
            if (userFromRepo == null)
                return Unauthorized ();

            //create claims, append username and id to user
            var claims = new [] {
                new Claim (ClaimTypes.NameIdentifier, userFromRepo.Id.ToString ()),
                new Claim (ClaimTypes.Name, userFromRepo.Username)
            };

            //create security key with signing credential
            var key = new SymmetricSecurityKey (Encoding.UTF8.GetBytes (_config.GetSection ("AppSettings:Token").Value));

            var creds = new SigningCredentials (key, SecurityAlgorithms.HmacSha512Signature);

            //pass claim, give expire date, signing credentials as well
            var tokenDescriptor = new SecurityTokenDescriptor {
                Subject = new ClaimsIdentity (claims),
                Expires = DateTime.Now.AddDays (1),
                SigningCredentials = creds
            };

            //get JWT handler to create in order to create token
            var tokenHandler = new JwtSecurityTokenHandler ();

            //store data in token
            var token = tokenHandler.CreateToken (tokenDescriptor);

            var user = _mapper.Map<UserForList>(userFromRepo);

            return Ok (new {
                token = tokenHandler.WriteToken (token),
                user
            });
        }
    }
}