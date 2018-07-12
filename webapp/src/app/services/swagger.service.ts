import {Injectable} from '@angular/core';
import {swaggerUIBundle, swaggerUIStandalonePreset} from '../../polyfills';
import {environment} from '../../environments/environment';
import {CurrentUser} from '../models';
import {AuthorizationService} from './authorization.service';

@Injectable()
export class SwaggerService {
    currentUser: CurrentUser;

    constructor(private authorizationService: AuthorizationService) {
        this.currentUser = this.authorizationService.getCurrentUser();
    }

    getSwagger(domNode: any) {
        const ui = swaggerUIBundle({
            urls: [{
                url: `${environment.apiBaseUrl}/swagger/v1/swagger.json`, name: 'Refundeo API v1'
            }],
            domNode: domNode,
            deepLinking: true,
            presets: [
                swaggerUIBundle.presets.apis,
                swaggerUIStandalonePreset
            ],
            requestInterceptor: (request) => {
                request.headers.Authorization = `Bearer ${this.currentUser.token}`;
            },
            layout: 'StandaloneLayout'
        });
    }
}
