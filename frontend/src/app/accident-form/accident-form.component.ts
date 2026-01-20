import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { AccidentService } from '../services/accident.service';
import { Accident } from '../models/accident.model';

@Component({
  selector: 'app-accident-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './accident-form.component.html',
  styleUrls: ['./accident-form.component.css']
})
export class AccidentFormComponent implements OnInit {
  accidentForm: FormGroup;
  isEditMode = false;
  accidentId?: number;
  isLoading = false;
  errorMessage = '';
  successMessage = '';

  constructor(
    private fb: FormBuilder,
    private accidentService: AccidentService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.accidentForm = this.fb.group({
      dateOfAccident: ['', Validators.required],
      timeOfAccident: ['', Validators.required],
      location: ['', [Validators.required, Validators.maxLength(200)]],
      opposition: ['', Validators.maxLength(200)],
      personInvolved: ['', [Validators.required, Validators.maxLength(200)]],
      age: [null, [Validators.min(1), Validators.max(17)]], // Only validate if value is provided
      personReporting: ['', [Validators.required, Validators.maxLength(200)]],
      description: ['', Validators.required],
      natureOfInjury: ['', Validators.maxLength(500)],
      treatmentGiven: ['', Validators.maxLength(1000)],
      actionTaken: ['', Validators.maxLength(1000)],
      witnesses: ['', Validators.maxLength(500)]
    });
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEditMode = true;
      this.accidentId = +id;
      this.loadAccident(this.accidentId);
    } else {
      // Set default date and time to now
      const now = new Date();
      this.accidentForm.patchValue({
        dateOfAccident: now.toISOString().split('T')[0],
        timeOfAccident: now.toTimeString().slice(0, 5)
      });
    }
  }

  loadAccident(id: number): void {
    this.isLoading = true;
    this.accidentService.getAccidentById(id).subscribe({
      next: (accident) => {
        // Format date and time for form inputs
        const date = new Date(accident.dateOfAccident);
        const time = new Date(accident.timeOfAccident);
        
        this.accidentForm.patchValue({
          dateOfAccident: date.toISOString().split('T')[0],
          timeOfAccident: time.toTimeString().slice(0, 5),
          location: accident.location,
          opposition: accident.opposition || '',
          personInvolved: accident.personInvolved,
          age: accident.age || null,
          personReporting: accident.personReporting,
          description: accident.description,
          natureOfInjury: accident.natureOfInjury || '',
          treatmentGiven: accident.treatmentGiven || '',
          actionTaken: accident.actionTaken || '',
          witnesses: accident.witnesses || ''
        });
        this.isLoading = false;
      },
      error: (error) => {
        this.errorMessage = 'Failed to load accident record';
        this.isLoading = false;
        console.error(error);
      }
    });
  }

  onSubmit(): void {
    if (this.accidentForm.valid) {
      this.isLoading = true;
      this.errorMessage = '';
      this.successMessage = '';

      const formValue = this.accidentForm.value;
      
      // Combine date and time into proper DateTime objects
      const dateTime = new Date(`${formValue.dateOfAccident}T${formValue.timeOfAccident}`);
      
      const accident: Accident = {
        dateOfAccident: dateTime.toISOString(),
        timeOfAccident: dateTime.toISOString(),
        location: formValue.location,
        opposition: formValue.opposition || '',
        personInvolved: formValue.personInvolved,
        age: formValue.age ? (typeof formValue.age === 'number' ? formValue.age : parseInt(formValue.age, 10)) : undefined,
        personReporting: formValue.personReporting,
        description: formValue.description,
        natureOfInjury: formValue.natureOfInjury || '',
        treatmentGiven: formValue.treatmentGiven || '',
        actionTaken: formValue.actionTaken || '',
        witnesses: formValue.witnesses || ''
      };

      if (this.isEditMode && this.accidentId) {
        this.accidentService.updateAccident(this.accidentId, accident).subscribe({
          next: () => {
            this.successMessage = 'Accident record updated successfully!';
            setTimeout(() => {
              this.router.navigate(['/accidents']);
            }, 1500);
          },
          error: (error) => {
            this.errorMessage = 'Failed to update accident record';
            this.isLoading = false;
            console.error(error);
          }
        });
      } else {
        this.accidentService.createAccident(accident).subscribe({
          next: () => {
            this.successMessage = 'Accident record created successfully!';
            setTimeout(() => {
              this.router.navigate(['/accidents']);
            }, 1500);
          },
          error: (error) => {
            this.errorMessage = 'Failed to create accident record';
            this.isLoading = false;
            console.error(error);
          }
        });
      }
    } else {
      this.errorMessage = 'Please fill in all required fields';
      Object.keys(this.accidentForm.controls).forEach(key => {
        const control = this.accidentForm.get(key);
        if (control?.invalid) {
          control.markAsTouched();
        }
      });
    }
  }

  cancel(): void {
    this.router.navigate(['/accidents']);
  }

  getFieldError(fieldName: string): string {
    const field = this.accidentForm.get(fieldName);
    if (field?.hasError('required')) {
      return `${this.getFieldLabel(fieldName)} is required`;
    }
    if (field?.hasError('maxlength')) {
      return `${this.getFieldLabel(fieldName)} is too long`;
    }
    if (field?.hasError('min')) {
      return 'Age must be at least 1';
    }
    if (field?.hasError('max')) {
      return 'Age must be 17 or less (only for persons under 18)';
    }
    return '';
  }

  getFieldLabel(fieldName: string): string {
    const labels: { [key: string]: string } = {
      dateOfAccident: 'Date of Accident',
      timeOfAccident: 'Time of Accident',
      location: 'Location',
      opposition: 'Opposition',
      personInvolved: 'Person Involved',
      age: 'Age',
      personReporting: 'Person Reporting',
      description: 'Description',
      natureOfInjury: 'Nature of Injury',
      treatmentGiven: 'Treatment Given',
      actionTaken: 'Action Taken',
      witnesses: 'Witnesses'
    };
    return labels[fieldName] || fieldName;
  }
}

