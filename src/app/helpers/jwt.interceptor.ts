import { Injectable } from '@angular/core';
import { HttpRequest, HttpHandler, HttpEvent, HttpInterceptor } from '@angular/common/http';
import { Observable } from 'rxjs/Observable';
import { JwtHelperService } from '@auth0/angular-jwt';
import { Router } from '@angular/router';

@Injectable()
export class JwtInterceptor implements HttpInterceptor {

    jwtHelperService: JwtHelperService;

    constructor(private router: Router) {
        this.jwtHelperService = new JwtHelperService();
    }

    intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        const currentUser = JSON.parse(localStorage.getItem('currentUser'));
        if (currentUser && currentUser.token) {
            const isExpired = this.jwtHelperService.isTokenExpired(currentUser.token);
            if (!isExpired) {
                request = request.clone({
                    setHeaders: {
                        Authorization: `Bearer ${currentUser.token}`
                    }
                });
                return next.handle(request);
            }
        }
        this.router.navigate(['/login']);
        return new Observable();
    }
}
