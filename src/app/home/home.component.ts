import { Component, OnInit } from '@angular/core';
import { User } from '../models/index';
import { UserService } from '../services/index';

@Component({
  selector: 'app-home',
  templateUrl: 'home.component.html',
  styleUrls: ['home.component.scss']
})

export class HomeComponent implements OnInit {
  currentUser: User;
  users: User[] = [];
  isApiUser;

  constructor(private userService: UserService) {
    this.currentUser = JSON.parse(localStorage.getItem('currentUser'));
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
