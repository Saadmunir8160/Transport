import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class AuthGuardService {
  canActivate(): boolean {
    return true;
  }
  checkSession(): boolean {
    return true;
  }
}
