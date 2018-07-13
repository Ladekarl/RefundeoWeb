import {BrowserModule} from '@angular/platform-browser';
import {BrowserAnimationsModule} from '@angular/platform-browser/animations';
import {LOCALE_ID, NgModule} from '@angular/core';
import {AppComponent} from './components/app.component';
import {FormsModule} from '@angular/forms';
import {HttpClientModule, HTTP_INTERCEPTORS} from '@angular/common/http';
import {AdminAuthGuard, AuthGuard} from './guards';
import {routing} from './app.routing';
import {JwtInterceptor} from './helpers';
import {
    AuthenticationService,
    UserService,
    MenuService,
    RefundCasesService,
    ColorsService,
    CustomerInfoService,
    AuthorizationService
} from './services';
import {RefundCasesComponent} from './components/home/merchant';
import {HomeComponent} from './components/home/home.component';
import {LoginComponent} from './components/login';
import {
    SwaggerComponent,
    AdminRefundcasesComponent,
    RetailersComponent,
    ShoppersComponent
} from './components/home/admin';
import {SwaggerService} from './services';
import {DashboardComponent} from './components/home/merchant';
import {DataViewModule} from 'primeng/dataview';
import {ConfirmDialogModule} from 'primeng/confirmdialog';
import {ConfirmationService} from 'primeng/api';
import {ChartModule} from 'primeng/chart';
import {DropdownModule} from 'primeng/dropdown';
import {registerLocaleData} from '@angular/common';
import localeDa from '@angular/common/locales/da';
import {Ng4LoadingSpinnerModule} from 'ng4-loading-spinner';

registerLocaleData(localeDa, 'da');

@NgModule({
    declarations: [
        AppComponent,
        HomeComponent,
        LoginComponent,
        SwaggerComponent,
        DashboardComponent,
        RefundCasesComponent,
        AdminRefundcasesComponent,
        RetailersComponent,
        ShoppersComponent
    ],
    imports: [
        BrowserModule,
        BrowserAnimationsModule,
        FormsModule,
        DataViewModule,
        ConfirmDialogModule,
        DropdownModule,
        HttpClientModule,
        ChartModule,
        routing,
        Ng4LoadingSpinnerModule.forRoot()
    ],
    providers: [
        {provide: LOCALE_ID, useValue: 'da'},
        AdminAuthGuard,
        AuthGuard,
        AuthenticationService,
        AuthorizationService,
        ConfirmationService,
        SwaggerService,
        ColorsService,
        CustomerInfoService,
        RefundCasesService,
        MenuService,
        UserService, {
            provide: HTTP_INTERCEPTORS,
            useClass: JwtInterceptor,
            multi: true
        }
    ],
    bootstrap: [AppComponent]
})
export class AppModule {
}
