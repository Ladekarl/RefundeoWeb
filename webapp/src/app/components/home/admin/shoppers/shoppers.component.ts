import {Component, OnInit} from '@angular/core';
import {CustomerInfoService} from '../../../../services';
import {SelectItem} from 'primeng/api';

@Component({
    selector: 'app-shoppers',
    templateUrl: './shoppers.component.html',
    styleUrls: ['./shoppers.component.scss']
})
export class ShoppersComponent implements OnInit {

    customers;
    loading = false;
    filteredCustomers;

    sortOrderOptions: SelectItem[] = [
        {label: 'Ascending', value: 1},
        {label: 'Descending', value: -1}
    ];

    sortOptions: SelectItem[] = [
        {label: 'Username', value: 'username'},
        {label: 'Email', value: 'email'},
        {label: 'Country', value: 'country'},
    ];

    searchOptions: SelectItem[] = [
        {label: 'Username', value: 'username'},
        {label: 'Email', value: 'email'},
        {label: 'Country', value: 'country'},
        {label: 'Phone', value: 'phone'},
        {label: 'Passport', value: 'passport'},
        {label: 'Swift/BIC', value: 'swift'},
        {label: 'Account number', value: 'accountNumber'},
    ];

    sortField: string;
    sortOrder: string;
    sortKey = 'username';
    searchKey = 'merchant.companyName';
    searchField: 'merchant.companyName';
    sortOrderKey = 1;

    constructor(private customerInfoService: CustomerInfoService) {
    }

    ngOnInit() {
        this.loading = true;
        this.customerInfoService.getAll().subscribe(customers => {
            this.customers = customers;
            this.loading = false;
        }, () => {
            this.loading = false;
        });
    }

    onSortChange(event) {
        this.sortField = event.value;
    }

    onSearchChange(event) {
        this.searchField = event.value;
    }

    onSortOrderChange(event) {
        this.sortOrder = event.value;
    }
}
