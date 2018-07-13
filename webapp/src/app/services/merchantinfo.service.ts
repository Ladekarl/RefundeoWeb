import {Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {Observable} from 'rxjs/Observable';
import {MerchantInfo} from '../models/merchantinfo';

@Injectable()
export class MerchantInfoService {

    merchantInfos: MerchantInfo[];

    constructor(private http: HttpClient) {
    }

    getAll(): Observable<MerchantInfo[]> {
        if (!this.merchantInfos || this.merchantInfos.length === 0) {
            let requestUrl = '/api/merchant/account';
            return this.http.get<MerchantInfo[]>(requestUrl).map(m => {
                this.merchantInfos = m;
                this.merchantInfos.sort((a, b) => {
                    return ('' + a.companyName).localeCompare(b.companyName);
                });
                return this.merchantInfos;
            });
        }
        else
            return Observable.of(this.merchantInfos);
    }

    resetMerchantInfos() {
        this.merchantInfos = [];
    }
}
