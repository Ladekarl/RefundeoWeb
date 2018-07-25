import {Injectable} from '@angular/core';
import {HttpClient, HttpHeaders} from '@angular/common/http';
import {Observable, of} from 'rxjs';
import {map} from 'rxjs/operators';
import {AttachedAccount, ChangePassword, Merchant, MerchantInfo, Tag} from '../models';
import {AuthorizationService} from './authorization.service';

@Injectable()
export class MerchantInfoService {

    merchantInfos: MerchantInfo[];
    merchantInfo: Map<string, MerchantInfo>;

    constructor(
        private http: HttpClient,
        private authorizationService: AuthorizationService) {
        this.merchantInfo = new Map<string, MerchantInfo>();
    }

    getMerchant(id: string): Observable<MerchantInfo> {
        let merchantInfo = this.merchantInfo.get(id);
        if (!merchantInfo) {
            return this.getMerchantNoCache(id);
        }
        return of(merchantInfo);
    }

    getMerchantNoCache(id: string): Observable<MerchantInfo> {
        return this.http.get<MerchantInfo>('/api/merchant/account/' + id).pipe(map(m => {
            this.merchantInfo.set(id, m);
            return this.merchantInfo.get(id);
        }));
    }

    updateMerchant(merchant: MerchantInfo): Observable<any> {
        const httpOptions = {
            headers: new HttpHeaders({
                'Content-Type': 'application/json'
            })
        };
        return this.http.put('/api/merchant/account', merchant, httpOptions);
    }

    getAllTags(): Observable<Tag[]> {
        return this.http.get<Tag[]>('/api/tag');
    }

    getAll(): Observable<MerchantInfo[]> {
        if (!this.merchantInfos || this.merchantInfos.length === 0) {
            let requestUrl = '/api/merchant/account';
            return this.http.get<MerchantInfo[]>(requestUrl).pipe(map(m => {
                this.merchantInfos = m;
                this.merchantInfos.sort((a, b) => {
                    return ('' + a.companyName).localeCompare(b.companyName);
                });
                return this.merchantInfos;
            }));
        }
        else
            return of(this.merchantInfos);
    }

    create(merchant: MerchantInfo): Observable<Merchant> {
        const httpOptions = {
            headers: new HttpHeaders({
                'Content-Type': 'application/json'
            })
        };
        return this.http.post<Merchant>('/api/merchant/account', merchant, httpOptions).pipe(map((success) => {
            if (this.merchantInfos && this.merchantInfos.length > 0) {
                this.merchantInfos.push(merchant);
            }
            return success;
        }));
    }

    createAttachedAccount(attachedAccount: AttachedAccount): Observable<any> {
        const httpOptions = {
            headers: new HttpHeaders({
                'Content-Type': 'application/json'
            })
        };
        return this.http.post('/api/merchant/attachedaccount', attachedAccount, httpOptions);
    }

    deleteAttachedAccount(id: string): Observable<any> {
        return this.http.delete('/api/merchant/attachedaccount/' + id);
    }

    changePassword(changePassword: ChangePassword): Observable<any> {
        const httpOptions = {
            headers: new HttpHeaders({
                'Content-Type': 'application/json'
            })
        };
        return this.http.put('/api/account/ChangePassword', changePassword, httpOptions);
    }

    resetMerchantInfos() {
        this.merchantInfos = [];
        this.merchantInfo.clear();
    }

    changePasswordAttachedAccount(id: string, changePassword: ChangePassword): Observable<any> {
        const httpOptions = {
            headers: new HttpHeaders({
                'Content-Type': 'application/json'
            })
        };
        return this.http.put('/api/merchant/attachedaccount/ChangePassword/' + id, changePassword, httpOptions);
    }
}
