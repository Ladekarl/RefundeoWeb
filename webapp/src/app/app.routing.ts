import { Routes, RouterModule } from '@angular/router';

import { HomeComponent, DashboardComponent, RefundCasesComponent } from './components/home/index';
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
                path: 'refundcases',
                component: RefundCasesComponent,
                canActivate: [AuthGuard]
            },
            {
                path: 'docs',
                component: SwaggerComponent,
                canActivate: [AuthGuard]
            },
            {
                path: 'admin',
                component: AdminComponent,
                canActivate: [AdminAuthGuard]
            },
        ]
    },
    { path: 'login', component: LoginComponent },
    { path: '**', redirectTo: '' }
];

export const routing = RouterModule.forRoot(appRoutes);
