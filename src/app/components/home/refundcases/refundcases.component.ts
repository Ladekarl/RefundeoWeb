import { Component, OnInit } from '@angular/core';
import { RefundCasesService } from '../../../services';
import { RefundCase } from '../../../models';
import { ConfirmationService } from 'primeng/api';

@Component({
  selector: 'app-refundcases',
  templateUrl: './refundcases.component.html',
  styleUrls: ['./refundcases.component.scss']
})
export class RefundCasesComponent implements OnInit {
  refundCases: RefundCase[];

  constructor(private refundCasesService: RefundCasesService, private confirmationService: ConfirmationService) { }

  ngOnInit() {
    this.refundCasesService.getAll().subscribe((refundCases: RefundCase[]) => {
      this.refundCases = refundCases;
    });
  }

  accept(refundCase: RefundCase) {
    this.confirmationService.confirm({
      message: 'Are you sure you want to accept this refund?',
      accept: () => {
      }
    });
  }

  reject(refundCase: RefundCase) {
    this.confirmationService.confirm({
      message: 'Are you sure you want to reject this refund?',
      accept: () => {
      }
    });
  }

}
