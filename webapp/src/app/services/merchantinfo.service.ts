import {Injectable} from '@angular/core';
import {HttpClient, HttpHeaders} from '@angular/common/http';
import {Observable, of} from 'rxjs';
import {map, share} from 'rxjs/operators';
import {AttachedAccount, ChangePassword, Merchant, MerchantInfo, Tag} from '../models';

@Injectable()
export class MerchantInfoService {

    merchantInfos: MerchantInfo[];
    merchantInfo: Map<string, MerchantInfo>;
    tags: Tag[];
    getMerchantObservable: Observable<MerchantInfo>;
    getAllObservable: Observable<MerchantInfo[]>;
    getAllTagsObservable: Observable<Tag[]>;

    constructor(
        private http: HttpClient) {
        this.merchantInfo = new Map<string, MerchantInfo>();
    }

    getMerchant(id: string): Observable<MerchantInfo> {
        const merchantInfo = this.merchantInfo.get(id);
        if (merchantInfo) {
            return of(merchantInfo);
        } else {
            return this.getMerchantNoCache(id);
        }
    }

    getMerchantNoCache(id: string): Observable<MerchantInfo> {
        if (this.getMerchantObservable) {
            return this.getMerchantObservable;
        } else {
            this.getMerchantObservable = this.http.get<MerchantInfo>('/api/merchant/account/' + id)
                .pipe(
                    map(m => {
                        this.getMerchantObservable = null;
                        this.merchantInfo.set(id, m);
                        return this.merchantInfo.get(id);
                    }),
                    share());
            return this.getMerchantObservable;
        }
    }

    updateMerchant(merchant: MerchantInfo): Observable<any> {
        return this.http.put('/api/merchant/account', merchant);
    }

    updateMerchantById(merchant: MerchantInfo): Observable<any> {
        const id = encodeURIComponent(merchant.id);
        return this.http.put(`/api/merchant/account/${id}`, merchant);
    }

    getAllTags(): Observable<Tag[]> {
        if (this.tags && this.tags.length > 0) {
            return of(this.tags);
        } else if (this.getAllTagsObservable) {
            return this.getAllTagsObservable;
        } else {
            const requestUrl = '/api/tag';
            this.getAllTagsObservable = this.http.get<Tag[]>(requestUrl)
                .pipe(
                    map(tags => {
                        this.getAllTagsObservable = null;
                        this.tags = tags;
                        return this.tags;
                    }),
                    share());
            return this.getAllTagsObservable;
        }
    }

    getAll(): Observable<MerchantInfo[]> {
        if (this.merchantInfos && this.merchantInfos.length > 0) {
            return of(this.merchantInfos);
        } else if (this.getAllObservable) {
            return this.getAllObservable;
        } else {
            const requestUrl = '/api/merchant/account';
            this.getAllObservable = this.http.get<MerchantInfo[]>(requestUrl)
                .pipe(
                    map(m => {
                        this.getAllObservable = null;
                        this.merchantInfos = m;
                        this.merchantInfos.sort((a, b) => {
                            return ('' + a.companyName).localeCompare(b.companyName);
                        });
                        return this.merchantInfos;
                    }),
                    share());
            return this.getAllObservable;

        }
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

    deleteMerchant(merchant: MerchantInfo): Observable<any> {
        return this.http.delete('/api/account/' + merchant.id).pipe(map(() => {
            this.merchantInfos.splice(this.merchantInfos.indexOf(merchant), 1);
        }));
    }
}
