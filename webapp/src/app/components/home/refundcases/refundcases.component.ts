import {Component, OnInit, ViewChild} from '@angular/core';
import {RefundCasesService} from '../../../services';
import {RefundCase} from '../../../models';
import {ConfirmationService, SelectItem} from 'primeng/api';
import {DataView} from 'primeng/dataview';
import {Ng4LoadingSpinnerService} from 'ng4-loading-spinner';
import * as JSZip from 'jszip';
import * as FileSaver from 'file-saver';

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
        let zip = new JSZip();
        let shouldMakeZip = false;
        this.loading = true;
        this.refundCases.forEach(r => {
            if (r.checked) {
                let folder = zip.folder(r.dateCreated.toLocaleDateString().replace(new RegExp('/', 'g'), '_'));
                if (r.receiptImage) {
                    folder.file(r.receiptNumber + '_receipt.png', r.receiptImage, {base64: true});
                }
                if (r.vatFormImage) {
                    folder.file(r.receiptNumber + '_vatform.png', r.vatFormImage, {base64: true});
                }
                shouldMakeZip = true;
            }
        });
        if (shouldMakeZip) {
            zip.generateAsync({type: 'blob'}).then((blob) => {
                FileSaver.saveAs(blob, 'refundeo.zip');
                this.loading = false;
            }).catch(() => {
                this.loading = false;
            });
        }
    }

    downloadPressed(refundCase) {
        let zip = new JSZip();
        if (refundCase.receiptImage) {
            zip.file(refundCase.receiptNumber + '_receipt.png', refundCase.receiptImage, {base64: true});
        }
        if (refundCase.vatFormImage) {
            zip.file(refundCase.receiptNumber + '_vatform.png', refundCase.vatFormImage, {base64: true, });
        }

        zip.generateAsync({type: 'blob'}).then((blob) => {
            FileSaver.saveAs(blob, refundCase.dateCreated.toLocaleDateString() + '.zip');
        });
    }
}
