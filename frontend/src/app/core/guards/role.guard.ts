import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';

export const roleGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  
  // Basic check for roles in localStorage. In a real app, use a service that decodes the JWT.
  const userStr = localStorage.getItem('user');
  if (userStr) {
    const user = JSON.parse(userStr);
    const requiredRoles = route.data['roles'] as string[];
    
    if (!requiredRoles || requiredRoles.length === 0) {
      return true;
    }

    const hasRole = requiredRoles.some(role => user.roles?.includes(role));
    if (hasRole) {
      return true;
    }
  }

  router.navigate(['/']); // Redirect to home/landing if unauthorized
  return false;
};
