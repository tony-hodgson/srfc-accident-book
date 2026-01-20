import { Component, OnInit, AfterViewInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { GoogleAuthService } from '../services/google-auth.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit, AfterViewInit, OnDestroy {
  loginForm: FormGroup;
  registerForm: FormGroup;
  isLoginMode = true;
  isLoading = false;
  errorMessage = '';
  successMessage = '';
  private googleAuthSubscription?: Subscription;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private googleAuthService: GoogleAuthService,
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

    // Subscribe to Google authentication
    this.googleAuthSubscription = this.googleAuthService.user$.subscribe({
      next: (googleUser) => {
        this.handleGoogleSignIn(googleUser);
      },
      error: (error) => {
        this.errorMessage = 'Google sign-in failed. Please try again.';
        this.isLoading = false;
        console.error('Google auth error:', error);
      }
    });
  }

  ngAfterViewInit(): void {
    // Render Google Sign-In buttons after view initializes
    setTimeout(() => {
      this.googleAuthService.renderButton('google-signin-button');
      this.googleAuthService.renderButton('google-signin-button-register');
    }, 100);
  }

  ngOnDestroy(): void {
    if (this.googleAuthSubscription) {
      this.googleAuthSubscription.unsubscribe();
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

  handleGoogleSignIn(googleUser: any): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.authService.loginWithGoogle({
      googleId: googleUser.sub,
      email: googleUser.email,
      fullName: googleUser.name
    }).subscribe({
      next: () => {
        this.router.navigate(['/accidents']);
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Google sign-in failed. Please try again.';
        this.isLoading = false;
      }
    });
  }
}

