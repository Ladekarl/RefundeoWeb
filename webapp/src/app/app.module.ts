import {BrowserModule} from '@angular/platform-browser';
import {BrowserAnimationsModule} from '@angular/platform-browser/animations';
import {LOCALE_ID, NgModule} from '@angular/core';
import {AppComponent} from './components/app.component';
import {FormsModule} from '@angular/forms';
import {HttpClientModule, HTTP_INTERCEPTORS} from '@angular/common/http';
import {AdminAuthGuard, AuthGuard} from './guards';
import {routing} from './app.routing';
import {JwtInterceptor} from './helpers';
import {AuthenticationService, UserService, MenuService, RefundCasesService, ColorsService, CustomerInfoService} from './services';
import {HomeComponent, RefundCasesComponent} from './components/home';
import {LoginComponent} from './components/login';
import {HttpModule} from '@angular/http';
import {SwaggerComponent} from './components/swagger';
import {SwaggerService} from './services';
import {AdminComponent} from './components/admin';
import {DashboardComponent} from './components/home';
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
        AdminComponent,
        DashboardComponent,
        RefundCasesComponent
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
        HttpModule,
        routing,
        Ng4LoadingSpinnerModule.forRoot()
    ],
    providers: [
        {provide: LOCALE_ID, useValue: 'da'},
        AdminAuthGuard,
        AuthGuard,
        AuthenticationService,
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
