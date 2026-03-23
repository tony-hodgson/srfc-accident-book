import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { AccidentService } from '../services/accident.service';
import { Accident } from '../models/accident.model';
import { Subscription, finalize } from 'rxjs';

@Component({
  selector: 'app-accident-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './accident-form.component.html',
  styleUrls: ['./accident-form.component.css']
})
export class AccidentFormComponent implements OnInit, OnDestroy {
  accidentForm!: FormGroup;
  isEditMode = false;
  accidentId?: number;
  isLoading = false;
  errorMessage = '';
  successMessage = '';
  private subscriptions = new Subscription();

  constructor(
    private fb: FormBuilder,
    private accidentService: AccidentService,
    private router: Router,
    private route: ActivatedRoute,
    private cdr: ChangeDetectorRef
  ) {
    this.accidentForm = this.fb.group({
      dateOfAccident: ['', Validators.required],
      timeOfAccident: ['', Validators.required],
      location: ['', [Validators.required, Validators.maxLength(200)]],
      opposition: ['', Validators.maxLength(200)],
      personInvolved: ['', [Validators.required, Validators.maxLength(200)]],
      age: [null],
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
    const sub = this.accidentService
      .getAccidentById(id)
      .pipe(
        finalize(() => {
          this.isLoading = false;
          this.cdr.detectChanges();
        })
      )
      .subscribe({
        next: (accident) => {
          if (!this.accidentForm) return;

          const mapped = AccidentFormComponent.normalizeAccidentFromApi(accident);

          const dateStr = AccidentFormComponent.toDateInputValue(mapped.dateOfAccident);
          const timeStr = AccidentFormComponent.toTimeInputValue(mapped.timeOfAccident);

          this.accidentForm.patchValue(
            {
              dateOfAccident: dateStr,
              timeOfAccident: timeStr,
              location: mapped.location,
              opposition: mapped.opposition,
              personInvolved: mapped.personInvolved,
              age: mapped.age,
              personReporting: mapped.personReporting,
              description: mapped.description,
              natureOfInjury: mapped.natureOfInjury,
              treatmentGiven: mapped.treatmentGiven,
              actionTaken: mapped.actionTaken,
              witnesses: mapped.witnesses
            },
            { emitEvent: true }
          );
          this.accidentForm.updateValueAndValidity({ emitEvent: true });
          this.accidentForm.markAsPristine();
        },
        error: () => {
          this.errorMessage = 'Failed to load accident record';
        }
      });
    this.subscriptions.add(sub);
  }

  /**
   * Handles camelCase or PascalCase JSON and coerces values so validators (required / maxLength) see correct types.
   */
  private static normalizeAccidentFromApi(raw: Accident): {
    dateOfAccident: string;
    timeOfAccident: string;
    location: string;
    opposition: string;
    personInvolved: string;
    age: number | null;
    personReporting: string;
    description: string;
    natureOfInjury: string;
    treatmentGiven: string;
    actionTaken: string;
    witnesses: string;
  } {
    const r = raw as Accident & Record<string, unknown>;
    const pickStr = (camel: keyof Accident, pascal: string): string => {
      const v = (r[camel] as unknown) ?? r[pascal];
      if (v == null) return '';
      return typeof v === 'string' ? v : String(v);
    };
    const pickIso = (camel: keyof Accident, pascal: string): string => {
      const v = (r[camel] as unknown) ?? r[pascal];
      if (v == null) return '';
      if (typeof v === 'string') return v;
      if (typeof v === 'number' || v instanceof Date) return new Date(v).toISOString();
      return String(v);
    };
    const pickAge = (): number | null => {
      const v = r.age ?? r['Age'];
      if (v == null || v === '') return null;
      const n = typeof v === 'number' ? v : parseInt(String(v), 10);
      return Number.isNaN(n) ? null : n;
    };

    return {
      dateOfAccident: pickIso('dateOfAccident', 'DateOfAccident'),
      timeOfAccident: pickIso('timeOfAccident', 'TimeOfAccident'),
      location: pickStr('location', 'Location'),
      opposition: pickStr('opposition', 'Opposition'),
      personInvolved: pickStr('personInvolved', 'PersonInvolved'),
      age: pickAge(),
      personReporting: pickStr('personReporting', 'PersonReporting'),
      description: pickStr('description', 'Description'),
      natureOfInjury: pickStr('natureOfInjury', 'NatureOfInjury'),
      treatmentGiven: pickStr('treatmentGiven', 'TreatmentGiven'),
      actionTaken: pickStr('actionTaken', 'ActionTaken'),
      witnesses: pickStr('witnesses', 'Witnesses')
    };
  }

  /** yyyy-MM-dd in local calendar for date inputs */
  private static toDateInputValue(iso: string): string {
    const d = new Date(iso);
    if (Number.isNaN(d.getTime())) {
      return '';
    }
    const y = d.getFullYear();
    const m = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${y}-${m}-${day}`;
  }

  /** HH:mm in local time for time inputs */
  private static toTimeInputValue(iso: string): string {
    const d = new Date(iso);
    if (Number.isNaN(d.getTime())) {
      return '';
    }
    const h = String(d.getHours()).padStart(2, '0');
    const min = String(d.getMinutes()).padStart(2, '0');
    return `${h}:${min}`;
  }

  /** Primary button: disabled while loading or when Angular validators fail (do not also gate on pristine — that blocked the UI for some users). */
  get submitDisabled(): boolean {
    return this.isLoading || this.accidentForm.invalid;
  }

  onSubmit(event?: Event): void {
    // Prevent default form submission
    if (event) {
      event.preventDefault();
    }

    // Prevent double submission and check if form exists
    if (this.isLoading || !this.accidentForm) {
      return;
    }

    // Update form value and validity
    this.accidentForm.updateValueAndValidity();

    // Mark all fields as touched to show validation errors
    if (this.accidentForm.invalid) {
      this.accidentForm.markAllAsTouched();
      this.errorMessage = 'Please fill in all required fields';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';

    const formValue = this.accidentForm.value;

    const dateTime = new Date(`${formValue.dateOfAccident}T${formValue.timeOfAccident}`);

    let ageValue: number | undefined;
    if (formValue.age !== null && formValue.age !== '' && formValue.age !== undefined) {
      const ageNum = typeof formValue.age === 'number' ? formValue.age : parseInt(String(formValue.age), 10);
      if (!isNaN(ageNum) && ageNum >= 1 && ageNum <= 17) {
        ageValue = ageNum;
      }
    }

    const accident: Accident = {
      dateOfAccident: dateTime.toISOString(),
      timeOfAccident: dateTime.toISOString(),
      location: formValue.location,
      opposition: formValue.opposition || '',
      personInvolved: formValue.personInvolved,
      age: ageValue,
      personReporting: formValue.personReporting,
      description: formValue.description,
      natureOfInjury: formValue.natureOfInjury || '',
      treatmentGiven: formValue.treatmentGiven || '',
      actionTaken: formValue.actionTaken || '',
      witnesses: formValue.witnesses || ''
    };

    if (this.isEditMode && this.accidentId) {
      const sub = this.accidentService.updateAccident(this.accidentId, accident).subscribe({
        next: () => {
          this.successMessage = 'Accident record updated successfully!';
          setTimeout(() => {
            if (this.accidentForm) {
              this.router.navigate(['/accidents']);
            }
          }, 1500);
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'Failed to update accident record';
          this.isLoading = false;
        }
      });
      this.subscriptions.add(sub);
    } else {
      const sub = this.accidentService.createAccident(accident).subscribe({
        next: () => {
          this.successMessage = 'Accident record created successfully!';
          setTimeout(() => {
            if (this.accidentForm) {
              this.router.navigate(['/accidents']);
            }
          }, 1500);
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'Failed to create accident record';
          this.isLoading = false;
        }
      });
      this.subscriptions.add(sub);
    }
  }

  cancel(): void {
    this.router.navigate(['/accidents']);
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
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

