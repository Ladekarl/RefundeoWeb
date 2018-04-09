import { Routes, RouterModule } from '@angular/router';

import { HomeComponent, DashboardComponent} from './components/home/index';
import { LoginComponent } from './components/login/index';
import { AuthGuard, AdminAuthGuard } from './guards/index';
import { SwaggerComponent } from './components/swagger/index';
import { AdminComponent } from './components/admin/index';

const appRoutes: Routes = [
    {
        path: '',
        component: HomeComponent,
        canActivate: [AuthGuard],
        children: [
            {
                path: '',
                component: DashboardComponent,
                canActivate: [AuthGuard]
            },
            {
                path: 'docs',
                component: SwaggerComponent,
                canActivate: [AuthGuard]
            }
        ]
    },
    { path: 'login', component: LoginComponent },
    { path: 'admin', component: AdminComponent, canActivate: [AdminAuthGuard] },
    { path: '**', redirectTo: '' }
];

export const routing = RouterModule.forRoot(appRoutes);
