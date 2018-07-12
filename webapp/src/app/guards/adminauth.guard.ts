import {Injectable} from '@angular/core';
import {Router, CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot} from '@angular/router';
import {AuthorizationService} from '../services/authorization.service';

@Injectable()
export class AdminAuthGuard implements CanActivate {

    constructor(private router: Router, private authorizationService: AuthorizationService) {
    }

    canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot) {
        const isAuthenticatedAdmin = this.authorizationService.isAuthenticatedAdmin();
        if (isAuthenticatedAdmin) {
            return true;
        }
        this.router.navigate(['/login'], {queryParams: {returnUrl: state.url}});
        return false;
    }
}
