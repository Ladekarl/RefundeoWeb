import {Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import 'rxjs/add/operator/map';
import {CustomerInfo} from '../models/customerinfo';

@Injectable()
export class CustomerInfoService {

    constructor(private http: HttpClient) {
    }

    getAll() {
        return this.http.get<CustomerInfo[]>('/api/merchant/customerinfo');
    }
}
