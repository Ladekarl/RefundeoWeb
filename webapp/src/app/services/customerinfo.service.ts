import {Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {map, share} from 'rxjs/operators';
import {CustomerInfo} from '../models';
import {Observable, of} from 'rxjs';

@Injectable()
export class CustomerInfoService {

    customerInfos: CustomerInfo[];
    getAllObservable: Observable<CustomerInfo[]>;

    constructor(private http: HttpClient) {
    }

    getAll(isAdmin: boolean): Observable<CustomerInfo[]> {
        if (this.customerInfos && this.customerInfos.length > 0) {
            return of(this.customerInfos);
        } else if (this.getAllObservable) {
            return this.getAllObservable;
        } else {
            let requestUrl = isAdmin ? '/api/user/account' : '/api/merchant/customerinfo';
            this.getAllObservable = this.http.get<CustomerInfo[]>(requestUrl)
                .pipe(
                    map(c => {
                        this.getAllObservable = null;
                        this.customerInfos = c;
                        this.customerInfos.sort((a, b) => {
                            return ('' + a.username).localeCompare(b.username);
                        });
                        return this.customerInfos;
                    }),
                    share());
            return this.getAllObservable;
        }
    }

    deleteCustomer(customer: CustomerInfo): Observable<any> {
        return this.http.delete('/api/account' + customer.id).pipe(map(() => {
            this.customerInfos.splice(this.customerInfos.indexOf(customer), 1);
        }));
    }

    resetCustomerInfos() {
        this.customerInfos = [];
    }
}
