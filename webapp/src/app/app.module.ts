import {BrowserModule, Title} from '@angular/platform-browser';
import {BrowserAnimationsModule} from '@angular/platform-browser/animations';
import {LOCALE_ID, NgModule} from '@angular/core';
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
    AuthorizationService,
    MerchantInfoService
} from './services';
import {
    AppComponent,
    RefundCasesComponent,
    HomeComponent,
    SwaggerComponent,
    AdminRefundcasesComponent,
    RetailersComponent,
    ShoppersComponent,
    DashboardComponent,
    RetailerComponent,
    StatisticsComponent,
    LoginComponent,
    SettingsComponent,
    AccountComponent
} from './components';
import {SwaggerService} from './services';
import {DataViewModule} from 'primeng/dataview';
import {ConfirmDialogModule} from 'primeng/confirmdialog';
import {ConfirmationService} from 'primeng/api';
import {ChartModule} from 'primeng/chart';
import {GrowlModule} from 'primeng/primeng';
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
        ShoppersComponent,
        RetailerComponent,
        SettingsComponent,
        AccountComponent,
        StatisticsComponent
    ],
    imports: [
        BrowserModule,
        BrowserAnimationsModule,
        FormsModule,
        DataViewModule,
        GrowlModule,
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
        Title,
        AuthenticationService,
        AuthorizationService,
        ConfirmationService,
        SwaggerService,
        ColorsService,
        CustomerInfoService,
        MerchantInfoService,
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
