import {Injectable} from '@angular/core';
import {Router, CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot} from '@angular/router';
import {AuthorizationService} from '../services';
import {Observable} from 'rxjs';
import {map} from 'rxjs/operators';

@Injectable()
export class AdminAuthGuard implements CanActivate {

    constructor(private router: Router, private authorizationService: AuthorizationService) {
    }

    canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean> {
        return this.authorizationService.isAuthenticatedAdmin().pipe(map(isAuthenticatedAdmin => {
            if (isAuthenticatedAdmin) {
                return true;
            }
            this.router.navigate(['/login'], {queryParams: {returnUrl: state.url}});
            return false;
        }));
    }
}
