import {Injectable} from '@angular/core';
import {Observable} from 'rxjs';

@Injectable()
export class FileService {

    upload(file: Blob, fileTypePattern: RegExp, mbLimit: number): Observable<any> {
        const reader = new FileReader();
        if (file && file.type && !file.type.match(fileTypePattern)) {
            alert('Invalid format' + file.type);
            return;
        }
        const fileSize = file.size / 1024 / 1024; // in MB
        if (fileSize > mbLimit) {
            alert('File cannot be bigger than ' + mbLimit + ' MB');
            return;
        }

        return Observable.create((observer) => {
            reader.onload = (e: any) => {
                const reader = e.target as MSBaseReader;
                observer.next(reader.result);
            };
            reader.readAsDataURL(file);
        });
    }
}
