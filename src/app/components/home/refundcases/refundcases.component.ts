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

  accept(refundCase: RefundCase, accept: boolean) {
    const sendObject: RefundCase = Object.assign({ isAccepted: accept }, refundCase);
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
