import {Component, OnInit} from '@angular/core';
import {AuthorizationService, ChartService, MerchantInfoService, RefundCasesService} from '../../../../../services';
import {Ng4LoadingSpinnerService} from 'ng4-loading-spinner';
import {MerchantInfo, RefundCase} from '../../../../../models';
import {forkJoin} from 'rxjs';
import {map} from 'rxjs/operators';
import {SelectItem} from 'primeng/api';

@Component({
    selector: 'app-static-stats',
    templateUrl: './static-stats.component.html',
    styleUrls: ['./static-stats.component.scss']
})
export class StaticStatsComponent implements OnInit {

    statisticsPeriodOptionsKey = 0;
    meanPurchaseTime: string;
    bestMonth: string;
    bestDay: string;
    averageSpent: number;
    merchantInfo: MerchantInfo;
    refundCases: RefundCase[];
    periodOptions: SelectItem[];

    constructor(private merchantInfoService: MerchantInfoService,
                private authorizationService: AuthorizationService,
                private spinnerService: Ng4LoadingSpinnerService,
                private chartService: ChartService,
                private refundCasesService: RefundCasesService) {
        this.merchantInfo = new MerchantInfo();
        this.periodOptions = this.chartService.getPeriodOptions();
    }

    ngOnInit() {
        this.spinnerService.show();
        this.authorizationService.getCurrentUser().subscribe(currentUser => {
            let tasks = [];
            tasks.push(this.refundCasesService.getAll(false)
                .pipe(map((refundCases: RefundCase[]) => {
                    this.refundCases = refundCases.filter(r => r.isAccepted);
                })));
            tasks.push(this.merchantInfoService.getMerchant(currentUser.id).pipe(map(merchantInfo => {
                this.merchantInfo = merchantInfo;
            })));

            forkJoin(tasks).subscribe(() => {
                if (this.refundCases && this.refundCases.length > 0) {
                    this.calculateStatistics(this.refundCases, this.statisticsPeriodOptionsKey);
                }
                this.spinnerService.hide();
            }, () => {
                this.spinnerService.hide();
            });
        }, () => {
            this.spinnerService.hide();
        });
    }

    onStatisticsPeriodChange(event) {
        if (this.refundCases && this.refundCases.length > 0) {
            this.calculateStatistics(this.refundCases, event.value);
        }
    }

    calculateStatistics(refundCases: RefundCase[], daysToShow: number) {
        refundCases = this.chartService.filterDays(daysToShow, refundCases);

        let times = refundCases.map(x => x.dateCreated.getHours());

        times.sort((a, b) => a - b);

        this.meanPurchaseTime = ('0' + times[Math.floor(times.length / 2)].toString()).slice(-2) + ':00';

        let monthsDateAmountMap = new Map<string, number>();
        let daysDateAmountMap = new Map<string, number>();

        let totalSpent = 0;
        let months = this.chartService.getMonths();
        let days = this.chartService.getDays();

        refundCases
            .sort((a: RefundCase, b: RefundCase) => a.dateCreated.getTime() - b.dateCreated.getTime())
            .forEach(refundCase => {
                const date = refundCase.dateCreated;
                let mappedMonth: number = monthsDateAmountMap.get(months[date.getMonth()]);
                let mappedDay: number = daysDateAmountMap.get(days[date.getDay()]);
                mappedMonth = mappedMonth ? mappedMonth + 1 : 1;
                mappedDay = mappedDay ? mappedDay + 1 : 1;
                monthsDateAmountMap.set(months[date.getMonth()], mappedMonth);
                daysDateAmountMap.set(days[date.getDay()], mappedDay);
                totalSpent += refundCase.amount;
            });

        let bestMonth = null;
        let bestMonthAmount = 0;
        monthsDateAmountMap.forEach((value, key) => {
            if (value > bestMonthAmount) {
                bestMonth = key;
                bestMonthAmount = value;
            }
        });
        this.bestMonth = bestMonth;

        let bestDay = null;
        let bestDayAmount = 0;
        daysDateAmountMap.forEach((value, key) => {
            if (value > bestDayAmount) {
                bestDay = key;
                bestDayAmount = value;
            }
        });
        this.bestDay = bestDay;

        this.averageSpent = (totalSpent / refundCases.length);
    }

}
