import {Injectable} from '@angular/core';
import {RefundCase} from '../models';
import {HttpClient} from '@angular/common/http';
import 'rxjs/add/operator/map';

@Injectable()
export class RefundCasesService {

    constructor(private http: HttpClient) {
    }

    private static mapDates(refundCases: RefundCase[]): RefundCase[] {
        if (refundCases) {
            refundCases.forEach(RefundCasesService.mapDate);
        }
        return refundCases;
    }

    private static mapDate(refundCase: RefundCase): RefundCase {
        if (refundCase) {
            refundCase.dateCreated = new Date(refundCase.dateCreated);
            refundCase.dateRequested = new Date(refundCase.dateRequested);
        }
        return refundCase;
    }

    getAll() {
        return this.http.get<RefundCase[]>('/api/merchant/refundcase').map(RefundCasesService.mapDates);
    }

    getPaginated(first: number, amount: number, sortBy: string, sortDir: string, filterBy: string) {
        return this
            .http
            .get(`/api/merchant/refundcase/${first}/${amount}/${sortBy}/${sortDir}/${filterBy}`)
            .map((r: any) => {
                RefundCasesService.mapDates(r.refundCases);
                return r;
            });
    }

    getById(id: number) {
        return this
            .http
            .get(`/api/merchant/refundcase/${id}`)
            .map(RefundCasesService.mapDate);
    }

    accept(refundCase: RefundCase) {
        return this
            .http
            .post(`/api/merchant/refundcase/${refundCase.id}/accept`, {isAccepted: refundCase.isAccepted});
    }

    delete(id: number) {
        return this
            .http
            .delete(`/api/merchant/refundcase/${id}`);
    }
}
