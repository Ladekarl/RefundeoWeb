import {Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {flatMap, map} from 'rxjs/operators';
import {CustomerInfo} from '../models';
import {Observable, of} from 'rxjs';
import {AuthorizationService} from './authorization.service';

@Injectable()
export class CustomerInfoService {

    constructor(private http: HttpClient, private authorizationService: AuthorizationService) {
    }

    customerInfos: CustomerInfo[];

    getAll(): Observable<CustomerInfo[]> {
        if (!this.customerInfos || this.customerInfos.length === 0) {
            return this.authorizationService.isAdmin().pipe(flatMap(isAdmin => {
                let requestUrl = isAdmin ? '/api/user/account' : '/api/merchant/customerinfo';
                return this.http.get<CustomerInfo[]>(requestUrl).pipe(map(c => {
                    this.customerInfos = c;
                    this.customerInfos.sort((a, b) => {
                        return ('' + a.username).localeCompare(b.username);
                    });
                    return this.customerInfos;
                }));
            }));
        }
        else
            return of(this.customerInfos);
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
