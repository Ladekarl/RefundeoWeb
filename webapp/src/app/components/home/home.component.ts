import {Component, OnInit} from '@angular/core';
import {MenuItem} from '../../models';
import {Router} from '@angular/router';
import {MenuService} from '../../services';

@Component({
    selector: 'app-home',
    templateUrl: 'home.component.html',
    styleUrls: ['home.component.scss']
})

export class HomeComponent implements OnInit {
    logoImageUrl = require('../../../assets/images/refundeo_logo.png');

    activeMenuItem: MenuItem;
    menuItems: MenuItem[];
    bottomMenuItems: MenuItem[];

    constructor(private router: Router, private menuService: MenuService) {
    }

    ngOnInit(): void {
        this.menuItems = this.menuService.getMenuItems();
        this.bottomMenuItems = this.menuService.getBottomMenuItems();
        const activeMenuItem = this.menuItems.find(menuItem => menuItem.routerLink === this.router.url);
        this.setActiveMenuItem(activeMenuItem);
    }

    setActiveMenuItem(menuItem: MenuItem) {
        this.activeMenuItem = menuItem;
    }
}
