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
        this.authenticationService.logout().subscribe();
        this.setTitle('Refundeo - Log In to Retailer Suite');
        // get return url from route parameters or default to '/'
        this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
    }

    setTitle(newTitle: string) {
        this.titleService.setTitle(newTitle);
    }

    getInitialData() {
        this.authorizationService.getCurrentUser().subscribe(currentUser => {
            const tasks = [];
            this.authorizationService.isAuthenticatedAdmin().subscribe(isAdmin => {
                if (isAdmin) {
                    tasks.push(this.merchantInfoService.getAll());
                    tasks.push(this.customerInfoSerivce.getAll(true));
                    tasks.push(this.refundCasesService.getAll(true));
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
                    tasks.push(this.customerInfoSerivce.getAll(false));
                    tasks.push(this.refundCasesService.getAll(false));
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
                this.errorText = null;
                this.getInitialData();
                this.authorizationService.isAuthenticatedAdmin().subscribe(isAuthenticatedAdmin => {
                    if (isAuthenticatedAdmin) {
                        this.returnUrl = this.returnUrl === '/' ? '/admin' : this.returnUrl;
                    }
                    this.router.navigate([this.returnUrl]);
                }, () => {
                    this.loading = false;
                    this.spinnerService.hide();
                });
            }, error => {
                this.loading = false;
                this.spinnerService.hide();
                if (error && error.error) {
                    this.errorText = error.error.message;
                }
            });
    }

    onForgotPassword() {
        if (!this.model.username) {
            this.errorText = 'Please provide a username';
            return;
        }
        this.errorText = null;
        this.spinnerService.show();
        this.authenticationService.requestResetPassword(this.model.username).subscribe((response) => {
            alert('A password reset link was sent to ' + response.email);
            this.spinnerService.hide();
        }, () => {
            this.errorText = 'Could not reset password';
            this.spinnerService.hide();
        });
    }
}
