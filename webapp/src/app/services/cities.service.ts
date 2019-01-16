import {Injectable} from '@angular/core';
import {HttpClient, HttpHeaders} from '@angular/common/http';
import {map, share} from 'rxjs/operators';
import {Observable, of} from 'rxjs';
import {City, Merchant, MerchantInfo} from '../models';

@Injectable()
export class CitiesService {

    cities: City[];
    getAllObservable: Observable<City[]>;

    constructor(private http: HttpClient) {
    }

    getAll(): Observable<City[]> {
        if (this.cities && this.cities.length > 0) {
            return of(this.cities);
        } else if (this.getAllObservable) {
            return this.getAllObservable;
        } else {
            const requestUrl = '/api/city';
            this.getAllObservable = this.http.get<City[]>(requestUrl)
                .pipe(
                    map(c => {
                        this.getAllObservable = null;
                        this.cities = c;
                        this.cities.sort((a, b) => {
                            return ('' + a.name).localeCompare(b.name);
                        });
                        return this.cities;
                    }),
                    share());
            return this.getAllObservable;
        }
    }

    create(city: City): Observable<City> {
        const httpOptions = {
            headers: new HttpHeaders({
                'Content-Type': 'application/json'
            })
        };
        return this.http.post<City>('/api/city', city, httpOptions).pipe(map((success) => {
            if (this.cities && this.cities.length > 0) {
                this.cities.push(city);
            }
            return success;
        }));
    }
}
