import {Component, OnInit} from '@angular/core';
import {MerchantInfoService} from '../../../../services';
import {SelectItem, ConfirmationService} from 'primeng/api';
import {MerchantInfo} from '../../../../models';
import {Router} from '@angular/router';
import {Ng4LoadingSpinnerService} from 'ng4-loading-spinner';

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

    constructor(
        private confirmationService: ConfirmationService,
        private router: Router,
        private spinnerService: Ng4LoadingSpinnerService,
        private merchantInfoService: MerchantInfoService) {
    }

    ngOnInit() {
        this.loading = true;
        this.getMerchants();
    }

    getMerchants() {
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

    onMerchantClick(merchant: MerchantInfo) {
        this.router.navigate(['/admin/editretailer'], {queryParams: {id: merchant.id}});
    }

    onDeleteMerchant(merchant: MerchantInfo) {
        this.confirmationService.confirm({
            message: `Are you sure you want to delete ${merchant.companyName}?`,
            accept: () => {
                this.spinnerService.show();
                this.merchantInfoService.deleteMerchant(merchant).subscribe(() => {
                    this.getMerchants();
                }, (e) => {
                    this.spinnerService.hide();
                    let errorString = `Could not delete ${merchant.companyName}\n`;
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
