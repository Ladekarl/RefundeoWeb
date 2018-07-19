import {Component, OnInit} from '@angular/core';
import {MerchantInfo} from '../../../../models';
import {MerchantInfoService} from '../../../../services';
import {Ng4LoadingSpinnerService} from 'ng4-loading-spinner';
import {ConfirmationService} from 'primeng/api';

@Component({
    selector: 'app-addretailer',
    templateUrl: './addretailer.component.html',
    styleUrls: ['./addretailer.component.scss']
})
export class AddRetailerComponent implements OnInit {

    model;
    tags;
    date;

    constructor(
        private confirmationService: ConfirmationService,
        private merchantInfoService: MerchantInfoService,
        private spinnerService: Ng4LoadingSpinnerService) {
    }

    ngOnInit() {
        this.model = new MerchantInfo();
        this.model.openingHours = [
            {day: 0},
            {day: 1},
            {day: 2},
            {day: 3},
            {day: 4},
            {day: 5},
            {day: 6}
        ];
        this.model.tags = [];
        this.date = new Date();
        this.merchantInfoService.getAllTags().subscribe(tags => {
            this.tags = tags;
        });
    }

    onSubmit() {
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
        if (!file.type.match(pattern)) {
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
        this.model.logo = reader.result;
    }

    _handleBannerLoaded(e) {
        const reader = e.target;
        this.model.banner = reader.result;
    }


}
