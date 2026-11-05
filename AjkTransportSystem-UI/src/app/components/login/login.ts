import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../services/auth';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './login.html',
  styleUrls: ['./login.css']
})
export class LoginComponent {
  // Login State
  email = '';
  password = '';
  message = '';
  loading = false;
  
  // View state
  activeTab = 'login'; // 'login' | 'register'
  
  // Registration & OTP states
  step = 'details'; // 'details' | 'otp'
  name = '';
  phone = '';
  regEmail = '';
  regPassword = '';
  language = 'English';
  
  otp = '';
  otpAttempts = 0;
  timer = 300;
  timerInterval: any;

  constructor(private authService: AuthService, private router: Router, private http: HttpClient) {
    if (this.router.url === '/register') {
      this.activeTab = 'register';
    }
  }

  onLogin() {
    this.loading = true;
    console.log('Login attempt:', { email: this.email });
    
    this.authService.login({ email: this.email, password: this.password })
      .subscribe({
        next: (res) => {
          console.log('Login response:', res);
          const userData = res.user ? res.user : res;
          console.log('Database role:', userData.role);
          
          const storedUser = {
            ...userData,
            token: res.token,
            loginTime: new Date().getTime()
          };
          localStorage.setItem('user', JSON.stringify(storedUser));
          this.message = 'Login successful!';
          
          if (userData.role === 'SuperAdmin') {
            console.log('Redirecting to SuperAdmin dashboard');
            this.router.navigate(['/superadmin']);
          } else if (userData.role === 'TransportAdmin' || userData.role === 'Admin') {
            console.log('Redirecting to Admin dashboard');
            this.router.navigate(['/admin']);
          } else if (userData.role === 'Driver') {
            console.log('Redirecting to Driver dashboard');
            this.router.navigate(['/driver']);
          } else {
            console.log('Redirecting to Passenger dashboard');
            this.router.navigate(['/passenger']);
          }
        },
        error: (err) => {
          console.error('Login error:', err);
          this.loading = false;
          if (err.status === 400 || err.status === 401) {
            this.message = 'Invalid credentials!';
          } else if (err.status === 0) {
            this.message = 'Cannot reach server';
          } else if (err.status >= 500) {
            this.message = 'Server error, try again';
          } else {
            this.message = err.error?.message || 'Invalid credentials!';
          }
        }
      });
  }

  switchTab(tab: string) {
    this.activeTab = tab;
    this.message = '';
    this.step = 'details';
  }

  requestOtp() {
    this.message = '';
    if (!this.name || !this.phone || !this.regPassword) {
      this.message = 'Please fill all required fields';
      return;
    }
    if (this.regPassword.length < 8) {
      this.message = 'Password must be at least 8 characters';
      return;
    }

    this.loading = true;
    
    this.http.post(`${environment.apiUrl}/Auth/send-otp`, { phone: this.phone }).subscribe({
      next: (res: any) => {
        this.loading = false;
        this.step = 'otp';
        this.message = 'OTP sent via SMS to ' + this.phone;
        this.startTimer();
      },
      error: (err: any) => {
        this.loading = false;
        this.message = err.error?.message || 'Failed to send OTP';
      }
    });
  }

  startTimer() {
    this.timer = 300;
    clearInterval(this.timerInterval);
    this.timerInterval = setInterval(() => {
      this.timer--;
      if (this.timer <= 0) clearInterval(this.timerInterval);
    }, 1000);
  }

  get formattedTimer() {
    const m = Math.floor(this.timer / 60);
    const s = this.timer % 60;
    return m + ':' + (s < 10 ? '0' : '') + s;
  }

  verifyOtp() {
    if (this.otp.length !== 6) {
      this.message = 'Enter a valid 6-digit OTP';
      return;
    }
    
    this.loading = true;
    
    const payload = {
      name: this.name,
      phone: this.phone,
      email: this.regEmail,
      password: this.regPassword,
      languagePreference: this.language,
      otp: this.otp,
      role: 'Passenger'
    };

    this.http.post(`${environment.apiUrl}/Auth/register`, payload).subscribe({
      next: (res: any) => {
        this.loading = false;
        alert('Registration successful! You can now login.');
        this.switchTab('login');
      },
      error: (err: any) => {
        this.loading = false;
        this.otpAttempts++;
        if (this.otpAttempts >= 3) {
          this.message = err.error?.message || 'Maximum attempts reached. Try again later.';
          this.step = 'details';
        } else {
          this.message = (err.error?.message || 'Invalid OTP') + '. Attempts left: ' + (3 - this.otpAttempts);
        }
      }
    });
  }

  onGoogleLogin() {
    this.message = 'Google Sign-In is not configured. Please use standard sign-in.';
  }
}
