import {Injectable} from '@angular/core';
import {Router, CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot} from '@angular/router';
import {AuthorizationService} from '../services/authorization.service';

@Injectable()
export class AuthGuard implements CanActivate {

    constructor(private router: Router, private authorizationService: AuthorizationService) {
    }

    canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot) {
        const isAuthenticated = this.authorizationService.isAuthenticated();
        const isAdmin = this.authorizationService.isAdmin();
        const isMerchant = this.authorizationService.isMerchant();
        if (isAuthenticated && (isMerchant || isAdmin)) {
            return true;
        }
        this.router.navigate(['/login'], {queryParams: {returnUrl: state.url}});
        return false;
    }
}
