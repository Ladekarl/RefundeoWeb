import {AfterViewInit, Component, OnInit} from '@angular/core';
import {MenuItem} from '../../models';
import {Router} from '@angular/router';
import {MenuService} from '../../services';
import * as $ from 'jquery';

@Component({
    selector: 'app-home',
    templateUrl: 'home.component.html',
    styleUrls: ['home.component.scss']
})

export class HomeComponent implements OnInit, AfterViewInit {
    logoImageUrl = require('../../../assets/images/refundeo_logo.png');

    activeMenuItem: MenuItem;
    menuItems: MenuItem[];
    bottomMenuItems: MenuItem[];

    constructor(private router: Router, private menuService: MenuService) {
    }

    ngAfterViewInit() {
        $('#menu').resizable({
            maxWidth: 300,
            minWidth: 75,
            handles: 'e',
            autoHide: true,
            resize: function () {
                const menuWidth = $('#menu').width();
                $('#main').css({left: menuWidth});

                const menuTexts = $('.menu-text');
                const menuBody = $('.menu-body');
                const menuIcons = $('.menu-icon');
                const menuBottomItems = $('.menu-bottom-item');
                if (menuWidth <= 160) {
                    menuTexts.hide();
                    menuBody.css({textAlign: 'center'});
                    menuIcons.css({padding: 0});
                    menuBottomItems.css({display: 'block'});
                } else {
                    menuTexts.show();
                    menuBody.css({textAlign: 'left'});
                    menuIcons.css({paddingRight: 30});
                    menuIcons.css({paddingLeft: 25});
                    menuBottomItems.css({display: 'table-cell   '});
                }
            }
        });
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
