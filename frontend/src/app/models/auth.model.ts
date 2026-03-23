export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  fullName: string;
}

export interface RegisterInitiatedResponse {
  message: string;
  email: string;
}

export interface VerifyEmailRequest {
  email: string;
  code: string;
}

export interface VerifyEmailResult {
  success: boolean;
  message: string;
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
