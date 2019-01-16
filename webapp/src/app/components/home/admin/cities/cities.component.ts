import {Component, OnInit} from '@angular/core';
import {CitiesService, FileService} from '../../../../services';
import {City} from '../../../../models';
import {Ng4LoadingSpinnerService} from 'ng4-loading-spinner';

@Component({
    selector: 'app-cities',
    templateUrl: './cities.component.html',
    styleUrls: ['./cities.component.scss']
})
export class CitiesComponent implements OnInit {

    cities: City[];
    filteredCities: City[];
    city: City;
    selectedCity: City;
    filter: string;

    constructor(private citiesService: CitiesService,
                private fileService: FileService,
                private spinnerService: Ng4LoadingSpinnerService) {
        this.city = new City();
    }

    ngOnInit() {
        this.updateCities();
    }

    updateCities() {
        this.spinnerService.show();
        this.citiesService.getAll().subscribe(cities => {
            this.cities = cities;
            this.spinnerService.hide();
            this.filterCities();
        }, () => {
            this.spinnerService.hide();
        });
    }

    onCityClicked(city: City) {
        if(city === this.selectedCity) {
            this.selectedCity = null;
            return;
        }
        this.selectedCity = city;
    }

    onAddCityForm() {
        this.spinnerService.show();
        this.citiesService.create(this.city).subscribe(() => {
            this.updateCities();
        }, () => {
            this.spinnerService.hide();
        })
    }

    onFilterChange(value) {
        this.filter = value;
        this.filterCities();
    }

    uploadCityImage(e) {
        const file = e.dataTransfer ? e.dataTransfer.files[0] : e.target.files[0];
        const pattern = /image\/png/;
        const limit = 1;

        this.fileService.upload(file, pattern, limit).subscribe(result => {
            this.city.image = result;
        });
    }

    filterCities() {
        if(this.filter) {
            this.filteredCities = this.cities.filter((city) => city.name.indexOf(this.filter) > -1);
        } else {
            this.filteredCities = this.cities;
        }
    }

}
