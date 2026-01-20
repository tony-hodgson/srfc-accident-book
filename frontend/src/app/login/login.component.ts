import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
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
  isLoginMode = true;
  isLoading = false;
  errorMessage = '';
  successMessage = '';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.loginForm = this.fb.group({
      username: ['', Validators.required],
      password: ['', Validators.required]
    });

    this.registerForm = this.fb.group({
      username: ['', [Validators.required, Validators.minLength(3)]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      fullName: ['', Validators.required]
    });
  }

  ngOnInit(): void {
    // If already logged in, redirect to accidents
    if (this.authService.isAuthenticated()) {
      this.router.navigate(['/accidents']);
    }
  }

  toggleMode(): void {
    this.isLoginMode = !this.isLoginMode;
    this.errorMessage = '';
    this.successMessage = '';
  }

  onLogin(): void {
    if (this.loginForm.valid) {
      this.isLoading = true;
      this.errorMessage = '';
      
      this.authService.login(this.loginForm.value).subscribe({
        next: () => {
          this.router.navigate(['/accidents']);
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'Invalid username or password';
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
        next: () => {
          this.successMessage = 'Registration successful! Redirecting...';
          setTimeout(() => {
            this.router.navigate(['/accidents']);
          }, 1500);
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'Registration failed. Username or email may already exist.';
          this.isLoading = false;
        }
      });
    } else {
      this.errorMessage = 'Please fill in all fields correctly';
    }
  }

  onGoogleLogin(): void {
    // Google OAuth implementation
    // For now, we'll use a simplified approach
    // In production, you'd integrate with Google Sign-In JavaScript library
    this.errorMessage = 'Google login will be available after configuring Google OAuth credentials';
    
    // TODO: Implement Google OAuth
    // Example flow:
    // 1. Load Google Sign-In script
    // 2. Initialize Google Sign-In
    // 3. On success, call authService.loginWithGoogle()
  }
}

