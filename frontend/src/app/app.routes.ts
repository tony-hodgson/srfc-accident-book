import { Routes } from '@angular/router';
import { AccidentFormComponent } from './accident-form/accident-form.component';
import { AccidentListComponent } from './accident-list/accident-list.component';

export const routes: Routes = [
  { path: '', redirectTo: '/accidents', pathMatch: 'full' },
  { path: 'accidents', component: AccidentListComponent },
  { path: 'accidents/new', component: AccidentFormComponent },
  { path: 'accidents/edit/:id', component: AccidentFormComponent },
  { path: '**', redirectTo: '/accidents' }
];

