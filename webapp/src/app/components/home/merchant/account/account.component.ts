import {Component, OnInit, AfterViewInit, ViewChild} from '@angular/core';
import {AuthorizationService, MerchantInfoService} from '../../../../services';
import {AttachedAccount, ChangePassword, CurrentUser, MerchantInfo, FeePoint} from '../../../../models';
import {Ng4LoadingSpinnerService} from 'ng4-loading-spinner';
import {ConfirmationService} from 'primeng/api';
import * as d3 from 'd3';
import {UIChart} from 'primeng/chart';

@Component({
    selector: 'app-account',
    templateUrl: './account.component.html',
    styleUrls: ['./account.component.scss']
})
export class AccountComponent implements OnInit, AfterViewInit {

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
    @ViewChild('chartInstance') chartInstance: UIChart;

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
    }

    ngOnInit() {
        this.getMerchantInfo();
    }

    ngAfterViewInit() {
        d3.select(this.chartInstance.chart.chart.canvas).call(
            d3.drag().container(this.chartInstance.chart.chart.canvas)
              .on('start', () => this.getElement())
              .on('drag', () => this.updateData())
              .on('end', () => this.callback())
          );
    }

    getElement () {
        const e = d3.event.sourceEvent;
        if (!this.chartInstance) {
            return;
        }
        this.element = this.chartInstance.chart.getElementAtEvent(e)[0];
        if (!this.element) {
            return;
        }
        this.scale = this.element['_yScale'].id;
    }

    updateData () {
        if (!this.element || !this.chartInstance) {
            return;
        }
        const e = d3.event.sourceEvent;
        const index = this.element['_index'];
        let value = this.chartInstance.chart.scales[this.scale].getValueForPixel(e.clientY);
        if (value < 0 || value > 90) {
            return;
        }
        value = Math.floor(value);
        this.feePoints[index].merchantFee = value;
        this.setRatesData(this.feePoints);
    }

    callback () {
        if (!this.element || !this.chartInstance) {
            return;
        }
        const datasetIndex = this.element['_datasetIndex'];
        const index = this.element['_index'];
        const value = this.chartInstance.data.datasets[datasetIndex].data[index];
        this.feePoints[index].merchantFee = value;
        this.setRatesData(this.feePoints);
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

    setRatesData(feePoints: FeePoint[]) {
        const ratesAmountMap = new Map<string, number>();
        for (let i = 0; i < feePoints.length; i++) {
            const endValue = feePoints[i].end;
            const label = this.merchantInfo.currency + ' ' +
                feePoints[i].start +
                (!endValue ? '+' : ' - ' + (this.merchantInfo.currency + ' ' + endValue));
            ratesAmountMap.set(label, feePoints[i].merchantFee);
        }

        this.ratesData = {
            labels: Array.from(ratesAmountMap.keys()),
            datasets: [
                {
                    label: 'Rate',
                    data: Array.from(ratesAmountMap.values()),
                    fill: false,
                    lineTension: 0,
                    borderColor: 'rgba(48,56,128,1)',
                    backgroundColor: 'rgba(48,56,128,0.8)',
                    borderWidth: 1
                },
            ]
        };
    }

    onDragFeePoint(event) {
        console.log(event);
    }

    setMerchantInfo(merchantInfo: MerchantInfo) {
        this.merchantInfo = merchantInfo;
        this.feePoints = merchantInfo.feePoints;
        if (this.merchantInfo.attachedAccounts) {
            for (let i = 0; i < this.merchantInfo.attachedAccounts.length; i++) {
                this.changeAttachedAccountModels[i] = new ChangePassword();
            }
        }

        this.setRatesData(this.feePoints);

        this.ratesOptions = {
            title: {
                display: false
            },
            animation: {
                duration: 0
            },
            responsive: true,
            legend: {
                display: false
            },
            scales: {
                yAxes: [{
                    display: true,
                    min: 0,
                    max: 100,
                    ticks: {
                        suggestedMin: 0,
                        suggestedMax: 100,
                        beginAtZero: true,
                        callback: (label) => {
                            if (Math.floor(label) === label) {
                                return label + ' %';
                            }
                        }
                    }
                }]
            }
        };
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
