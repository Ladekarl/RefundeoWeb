import {Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import 'rxjs/add/operator/map';
import {CustomerInfo} from '../models/customerinfo';
import {Observable} from 'rxjs/Observable';
import 'rxjs/Rx';

@Injectable()
export class CustomerInfoService {

    constructor(private http: HttpClient) {
    }

    customerInfos: CustomerInfo[];

    getAll(): Observable<CustomerInfo[]> {
        if (!this.customerInfos || this.customerInfos.length > 0)
            return this.http.get<CustomerInfo[]>('/api/merchant/customerinfo').map(c => {
                this.customerInfos = c;
                return this.customerInfos;
            });
        else
            return Observable.of(this.customerInfos);
    }

    resetCustomerInfos() {
        this.customerInfos = [];
    }
}
