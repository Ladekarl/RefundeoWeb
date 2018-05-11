import {Injectable} from '@angular/core';
import {RefundCase} from '../models';
import {HttpClient} from '@angular/common/http';
import 'rxjs/add/operator/map';
import {Observable} from 'rxjs/Observable';
import {CustomerInfo} from '../models/customerinfo';

@Injectable()
export class CustomerInfoService {

    constructor(private http: HttpClient) {
    }

    getAll() {
        return this.http.get<CustomerInfo[]>('/api/merchant/customerinfo');
    }
}
