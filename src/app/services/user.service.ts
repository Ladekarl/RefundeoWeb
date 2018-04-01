import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

import { User } from '../models/user';

@Injectable()
export class UserService {
    constructor(private http: HttpClient) { }

    getAll() {
        return this.http.get<User[]>('/api/account');
    }

    getById(id: number) {
        return this.http.get('/api/account/' + id);
    }

    create(user: User) {
        return this.http.post('/api/account', user);
    }

    update(user: User) {
        return this.http.put('/api/account/' + user.id, user);
    }

    delete(id: number) {
        return this.http.delete('/api/account/' + id);
    }
}
