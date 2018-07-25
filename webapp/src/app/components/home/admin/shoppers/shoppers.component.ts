import {Component, OnInit} from '@angular/core';
import {CustomerInfoService} from '../../../../services';
import {ConfirmationService, SelectItem} from 'primeng/api';
import {CustomerInfo} from '../../../../models';
import {Ng4LoadingSpinnerService} from 'ng4-loading-spinner';

@Component({
    selector: 'app-shoppers',
    templateUrl: './shoppers.component.html',
    styleUrls: ['./shoppers.component.scss']
})
export class ShoppersComponent implements OnInit {

    customers: CustomerInfo[];
    loading = false;
    filteredCustomers: CustomerInfo[];

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

    constructor(private customerInfoService: CustomerInfoService,
                private spinnerService: Ng4LoadingSpinnerService,
                private confirmationService: ConfirmationService) {
    }

    ngOnInit() {
        this.getCustomers();
    }

    getCustomers() {
        this.loading = true;
        this.customerInfoService.getAll(true).subscribe(customers => {
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

    onDeleteCustomer(customer: CustomerInfo) {
        this.confirmationService.confirm({
            message: `Are you sure you want to delete ${customer.username}?`,
            accept: () => {
                this.spinnerService.show();
                this.customerInfoService.deleteCustomer(customer).subscribe(() => {
                    this.getCustomers();
                }, (e) => {
                    this.spinnerService.hide();
                    let errorString = `Could not delete ${customer.username}\n`;
                    if (e.error && e.errors) {
                        e.error.errors.forEach(e => {
                            errorString = errorString + e.description + '\n';
                        });
                    }
                    alert(errorString);
                });
            }
        });
    }
}
