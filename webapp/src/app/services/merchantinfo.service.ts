import {Injectable} from '@angular/core';
import {HttpClient, HttpHeaders} from '@angular/common/http';
import {Observable} from 'rxjs/Observable';
import {Merchant, MerchantInfo, Tag} from '../models';
import {AuthorizationService} from './authorization.service';

@Injectable()
export class MerchantInfoService {

    merchantInfos: MerchantInfo[];

    constructor(
        private http: HttpClient,
        private authorizationService: AuthorizationService) {
    }

    getAllTags(): Observable<Tag[]> {
        return this.http.get<Tag[]>('/api/tag');
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

    create(merchant: MerchantInfo): Observable<Merchant> {
        const httpOptions = {
            headers: new HttpHeaders({
                'Content-Type': 'application/json',
                'Authorization': 'Bearer ' + this.authorizationService.getToken()
            })
        };
        return this.http.post<Merchant>('api/merchant/account', merchant, httpOptions).map((success) => {
            if (this.merchantInfos && this.merchantInfos.length > 0) {
                this.merchantInfos.push(merchant);
            }
            return success;
        });
    }

    resetMerchantInfos() {
        this.merchantInfos = [];
    }
}
