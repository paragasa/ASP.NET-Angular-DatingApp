import { Component, OnInit, Input } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../_services/auth.service';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit {
  registerMode = false;
  logged :any;
  // values: any;

  constructor(private http: HttpClient,private authService: AuthService) { }

  ngOnInit() {
    this.authService.isLogged.subscribe(log => this.logged = log);
  }

  registerToggle() {
    this.registerMode = true;
  }

  loggedIn() {
    return this.authService.loggedIn();
  }
  cancelRegisterMode(registerMode: boolean) {
    this.registerMode = registerMode;
  }

}
