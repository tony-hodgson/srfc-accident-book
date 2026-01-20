export interface LoginRequest {
  username: string;
  password: string;
}

export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
  fullName: string;
}

export interface AuthResponse {
  token: string;
  username: string;
  email: string;
  fullName?: string;
  expiresAt: string;
}

export interface UserInfo {
  username: string;
  email: string;
  fullName?: string;
}

export interface GoogleLoginRequest {
  googleId: string;
  email: string;
  fullName?: string;
}

