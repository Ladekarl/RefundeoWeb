import {AfterViewInit, Component, OnInit} from '@angular/core';
import {MenuItem} from '../../models';
import {Router} from '@angular/router';
import {AuthorizationService, MenuService} from '../../services';
import * as $ from 'jquery';
import {Title} from '@angular/platform-browser';

@Component({
    selector: 'app-home',
    templateUrl: 'home.component.html',
    styleUrls: ['home.component.scss']
})

export class HomeComponent implements OnInit, AfterViewInit {
    logoImageUrl = require('../../../assets/images/refundeo_logo.png');
    brandLink: string;
    activeMenuItem: MenuItem;
    menuItems: MenuItem[];
    bottomMenuItems: MenuItem[];

    constructor(
        private router: Router,
        private menuService: MenuService,
        private titleService: Title,
        private authorizationService: AuthorizationService
    ) {
    }

    ngAfterViewInit() {
        $('.menu-content li').on('click', () => {
            if ($(window).width() <= 768)
                $('.toggle-btn').click(); //bootstrap 3.x by Richard
        });
        $('#menu').resizable({
            maxWidth: 300,
            minWidth: 75,
            handles: 'e',
            autoHide: true,
            resize: () => {
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
        this.brandLink = this.authorizationService.isAuthenticatedAdmin() ? '/admin' : '/';
    }

    setActiveMenuItem(menuItem: MenuItem) {
        this.activeMenuItem = menuItem;
        this.setTitle('Refundeo - ' + menuItem.displayName);
    }

    setTitle(newTitle: string) {
        this.titleService.setTitle(newTitle);
    }
}
