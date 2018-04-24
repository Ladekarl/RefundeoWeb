import { Component, OnInit, ViewChild } from '@angular/core';
import { RefundCasesService } from '../../../services';
import { RefundCase } from '../../../models';
import { ConfirmationService, SelectItem } from 'primeng/api';
import { ElementRef } from '@angular/core';
import { DataView } from 'primeng/dataview';

@Component({
  selector: 'app-refundcases',
  templateUrl: './refundcases.component.html',
  styleUrls: ['./refundcases.component.scss']
})
export class RefundCasesComponent {
  @ViewChild('refundCasesDataView') refundCasesDataView: DataView;

  refundCases: RefundCase[];

  sortOptions: SelectItem[] = [
    { label: 'Newest', value: '!dateCreated' },
    { label: 'Oldest', value: 'dateCreated' },
    { label: 'Newest requested', value: '!dateRequested' },
    { label: 'Oldest requested', value: 'dateRequested' },
    { label: 'Status', value: '!isRequested' },
    { label: 'Purchase amount', value: '!amount' },
    { label: 'Refund amount', value: '!refundAmount' },
    { label: 'Customer', value: 'customerinformation' },
    { label: 'Documentation', value: '!documentation' }
  ];
  sortKey: string;
  sortField = 'dateCreated';
  sortOrder = 'desc';

  filterOptions: SelectItem[] = [
    { label: 'None', value: 'none' },
    { label: 'Requested', value: 'isRequested' },
    { label: 'Claimed', value: 'customerinformation' },
    { label: 'Accepted', value: 'isAccepted' },
    { label: 'Documented', value: 'documentation' }
  ];
  filterKey: string;
  filterField = 'none';
  filterOrder = 'desc';

  totalRecords: number;

  constructor(private refundCasesService: RefundCasesService, private confirmationService: ConfirmationService) { }

  onSortChange(event) {
    const value = event.value;

    if (value.indexOf('!') === 0) {
      this.sortOrder = 'desc';
      this.sortField = value.substring(1, value.length);
    } else {
      this.sortOrder = 'asc';
      this.sortField = value;
    }
  }

  onFilterChange(event) {
    const value = event.value;

    if (value.indexOf('!') === 0) {
      this.filterOrder = 'desc';
      this.filterField = value.substring(1, value.length);
    } else {
      this.filterOrder = 'asc';
      this.filterField = value;
    }
    this.refundCasesDataView.sort();
  }

  loadData(event) {
    this.refundCasesService.getPaginated(event.first, event.rows, this.sortField, this.sortOrder, this.filterField)
      .subscribe((response: any) => {
        this.refundCases = response.refundCases;
        this.totalRecords = response.totalRecords;
      });
  }

  accept(refundCase: RefundCase, accept: boolean) {
    const sendObject: RefundCase = Object.assign({}, refundCase);
    sendObject.isAccepted = accept;
    this.confirmationService.confirm({
      message: `Are you sure you want to ${accept ? 'accept' : 'reject'} this refund?`,
      accept: () => {
        this.refundCasesService.accept(sendObject).subscribe(data => {
          refundCase.isAccepted = accept;
        });
      }
    });
  }
}
