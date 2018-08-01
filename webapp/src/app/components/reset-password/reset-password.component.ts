import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthenticationService } from '../../services';
import { Ng4LoadingSpinnerService } from 'ng4-loading-spinner';

@Component({
  selector: 'app-reset-password',
  templateUrl: './reset-password.component.html',
  styleUrls: ['./reset-password.component.scss']
})
export class ResetPasswordComponent implements OnInit {
    bannerImageUrl = require('../../../assets/images/refundeo_banner_small_border.png');
    loading = false;
    errorText: string;
    passwordChangedSuccess = false;

    token: string;
    userId: string;
    password: string;
    passwordConfirmation: string;

    constructor(
        private router: Router,
        private activatedRoute: ActivatedRoute,
        private authenticationService: AuthenticationService,
        private spinnerService: Ng4LoadingSpinnerService) {
    }

    ngOnInit() {
        this.activatedRoute.queryParams.subscribe(params => {
            this.token = params['token'];
            this.userId = params['id'];
        });
    }

    navigateLogin() {
        this.router.navigate(['/login']);
    }

    onSubmit() {
        this.loading = true;
        this.spinnerService.show();
        this.authenticationService.resetPassword(this.userId, this.token, this.password, this.passwordConfirmation).subscribe(() => {
            this.spinnerService.hide();
            this.loading = false;
            this.passwordChangedSuccess = true;
        }, (e) => {
            this.loading = false;
            this.spinnerService.hide();
            let errorString = '';
            if (e.error && e.error.errors) {
                e.error.errors.forEach(eText => {
                    if (eText.description) {
                        errorString = errorString + eText.description + '\n';
                    } else {
                        errorString = errorString + eText + '\n';
                    }
                });
            }
            this.errorText = errorString;
        });
    }
}
