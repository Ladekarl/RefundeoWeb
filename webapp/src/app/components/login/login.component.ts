import {Component, OnInit} from '@angular/core';
import {Router, ActivatedRoute} from '@angular/router';

import {
    AuthenticationService,
    AuthorizationService,
    CustomerInfoService, MerchantInfoService,
    RefundCasesService
} from '../../services';
import {Ng4LoadingSpinnerService} from 'ng4-loading-spinner';
import {forkJoin} from 'rxjs';
import {Title} from '@angular/platform-browser';

@Component({
    selector: 'app-login',
    templateUrl: 'login.component.html',
    styleUrls: ['login.component.scss']
})

export class LoginComponent implements OnInit {
    model: any = {};
    loading = false;
    returnUrl: string;
    errorText: string;

    bannerImageUrl = require('../../../assets/images/refundeo_banner_small_border.png');

    constructor(
        private route: ActivatedRoute,
        private router: Router,
        private authenticationService: AuthenticationService,
        private refundCasesService: RefundCasesService,
        private customerInfoSerivce: CustomerInfoService,
        private authorizationService: AuthorizationService,
        private merchantInfoService: MerchantInfoService,
        private titleService: Title,
        private spinnerService: Ng4LoadingSpinnerService) {
    }

    ngOnInit() {
        // reset login status
        this.authenticationService.logout();
        this.setTitle('Refundeo - Log In to Retailer Suite');
        // get return url from route parameters or default to '/'
        this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
    }

    setTitle(newTitle: string) {
        this.titleService.setTitle(newTitle);
    }

    getInitialData() {
        this.authorizationService.getCurrentUser().subscribe(currentUser => {
            let tasks = [];
            this.authorizationService.isAuthenticatedAdmin().subscribe(isAdmin => {
                if (isAdmin) {
                    tasks.push(this.merchantInfoService.getAll());
                    tasks.push(this.customerInfoSerivce.getAll());
                    tasks.push(this.refundCasesService.getAll());
                    forkJoin(tasks).subscribe(() => {
                    }, () => {
                    }, () => {
                        this.spinnerService.hide();
                        this.loading = false;
                    });
                }
            });

            this.authorizationService.isAuthenticatedMerchant().subscribe(isMerchant => {
                if (isMerchant && currentUser) {
                    tasks.push(this.merchantInfoService.getMerchant(currentUser.id));
                    tasks.push(this.customerInfoSerivce.getAll());
                    tasks.push(this.refundCasesService.getAll());
                    forkJoin(tasks).subscribe(() => {
                        this.spinnerService.hide();
                        this.loading = false;
                    }, () => {
                        this.spinnerService.hide();
                        this.loading = false;
                    });
                }
            }, () => {
                this.spinnerService.hide();
                this.loading = false;
            });
        }, () => {
            this.spinnerService.hide();
            this.loading = false;
        });
    }

    login() {
        this.loading = true;
        this.spinnerService.show();
        this.authenticationService.login(this.model.username, this.model.password)
            .subscribe(() => {
                this.getInitialData();
                this.authorizationService.isAuthenticatedAdmin().subscribe(isAuthenticatedAdmin => {
                    if (isAuthenticatedAdmin) {
                        this.returnUrl === '/' ? this.returnUrl = 'admin' : this.returnUrl;
                    }
                    this.router.navigate([this.returnUrl]);
                });
            }, error => {
                this.loading = false;
                this.spinnerService.hide();
                this.errorText = error.error.message;
            });
    }
}
