import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { AccidentService } from '../services/accident.service';
import { Accident } from '../models/accident.model';

@Component({
  selector: 'app-accident-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './accident-list.component.html',
  styleUrls: ['./accident-list.component.css']
})
export class AccidentListComponent implements OnInit {
  accidents: Accident[] = [];
  isLoading = false;
  errorMessage = '';

  constructor(
    private accidentService: AccidentService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadAccidents();
  }

  loadAccidents(): void {
    this.isLoading = true;
    this.errorMessage = '';
    
    this.accidentService.getAllAccidents().subscribe({
      next: (accidents) => {
        this.accidents = accidents;
        this.isLoading = false;
      },
      error: (error) => {
        this.errorMessage = 'Failed to load accident records. Please check if the API is running.';
        this.isLoading = false;
        console.error(error);
      }
    });
  }

  editAccident(id: number): void {
    this.router.navigate(['/accidents/edit', id]);
  }

  deleteAccident(id: number): void {
    if (confirm('Are you sure you want to delete this accident record?')) {
      this.accidentService.deleteAccident(id).subscribe({
        next: () => {
          this.loadAccidents();
        },
        error: (error) => {
          this.errorMessage = 'Failed to delete accident record';
          console.error(error);
        }
      });
    }
  }

  formatDate(dateString: string): string {
    if (!dateString) return '';
    const date = new Date(dateString);
    return date.toLocaleDateString('en-GB', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  }

  formatTime(dateString: string): string {
    if (!dateString) return '';
    const date = new Date(dateString);
    return date.toLocaleTimeString('en-GB', {
      hour: '2-digit',
      minute: '2-digit'
    });
  }
}

