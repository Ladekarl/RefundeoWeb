import {Injectable} from '@angular/core';
import {MenuItem} from '../models';
import {AuthorizationService} from './authorization.service';
import {Observable} from 'rxjs';
import {map} from 'rxjs/operators';

@Injectable()
export class MenuService {

    private static readonly merchantMenuItems: MenuItem[] = [
        {
            routerLink: '/',
            displayName: 'Overview',
            title: 'Overview',
            iconClass: 'fa-home'
        },
        {
            routerLink: '/statistics',
            displayName: 'Statistics',
            title: 'Statistics',
            iconClass: 'fa-line-chart'
        },
        {
            routerLink: '/refunds',
            displayName: 'Refunds',
            title: 'Refunds',
            iconClass: 'fa-exchange'
        },
        {
            routerLink: '/retailer',
            displayName: 'Retailer Information',
            title: 'Retailer Information',
            iconClass: 'fa-edit'
        },
        {
            routerLink: '/account',
            displayName: 'Account',
            title: 'Account',
            iconClass: 'fa-user-circle',
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

    private static readonly adminMenuItems: MenuItem[] = [
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
            routerLink: '/admin/tags',
            displayName: 'Tags',
            title: 'Tags',
            iconClass: 'fa-tags'
        },
        {
            routerLink: '/admin/docs',
            displayName: 'API',
            title: 'Refundeo API',
            iconClass: 'fa-cloud'
        }
    ];

    private static readonly merchantBottomMenuItems: MenuItem[] = [
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

    private static readonly adminBottomMenuItems: MenuItem[] = [
        {
            routerLink: '/login',
            displayName: 'Logout',
            title: 'Logout',
            iconClass: 'fa-sign-out'
        }
    ];

    constructor(private authorizationService: AuthorizationService) {
    }

    getMenuItems(): Observable<MenuItem[]> {
        return this.authorizationService.isAdmin().pipe(map(isAdmin => {
            if (isAdmin) {
                return MenuService.adminMenuItems;
            }
            return MenuService.merchantMenuItems;
        }));
    }

    getBottomMenuItems(): Observable<MenuItem[]> {
        return this.authorizationService.isAdmin().pipe(map(isAdmin => {
            if (isAdmin) {
                return MenuService.adminBottomMenuItems;
            }
            return MenuService.merchantBottomMenuItems;
        }));
    }

}
