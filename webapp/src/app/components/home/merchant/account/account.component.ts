import {Component, OnInit} from '@angular/core';
import {AuthorizationService, MerchantInfoService} from '../../../../services';
import {AttachedAccount, ChangePassword, CurrentUser, MerchantInfo} from '../../../../models';
import {Ng4LoadingSpinnerService} from 'ng4-loading-spinner';
import {ConfirmationService} from 'primeng/api';

@Component({
    selector: 'app-account',
    templateUrl: './account.component.html',
    styleUrls: ['./account.component.scss']
})
export class AccountComponent implements OnInit {

    attachedAccountModel: AttachedAccount;
    merchantInfo: MerchantInfo;
    changeAccountModel: ChangePassword;
    changeAttachedAccountModels: ChangePassword[];
    account: CurrentUser;

    constructor(private merchantInfoService: MerchantInfoService,
                private authorizationService: AuthorizationService,
                private confirmationService: ConfirmationService,
                private spinnerService: Ng4LoadingSpinnerService) {
        this.attachedAccountModel = new AttachedAccount();
        this.changeAccountModel = new ChangePassword();
        this.merchantInfo = new MerchantInfo();
        this.changeAttachedAccountModels = [];
    }

    ngOnInit() {
        this.getMerchantInfo();
        this.account = this.authorizationService.getCurrentUser();
    }

    getMerchantInfo() {
        this.spinnerService.show();
        this.merchantInfoService.getMerchant(this.authorizationService.getCurrentUser().id).subscribe(merchantInfo => {
            this.setMerchantInfo(merchantInfo);
            this.spinnerService.hide();
        }, () => {
            this.spinnerService.hide();
        });
    }

    getMerchantInfoNoCache() {
        this.spinnerService.show();
        this.merchantInfoService.getMerchantNoCache(this.authorizationService.getCurrentUser().id).subscribe(merchantInfo => {
            this.setMerchantInfo(merchantInfo);
            this.spinnerService.hide();
        }, () => {
            this.spinnerService.hide();
        });
    }

    setMerchantInfo(merchantInfo: MerchantInfo) {
        this.merchantInfo = merchantInfo;
        for (let i = 0; i < this.merchantInfo.attachedAccounts.length; i++) {
            this.changeAttachedAccountModels[i] = new ChangePassword();
        }
    }

    onChangeAccount() {
        this.spinnerService.show();
        this.merchantInfoService.changePassword(this.changeAccountModel).subscribe(() => {
            this.spinnerService.hide();
            alert('Successfully changed password');
        }, (e) => {
            this.spinnerService.hide();
            let errorString = 'Could not change password\n';
            if (e.error && e.error.errors) {
                e.error.errors.forEach(e => {
                    errorString = errorString + e + '\n';
                });
            }
            alert(errorString);
        });
    }

    onSubmitAttachedAccount() {
        this.spinnerService.show();
        this.merchantInfoService.createAttachedAccount(this.attachedAccountModel).subscribe(() => {
            this.getMerchantInfoNoCache();
        }, (e) => {
            this.spinnerService.hide();
            let errorString = 'Could not create account\n';
            if (e.error && e.error.errors) {
                e.error.errors.forEach(e => {
                    errorString = errorString + e.description + '\n';
                });
            }
            alert(errorString);
        });
    }

    onDeleteAttachedAccount(attachedAccount: AttachedAccount) {
        this.confirmationService.confirm({
            message: `Are you sure you want to delete ${attachedAccount.username}?`,
            accept: () => {
                this.spinnerService.show();
                this.merchantInfoService.deleteAttachedAccount(attachedAccount.id).subscribe(() => {
                    this.getMerchantInfoNoCache();
                }, (e) => {
                    this.spinnerService.hide();
                    let errorString = 'Could not delete account\n';
                    if (e.error && e.errors) {
                        e.error.errors.forEach(e => {
                            errorString = errorString + e.description + '\n';
                        });
                    }
                    alert(errorString);
                });
            }
        });
    }

    onChangePasswordAttachedAccount(attachedAccount: AttachedAccount, index: number) {
        this.spinnerService.show();
        this.merchantInfoService.changePasswordAttachedAccount(attachedAccount.id, this.changeAttachedAccountModels[index]).subscribe(() => {
            this.spinnerService.hide();
            alert('Successfully changed password for ' + attachedAccount.username);
        }, (e) => {
            this.spinnerService.hide();
            let errorString = 'Could not change password for ' + attachedAccount.username + '\n';
            if (e.error && e.error.errors) {
                e.error.errors.forEach(e => {
                    errorString = errorString + e + '\n';
                });
            }
            alert(errorString);
        });
    }

}
