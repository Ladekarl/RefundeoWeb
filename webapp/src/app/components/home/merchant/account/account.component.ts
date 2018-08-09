import {Component, OnInit, AfterViewInit, ViewChild} from '@angular/core';
import {AuthorizationService, MerchantInfoService} from '../../../../services';
import {AttachedAccount, ChangePassword, CurrentUser, MerchantInfo, FeePoint} from '../../../../models';
import {Ng4LoadingSpinnerService} from 'ng4-loading-spinner';
import {ConfirmationService} from 'primeng/api';
import {UIChart} from 'primeng/chart';
import {Options} from 'ng5-slider';

interface SliderModel {
    options: Options;
    label: string;
  }

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
    feePoints: FeePoint[];
    ratesOptions: any;
    ratesData: any;
    element: any;
    scale: any;
    sliderWidth: number;
    feeSliders: SliderModel[];

    constructor(private merchantInfoService: MerchantInfoService,
                private authorizationService: AuthorizationService,
                private confirmationService: ConfirmationService,
                private spinnerService: Ng4LoadingSpinnerService) {
        this.attachedAccountModel = new AttachedAccount();
        this.changeAccountModel = new ChangePassword();
        this.merchantInfo = new MerchantInfo();
        this.changeAttachedAccountModels = [];
        this.account = new CurrentUser();
        this.feePoints = [];
        this.feeSliders = [];
    }

    ngOnInit() {
        this.getMerchantInfo();
    }

    getMerchantInfo() {
        this.spinnerService.show();
        this.authorizationService.getCurrentUser().subscribe(currentUser => {
            this.account = currentUser;
            this.merchantInfoService.getMerchant(currentUser.id).subscribe(merchantInfo => {
                this.setMerchantInfo(merchantInfo);
                this.spinnerService.hide();
            }, () => {
                this.spinnerService.hide();
            });
        });
    }

    getMerchantInfoNoCache() {
        this.spinnerService.show();
        this.authorizationService.getCurrentUser().subscribe(currentUser => {
            this.merchantInfoService.getMerchantNoCache(currentUser.id).subscribe(merchantInfo => {
                this.setMerchantInfo(merchantInfo);
                this.spinnerService.hide();
            }, () => {
                this.spinnerService.hide();
            });
        });
    }

    setRatesData() {
        this.sliderWidth = Math.floor(12 / this.feePoints.length);
        for (let i = 0; i < this.feePoints.length; i++) {
            const endValue = this.feePoints[i].end;
            const label = this.merchantInfo.currency + ' ' +
                this.feePoints[i].start +
                (!endValue ? '+' : ' - ' + (this.merchantInfo.currency + ' ' + endValue));
            const slider: SliderModel = {
                label,
                options: {
                  floor: 0,
                  ceil: 50,
                  vertical: true,
                  showSelectionBar: true,
                  translate: (value: number): string => {
                    return value + ' %';
                  }
                }
            };
            this.feeSliders.push(slider);
        }
    }


    setMerchantInfo(merchantInfo: MerchantInfo) {
        this.merchantInfo = merchantInfo;
        this.feePoints = merchantInfo.feePoints;
        if (this.merchantInfo.attachedAccounts) {
            for (let i = 0; i < this.merchantInfo.attachedAccounts.length; i++) {
                this.changeAttachedAccountModels[i] = new ChangePassword();
            }
        }

        this.setRatesData();
    }

    onChangeRates() {
        this.spinnerService.show();
        const oldMerchantInfo = Object.assign({}, this.merchantInfo);
        this.merchantInfo.feePoints = this.feePoints;
        this.merchantInfoService.updateMerchant(this.merchantInfo).subscribe(() => {
            alert('Successfully changed rates');
            this.spinnerService.hide();
        }, () => {
            alert('Successfully changed rates');
            this.spinnerService.hide();
            this.setMerchantInfo(oldMerchantInfo);
        });
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
                e.error.errors.forEach(err => {
                    errorString = errorString + err + '\n';
                });
            }
            alert(errorString);
        });
    }

    onChangeMerchantForm() {
        this.spinnerService.show();
        this.merchantInfoService.updateMerchant(this.merchantInfo).subscribe(() => {
            this.spinnerService.hide();
            alert('Successfully changed email');
        }, (e) => {
            this.spinnerService.hide();
            let errorString = 'Could not change email\n';
            if (e.error && e.error.errors) {
                e.error.errors.forEach(err => {
                    errorString = errorString + err + '\n';
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
                e.error.errors.forEach(err => {
                    errorString = errorString + err.description + '\n';
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
                        e.error.errors.forEach(err => {
                            errorString = errorString + err.description + '\n';
                        });
                    }
                    alert(errorString);
                });
            }
        });
    }

    onChangePasswordAttachedAccount(attachedAccount: AttachedAccount, index: number) {
        this.spinnerService.show();
        this.merchantInfoService.changePasswordAttachedAccount(
            attachedAccount.id,
            this.changeAttachedAccountModels[index])
                .subscribe(() => {
                    this.spinnerService.hide();
                    alert('Successfully changed password for ' + attachedAccount.username);
                }, (e) => {
                    this.spinnerService.hide();
                    let errorString = 'Could not change password for ' + attachedAccount.username + '\n';
                    if (e.error && e.error.errors) {
                        e.error.errors.forEach(err => {
                            errorString = errorString + err + '\n';
                        });
                    }
                    alert(errorString);
                });
    }

}
