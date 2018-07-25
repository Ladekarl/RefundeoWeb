import {Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {Observable, forkJoin} from 'rxjs';
import {map, flatMap} from 'rxjs/operators';
import {JwtHelperService} from '@auth0/angular-jwt';
import {CurrentUser} from '../models';
import {RefundCasesService} from './refundcases.service';
import {AuthorizationService} from './authorization.service';
import {CustomerInfoService} from './customerinfo.service';
import {MerchantInfoService} from './merchantinfo.service';

@Injectable()
export class AuthenticationService {

    jwtHelperService: JwtHelperService;

    constructor(private http: HttpClient,
                private refundCasesService: RefundCasesService,
                private authorizationService: AuthorizationService,
                private customerInfoService: CustomerInfoService,
                private merchantInfoService: MerchantInfoService) {
        this.jwtHelperService = new JwtHelperService();
    }

    login(username: string, password: string): Observable<CurrentUser> {
        return this.http.post<any>('/Token', {username: username, password: password})
            .pipe(flatMap((response: CurrentUser) => {
                if (response && response.token) {
                    return this.authorizationService.setCurrentUser(response).pipe(map(() => {
                        return response;
                    }));
                }
            }));
    };

    logout(): Observable<any> {
        let tasks = [];
        tasks.push(this.authorizationService.removeCurrentUser());
        tasks.push(this.refundCasesService.resetRefundCases());
        tasks.push(this.customerInfoService.resetCustomerInfos());
        tasks.push(this.merchantInfoService.resetMerchantInfos());
        return forkJoin(tasks);
    }
}
