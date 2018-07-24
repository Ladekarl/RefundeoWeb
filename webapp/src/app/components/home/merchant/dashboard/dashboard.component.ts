import {Component, OnInit} from '@angular/core';
import {Router} from '@angular/router';
import {MerchantInfo, RefundCase} from '../../../../models';
import {AuthorizationService, MerchantInfoService, RefundCasesService} from '../../../../services';
import {Ng4LoadingSpinnerService} from 'ng4-loading-spinner';
import {Observable} from 'rxjs/Observable';

@Component({
    selector: 'app-dashboard',
    templateUrl: 'dashboard.component.html',
    styleUrls: ['dashboard.component.scss']
})

export class DashboardComponent implements OnInit {

    innerHeight: number;
    loading = false;
    refundCases: RefundCase[];
    merchantInfo: MerchantInfo;

    constructor(
        private router: Router,
        private refundCasesService: RefundCasesService,
        private merchantInfoService: MerchantInfoService,
        private authorizationService: AuthorizationService,
        private spinnerService: Ng4LoadingSpinnerService) {
        this.innerHeight = (window.innerHeight) - 300;
        this.merchantInfo = new MerchantInfo();
    }

    ngOnInit() {
        this.loadData();
    }

    loadData() {
        this.loading = true;
        this.spinnerService.show();
        let tasks = [];

        tasks.push(this.refundCasesService.getAll()
            .map((refundCases: RefundCase[]) => {
                this.refundCases = refundCases.reverse().slice(0, 5);
            }));
        tasks.push(this.merchantInfoService.getMerchant(this.authorizationService.getCurrentUser().id)
            .map(merchantInfo => {
                this.merchantInfo = merchantInfo;
            }));

        Observable.forkJoin(tasks).subscribe(() => {
            this.spinnerService.hide();
            this.loading = false;
        }, () => {
            this.spinnerService.hide();
            this.loading = false;
        });
    }

    onStatsClick(): void {
        this.router.navigate(['/statistics']);
    }

    onRefundsClick(): void {
        this.router.navigate(['/refunds']);
    }

    onAccountClick(): void {
        this.router.navigate(['/retailer']);
    }
}
