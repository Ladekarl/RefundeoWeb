import {Component, OnInit, Input} from '@angular/core';
import {Message, SelectItem} from 'primeng/api';
import {MerchantInfo, RefundCase} from '../../../../../models';
import {forkJoin} from 'rxjs';
import {map} from 'rxjs/operators';
import {AuthorizationService, ChartService, MerchantInfoService, RefundCasesService} from '../../../../../services';
import {Ng4LoadingSpinnerService} from 'ng4-loading-spinner';

@Component({
    selector: 'app-purchase-chart',
    templateUrl: './purchase-chart.component.html',
    styleUrls: ['./purchase-chart.component.scss']
})
export class PurchaseChartComponent implements OnInit {
    @Input('height') height: number;
    periodOptions: SelectItem[];
    growls: Message[];
    purchasePeriodOptionsKey = 30;
    purchaseOptions: any;
    purchaseData: any;
    refundCases: RefundCase[];
    purchasesDateAmountMap: Map<number, number>;
    merchantInfo: MerchantInfo;

    constructor(private merchantInfoService: MerchantInfoService,
                private authorizationService: AuthorizationService,
                private spinnerService: Ng4LoadingSpinnerService,
                private chartService: ChartService,
                private refundCasesService: RefundCasesService) {
        this.merchantInfo = new MerchantInfo();
        this.growls = [];
        this.periodOptions = this.chartService.getPeriodOptions();
    }

    ngOnInit() {
        this.spinnerService.show();
        this.authorizationService.getCurrentUser().subscribe(currentUser => {
            const tasks = [];
            tasks.push(this.refundCasesService.getAll(false)
                .pipe(map((refundCases: RefundCase[]) => {
                    this.refundCases = refundCases.filter(r => r.isAccepted);
                })));
            tasks.push(this.merchantInfoService.getMerchant(currentUser.id).pipe(map(merchantInfo => {
                this.merchantInfo = merchantInfo;
            })));

            forkJoin(tasks).subscribe(() => {
                if (this.refundCases && this.refundCases.length > 0) {
                    this.setPurchaseData(this.refundCases, this.purchasePeriodOptionsKey);
                }
                this.spinnerService.hide();
            }, () => {
                this.spinnerService.hide();
            });
        }, () => {
            this.spinnerService.hide();
        });
    }

    onPurchasePeriodChange(event) {
        if (this.refundCases && this.refundCases.length > 0) {
            this.setPurchaseData(this.refundCases, event.value);
        }
    }

    selectPurchaseData(event) {
        const keys = Array.from(this.purchasesDateAmountMap.keys());
        const time = keys[event.element._index];
        const amount = this.purchasesDateAmountMap.get(time);

        this.growls = [];

        if (time) {
            const date = new Date(time);
            this.growls.push({
                severity: 'info',
                summary: `Tax free purchases on ${date.toLocaleDateString()}`,
                'detail': `${this.merchantInfo.currency} ${amount}`
            });
        }
    }

    setPurchaseData(refundCases: RefundCase[], daysToShow: number) {
        this.purchasesDateAmountMap = new Map<number, number>();

        refundCases = this.chartService.filterDays(daysToShow, refundCases);

        refundCases
            .sort((a: RefundCase, b: RefundCase) => a.dateCreated.getTime() - b.dateCreated.getTime())
            .forEach(refundCase => {
                const date: Date = refundCase.dateCreated;
                const dateFormatted: Date = new Date(date.getFullYear(), date.getMonth(), date.getDate());
                let mappedAmount: number = this.purchasesDateAmountMap.get(dateFormatted.getTime());
                mappedAmount = mappedAmount ? mappedAmount + refundCase.amount : refundCase.amount;
                this.purchasesDateAmountMap.set(dateFormatted.getTime(), mappedAmount);
            });

        const chartLabels = this.chartService.getChartLabels(daysToShow, Array.from(this.purchasesDateAmountMap.keys()));

        this.purchaseData = {
            labels: chartLabels,
            datasets: [
                {
                    label: 'Tax free purchases',
                    data: Array.from(this.purchasesDateAmountMap.values()),
                    fill: false,
                    lineTension: 0,
                    borderColor: 'rgba(48,56,128,1)',
                    backgroundColor: 'rgba(48,56,128,0.8)',
                    borderWidth: 1
                },
            ]
        };

        this.purchaseOptions = {
            title: {
                display: true,
                text: 'Tax free purchases',
                fontSize: 16
            },
            responsive: !this.height,
            maintainAspectRatio: !this.height,
            legend: {
                display: false
            },
            scales: {
                xAxes: [{
                    display: true,
                    scaleLabel: {
                        display: false,
                    }
                }],
                yAxes: [{
                    display: true,
                    scaleLabel: {
                        display: false,
                    },
                    ticks: {
                        callback: (label) => {
                            return this.merchantInfo.currency + ' ' + label;
                        }
                    }
                }]
            }
        };
    }
}
