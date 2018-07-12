import {Routes, RouterModule} from '@angular/router';

import {HomeComponent, DashboardComponent, RefundCasesComponent} from './components/home';
import {LoginComponent} from './components/login';
import {AuthGuard, AdminAuthGuard} from './guards';
import {SwaggerComponent} from './components/home/swagger';
import {AdminComponent} from './components/home/admin';

const appRoutes: Routes = [
    {
        path: '',
        component: HomeComponent,
        canActivate: [AuthGuard],
        children: [
            {
                path: '',
                component: RefundCasesComponent,
                canActivate: [AuthGuard]
            },
            {
                path: 'refunds',
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
    {path: 'login', component: LoginComponent},
    {path: '**', redirectTo: ''}
];

export const routing = RouterModule.forRoot(appRoutes);
