import {Injectable} from '@angular/core';
import {swaggerUIBundle, swaggerUIStandalonePreset} from '../../polyfills';
import {environment} from '../../environments/environment';
import {AuthorizationService} from './authorization.service';

@Injectable()
export class SwaggerService {

    constructor(private authorizationService: AuthorizationService) {
    }

    getSwagger(domNode: any) {
        this.authorizationService.getCurrentUser().subscribe(currentUser => {
            if (currentUser) {
                swaggerUIBundle({
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
                        request.headers.Authorization = `Bearer ${currentUser.token}`;
                    },
                    layout: 'StandaloneLayout'
                });
            }
        });
    }
}
