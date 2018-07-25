import {Injectable} from '@angular/core';
import {Router, CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot} from '@angular/router';
import {AuthorizationService} from '../services';
import {Observable, forkJoin} from 'rxjs';
import {map} from 'rxjs/operators';

@Injectable()
export class AuthGuard implements CanActivate {

    constructor(private router: Router, private authorizationService: AuthorizationService) {
    }

    canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean> {
        let tasks = [];
        tasks.push(this.authorizationService.isAuthenticated());
        tasks.push(this.authorizationService.isAdmin());
        tasks.push(this.authorizationService.isMerchant());
        return forkJoin(tasks).pipe(map(([isAuthenticated, isMerchant, isAdmin]) => {
            if (isAuthenticated && (isMerchant || isAdmin)) {
                return true;
            }
            this.router.navigate(['/login'], {queryParams: {returnUrl: state.url}});
            return false;
        }));
    }
}
