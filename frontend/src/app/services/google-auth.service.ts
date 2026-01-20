import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';
import { environment } from '../../environments/environment';

declare var google: any;

export interface GoogleUser {
  sub: string; // Google ID
  email: string;
  name: string;
  picture?: string;
}

@Injectable({
  providedIn: 'root'
})
export class GoogleAuthService {
  private clientId = environment.googleClientId;
  private initialized = false;
  private userSubject = new Subject<GoogleUser>();
  public user$ = this.userSubject.asObservable();

  constructor() {
    this.initializeGoogleSignIn();
  }

  private initializeGoogleSignIn(): void {
    if (typeof google === 'undefined') {
      // Wait for Google script to load
      const checkGoogle = setInterval(() => {
        if (typeof google !== 'undefined') {
          clearInterval(checkGoogle);
          this.initGoogle();
        }
      }, 100);
      
      // Timeout after 5 seconds
      setTimeout(() => {
        clearInterval(checkGoogle);
        if (typeof google === 'undefined') {
          console.error('Google Sign-In script failed to load');
        }
      }, 5000);
    } else {
      this.initGoogle();
    }
  }

  private initGoogle(): void {
    if (this.initialized) return;

    try {
      google.accounts.id.initialize({
        client_id: this.clientId,
        callback: this.handleCredentialResponse.bind(this),
        auto_select: false,
        cancel_on_tap_outside: true
      });

      this.initialized = true;
    } catch (error) {
      console.error('Error initializing Google Sign-In:', error);
    }
  }

  private handleCredentialResponse(response: any): void {
    try {
      // Decode the JWT token
      const payload = this.decodeJwt(response.credential);
      
      const googleUser: GoogleUser = {
        sub: payload.sub,
        email: payload.email,
        name: payload.name,
        picture: payload.picture
      };

      this.userSubject.next(googleUser);
    } catch (error) {
      console.error('Error handling Google credential:', error);
      this.userSubject.error(error);
    }
  }

  private decodeJwt(token: string): any {
    const base64Url = token.split('.')[1];
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const jsonPayload = decodeURIComponent(
      atob(base64)
        .split('')
        .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
        .join('')
    );
    return JSON.parse(jsonPayload);
  }

  renderButton(elementId: string): void {
    if (!this.initialized) {
      this.initializeGoogleSignIn();
      // Wait a bit for initialization
      setTimeout(() => this.renderButton(elementId), 500);
      return;
    }

    try {
      google.accounts.id.renderButton(
        document.getElementById(elementId),
        {
          theme: 'outline',
          size: 'large',
          width: '100%',
          text: 'signin_with',
          locale: 'en'
        }
      );
    } catch (error) {
      console.error('Error rendering Google button:', error);
    }
  }

  prompt(): void {
    if (!this.initialized) {
      this.initializeGoogleSignIn();
      setTimeout(() => this.prompt(), 500);
      return;
    }

    try {
      google.accounts.id.prompt();
    } catch (error) {
      console.error('Error prompting Google Sign-In:', error);
    }
  }
}

