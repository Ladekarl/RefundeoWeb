import {Injectable} from '@angular/core';
import {AuthenticationService} from './authentication.service';
import {MenuItem} from '../models';

@Injectable()
export class MenuService {

    private static merchantMenuItems: MenuItem[] = [{
        routerLink: '/',
        displayName: 'Dashboard',
        title: 'Dashboard',
        iconClass: 'fa-home'
    }, {
        routerLink: '/refundcases',
        displayName: 'Refunds',
        title: 'Refunds',
        iconClass: 'fa-exchange'
    }, {
        routerLink: '/docs',
        displayName: 'API',
        title: 'Refundeo API',
        iconClass: 'fa-cloud'
    }];

    private static adminMenuItems: MenuItem[] = [{
        routerLink: '/',
        displayName: 'Home',
        title: 'Home',
        iconClass: 'fa-home'
    }, {
        routerLink: '/docs',
        displayName: 'API',
        title: 'Refundeo API',
        iconClass: 'fa-cloud'
    }, {
        routerLink: '/admin',
        displayName: 'Admin',
        title: 'Refundeo Admin',
        iconClass: 'fa-cogs'
    }];

    private static bottomMenuItems: MenuItem[] = [{
        routerLink: '/settings',
        displayName: 'Settings',
        title: 'Settings',
        iconClass: 'fa-cog'
    }, {
        routerLink: '/login',
        displayName: 'Logout',
        title: 'Logout',
        iconClass: 'fa-sign-out'
    }];

    constructor(private authenticationService: AuthenticationService) {
    }

    getMenuItems(): MenuItem[] {
        if (this.authenticationService.isAdmin()) {
            return MenuService.adminMenuItems;
        }
        return MenuService.merchantMenuItems;
    }

    getBottomMenuItems(): MenuItem[] {
        return MenuService.bottomMenuItems;
    }

}
