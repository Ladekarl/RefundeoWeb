import {Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {Observable} from 'rxjs/Observable';
import 'rxjs/add/operator/map';
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
            .map((response: CurrentUser) => {
                if (response && response.token) {
                    this.authorizationService.setCurrentUser(response);
                }
                return response;
            });
    }

    logout() {
        this.authorizationService.removeCurrentUser();
        this.refundCasesService.resetRefundCases();
        this.customerInfoService.resetCustomerInfos();
        this.merchantInfoService.resetMerchantInfos();
    }
}
