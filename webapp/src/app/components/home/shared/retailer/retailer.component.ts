import {Component, OnInit} from '@angular/core';
import {MerchantInfo, Tag} from '../../../../models';
import {AuthorizationService, MerchantInfoService} from '../../../../services';
import {Ng4LoadingSpinnerService} from 'ng4-loading-spinner';
import {ConfirmationService} from 'primeng/api';

@Component({
    selector: 'app-retailer',
    templateUrl: './retailer.component.html',
    styleUrls: ['./retailer.component.scss']
})
export class RetailerComponent implements OnInit {

    model: MerchantInfo;
    tags: Tag[];
    date: Date;
    isMerchant: boolean;
    isAdmin: boolean;

    constructor(
        private confirmationService: ConfirmationService,
        private merchantInfoService: MerchantInfoService,
        private authorizationService: AuthorizationService,
        private spinnerService: Ng4LoadingSpinnerService) {
    }

    ngOnInit() {
        this.isMerchant = this.authorizationService.isAuthenticatedMerchant();
        this.isAdmin = this.authorizationService.isAuthenticatedAdmin();
        this.model = new MerchantInfo();
        this.model.openingHours = [
            {day: 0, close: '', open: ''},
            {day: 1, close: '', open: ''},
            {day: 2, close: '', open: ''},
            {day: 3, close: '', open: ''},
            {day: 4, close: '', open: ''},
            {day: 5, close: '', open: ''},
            {day: 6, close: '', open: ''}
        ];
        this.model.tags = [];
        this.date = new Date();
        this.merchantInfoService.getAllTags().subscribe(tags => {
            this.tags = tags;
        });

        if (this.isMerchant) {
            this.getMerchant();
        }
    }

    getMerchant() {
        this.spinnerService.show();
        this.merchantInfoService.getMerchant(this.authorizationService.getCurrentUser().id).subscribe(merchantInfo => {
            this.model = merchantInfo;
            this.spinnerService.hide();
        }, () => {
            this.spinnerService.hide();
        });
    }

    getMerchantNoCache() {
        this.spinnerService.show();
        this.merchantInfoService.getMerchantNoCache(this.authorizationService.getCurrentUser().id).subscribe(merchantInfo => {
            this.model = merchantInfo;
            this.spinnerService.hide();
        }, () => {
            this.spinnerService.hide();
        });
    }

    onSubmit() {
        if (this.isAdmin) {
            this.createMerchant();
        } else if (this.isMerchant) {
            this.updateMerchant();
        }
    }

    updateMerchant() {
        this.confirmationService.confirm({
            message: `Are you sure you want to update your information?`,
            accept: () => {
                this.spinnerService.show();
                this.merchantInfoService.updateMerchant(this.model).subscribe(() => {
                    this.getMerchantNoCache();
                }, (e) => {
                    this.spinnerService.hide();
                    let errorString = 'Could not update your information\n';
                    if (e.error && e.error.errors) {
                        e.error.errors.forEach(e => {
                            errorString = errorString + e.description + '\n';
                        });
                    }
                    alert(errorString);
                });
            }
        });
    }

    createMerchant() {
        this.confirmationService.confirm({
            message: `Are you sure you want to create this retailer?`,
            accept: () => {
                this.spinnerService.show();
                this.merchantInfoService.create(this.model).subscribe(() => {
                    this.spinnerService.hide();
                    alert('Successfully created retailer');
                }, (e) => {
                    this.spinnerService.hide();
                    let errorString = 'Could not create retailer\n';
                    if (e.error && e.error.errors) {
                        e.error.errors.forEach(e => {
                            errorString = errorString + e.description + '\n';
                        });
                    }
                    alert(errorString);
                });
            }
        });
    }

    onCheckTag(tag) {
        const tagIndex = this.model.tags.indexOf(tag.key);
        if (tagIndex === -1) {
            this.model.tags.push(tag.key);
        } else {
            this.model.tags.splice(tagIndex, 1);
        }
    }

    uploadLogo(e) {
        const file = e.dataTransfer ? e.dataTransfer.files[0] : e.target.files[0];
        const pattern = /image\/png/;
        const reader = new FileReader();
        if (file && file.type && !file.type.match(pattern)) {
            alert('invalid format' + file.type);
            return;
        }
        reader.onload = this._handleLogoLoaded.bind(this);
        reader.readAsDataURL(file);
    }

    uploadBanner(e) {
        const file = e.dataTransfer ? e.dataTransfer.files[0] : e.target.files[0];
        const pattern = /image\/png/;
        const reader = new FileReader();
        if (!file.type.match(pattern)) {
            alert('invalid format');
            return;
        }
        reader.onload = this._handleBannerLoaded.bind(this);
        reader.readAsDataURL(file);
    }

    _handleLogoLoaded(e) {
        const reader = e.target;
        this.model.logo = reader.result.replace('data:image/png;base64,', '');
    }

    _handleBannerLoaded(e) {
        const reader = e.target;
        this.model.banner = reader.result.replace('data:image/png;base64,', '');
    }


}
