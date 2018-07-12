import {Injectable} from '@angular/core';
import 'rxjs/add/operator/map';
import {JwtHelperService} from '@auth0/angular-jwt';
import {CurrentUser} from '../models';

@Injectable()
export class AuthorizationService {

    jwtHelperService: JwtHelperService;

    constructor() {
        this.jwtHelperService = new JwtHelperService();
    }

    getToken(): string {
        const currentUser = this.getCurrentUser();
        if (!currentUser) {
            return null;
        }
        return currentUser.token;
    }

    getCurrentUser(): CurrentUser {
        return JSON.parse(localStorage.getItem('currentUser'));
    }

    removeCurrentUser() {
        localStorage.removeItem('currentUser');
    }

    isMerchant(): boolean {
        const user = this.getCurrentUser();
        if (!user) {
            return false;
        }
        return user.roles.indexOf('Merchant') > -1;
    }

    isAdmin(): boolean {
        const user = this.getCurrentUser();
        if (!user) {
            return false;
        }
        return user.roles.indexOf('Admin') > -1;
    }

    isAuthenticated(): boolean {
        const token = this.getToken();
        if (!token) {
            return false;
        }
        return !this.jwtHelperService.isTokenExpired(token);
    }

    isAuthenticatedMerchant(): boolean {
        return this.isAuthenticated() && this.isMerchant();
    }

    isAuthenticatedAdmin(): boolean {
        return this.isAuthenticated() && this.isAdmin();
    }

    setCurrentUser(currentUser: CurrentUser) {
        localStorage.setItem('currentUser', JSON.stringify(currentUser));
    }
}
