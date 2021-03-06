import {AfterViewInit, Component, OnInit} from '@angular/core';
import {ChildMenuItem, MenuItem} from '../../models';
import {Router, NavigationStart} from '@angular/router';
import {AuthorizationService, MenuService} from '../../services';
import * as $ from 'jquery';
import {Title} from '@angular/platform-browser';
import {forkJoin} from 'rxjs';

@Component({
    selector: 'app-home',
    templateUrl: 'home.component.html',
    styleUrls: ['home.component.scss']
})

export class HomeComponent implements OnInit, AfterViewInit {
    logoImageUrl = require('../../../assets/images/refundeo_logo.png');
    brandLink: string;
    activeMenuItem: MenuItem | ChildMenuItem;
    menuItems: MenuItem[];
    bottomMenuItems: MenuItem[];
    activeChild: boolean;

    constructor(
        private router: Router,
        private menuService: MenuService,
        private titleService: Title,
        private authorizationService: AuthorizationService
    ) {
        this.activeMenuItem = new MenuItem();
        this.bottomMenuItems = [];
        this.menuItems = [];
        this.activeChild = false;
        router.events
            .subscribe((event) => {
                if (event instanceof NavigationStart) {
                    const menuItem = this.menuItems.find(m => m.routerLink === event.url);
                    if (menuItem) {
                        this.setActiveMenuItem(menuItem, false);
                    }
                }
            });
    }

    ngAfterViewInit() {
        $('.item-collapse').on('click', () => {
            if ($(window).width() <= 768) {
                $('.toggle-btn').click();
            }
        });
    }

    ngOnInit(): void {
        const tasks = [];
        tasks.push(this.menuService.getMenuItems());
        tasks.push(this.menuService.getBottomMenuItems());
        forkJoin(tasks).subscribe(([menuItems, bottomMenuItems]) => {
            this.menuItems = menuItems;
            this.bottomMenuItems = bottomMenuItems;
            this.setInitialActiveMenuItem();
            this.authorizationService.isAuthenticatedAdmin().subscribe(isAuthenticatedAdmin => {
                this.brandLink = isAuthenticatedAdmin ? '/admin' : '/';
            });
        });
    }

    setInitialActiveMenuItem(): void {
        for (const menuItem of this.menuItems) {
            if (this.isActiveMenuItem(menuItem)) {
                this.setActiveMenuItem(menuItem, false);
                return;
            } else if (menuItem.children && menuItem.children.length > 0) {
                for (const childItem of menuItem.children) {
                    if (this.isActiveMenuItem(childItem)) {
                        this.setActiveMenuItem(childItem, true);
                        return;
                    }
                }
            }
        }
    }

    isChildActive(menuItem: MenuItem) {
        for (const childItem of menuItem.children) {
            if (this.isActiveMenuItem(childItem)) {
                return true;
            }
        }
    }

    isActiveMenuItem(menuItem: MenuItem | ChildMenuItem) {
        return menuItem.routerLink === this.router.url;
    }

    setActiveMenuItem(menuItem: MenuItem | ChildMenuItem, isChild: boolean) {
        this.activeMenuItem = menuItem;
        this.activeChild = isChild;
        this.setTitle('Refundeo - ' + menuItem.displayName);
    }

    setTitle(newTitle: string) {
        this.titleService.setTitle(newTitle);
    }
}
