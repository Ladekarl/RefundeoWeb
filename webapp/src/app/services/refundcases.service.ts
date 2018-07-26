import {Injectable} from '@angular/core';
import {RefundCase} from '../models';
import {HttpClient} from '@angular/common/http';
import {map, share} from 'rxjs/operators';
import {Observable, of} from 'rxjs';

@Injectable()
export class RefundCasesService {

    constructor(private http: HttpClient) {
    }

    refundCases: RefundCase[];
    getAllObservable: Observable<RefundCase[]>;
    getPaginatedObservable: Observable<RefundCase[]>;

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

    getAll(isAdmin: boolean): Observable<RefundCase[]> {
        if (this.refundCases && this.refundCases.length > 0) {
            return of(this.refundCases);
        } else if (this.getAllObservable) {
            return this.getAllObservable;
        } else {
            let requestUrl = isAdmin ? '/api/admin/refundcase' : '/api/merchant/refundcase';
            this.getAllObservable = this.http.get<RefundCase[]>(requestUrl)
                .pipe(
                    map(r => {
                        this.getAllObservable = null;
                        this.refundCases = RefundCasesService.mapDates(r);
                        return this.refundCases.sort((a, b) => a.dateCreated.getTime() - b.dateCreated.getTime());
                    }),
                    share());
            return this.getAllObservable;
        }
    }

    resetRefundCases() {
        this.refundCases = [];
    }

    getPaginated(sortBy: string, sortDir: string, filterBy: string, isAdmin: boolean): Observable<RefundCase[]> {
        if (this.refundCases && this.refundCases.length === 0) {
            return of(this.filterRefundCases(this.refundCases, sortBy, sortDir, filterBy));
        } else if (this.getPaginatedObservable) {
            return this.getPaginatedObservable;
        } else {
            this.getPaginatedObservable = this.getAll(isAdmin)
                .pipe(
                    map(r => {
                        this.getPaginatedObservable = null;
                        return this.filterRefundCases(r, sortBy, sortDir, filterBy);
                    }),
                    share());
            return this.getPaginatedObservable;
        }
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
        return this.http.get(`/api/merchant/refundcase/${id}`).pipe(map(RefundCasesService.mapDate));
    }

    accept(refundCase: RefundCase, isAccepted: boolean) {
        return this.http.post(`/api/admin/refundcase/${refundCase.id}/accept`, {isAccepted: isAccepted});
    }

    delete(id: number) {
        return this.http.delete(`/api/merchant/refundcase/${id}`);
    }
}
