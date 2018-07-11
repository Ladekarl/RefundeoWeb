import {Injectable} from '@angular/core';
import {RefundCase} from '../models';
import {HttpClient} from '@angular/common/http';
import 'rxjs/add/operator/map';
import {Observable} from 'rxjs/Observable';
import 'rxjs/Rx';

@Injectable()
export class RefundCasesService {

    constructor(private http: HttpClient) {
    }

    refundCases: RefundCase[];

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

    getAll(): Observable<RefundCase[]> {
        if (!this.refundCases || this.refundCases.length === 0)
            return this.http.get<RefundCase[]>('/api/merchant/refundcase').map(r => {
                this.refundCases = RefundCasesService.mapDates(r);
                return this.refundCases;
            });
        else
            return Observable.of(this.refundCases);
    }

    resetRefundCases() {
        this.refundCases = [];
    }

    getPaginated(sortBy: string, sortDir: string, filterBy: string): Observable<RefundCase[]> {
        if (!this.refundCases || this.refundCases.length === 0)
            return this.getAll().map(r => {
                return this.filterRefundCases(r, sortBy, sortDir, filterBy);
            });
        else
            return Observable.of(this.filterRefundCases(this.refundCases, sortBy, sortDir, filterBy));
    }

    private filterRefundCases(refundCases: RefundCase[], sortBy: string, sortDir: string, filterBy: string) {
        let filteredRefundCases = refundCases;
        if (filterBy !== 'none')
            filteredRefundCases = refundCases.filter(r => r[filterBy]);

        filteredRefundCases = filteredRefundCases.sort((a, b) => {
            switch (sortBy) {
                case 'dateCreated':
                case 'dateRequested':
                    return sortDir === 'asc' ? (a > b ? -1 : a < b ? 1 : 0) : (a > b ? 1 : a < b ? -1 : 0);
            }
        });
        return filteredRefundCases;
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
