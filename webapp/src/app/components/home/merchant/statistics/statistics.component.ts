import {Component, OnInit} from '@angular/core';
import {CustomerInfo, RefundCase} from '../../../../models';
import {AuthenticationService, ColorsService, CustomerInfoService, RefundCasesService} from '../../../../services';
import {Ng4LoadingSpinnerService} from 'ng4-loading-spinner';
import {Message, SelectItem} from 'primeng/api';
import index from '@angular/cli/lib/cli';

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
    purchasePeriodOptionsKey = 14;
    earningsPeriodOptionsKey = 14;
    refundCases: RefundCase[];
    growls: Message[];
    purchasesDateAmountMap: Map<number, number>;
    earningsDateAmountMap: Map<number, number>;

    periodOptions: SelectItem[] = [
        {label: 'Last week', value: 7},
        {label: 'Last 14 days', value: 14},
        {label: 'Last month', value: 30},
        {label: 'Last year', value: 365},
        {label: 'All time', value: 0}
    ];

    months: string[];

    constructor(private authenticationService: AuthenticationService, private customerInfoService: CustomerInfoService,
                private colorsService: ColorsService, private refundCasesService: RefundCasesService) {
        let month = [];
        month[0] = 'January';
        month[1] = 'February';
        month[2] = 'March';
        month[3] = 'April';
        month[4] = 'May';
        month[5] = 'June';
        month[6] = 'July';
        month[7] = 'August';
        month[8] = 'September';
        month[9] = 'October';
        month[10] = 'November';
        month[11] = 'December';
        this.months = month;
        this.growls = [];
    }

    ngOnInit(): void {
        this.customerInfoService.getAll().subscribe(customerInfos => {
            this.initUsersByCountryData(customerInfos);
        });
        this.refundCasesService.getAll().subscribe(refundCases => {
            this.refundCases = refundCases.filter(r => r.isAccepted);
            if (this.refundCases && this.refundCases.length > 0) {
                this.setPurchaseData(this.refundCases, this.purchasePeriodOptionsKey);
                this.setEarningsData(this.refundCases, this.earningsPeriodOptionsKey);
                this.setMonthsData(this.refundCases);
                this.calculateStatistics(this.refundCases);
            }
        });
    }

    calculateStatistics(refundCases: RefundCase[]) {
        let times = refundCases.map(x => x.dateCreated.getHours());

        times.sort((a, b) => a - b);

        this.meanPurchaseTime = times[times.length / 2].toString();

        let monthsDateAmountMap = new Map<string, number>();

        refundCases
            .sort((a: RefundCase, b: RefundCase) => a.dateCreated.getTime() - b.dateCreated.getTime())
            .forEach(refundCase => {
                const date = refundCase.dateCreated;
                let mappedAmount: number = monthsDateAmountMap.get(this.months[date.getMonth()]);
                mappedAmount = mappedAmount ? mappedAmount + 1 : 1;
                monthsDateAmountMap.set(this.months[date.getMonth()], mappedAmount);
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
    }

    selectPurchaseData(event) {
        let keys = Array.from(this.purchasesDateAmountMap.keys());
        let time = keys[event.element._index];
        let amount = this.purchasesDateAmountMap.get(time);

        this.growls = [];

        if (time && amount) {
            const date = new Date(time);
            this.growls.push({
                severity: 'info',
                summary: `Purchases on ${date.toLocaleDateString()}`,
                'detail': `Purchase amount: ${amount}`
            });
        }
    }

    selectEarningsData(event) {
        let keys = Array.from(this.earningsDateAmountMap.keys());
        let time = keys[event.element._index];
        let amount = this.earningsDateAmountMap.get(time);

        this.growls = [];

        if (time && amount) {
            const date = new Date(time);
            this.growls.push({
                severity: 'info',
                summary: `Earnings on ${date.toLocaleDateString()}`,
                'detail': `Earnings amount: ${amount}`
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

    initUsersByCountryData(customerInfos: CustomerInfo[]) {
        const usersByCountryMap = this.getUsersByCountry(customerInfos);
        const countries = Array.from(usersByCountryMap.keys());
        const amounts = Array.from(usersByCountryMap.values());
        const colorPalette = this.colorsService.getColorPallete(usersByCountryMap.keys.length);
        this.totalRefundCases = customerInfos.length;
        this.usersByCountry = {
            labels: countries.map(c => `${c} ${usersByCountryMap.get(c)}`),
            datasets: [
                {
                    backgroundColor: colorPalette,
                    hoverBackgroundColor: colorPalette,
                    data: amounts
                }]
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

    setMonthsData(refundCases: RefundCase[]) {
        let monthsDateAmountMap = new Map<string, number>();

        this.months.forEach(month => {
            monthsDateAmountMap.set(month, 0);
        });

        refundCases
            .sort((a: RefundCase, b: RefundCase) => a.dateCreated.getTime() - b.dateCreated.getTime())
            .forEach(refundCase => {
                const date: Date = refundCase.dateCreated;
                let mappedAmount: number = monthsDateAmountMap.get(this.months[date.getMonth()]);
                mappedAmount++;
                monthsDateAmountMap.set(this.months[date.getMonth()], mappedAmount);
            });

        this.monthsData = {
            labels: Array.from(monthsDateAmountMap.keys()),
            datasets: [
                {
                    label: 'Customers',
                    data: Array.from(monthsDateAmountMap.values()),
                    borderColor: '#303880',
                    backgroundColor: '#303880'
                },
            ]
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
                    label: 'Earnings amount',
                    data: Array.from(this.earningsDateAmountMap.values()),
                    fill: false,
                    borderColor: '#303880'
                },
            ]
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
                    label: 'Purchase amount',
                    data: Array.from(this.purchasesDateAmountMap.values()),
                    fill: false,
                    borderColor: '#303880'
                },
            ]
        };
    }

    getChartLabels(daysToShow: number, keys: number[]): string[] {
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
                if (chartLabels.indexOf(weekString) > -1) {
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
                if (chartLabels.indexOf(month) > -1) {
                    chartLabels.push('');
                } else {
                    chartLabels.push(month);
                }
            }
        }

        return chartLabels;
    }

    getUsersByCountry(customerInfos: CustomerInfo[]): Map<string, number> {
        const usersByCountryMap = new Map<string, number>();
        customerInfos.forEach(c => {
            let amount: number = usersByCountryMap.get(c.country);
            amount = amount ? ++amount : 1;
            usersByCountryMap.set(c.country, amount);
        });
        return usersByCountryMap;
    }

}
