import {Routes, RouterModule} from '@angular/router';

import {HomeComponent, DashboardComponent, RefundCasesComponent} from './components/home';
import {LoginComponent} from './components/login';
import {AuthGuard, AdminAuthGuard} from './guards';
import {SwaggerComponent} from './components/swagger';
import {AdminComponent} from './components/admin';

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
    {path: 'login', component: LoginComponent},
    {path: '**', redirectTo: ''}
];

export const routing = RouterModule.forRoot(appRoutes);
