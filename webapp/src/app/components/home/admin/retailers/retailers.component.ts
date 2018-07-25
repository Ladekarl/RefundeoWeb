import {Component, OnInit} from '@angular/core';
import {MerchantInfoService} from '../../../../services';
import {SelectItem} from 'primeng/api';
import {MerchantInfo} from '../../../../models';

@Component({
    selector: 'app-retailers',
    templateUrl: './retailers.component.html',
    styleUrls: ['./retailers.component.scss']
})
export class RetailersComponent implements OnInit {

    merchants: MerchantInfo[];
    loading = false;
    filteredMerchants: MerchantInfo[];

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

    constructor(private merchantInfoService: MerchantInfoService) {
    }

    ngOnInit() {
        this.loading = true;
        this.merchantInfoService.getAll().subscribe(merchants => {
            this.merchants = merchants;
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
