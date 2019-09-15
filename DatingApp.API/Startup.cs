using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using DatingApp.API.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using DatingApp.API.Helpers;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace DatingApp.Api
{
    public class Startup
    {
        //passed from our appsetting.json class config db
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        
        public IConfiguration Configuration { get; }


        // //  ConfigureDevelopmentServices
        // public void ConfigureDevelopmentServices(IServiceCollection services)
        // {
        //     services.AddDbContext<DataContext>(options => options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));
            
        //    ConfigureServices(services);
        // }
        // ConfigureDevelopmentServices
        public void ConfigureDevelopmentServices(IServiceCollection services)
        {
            services.AddDbContext<DataContext>(options => options
                .UseMySql(Configuration.GetConnectionString("DefaultConnection")));
               // .ConfigureWarnings(warning => warning.Ignore(CoreEventId.IncludeIgnoredWarning)));
            
           ConfigureServices(services);
        }
        //AZURE PRODUCTION
         public void ConfigureProductionServices(IServiceCollection services)
        {
            services.AddDbContext<DataContext>(options => 
            options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
                //.ConfigureWarnings(warning => warning.Ignore(CoreEventId.IncludeIgnoredWarning)));
            
           ConfigureServices(services);
        }
        
        public void ConfigureServices(IServiceCollection services)
        {
         
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddJsonOptions(opt => {
                    opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                });
         
            services.BuildServiceProvider().GetService<DataContext>().Database.Migrate();
            services.AddCors(); //cor serv web api to ang
            services.Configure<CloudinarySettings>(Configuration.GetSection("CloudinarySettings")); //get our cloud settings
            services.AddAutoMapper();
            services.AddTransient<Seed>(); //trans user seeding
            services.AddScoped<IAuthRepository, AuthRepository>(); // needed fo interfaces
            services.AddScoped<IDatingRepository, DatingRepository>();
            services.AddScoped<LogUserActivity>();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme) //auth tokens
            .AddJwtBearer(options => {
                options.TokenValidationParameters= new TokenValidationParameters{
                    ValidateIssuerSigningKey= true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration.GetSection("AppSettings:Token").Value)),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });
           
        }
       
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline. , Seed seeder
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, Seed seeder)
        {
            if (env.IsDevelopment())
            { 
                app.UseDeveloperExceptionPage();
            }
            else
            {
            //    in production use our exception handler
                app.UseExceptionHandler(builder => {
                    builder.Run(async context => {
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        var error = context.Features.Get<IExceptionHandlerFeature>();
                        if(error != null){
                            context.Response.AddApplicationError(error.Error.Message); //static helper for editing message
                            await context.Response.WriteAsync(error.Error.Message);
                        } 
                    });
                });
             // app.UseHsts();
            }
        
            // app.UseDeveloperExceptionPage();
            //app.UseHttpsRedirection();
            seeder.SeedUsers();
            app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
        
            app.UseAuthentication();
            app.UseDefaultFiles();//look for index.html in www.root
            app.UseStaticFiles(); //for www files
            app.UseMvc(routes =>{routes.MapSpaFallbackRoute(
                    name: "spa-fallback",
                    defaults: new { controller = "Fallback", action = "Index"}
                );
            });
            
        }
    }
}
