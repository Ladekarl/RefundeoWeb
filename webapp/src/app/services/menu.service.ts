import {Injectable} from '@angular/core';
import {MenuItem} from '../models';
import {AuthorizationService} from './authorization.service';

@Injectable()
export class MenuService {

    private static merchantMenuItems: MenuItem[] = [
        // {
        //     routerLink: '/',
        //     displayName: 'Dashboard',
        //     title: 'Dashboard',
        //     iconClass: 'fa-home'
        // },
        {
            routerLink: '/',
            displayName: 'Refunds',
            title: 'Refunds',
            iconClass: 'fa-exchange'
        },
        // {
        //     routerLink: '/refunds',
        //     displayName: 'Refunds',
        //     title: 'Refunds',
        //     iconClass: 'fa-exchange'
        // },
        // {
        //     routerLink: '/docs',
        //     displayName: 'API',
        //     title: 'Refundeo API',
        //     iconClass: 'fa-cloud'
        // }
    ];

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

    private static bottomMenuItems: MenuItem[] = [
        // {
        //     routerLink: '/settings',
        //     displayName: 'Settings',
        //     title: 'Settings',
        //     iconClass: 'fa-cog'
        // },
        {
            routerLink: '/login',
            displayName: 'Logout',
            title: 'Logout',
            iconClass: 'fa-sign-out'
        }];

    constructor(private authorizationService: AuthorizationService) {
    }

    getMenuItems(): MenuItem[] {
        if (this.authorizationService.isAdmin()) {
            return MenuService.adminMenuItems;
        }
        return MenuService.merchantMenuItems;
    }

    getBottomMenuItems(): MenuItem[] {
        return MenuService.bottomMenuItems;
    }

}
