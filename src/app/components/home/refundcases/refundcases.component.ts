import { Component, OnInit } from '@angular/core';
import { RefundCasesService } from '../../../services';
import { RefundCase } from '../../../models';
import { ConfirmationService, SelectItem } from 'primeng/api';

@Component({
  selector: 'app-refundcases',
  templateUrl: './refundcases.component.html',
  styleUrls: ['./refundcases.component.scss']
})
export class RefundCasesComponent {
  refundCases: RefundCase[];
  sortOptions: SelectItem[] = [
    { label: 'Newest created', value: '!dateCreated' },
    { label: 'Oldest created', value: 'dateCreated' },
    { label: 'Newest requested', value: '!dateRequested' },
    { label: 'Oldest requested', value: 'dateRequested' },
    { label: 'Status', value: '!isRequested' },
    { label: 'Purchase amount', value: '!amount' },
    { label: 'Refund amount', value: '!refundAmount' },
    { label: 'Customer', value: '!customer.id' },
    { label: 'Documentation', value: '!documentation' }
  ];
  sortKey: string;
  sortField: string;
  sortOrder: number;
  totalRecords: number;

  constructor(private refundCasesService: RefundCasesService, private confirmationService: ConfirmationService) { }

  onSortChange(event) {
    const value = event.value;

    if (value.indexOf('!') === 0) {
      this.sortOrder = -1;
      this.sortField = value.substring(1, value.length);
    } else {
      this.sortOrder = 1;
      this.sortField = value;
    }
  }


  loadData(event) {
    const sortBy = this.sortField || 'dateCreated';
    const sortDir = this.sortOrder || -1;
    this.refundCasesService.getPaginated(event.first, event.rows, sortBy, sortDir)
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
        this.refundCasesService.accept(refundCase).subscribe(data => {
          refundCase.isAccepted = accept;
        });
      }
    });
  }
}
