import {BrowserModule} from '@angular/platform-browser';
import {BrowserAnimationsModule} from '@angular/platform-browser/animations';
import {LOCALE_ID, NgModule} from '@angular/core';
import {AppComponent} from './components/app.component';
import {FormsModule} from '@angular/forms';
import {HttpClientModule, HTTP_INTERCEPTORS} from '@angular/common/http';
import {AdminAuthGuard, AuthGuard} from './guards/index';
import {routing} from './app.routing';
import {JwtInterceptor} from './helpers/index';
import {AuthenticationService, UserService, MenuService, RefundCasesService, ColorsService} from './services/index';
import {HomeComponent, RefundCasesComponent} from './components/home/index';
import {LoginComponent} from './components/login/index';
import {HttpModule} from '@angular/http';
import {SwaggerComponent} from './components/swagger/index';
import {SwaggerService} from './services/swagger.service';
import {AdminComponent} from './components/admin/admin.component';
import {DashboardComponent} from './components/home/dashboard/dashboard.component';
import {DataViewModule} from 'primeng/dataview';
import {ConfirmDialogModule} from 'primeng/confirmdialog';
import {ConfirmationService} from 'primeng/api';
import {ChartModule} from 'primeng/chart';
import {DropdownModule} from 'primeng/dropdown';
import {registerLocaleData} from '@angular/common';
import localeDa from '@angular/common/locales/da';

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
    routing
  ],
  providers: [
    { provide: LOCALE_ID, useValue: 'da' },
    AdminAuthGuard,
    AuthGuard,
    AuthenticationService,
    ConfirmationService,
    SwaggerService,
    ColorsService,
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
export class AppModule {}
