import {Component, OnInit, ViewChild} from '@angular/core';
import {AuthorizationService, MerchantInfoService, RefundCasesService} from '../../../../services';
import {MerchantInfo, RefundCase} from '../../../../models';
import {SelectItem} from 'primeng/api';
import {DataView} from 'primeng/dataview';
import * as JSZip from 'jszip';
import * as FileSaver from 'file-saver';
import {Ng4LoadingSpinnerService} from 'ng4-loading-spinner';
import {forkJoin} from 'rxjs';
import {map} from 'rxjs/operators';

@Component({
    selector: 'app-refundcases',
    templateUrl: './refundcases.component.html',
    styleUrls: ['./refundcases.component.scss']
})
export class RefundCasesComponent implements OnInit {
    @ViewChild('refundCasesDataView') refundCasesDataView: DataView;

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
        {label: 'Refund amount', value: 'refundAmount'}
    ];

    sortField: string;
    sortOrder: string;
    sortKey = 'dateCreated';
    sortOrderKey = -1;
    filterField = 'none';
    loading = false;
    checkAll: boolean;
    merchantInfo: MerchantInfo;

    filterOptions: SelectItem[] = [
        {label: 'None', value: 'none'},
        {label: 'New', value: '!isRequested'},
        {label: 'Requested', value: 'isRequested'},
        {label: 'Accepted', value: 'isAccepted'},
        {label: 'Rejected', value: 'isRejected'}
    ];

    constructor(
        private refundCasesService: RefundCasesService,
        private merchantInfoService: MerchantInfoService,
        private authorizationService: AuthorizationService,
        private spinnerService: Ng4LoadingSpinnerService) {
        this.merchantInfo = new MerchantInfo();
    }

    ngOnInit() {
        this.loadData();
    }


    onSortChange(event) {
        this.sortField = event.value;
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

    onCheckAllChange() {
        this.refundCases.forEach(r => {
            if (r.isRequested) {
                r.checked = this.checkAll;
            }
            return r;
        });
    }

    loadData() {
        this.loading = true;
        this.spinnerService.show();

        this.authorizationService.getCurrentUser().subscribe(currentUser => {
            let tasks = [];
            tasks.push(this.refundCasesService.getAll(false)
                .pipe(map((refundCases: RefundCase[]) => {
                    this.refundCases = refundCases.reverse().slice(0, 5);
                })));
            tasks.push(this.merchantInfoService.getMerchant(currentUser.id).pipe(map(merchantInfo => {
                this.merchantInfo = merchantInfo;
            })));

            forkJoin(tasks).subscribe(() => {
                this.spinnerService.hide();
                this.loading = false;
            }, () => {
                this.spinnerService.hide();
                this.loading = false;
            });
        }, () => {
            this.spinnerService.hide();
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
}
