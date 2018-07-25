import {Component, OnInit} from '@angular/core';
import {Router} from '@angular/router';
import {MerchantInfo, RefundCase} from '../../../../models';
import {AuthorizationService, MerchantInfoService, RefundCasesService} from '../../../../services';
import {Ng4LoadingSpinnerService} from 'ng4-loading-spinner';
import {forkJoin} from 'rxjs';
import {map} from 'rxjs/operators';

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
        this.authorizationService.getCurrentUser().subscribe(currentUser => {
            let tasks = [];
            tasks.push(this.refundCasesService.getAll()
                .pipe(map((refundCases: RefundCase[]) => {
                    this.refundCases = refundCases.reverse().slice(0, 5);
                })));
            tasks.push(this.merchantInfoService.getMerchant(currentUser.id).subscribe(merchantInfo => {
                this.merchantInfo = merchantInfo;
            }));

            forkJoin(tasks).subscribe(() => {
                this.spinnerService.hide();
                this.loading = false;
            }, () => {
                this.spinnerService.hide();
                this.loading = false;
            });
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
