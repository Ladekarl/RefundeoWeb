import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { AppComponent } from './app.component';
import { FormsModule } from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { AuthGuard } from './guards/index';
import { routing } from './app.routing';
import { JwtInterceptor } from './helpers/index';
import { AuthenticationService, UserService } from './services/index';
import { HomeComponent } from './home/index';
import { LoginComponent } from './login/index';
import { HttpModule } from '@angular/http';
import { SwaggerComponent } from './swagger/index';
import { SwaggerService } from './services/swagger.service';

@NgModule({
    declarations: [
        AppComponent,
        HomeComponent,
        LoginComponent,
        SwaggerComponent
    ],
    imports: [
        BrowserModule,
        FormsModule,
        HttpClientModule,
        HttpModule,
        routing
    ],
    providers: [
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
