import {Injectable} from '@angular/core';
import {JwtHelperService} from '@auth0/angular-jwt';
import {CurrentUser} from '../models';
import {LocalStorage, JSONSchema} from '@ngx-pwa/local-storage';
import {Observable} from 'rxjs';
import {map, share} from 'rxjs/operators';

@Injectable()
export class AuthorizationService {

    jwtHelperService: JwtHelperService;

    schema: JSONSchema = {
        properties: {
            id: {type: 'string'},
            username: {type: 'string'},
            roles: {type: 'string'},
            expiration: {type: 'array'},
            token: {type: 'string'}
        },
        required: ['id', 'username', 'roles', 'expiration', 'token']
    };

    constructor(private localStorage: LocalStorage) {
        this.jwtHelperService = new JwtHelperService();
    }

    getToken(): Observable<string> {
        return this.getCurrentUser().pipe(map(currentUser => {
            if (!currentUser) {
                return null;
            }
            return currentUser.token;
        })).pipe(share());
    }

    getCurrentUser(): Observable<CurrentUser> {
        return this.localStorage.getItem<CurrentUser>('currentUser').pipe(share());
    }

    removeCurrentUser(): Observable<boolean> {
        return this.localStorage.removeItem('currentUser');
    }

    isMerchant(): Observable<boolean> {
        return this.getCurrentUser().pipe(map(user => {
            if (!user) {
                return false;
            }
            return user.roles.indexOf('Merchant') > -1;
        })).pipe(share());
    }

    isAdmin(): Observable<boolean> {
        return this.getCurrentUser().pipe(map(user => {
            if (!user) {
                return false;
            }
            return user.roles.indexOf('Admin') > -1;
        })).pipe(share());
    }

    isAuthenticated(): Observable<boolean> {
        return this.getToken().pipe(map(token => {
            if (!token) {
                return false;
            }
            return !this.jwtHelperService.isTokenExpired(token);
        })).pipe(share());
    }

    isAuthenticatedMerchant(): Observable<boolean> {
        return this.getCurrentUser().pipe(map(currentUser => {
            if (!currentUser) {
                return null;
            }
            const token = currentUser.token;
            return token && !this.jwtHelperService.isTokenExpired(token) && currentUser.roles.indexOf('Merchant') > -1;
        })).pipe(share());
    }

    isAuthenticatedAdmin(): Observable<boolean> {
        return this.getCurrentUser().pipe(map(currentUser => {
            if (!currentUser) {
                return null;
            }
            const token = currentUser.token;
            return token && !this.jwtHelperService.isTokenExpired(token) && currentUser.roles.indexOf('Admin') > -1;
        })).pipe(share());
    }

    setCurrentUser(currentUser: CurrentUser): Observable<boolean> {
        return this.localStorage.setItem('currentUser', currentUser);
    }
}
