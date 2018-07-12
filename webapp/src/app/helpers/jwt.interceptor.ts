import {Injectable} from '@angular/core';
import {HttpRequest, HttpHandler, HttpEvent, HttpInterceptor, HttpErrorResponse} from '@angular/common/http';
import {Observable} from 'rxjs/Observable';
import {Router} from '@angular/router';
import 'rxjs/add/operator/do';
import {AuthorizationService} from '../services/authorization.service';

@Injectable()
export class JwtInterceptor implements HttpInterceptor {
    constructor(private authorizationService: AuthorizationService, private router: Router) {
    }

    intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        const isAuthenticated = this.authorizationService.isAuthenticated();
        if (isAuthenticated) {
            request = request.clone({
                setHeaders: {
                    Authorization: `Bearer ${this.authorizationService.getToken()}`
                }
            });
        }
        return next.handle(request).do((event: HttpEvent<any>) => {
        }, (err: any) => {
            if (err instanceof HttpErrorResponse) {
                if (err.status === 401 && !this.authorizationService.isAuthenticated()) {
                    this.router.navigate(['/login']);
                }
            }
        });
    }
}
