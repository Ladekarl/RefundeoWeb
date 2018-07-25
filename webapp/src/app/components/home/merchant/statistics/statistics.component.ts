import {Component, OnInit} from '@angular/core';
import {MerchantInfo, RefundCase} from '../../../../models';
import {
    MerchantInfoService,
    ColorsService,
    RefundCasesService,
    AuthorizationService, ChartService
} from '../../../../services';
import {Message, SelectItem} from 'primeng/api';
import {Ng4LoadingSpinnerService} from 'ng4-loading-spinner';
import {forkJoin} from 'rxjs';
import {map} from 'rxjs/operators';

@Component({
    selector: 'app-statistics',
    templateUrl: './statistics.component.html',
    styleUrls: ['./statistics.component.scss']
})
export class StatisticsComponent implements OnInit {

    usersByCountry: any;
    totalRefundCases: number;
    earningsData: any;
    monthsData: any;
    earningsPeriodOptionsKey = 30;
    monthsPeriodOptionsKey = 0;
    customersPeriodOptionsKey = 0;
    refundCases: RefundCase[];
    growls: Message[];
    earningsDateAmountMap: Map<number, number>;
    merchantInfo: MerchantInfo;
    earningsOptions: any;
    monthsOptions: any;
    customersOptions: any;

    periodOptions: SelectItem[];

    constructor(private merchantInfoService: MerchantInfoService,
                private authorizationService: AuthorizationService,
                private spinnerService: Ng4LoadingSpinnerService,
                private colorsService: ColorsService,
                private chartService: ChartService,
                private refundCasesService: RefundCasesService) {
        this.merchantInfo = new MerchantInfo();
        this.growls = [];
        this.periodOptions = this.chartService.getPeriodOptions();
    }

    ngOnInit(): void {
        this.spinnerService.show();
        this.authorizationService.getCurrentUser().subscribe(currentUser => {
            let tasks = [];
            tasks.push(this.refundCasesService.getAll()
                .pipe(map((refundCases: RefundCase[]) => {
                    this.refundCases = refundCases.filter(r => r.isAccepted);
                })));
            tasks.push(this.merchantInfoService.getMerchant(currentUser.id).subscribe(merchantInfo => {
                this.merchantInfo = merchantInfo;
            }));

            forkJoin(tasks).subscribe(() => {
                if (this.refundCases && this.refundCases.length > 0) {
                    this.setUsersByCountryData(this.refundCases, this.customersPeriodOptionsKey);
                    this.setEarningsData(this.refundCases, this.earningsPeriodOptionsKey);
                    this.setMonthsData(this.refundCases, this.monthsPeriodOptionsKey);
                }
                this.spinnerService.hide();
            }, () => {
                this.spinnerService.hide();
            });
        }, () => {
            this.spinnerService.hide();
        });
    }

    selectEarningsData(event) {
        let keys = Array.from(this.earningsDateAmountMap.keys());
        let time = keys[event.element._index];
        let amount = this.earningsDateAmountMap.get(time);

        this.growls = [];

        if (time) {
            const date = new Date(time);
            this.growls.push({
                severity: 'info',
                summary: `Tax free earnings on ${date.toLocaleDateString()}`,
                'detail': `${this.merchantInfo.currency} ${amount}`
            });
        }
    }

    onEarningsPeriodChange(event) {
        if (this.refundCases && this.refundCases.length > 0) {
            this.setEarningsData(this.refundCases, event.value);
        }
    }

    onCustomersPeriodChange(event) {
        if (this.refundCases && this.refundCases.length > 0) {
            this.setUsersByCountryData(this.refundCases, event.value);
        }
    }

    onMonthsPeriodChange(event) {
        if (this.refundCases && this.refundCases.length > 0) {
            this.setMonthsData(this.refundCases, event.value);
        }
    }

    setUsersByCountryData(refundCases: RefundCase[], daysToShow: number) {
        refundCases = this.chartService.filterDays(daysToShow, refundCases);

        const usersByCountryMap = this.getUsersByCountry(refundCases);

        const countries = Array.from(usersByCountryMap.keys());
        const amounts = Array.from(usersByCountryMap.values());
        const colorPalette = this.colorsService.getColorPallete(usersByCountryMap.keys.length);
        this.totalRefundCases = refundCases.length;
        this.usersByCountry = {
            labels: countries,
            datasets: [
                {
                    backgroundColor: colorPalette,
                    hoverBackgroundColor: colorPalette,
                    data: amounts
                }]
        };

        this.customersOptions = {
            title: {
                display: true,
                text: 'Nationalities',
                fontSize: 16
            },
            legend: {
                position: 'bottom'
            },
            responsive: true
        };
    }

    getMonday(date) {
        date = new Date(date.getTime());
        let day = date.getDay() || 7;
        if (day !== 1)
            date.setHours(-24 * (day - 1));
        return new Date(date.getFullYear(), date.getMonth(), date.getDate());
    }

    getDateFormatted(date: Date, daysToShow: number) {
        let dateFormatted: Date = null;
        if (daysToShow > 0 && daysToShow <= 7)
            dateFormatted = new Date(date.getFullYear(), date.getMonth(), date.getDate());
        else if (daysToShow > 7 && daysToShow < 365)
            dateFormatted = this.getMonday(date);
        else if (daysToShow >= 365 || daysToShow === 0)
            dateFormatted = new Date(date.getFullYear(), date.getMonth());
        return dateFormatted;
    }

    setMonthsData(refundCases: RefundCase[], daysToShow: number) {
        let monthsDateAmountMap = new Map<number, number>();

        refundCases = this.chartService.filterDays(daysToShow, refundCases);

        refundCases
            .sort((a: RefundCase, b: RefundCase) => a.dateCreated.getTime() - b.dateCreated.getTime())
            .forEach(refundCase => {
                const dateFormatted = this.getDateFormatted(refundCase.dateCreated, daysToShow);
                let mappedAmount: number = monthsDateAmountMap.get(dateFormatted.getTime());
                mappedAmount = mappedAmount ? mappedAmount + 1 : 1;
                monthsDateAmountMap.set(dateFormatted.getTime(), mappedAmount);
            });

        const chartLabels = this.chartService.getChartLabels(daysToShow, Array.from(monthsDateAmountMap.keys()), false);

        this.monthsData = {
            labels: chartLabels,
            datasets: [
                {
                    label: 'Customers',
                    data: Array.from(monthsDateAmountMap.values()),
                    borderColor: 'rgba(48,56,128,1)',
                    backgroundColor: 'rgba(48,56,128,0.8)',
                    borderWidth: 1
                },
            ]
        };

        this.monthsOptions = {
            title: {
                display: true,
                text: 'Customers',
                fontSize: 16
            },
            responsive: true,
            legend: {
                display: false
            },
            scales: {
                yAxes: [{
                    display: true,
                    min: 0,
                    ticks: {
                        suggestedMin: 0,
                        beginAtZero: true,
                        callback: (label) => {
                            if (Math.floor(label) === label) {
                                return label;
                            }
                        }
                    }
                }]
            }
        };
    }

    setEarningsData(refundCases: RefundCase[], daysToShow: number) {
        this.earningsDateAmountMap = new Map<number, number>();

        refundCases = this.chartService.filterDays(daysToShow, refundCases);

        refundCases
            .sort((a: RefundCase, b: RefundCase) => a.dateCreated.getTime() - b.dateCreated.getTime())
            .forEach(refundCase => {
                const date: Date = refundCase.dateCreated;
                const dateFormatted: Date = new Date(date.getFullYear(), date.getMonth(), date.getDate());
                let mappedAmount: number = this.earningsDateAmountMap.get(dateFormatted.getTime());
                mappedAmount = mappedAmount ? mappedAmount + refundCase.merchantAmount : refundCase.merchantAmount;
                this.earningsDateAmountMap.set(dateFormatted.getTime(), mappedAmount);
            });

        const chartLabels = this.chartService.getChartLabels(daysToShow, Array.from(this.earningsDateAmountMap.keys()));

        this.earningsData = {
            labels: chartLabels,
            datasets: [
                {
                    label: 'Tax free earnings',
                    data: Array.from(this.earningsDateAmountMap.values()),
                    fill: false,
                    lineTension: 0,
                    borderColor: 'rgba(48,56,128,1)',
                    backgroundColor: 'rgba(48,56,128,0.8)',
                    borderWidth: 1
                },
            ]
        };

        this.earningsOptions = {
            title: {
                display: true,
                text: 'Tax free earnings',
                fontSize: 16
            },
            responsive: true,
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


    getUsersByCountry(refundCases: RefundCase[]): Map<string, number> {
        const usersByCountryMap = new Map<string, number>();
        refundCases
            .forEach(r => {
                if (r.customer && r.customer.country) {
                    let amount: number = usersByCountryMap.get(r.customer.country);
                    amount = amount ? ++amount : 1;
                    usersByCountryMap.set(r.customer.country, amount);
                }
            });
        return usersByCountryMap;
    }

}
