import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Accident } from '../models/accident.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AccidentService {
  private apiUrl = `${environment.apiUrl}/accidents`;

  constructor(private http: HttpClient) { }

  getAllAccidents(): Observable<Accident[]> {
    return this.http.get<Accident[]>(this.apiUrl);
  }

  getAccidentById(id: number): Observable<Accident> {
    return this.http.get<Accident>(`${this.apiUrl}/${id}`);
  }

  createAccident(accident: Accident): Observable<Accident> {
    return this.http.post<Accident>(this.apiUrl, accident);
  }

  updateAccident(id: number, accident: Accident): Observable<Accident> {
    return this.http.put<Accident>(`${this.apiUrl}/${id}`, accident);
  }

  deleteAccident(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}

