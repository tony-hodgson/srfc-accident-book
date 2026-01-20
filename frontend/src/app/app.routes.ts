import { Routes } from '@angular/router';
import { AccidentFormComponent } from './accident-form/accident-form.component';
import { AccidentListComponent } from './accident-list/accident-list.component';
import { LoginComponent } from './login/login.component';
import { AuthGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: '/accidents', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'accidents', component: AccidentListComponent, canActivate: [AuthGuard] },
  { path: 'accidents/new', component: AccidentFormComponent, canActivate: [AuthGuard] },
  { path: 'accidents/edit/:id', component: AccidentFormComponent, canActivate: [AuthGuard] },
  { path: '**', redirectTo: '/accidents' }
];

