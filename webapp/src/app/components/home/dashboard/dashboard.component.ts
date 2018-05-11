import {Component, OnInit} from '@angular/core';
import {User, RefundCase} from '../../../models/index';
import {UserService, RefundCasesService, AuthenticationService, ColorsService, CustomerInfoService} from '../../../services/index';
import {CurrentUser} from '../../../models';
import {CustomerInfo} from '../../../models/customerinfo';

@Component({
    selector: 'app-dashboard',
    templateUrl: 'dashboard.component.html',
    styleUrls: ['dashboard.component.scss']
})

export class DashboardComponent implements OnInit {
    currentUser: CurrentUser;
    usersByCountry: any;
    distinctUsersByCountry: any;
    totalCustomers: number;
    totalRefundCases: number;

    constructor(private authenticationService: AuthenticationService, private customerInfoService: CustomerInfoService,
                private colorsService: ColorsService) {
    }

    ngOnInit(): void {
        this.customerInfoService.getAll().subscribe(customerInfos => {
            const usersByCountryMap = this.getUsersByCountry(customerInfos);
            const distinctUsersByCountryMap = this.getDistinctUsersByCountry(customerInfos);
            const countries = Array.from(usersByCountryMap.keys());
            const amounts = Array.from(usersByCountryMap.values());
            const distinctAmounts = Array.from(distinctUsersByCountryMap.values());
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
            this.distinctUsersByCountry = {
                labels: countries.map(c => `${c} ${distinctUsersByCountryMap.get(c)}`),
                datasets: [
                    {
                        backgroundColor: colorPalette,
                        hoverBackgroundColor: colorPalette,
                        data: distinctAmounts
                    }]
            };
        });
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

    getDistinctUsersByCountry(customerInfos: CustomerInfo[]): Map<string, number> {
        const usersByCountryMap = new Map<string, number>();
        const usersMap = new Map<string, boolean>();
        customerInfos.forEach(c => {
            if (usersMap.get(c.id)) {
                return;
            }
            let amount: number = usersByCountryMap.get(c.country);
            amount = amount ? ++amount : 1;
            usersByCountryMap.set(c.country, amount);
            usersMap.set(c.id, true);
        });
        this.totalCustomers = usersMap.size;
        return usersByCountryMap;
    }
}
