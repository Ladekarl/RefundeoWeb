import { Routes, RouterModule } from '@angular/router';

import { HomeComponent } from './home/index';
import { LoginComponent } from './login/index';
import { AuthGuard } from './guards/index';
import { SwaggerComponent } from './swagger/index';

const appRoutes: Routes = [
    { path: '', component: HomeComponent, canActivate: [AuthGuard] },
    { path: 'login', component: LoginComponent },
    { path: 'docs', component: SwaggerComponent, canActivate: [AuthGuard]},
    { path: '**', redirectTo: '' }
];

export const routing = RouterModule.forRoot(appRoutes);
