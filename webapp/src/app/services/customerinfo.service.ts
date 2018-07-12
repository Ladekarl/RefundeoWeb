import {Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import 'rxjs/add/operator/map';
import {CustomerInfo} from '../models/customerinfo';
import {Observable} from 'rxjs/Observable';
import 'rxjs/Rx';
import {AuthorizationService} from './authorization.service';

@Injectable()
export class CustomerInfoService {

    constructor(private http: HttpClient, private authorizationService: AuthorizationService) {
    }

    customerInfos: CustomerInfo[];

    getAll(): Observable<CustomerInfo[]> {
        const isAdmin = this.authorizationService.isAdmin();
        if (!this.customerInfos || this.customerInfos.length > 0) {
            let requestUrl = isAdmin ? '/api/admin/customerinfo' : '/api/merchant/customerinfo';
            return this.http.get<CustomerInfo[]>('/api/merchant/customerinfo').map(c => {
                this.customerInfos = c;
                return this.customerInfos;
            });
        }
        else
            return Observable.of(this.customerInfos);
    }

    resetCustomerInfos() {
        this.customerInfos = [];
    }
}
