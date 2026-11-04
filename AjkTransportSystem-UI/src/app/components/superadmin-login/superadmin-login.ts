import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth';

@Component({
  selector: 'app-superadmin-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './superadmin-login.html',
  styleUrls: ['./superadmin-login.css']
})
export class SuperAdminLoginComponent {
  email = '';
  password = '';
  message = '';
  loading = false;

  constructor(private authService: AuthService, private router: Router) {}

  onLogin() {
    this.loading = true;
    this.authService.login({ email: this.email, password: this.password })
      .subscribe({
        next: (res) => {
          if (res.role === 'SuperAdmin') {
            const userData = {
              ...res,
              loginTime: new Date().getTime()
            };
            localStorage.setItem('user', JSON.stringify(userData));
            this.message = 'Login successful!';
            this.router.navigate(['/superadmin']);
          } else {
            this.message = 'Access denied! SuperAdmin credentials required.';
            this.loading = false;
          }
        },
        error: (err) => {
          this.message = 'Invalid superadmin credentials!';
          this.loading = false;
        }
      });
  }
}
