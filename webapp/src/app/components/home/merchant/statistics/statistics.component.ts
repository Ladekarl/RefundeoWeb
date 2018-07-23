import {Component, OnInit} from '@angular/core';
import {MerchantInfo, RefundCase} from '../../../../models';
import {
    MerchantInfoService,
    authorizationService,
    ColorsService,
    RefundCasesService,
    AuthorizationService
} from '../../../../services';
import {Message, SelectItem} from 'primeng/api';
import {Ng4LoadingSpinnerService} from 'ng4-loading-spinner';
import {Observable} from 'rxjs/Observable';
import 'rxjs/add/observable/forkJoin';

@Component({
    selector: 'app-statistics',
    templateUrl: './statistics.component.html',
    styleUrls: ['./statistics.component.scss']
})
export class StatisticsComponent implements OnInit {

    usersByCountry: any;
    totalRefundCases: number;
    purchaseData: any;
    earningsData: any;
    monthsData: any;
    meanPurchaseTime: string;
    bestMonth: string;
    bestDay: string;
    averageSpent: number;
    purchasePeriodOptionsKey = 30;
    earningsPeriodOptionsKey = 30;
    monthsPeriodOptionsKey = 0;
    statisticsPeriodOptionsKey = 0;
    customersPeriodOptionsKey = 0;
    refundCases: RefundCase[];
    growls: Message[];
    purchasesDateAmountMap: Map<number, number>;
    earningsDateAmountMap: Map<number, number>;
    merchantInfo: MerchantInfo;
    purchaseOptions: any;
    earningsOptions: any;
    monthsOptions: any;
    customersOptions: any;

    periodOptions: SelectItem[] = [
        {label: 'Last week', value: 7},
        {label: 'Last 14 days', value: 14},
        {label: 'Last month', value: 30},
        {label: 'Last year', value: 365},
        {label: 'All time', value: 0}
    ];

    months: string[];
    days: string[];

    constructor(private merchantInfoService: MerchantInfoService,
                private authorizationService: AuthorizationService,
                private spinnerService: Ng4LoadingSpinnerService,
                private colorsService: ColorsService,
                private refundCasesService: RefundCasesService) {
        let months = [];
        months[0] = 'January';
        months[1] = 'February';
        months[2] = 'March';
        months[3] = 'April';
        months[4] = 'May';
        months[5] = 'June';
        months[6] = 'July';
        months[7] = 'August';
        months[8] = 'September';
        months[9] = 'October';
        months[10] = 'November';
        months[11] = 'December';
        this.months = months;

        let days = [];
        days[0] = 'Sunday';
        days[1] = 'Monday';
        days[2] = 'Tuesday';
        days[3] = 'Wednesday';
        days[4] = 'Thursday';
        days[5] = 'Friday';
        days[6] = 'Saturday';
        this.days = days;
        this.merchantInfo = new MerchantInfo();
        this.growls = [];
    }

    ngOnInit(): void {
        let tasks = [];
        this.spinnerService.show();

        tasks.push(this.refundCasesService.getAll().map(refundCases => {
            this.refundCases = refundCases.filter(r => r.isAccepted);
        }));
        tasks.push(this.merchantInfoService.getMerchant(this.authorizationService.getCurrentUser().id).map(merchantInfo => {
            this.merchantInfo = merchantInfo;
        }));

        Observable.forkJoin(tasks).subscribe(() => {
            if (this.refundCases && this.refundCases.length > 0) {
                this.setUsersByCountryData(this.refundCases, this.customersPeriodOptionsKey);
                this.setPurchaseData(this.refundCases, this.purchasePeriodOptionsKey);
                this.setEarningsData(this.refundCases, this.earningsPeriodOptionsKey);
                this.setMonthsData(this.refundCases, this.monthsPeriodOptionsKey);
                this.calculateStatistics(this.refundCases, this.statisticsPeriodOptionsKey);
            }
            this.spinnerService.hide();
        }, () => {
            this.spinnerService.hide();
        });
    }

    calculateStatistics(refundCases: RefundCase[], daysToShow: number) {
        refundCases = this.filterDays(daysToShow, refundCases);

        let times = refundCases.map(x => x.dateCreated.getHours());

        times.sort((a, b) => a - b);

        this.meanPurchaseTime = ('0' + times[Math.floor(times.length / 2)].toString()).slice(-2) + ':00';

        let monthsDateAmountMap = new Map<string, number>();
        let daysDateAmountMap = new Map<string, number>();

        let totalSpent = 0;

        refundCases
            .sort((a: RefundCase, b: RefundCase) => a.dateCreated.getTime() - b.dateCreated.getTime())
            .forEach(refundCase => {
                const date = refundCase.dateCreated;
                let mappedMonth: number = monthsDateAmountMap.get(this.months[date.getMonth()]);
                let mappedDay: number = daysDateAmountMap.get(this.days[date.getDay()]);
                mappedMonth = mappedMonth ? mappedMonth + 1 : 1;
                mappedDay = mappedDay ? mappedDay + 1 : 1;
                monthsDateAmountMap.set(this.months[date.getMonth()], mappedMonth);
                daysDateAmountMap.set(this.days[date.getDay()], mappedDay);
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

    selectPurchaseData(event) {
        let keys = Array.from(this.purchasesDateAmountMap.keys());
        let time = keys[event.element._index];
        let amount = this.purchasesDateAmountMap.get(time);

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

    onPurchasePeriodChange(event) {
        if (this.refundCases && this.refundCases.length > 0) {
            this.setPurchaseData(this.refundCases, event.value);
        }
    }

    onEarningsPeriodChange(event) {
        if (this.refundCases && this.refundCases.length > 0) {
            this.setEarningsData(this.refundCases, event.value);
        }
    }

    onStatisticsPeriodChange(event) {
        if (this.refundCases && this.refundCases.length > 0) {
            this.calculateStatistics(this.refundCases, event.value);
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
        refundCases = this.filterDays(daysToShow, refundCases);

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

    getWeekNumber(d: Date) {
        d = new Date(Date.UTC(d.getFullYear(), d.getMonth(), d.getDate()));
        d.setUTCDate(d.getUTCDate() + 4 - (d.getUTCDay() || 7));
        let yearStart: Date = new Date(Date.UTC(d.getUTCFullYear(), 0, 1));
        return Math.ceil((((d.getTime() - yearStart.getTime()) / 86400000) + 1) / 7);
    }

    filterDays(daysToShow: number, refundCases: RefundCase[]): RefundCase[] {
        if (daysToShow !== 0) {
            let stopDate = new Date();
            stopDate.setTime(stopDate.getTime() - 1000 * 60 * 60 * 24 * daysToShow);
            refundCases = refundCases.filter(x => x.dateCreated >= stopDate);
        }
        return refundCases;
    }

    getMonday(date) {
        let date = new Date(date.getTime());
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
        let monthsDateAmountMap = new Map<string, number>();

        refundCases = this.filterDays(daysToShow, refundCases);

        refundCases
            .sort((a: RefundCase, b: RefundCase) => a.dateCreated.getTime() - b.dateCreated.getTime())
            .forEach(refundCase => {
                const dateFormatted = this.getDateFormatted(refundCase.dateCreated, daysToShow);
                let mappedAmount: number = monthsDateAmountMap.get(dateFormatted.getTime());
                mappedAmount = mappedAmount ? mappedAmount + 1 : 1;
                monthsDateAmountMap.set(dateFormatted.getTime(), mappedAmount);
            });

        const chartLabels = this.getChartLabels(daysToShow, Array.from(monthsDateAmountMap.keys()), false);

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

        refundCases = this.filterDays(daysToShow, refundCases);

        refundCases
            .sort((a: RefundCase, b: RefundCase) => a.dateCreated.getTime() - b.dateCreated.getTime())
            .forEach(refundCase => {
                const date: Date = refundCase.dateCreated;
                const dateFormatted: Date = new Date(date.getFullYear(), date.getMonth(), date.getDate());
                let mappedAmount: number = this.earningsDateAmountMap.get(dateFormatted.getTime());
                mappedAmount = mappedAmount ? mappedAmount + refundCase.merchantAmount : refundCase.merchantAmount;
                this.earningsDateAmountMap.set(dateFormatted.getTime(), mappedAmount);
            });

        const chartLabels = this.getChartLabels(daysToShow, Array.from(this.earningsDateAmountMap.keys()));

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

    setPurchaseData(refundCases: RefundCase[], daysToShow: number) {
        this.purchasesDateAmountMap = new Map<number, number>();

        refundCases = this.filterDays(daysToShow, refundCases);

        refundCases
            .sort((a: RefundCase, b: RefundCase) => a.dateCreated.getTime() - b.dateCreated.getTime())
            .forEach(refundCase => {
                const date: Date = refundCase.dateCreated;
                const dateFormatted: Date = new Date(date.getFullYear(), date.getMonth(), date.getDate());
                let mappedAmount: number = this.purchasesDateAmountMap.get(dateFormatted.getTime());
                mappedAmount = mappedAmount ? mappedAmount + refundCase.amount : refundCase.amount;
                this.purchasesDateAmountMap.set(dateFormatted.getTime(), mappedAmount);
            });

        const chartLabels = this.getChartLabels(daysToShow, Array.from(this.purchasesDateAmountMap.keys()));

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

    getChartLabels(daysToShow: number, keys: number[], pushIfExists: boolean = true): string[] {
        let chartLabels: string[] = [];
        if (daysToShow > 0 && daysToShow <= 7) {
            for (let time of keys) {
                const date = new Date(time);
                chartLabels.push(date.toLocaleDateString());
            }
        }

        if (daysToShow > 7 && daysToShow < 365) {
            for (let time of keys) {
                const date = new Date(time);
                let week = this.getWeekNumber(date);
                let weekString = 'Week ' + week;
                if (chartLabels.indexOf(weekString) > -1 && pushIfExists) {
                    chartLabels.push('');
                } else {
                    chartLabels.push(weekString);
                }
            }
        }

        if (daysToShow >= 365 || daysToShow === 0) {
            for (let time of keys) {
                const date = new Date(time);
                let month = this.months[date.getMonth()];
                if (chartLabels.indexOf(month) > -1 && pushIfExists) {
                    chartLabels.push('');
                } else {
                    chartLabels.push(month);
                }
            }
        }

        return chartLabels;
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
