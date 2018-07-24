import {Injectable} from '@angular/core';
import {RefundCase} from '../models';
import {SelectItem} from 'primeng/api';

@Injectable()
export class ChartService {
    months: string[];
    days: string[];

    periodOptions: SelectItem[] = [
        {label: 'Last week', value: 7},
        {label: 'Last 14 days', value: 14},
        {label: 'Last month', value: 30},
        {label: 'Last year', value: 365},
        {label: 'All time', value: 0}
    ];

    constructor() {
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
    }

    getPeriodOptions(): SelectItem[] {
        return this.periodOptions;
    }

    getMonths(): string[] {
        return this.months;
    }

    getDays(): string[] {
        return this.days;
    }

    getWeekNumber(d: Date): number {
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

}
