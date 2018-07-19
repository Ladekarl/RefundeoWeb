import {Routes, RouterModule} from '@angular/router';
import {AuthGuard, AdminAuthGuard} from './guards';
import {
    LoginComponent,
    HomeComponent,
    DashboardComponent,
    RefundCasesComponent,
    AdminRefundcasesComponent,
    SwaggerComponent,
    RetailersComponent,
    ShoppersComponent,
    RetailerComponent
} from './components';

const appRoutes: Routes = [
    {
        path: 'admin',
        component: HomeComponent,
        canActivate: [AdminAuthGuard],
        children: [
            {
                path: '',
                component: AdminRefundcasesComponent,
                canActivate: [AdminAuthGuard]
            },
            {
                path: 'retailers',
                component: RetailersComponent,
                canActivate: [AdminAuthGuard]
            },
            {
                path: 'addretailer',
                component: RetailerComponent,
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
                path: '',
                component: RefundCasesComponent,
                canActivate: [AuthGuard]
            },
            {
                path: 'account',
                component: RetailerComponent,
                canActivate: [AuthGuard]
            }
        ]
    },
    {path: 'login', component: LoginComponent},
    {path: '**', redirectTo: ''}
];

export const routing = RouterModule.forRoot(appRoutes);
