import {Component, OnInit} from '@angular/core';
import {RefundCasesService} from '../../../../services';
import {RefundCase} from '../../../../models';
import {ConfirmationService, SelectItem} from 'primeng/api';
import {Ng4LoadingSpinnerService} from 'ng4-loading-spinner';
import * as JSZip from 'jszip';
import * as FileSaver from 'file-saver';

@Component({
    selector: 'app-adminrefundcases',
    templateUrl: './adminrefundcases.component.html',
    styleUrls: ['./adminrefundcases.component.scss']
})
export class AdminRefundcasesComponent implements OnInit {
    refundCases: RefundCase[];
    filteredRefundCases: RefundCase[];

    sortOrderOptions: SelectItem[] = [
        {label: 'Ascending', value: 1},
        {label: 'Descending', value: -1}
    ];

    sortOptions: SelectItem[] = [
        {label: 'Date created', value: 'dateCreated'},
        {label: 'Receipt number', value: 'receiptNumber'},
        {label: 'Purchase amount', value: 'amount'},
        {label: 'Refund amount', value: 'refundAmount'},
        {label: 'Company', value: 'merchant.companyName'},
        {label: 'Customer', value: 'customer.email'}
    ];

    searchOptions: SelectItem[] = [
        {label: 'Company name', value: 'merchant.companyName'},
        {label: 'Customer email', value: 'customer.email'},
        {label: 'Receipt number', value: 'receiptNumber'}
    ];

    sortField: string;
    sortOrder: string;
    sortKey = 'dateCreated';
    searchKey = 'merchant.companyName';
    searchField: 'merchant.companyName';
    sortOrderKey = -1;
    filterField = 'none';
    loading = false;
    checkAll: boolean;

    filterOptions: SelectItem[] = [
        {label: 'None', value: 'none'},
        {label: 'New', value: '!isRequested'},
        {label: 'Requested', value: 'isRequested'},
        {label: 'Accepted', value: 'isAccepted'},
        {label: 'Rejected', value: 'isRejected'}
    ];

    constructor(private refundCasesService: RefundCasesService, private confirmationService: ConfirmationService, private spinnerService: Ng4LoadingSpinnerService) {
    }

    ngOnInit() {
        this.loadData();
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

    onFilterChange(event) {
        if (!this.refundCases) return;

        let filterField = event.value;
        let isNegated = false;

        if (filterField.indexOf('!') === 0) {
            filterField = filterField.substring(1, filterField.length);
            isNegated = true;
        }

        if (filterField === 'none')
            this.filteredRefundCases = this.refundCases;
        else {
            if (isNegated)
                this.filteredRefundCases = this.refundCases.filter(r => !r[filterField]);
            else
                this.filteredRefundCases = this.refundCases.filter(r => r[filterField]);
        }
    }

    onCheckAllChange(event) {
        this.refundCases.forEach(r => {
            if (r.isRequested) {
                r.checked = this.checkAll;
            }
            return r;
        });
    }

    loadData() {
        this.loading = true;
        this.refundCasesService.getAll()
            .subscribe((refundCases: RefundCase[]) => {
                this.refundCases = refundCases.reverse();
                this.loading = false;
            }, () => {
                this.loading = false;
            });
    }

    downloadCheckedPressed() {
        if (!this.refundCases) return;
        let zip = new JSZip();
        let shouldMakeZip = false;
        this.loading = true;
        this.refundCases.forEach(r => {
            if (r.checked) {
                let folder = zip.folder(r.dateCreated.toLocaleDateString().replace(new RegExp('/', 'g'), '_'));
                if (r.receiptImage) {
                    shouldMakeZip = true;
                    folder.file(r.receiptNumber + '_receipt.png', r.receiptImage, {base64: true});
                }
                if (r.vatFormImage) {
                    shouldMakeZip = true;
                    folder.file(r.receiptNumber + '_vatform.png', r.vatFormImage, {base64: true});
                }
            }
        });
        if (shouldMakeZip) {
            zip.generateAsync({type: 'blob'}).then((blob) => {
                FileSaver.saveAs(blob, 'refundeo.zip');
                this.loading = false;
            }).catch(() => {
                this.loading = false;
            });
        } else {
            this.loading = false;
        }
    }

    downloadPressed(refundCase) {
        let zip = new JSZip();
        let shouldMakeZip = false;
        if (refundCase.receiptImage) {
            shouldMakeZip = true;
            zip.file(refundCase.receiptNumber + '_receipt.png', refundCase.receiptImage, {base64: true});
        }
        if (refundCase.vatFormImage) {
            shouldMakeZip = true;
            zip.file(refundCase.receiptNumber + '_vatform.png', refundCase.vatFormImage, {base64: true,});
        }
        if (shouldMakeZip) {
            zip.generateAsync({type: 'blob'}).then((blob) => {
                FileSaver.saveAs(blob, refundCase.dateCreated.toLocaleDateString() + '.zip');
            });
        }
    }

    onAcceptClick(refundCase) {
        this.confirmChoise(refundCase, true);
    }

    onRejectClick(refundCase) {
        this.confirmChoise(refundCase, false);
    }

    confirmChoise(refundCase, accept) {
        this.confirmationService.confirm({
            message: `Are you sure you want to ${accept ? 'accept' : 'reject'} this refund?`,
            accept: () => {
                this.loading = true;
                this.refundCasesService.accept(refundCase, accept).subscribe(() => {
                    refundCase.isAccepted = accept;
                    refundCase.isRejected = !accept;
                    this.loading = false;
                }, () => {
                    this.loading = false;
                });
            }
        });
    }
}
