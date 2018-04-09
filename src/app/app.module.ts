import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { AppComponent } from './components/app.component';
import { FormsModule } from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { AdminAuthGuard, AuthGuard } from './guards/index';
import { routing } from './app.routing';
import { JwtInterceptor } from './helpers/index';
import { AuthenticationService, UserService } from './services/index';
import { HomeComponent } from './components/home/index';
import { LoginComponent } from './components/login/index';
import { HttpModule } from '@angular/http';
import { SwaggerComponent } from './components/swagger/index';
import { SwaggerService } from './services/swagger.service';
import { AdminComponent } from './components/admin/admin.component';
import { DashboardComponent } from './components/home/dashboard/dashboard.component';

@NgModule({
    declarations: [
        AppComponent,
        HomeComponent,
        LoginComponent,
        SwaggerComponent,
        AdminComponent,
        DashboardComponent
    ],
    imports: [
        BrowserModule,
        FormsModule,
        HttpClientModule,
        HttpModule,
        routing
    ],
    providers: [
        AdminAuthGuard,
        AuthGuard,
        AuthenticationService,
        SwaggerService,
        UserService,
        {
            provide: HTTP_INTERCEPTORS,
            useClass: JwtInterceptor,
            multi: true
        },

    ],
    bootstrap: [AppComponent]
})
export class AppModule { }
