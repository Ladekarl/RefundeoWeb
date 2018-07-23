import {Component, OnInit} from '@angular/core';
import {AuthenticationService, ColorsService, CustomerInfoService, RefundCasesService} from '../../../../services';
import {CustomerInfo} from '../../../../models/customerinfo';
import {RefundCase} from '../../../../models';
import {Ng4LoadingSpinnerService} from 'ng4-loading-spinner';

@Component({
    selector: 'app-dashboard',
    templateUrl: 'dashboard.component.html',
    styleUrls: ['dashboard.component.scss']
})

export class DashboardComponent implements OnInit {
    usersByCountry: any;
    totalRefundCases: number;
    chartData: any;

    constructor(private authenticationService: AuthenticationService, private customerInfoService: CustomerInfoService,
                private colorsService: ColorsService, private refundCasesService: RefundCasesService, private spinnerService: Ng4LoadingSpinnerService) {
    }

    ngOnInit(): void {
        this.customerInfoService.getAll().subscribe(customerInfos => {
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
        });
        this.refundCasesService.getAll().subscribe(refundCases => {
            this.initChartData(refundCases);
        });
    }

    initChartData(refundCases: RefundCase[]) {
        let dateAmountMap = new Map<string, number>();
        refundCases.forEach(refundCase => {
            if (refundCase.isAccepted) {
                const date = refundCase.dateCreated.toLocaleDateString();
                let mappedAmount = dateAmountMap.get(date);
                mappedAmount = mappedAmount ? mappedAmount + refundCase.amount : refundCase.amount;
                dateAmountMap.set(date, mappedAmount);
            }
        });
        this.chartData = {
            labels: Array.from(dateAmountMap.keys()),
            datasets: [
                {
                    label: 'Amount refunded',
                    data: Array.from(dateAmountMap.values()),
                    fill: false,
                    borderColor: '#303880'
                },
            ]
        };
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
