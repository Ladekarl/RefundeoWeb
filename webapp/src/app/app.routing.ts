import {Routes, RouterModule} from '@angular/router';

import {
    DashboardComponent,
    RefundCasesComponent
} from './components/home/merchant';
import {LoginComponent} from './components/login';
import {AuthGuard, AdminAuthGuard} from './guards';
import {
    AdminRefundcasesComponent,
    SwaggerComponent,
    RetailersComponent,
    ShoppersComponent
} from './components/home/admin';
import {HomeComponent} from './components/home/home.component';

const appRoutes: Routes = [
    {
        path: '',
        component: HomeComponent,
        canActivate: [AdminAuthGuard],
        children: [
            {
                path: '',
                component: AdminRefundcasesComponent,
                canActivate: [AdminAuthGuard]
            },
            {
                path: 'refunds',
                component: AdminRefundcasesComponent,
                canActivate: [AdminAuthGuard]
            },
            {
                path: 'retailers',
                component: RetailersComponent,
                canActivate: [AdminAuthGuard]
            },
            {
                path: 'shoppers',
                component: ShoppersComponent,
                canActivate: [AdminAuthGuard]
            },
            {
                path: 'docs',
                component: SwaggerComponent,
                canActivate: [AdminAuthGuard]
            }
        ]
    },
    {
        path: '',
        component: HomeComponent,
        canActivate: [AuthGuard],
        children: [
            {
                path: 'refunds',
                component: RefundCasesComponent,
                canActivate: [AuthGuard]
            }
        ]
    },
    {path: 'login', component: LoginComponent},
    {path: '**', redirectTo: ''}
];

export const routing = RouterModule.forRoot(appRoutes);
