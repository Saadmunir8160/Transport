import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth';

@Component({
  selector: 'app-admin-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './admin-login.html',
  styleUrls: ['./admin-login.css']
})
export class AdminLoginComponent {
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
          if (res.role === 'Admin') {
            const userData = {
              ...res,
              loginTime: new Date().getTime()
            };
            localStorage.setItem('user', JSON.stringify(userData));
            this.message = 'Login successful!';
            
            if (res.role === 'SuperAdmin') {
              this.router.navigate(['/superadmin']);
            } else {
              this.router.navigate(['/admin']);
            }
          } else {
            this.message = 'Access denied! Admin credentials required.';
            this.loading = false;
          }
        },
        error: (err) => {
          this.message = 'Invalid admin credentials!';
          this.loading = false;
        }
      });
  }
}
