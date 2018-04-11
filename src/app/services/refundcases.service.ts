import { Injectable } from '@angular/core';
import { RefundCase } from '../models';
import { HttpClient } from '@angular/common/http';

@Injectable()
export class RefundCasesService {

  constructor(private http: HttpClient) { }

  getAll() {
    return this.http.get<RefundCase[]>('/api/merchant/refundcase');
  }

  getById(id: number) {
    return this.http.get(`/api/merchant/refundcase/${id}`);
  }

  accept(refundCase: RefundCase) {
    return this.http.post(`/api/merchant/refundcase/${refundCase.id}/accept`, {
      isAccepted: refundCase.isAccepted
    });
  }

  delete(id: number) {
    return this.http.delete(`/api/merchant/refundcase/${id}`);
  }
}
