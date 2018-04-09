import { Component, OnInit } from '@angular/core';
import { User } from '../../../models/index';
import { UserService } from '../../../services/index';
import { AuthenticationService } from '../../../services/authentication.service';
import { CurrentUser } from '../../../models';

@Component({
  selector: 'app-dashboard',
  templateUrl: 'dashboard.component.html',
  styleUrls: ['dashboard.component.scss']
})

export class DashboardComponent {
  currentUser: CurrentUser;
  users: User[] = [];

  constructor(private userService: UserService, private authenticationService: AuthenticationService) {
    this.currentUser = this.authenticationService.getCurrentUser();
  }
}
