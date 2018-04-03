import { Injectable } from '@angular/core';
import { swaggerUIBundle, swaggerUIStandalonePreset } from '../../polyfills';
import { User } from '../models';

import { environment } from '../../environments/environment';

@Injectable()
export class SwaggerService {
  currentUser;

  constructor() {
    this.currentUser = JSON.parse(localStorage.getItem('currentUser'));
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
