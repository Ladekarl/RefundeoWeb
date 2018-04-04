import { Component, OnInit } from '@angular/core';
import { User } from '../models/index';
import { UserService } from '../services/index';
import { AuthenticationService } from '../services/authentication.service';
import { CurrentUser } from '../models';

@Component({
  selector: 'app-home',
  templateUrl: 'home.component.html',
  styleUrls: ['home.component.scss']
})

export class HomeComponent implements OnInit {
  currentUser: CurrentUser;
  users: User[] = [];
  isApiUser: boolean;

  constructor(private userService: UserService, private authenticationService: AuthenticationService) {
    this.currentUser = this.authenticationService.getCurrentUser();
    this.isApiUser = this.currentUser.roles &&
      (this.currentUser.roles.indexOf('Merchant') > -1 || this.currentUser.roles.indexOf('Admin') > -1);
  }

  ngOnInit() {
    this.loadAllUsers();
  }

  deleteUser(id: number) {
    this.userService.delete(id).subscribe(() => { this.loadAllUsers(); });
  }

  private loadAllUsers() {
    this.userService.getAll().subscribe(users => { this.users = users; });
  }

}
