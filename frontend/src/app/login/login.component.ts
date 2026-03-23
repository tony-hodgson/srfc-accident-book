import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {
  loginForm: FormGroup;
  registerForm: FormGroup;
  verifyForm: FormGroup;
  isLoginMode = true;
  /** 'register' = show signup form; 'verify' = enter 4-digit code */
  registerStep: 'register' | 'verify' = 'register';
  /** Email used for verify/resend (same as registration email) */
  pendingVerificationEmail = '';
  isLoading = false;
  errorMessage = '';
  successMessage = '';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required]
    });

    this.registerForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      fullName: ['', Validators.required]
    });

    this.verifyForm = this.fb.group({
      code: ['', [Validators.required, Validators.pattern(/^\d{4}$/)]]
    });
  }

  ngOnInit(): void {
    if (this.authService.isAuthenticated()) {
      this.router.navigate(['/accidents']);
    }
  }

  toggleMode(): void {
    this.isLoginMode = !this.isLoginMode;
    this.errorMessage = '';
    this.successMessage = '';
    this.registerStep = 'register';
    this.pendingVerificationEmail = '';
  }

  onLogin(): void {
    if (this.loginForm.valid) {
      this.isLoading = true;
      this.errorMessage = '';

      this.authService.login(this.loginForm.value).subscribe({
        next: () => {
          this.router.navigate(['/accidents']);
        },
        error: (error: HttpErrorResponse) => {
          this.errorMessage = this.readApiError(error) || 'Invalid email or password';
          this.isLoading = false;
        }
      });
    } else {
      this.errorMessage = 'Please fill in all fields';
    }
  }

  onRegister(): void {
    if (this.registerForm.valid) {
      this.isLoading = true;
      this.errorMessage = '';
      this.successMessage = '';

      this.authService.register(this.registerForm.value).subscribe({
        next: (res) => {
          this.successMessage = res.message;
          this.pendingVerificationEmail = res.email;
          this.registerStep = 'verify';
          this.verifyForm.patchValue({ code: '' });
          this.isLoading = false;
        },
        error: (error: HttpErrorResponse) => {
          this.errorMessage = this.readApiError(error) || 'Registration failed. This email may already be registered.';
          this.isLoading = false;
        }
      });
    } else {
      this.errorMessage = 'Please fill in all fields correctly';
    }
  }

  onVerifyEmail(): void {
    const code = this.verifyForm.get('code')?.value as string;
    if (!this.pendingVerificationEmail || !/^\d{4}$/.test(code)) {
      this.errorMessage = 'Enter the 4-digit code from your email.';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.authService.verifyEmail({ email: this.pendingVerificationEmail, code }).subscribe({
      next: (res) => {
        this.successMessage = res.message;
        this.isLoading = false;
        this.registerStep = 'register';
        this.isLoginMode = true;
        this.pendingVerificationEmail = '';
        this.registerForm.reset();
        this.verifyForm.reset();
      },
      error: (error: HttpErrorResponse) => {
        const body = error.error as { message?: string } | undefined;
        this.errorMessage = body?.message || this.readApiError(error) || 'Verification failed.';
        this.isLoading = false;
      }
    });
  }

  resendCode(): void {
    if (!this.pendingVerificationEmail) {
      this.errorMessage = 'Email is missing. Go back to registration.';
      return;
    }
    this.isLoading = true;
    this.errorMessage = '';
    this.authService.resendVerification(this.pendingVerificationEmail).subscribe({
      next: (res) => {
        this.successMessage = res.message;
        this.isLoading = false;
      },
      error: (error: HttpErrorResponse) => {
        const body = error.error as { message?: string } | undefined;
        this.errorMessage = body?.message || this.readApiError(error) || 'Could not resend code.';
        this.isLoading = false;
      }
    });
  }

  backToRegisterForm(): void {
    this.registerStep = 'register';
    this.errorMessage = '';
    this.successMessage = '';
    this.pendingVerificationEmail = '';
    this.verifyForm.reset();
  }

  private readApiError(error: HttpErrorResponse): string | null {
    const e = error.error;
    if (typeof e === 'string' && e.length) return e;
    if (e && typeof e === 'object' && 'message' in e && typeof (e as { message: string }).message === 'string') {
      return (e as { message: string }).message;
    }
    return null;
  }
}
