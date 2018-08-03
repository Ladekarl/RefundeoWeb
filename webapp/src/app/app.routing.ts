import {Routes, RouterModule} from '@angular/router';
import {AuthGuard, AdminAuthGuard} from './guards';
import {
    LoginComponent,
    HomeComponent,
    DashboardComponent,
    SettingsComponent,
    RefundCasesComponent,
    AdminRefundcasesComponent,
    SwaggerComponent,
    RetailersComponent,
    ShoppersComponent,
    RetailerComponent,
    AccountComponent,
    ResetPasswordComponent,
    StatisticsComponent
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
                path: 'editretailer',
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
                component: DashboardComponent,
                canActivate: [AuthGuard]
            },
            {
                path: 'statistics',
                component: StatisticsComponent,
                canActivate: [AuthGuard]
            },
            {
                path: 'refunds',
                component: RefundCasesComponent,
                canActivate: [AuthGuard]
            },
            {
                path: 'account',
                component: AccountComponent,
                canActivate: [AuthGuard],
            },
            {
                path: 'retailer',
                component: RetailerComponent,
                canActivate: [AuthGuard]
            }
            // {
            //     path: 'settings',
            //     component: SettingsComponent,
            //     canActivate: [AuthGuard]
            // }
        ]
    },
    {path: 'login', component: LoginComponent},
    {path: 'resetpassword', component: ResetPasswordComponent},
    {path: '**', redirectTo: ''}
];

export const routing = RouterModule.forRoot(appRoutes);
