import {Component, OnInit} from '@angular/core';
import {Router, ActivatedRoute} from '@angular/router';

import {AuthenticationService, CustomerInfoService, RefundCasesService} from '../../services';
import {Ng4LoadingSpinnerService} from 'ng4-loading-spinner';
import {Observable} from 'rxjs/Observable';
import 'rxjs/add/observable/forkJoin';

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
        private spinnerService: Ng4LoadingSpinnerService) {
    }

    ngOnInit() {
        // reset login status
        this.authenticationService.logout();

        // get return url from route parameters or default to '/'
        this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
    }

    getInitialData() {
        let tasks = [];
        const customersObs = this.customerInfoSerivce.getAll();
        const refundsObs = this.refundCasesService.getAll();
        tasks.push(customersObs);
        tasks.push(refundsObs);
        Observable.forkJoin(tasks).subscribe(() => {
            this.spinnerService.hide();
            this.loading = false;
        });
    }

    login() {
        this.loading = true;
        this.spinnerService.show();
        this.authenticationService.login(this.model.username, this.model.password)
            .subscribe(() => {
                if (this.authenticationService.isMerchant()) {
                    this.getInitialData();
                }
                this.router.navigate([this.returnUrl]);
            }, error => {
                this.loading = false;
                this.spinnerService.hide();
                this.errorText = error.error.message;
            });
    }
}
