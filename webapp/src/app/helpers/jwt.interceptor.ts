import {Injectable} from '@angular/core';
import {HttpRequest, HttpHandler, HttpEvent, HttpInterceptor, HttpErrorResponse} from '@angular/common/http';
import {Observable} from 'rxjs';
import {Router} from '@angular/router';
import {tap, flatMap} from 'rxjs/operators';

import {AuthorizationService} from '../services';

@Injectable()
export class JwtInterceptor implements HttpInterceptor {
    constructor(private authorizationService: AuthorizationService, private router: Router) {
    }

    intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        return this.authorizationService.getToken().pipe(flatMap(token => {
            if (token) {
                request = request.clone({
                    setHeaders: {
                        'Content-Type': 'application/json',
                        Authorization: `Bearer ${token}`
                    }
                });
            }
            return this.handleAuthenticated(request, next);
        }));
    }


    handleAuthenticated(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        return next.handle(request).pipe(tap((event: HttpEvent<any>) => {
        }, (err: any) => {
            if (err instanceof HttpErrorResponse) {
                if (err.status === 401 || err.status === 403) {
                    this.router.navigate(['/login']);
                }
            }
        }));
    }
}
