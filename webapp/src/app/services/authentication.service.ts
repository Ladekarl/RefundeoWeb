import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/map';
import { JwtHelperService } from '@auth0/angular-jwt';
import { CurrentUser } from '../models/currentUser';

@Injectable()
export class AuthenticationService {

    jwtHelperService: JwtHelperService;

    constructor(private http: HttpClient) {
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

    login(username: string, password: string): Observable<CurrentUser> {
        return this.http.post<any>('/Token', { username: username, password: password })
            .map((response: CurrentUser) => {
                if (response && response.token) {
                    this.setCurrentUser(response);
                }
                return response;
            });
    }

    logout() {
        localStorage.removeItem('currentUser');
    }

    private setCurrentUser(currentUser: CurrentUser) {
        localStorage.setItem('currentUser', JSON.stringify(currentUser));
    }
}
