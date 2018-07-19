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
        {
            routerLink: '/account',
            displayName: 'Account',
            title: 'Account',
            iconClass: 'fa-user-circle',
        },
        {
            routerLink: '/retailer',
            displayName: 'Retailer Information',
            title: 'Retailer Information',
            iconClass: 'fa-edit'
        }
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

    private static adminMenuItems: MenuItem[] = [
        {
            routerLink: '/admin',
            displayName: 'Refunds',
            title: 'Refunds',
            iconClass: 'fa-exchange'
        },
        {
            routerLink: '/admin/shoppers',
            displayName: 'Shoppers',
            title: 'Shoppers',
            iconClass: 'fa-users'
        },
        {
            routerLink: '/admin/retailers',
            displayName: 'Retailers',
            title: 'Retailers',
            iconClass: 'fa-home'
        },
        {
            routerLink: '/admin/addretailer',
            displayName: 'Add Retailer',
            title: 'Add Retailer',
            iconClass: 'fa-plus'
        },
        {
            routerLink: '/admin/docs',
            displayName: 'API',
            title: 'Refundeo API',
            iconClass: 'fa-cloud'
        }
    ];

    private static merchantBottomMenuItems: MenuItem[] = [
        {
            routerLink: '/login',
            displayName: 'Logout',
            title: 'Logout',
            iconClass: 'fa-sign-out'
        },
        // {
        //     routerLink: '/settings',
        //     displayName: 'Settings',
        //     title: 'Settings',
        //     iconClass: 'fa-cog'
        // }
    ];

    private static adminBottomMenuItems: MenuItem[] = [
        {
            routerLink: '/login',
            displayName: 'Logout',
            title: 'Logout',
            iconClass: 'fa-sign-out'
        }
    ];

    constructor(private authorizationService: AuthorizationService) {
    }

    getMenuItems(): MenuItem[] {
        if (this.authorizationService.isAdmin()) {
            return MenuService.adminMenuItems;
        }
        return MenuService.merchantMenuItems;
    }

    getBottomMenuItems(): MenuItem[] {
        if (this.authorizationService.isAdmin()) {
            return MenuService.adminBottomMenuItems;
        }
        return MenuService.merchantBottomMenuItems;
    }

}
