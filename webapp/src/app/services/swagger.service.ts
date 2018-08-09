import {Injectable} from '@angular/core';
import {swaggerUIBundle, swaggerUIStandalonePreset} from '../../polyfills';
import {AuthorizationService} from './authorization.service';

@Injectable()
export class SwaggerService {

    constructor(private authorizationService: AuthorizationService) {
    }

    getSwagger(domNode: any) {
        this.authorizationService.getToken().subscribe(token => {
            if (token) {
                const url = window.location.href;
                const host = url.split("/")[0];
                swaggerUIBundle({
                    urls: [{
                        url: `${host}/swagger/v1/swagger.json`, name: 'Refundeo API v1'
                    }],
                    domNode: domNode,
                    deepLinking: true,
                    presets: [
                        swaggerUIBundle.presets.apis,
                        swaggerUIStandalonePreset
                    ],
                    requestInterceptor: (request) => {
                        request.headers.Authorization = `Bearer ${token}`;
                    },
                    layout: 'StandaloneLayout'
                });
            }
        });
    }
}
