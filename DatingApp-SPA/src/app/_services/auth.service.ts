import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import {  map } from 'rxjs/operators';
import { JwtHelperService } from '@auth0/angular-jwt';
import { User } from '../../../../../DatingApp/DatingApp-SPA/src/app/_models/user';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

baseUrl = '/api/auth/';
jwtHelper = new JwtHelperService();
decodedToken: any;
currentUser: User;
loggedin = new BehaviorSubject<boolean>(false);
isLogged= this.loggedin.asObservable();
photoUrl = new BehaviorSubject<string>('../../assets/user.png');
currentPhotoUrl = this.photoUrl.asObservable();

constructor(private http: HttpClient) { }

login(model: any) {
  return this.http.post(this.baseUrl + 'login', model)
  .pipe(
    map((response: any) => {
      const user = response;
      if (user) {
        localStorage.setItem('token', user.token); // matched token passed api
        localStorage.setItem('user', JSON.stringify(user.user));
        this.decodedToken = this.jwtHelper.decodeToken(user.token);
        this.currentUser = user.user;
        this.setLoggedIn(true);
        this.ChangeProfilePic(this.currentUser.photoUrl);
        // console.log(this.decodedToken);
      }
    })
  );
}
  ChangeProfilePic(photoUrl: string) {
    this.photoUrl.next(photoUrl);
  }
  setLoggedIn(log: boolean){
    this.loggedin.next(log);
  }
  register(user: User) {
    return this.http.post(this.baseUrl + 'register', user);
  }
  logoff(){
    this.loggedin.next(false);
  }

  loggedIn() {
    const token = localStorage.getItem('token');
    return !this.jwtHelper.isTokenExpired(token);
  }
}
