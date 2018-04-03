import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/map';

@Injectable()
export class AuthenticationService {
    constructor(private http: HttpClient) { }

    login(username: string, password: string) {
        return this.http.post<any>('/Token', { username: username, password: password })
            .map(response => {
                if (response && response.token) {
                    localStorage.setItem('currentUser', JSON.stringify(response));
                }

                return response;
            });
    }

    logout() {
        localStorage.removeItem('currentUser');
    }
}
