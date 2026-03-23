import { HttpInterceptorFn } from '@angular/common/http';

/**
 * Attaches JWT to API requests.
 * Uses localStorage directly (same key as AuthService) to avoid a circular dependency:
 * AuthService → HttpClient → this interceptor → AuthService.
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = localStorage.getItem('token');

  if (token) {
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  return next(req);
};
