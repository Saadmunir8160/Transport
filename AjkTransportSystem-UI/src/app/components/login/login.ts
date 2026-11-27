import { Component, OnInit, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../services/auth';
import { environment } from '../../../environments/environment';

declare const google: any;

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './login.html',
  styleUrls: ['./login.css']
})
export class LoginComponent implements OnInit, AfterViewInit {
  // Login State
  email = '';
  password = '';
  showPassword = false;
  showRegPassword = false;
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

  ngOnInit() {
  }

  ngAfterViewInit() {
    this.initGoogleSignIn();
  }

  initGoogleSignIn() {
    const checkGoogle = setInterval(() => {
      if (typeof google !== 'undefined' && google.accounts && google.accounts.id) {
        clearInterval(checkGoogle);
        google.accounts.id.initialize({
          client_id: environment.googleClientId,
          callback: (response: any) => this.handleGoogleCredentialResponse(response),
          auto_select: false,
          cancel_on_tap_outside: true
        });
      }
    }, 300);
  }

  onGoogleLogin() {
    this.message = '';
    if (typeof google !== 'undefined' && google.accounts && google.accounts.id) {
      google.accounts.id.initialize({
        client_id: environment.googleClientId,
        callback: (response: any) => this.handleGoogleCredentialResponse(response),
        auto_select: false,
        cancel_on_tap_outside: true
      });
      google.accounts.id.prompt((notification: any) => {
        if (notification.isNotDisplayed() || notification.isSkippedMoment()) {
          console.log('Google prompt not displayed/skipped, opening fallback dialog');
        }
      });
    } else {
      this.message = 'Google service is initializing, please try again in a few seconds.';
    }
  }

  handleGoogleCredentialResponse(response: any) {
    if (!response || !response.credential) {
      this.message = 'Unable to get Google credentials';
      return;
    }

    this.loading = true;
    this.message = 'Verifying Google account...';

    this.authService.googleLogin(response.credential).subscribe({
      next: (res: any) => {
        this.loading = false;
        const userData = res.user ? res.user : res;
        const storedUser = {
          ...userData,
          token: res.token,
          loginTime: new Date().getTime()
        };
        localStorage.setItem('user', JSON.stringify(storedUser));
        this.message = 'Google Login successful!';

        if (userData.role === 'SuperAdmin') {
          this.router.navigate(['/superadmin']);
        } else if (userData.role === 'TransportAdmin' || userData.role === 'Admin') {
          this.router.navigate(['/admin']);
        } else if (userData.role === 'Driver') {
          this.router.navigate(['/driver']);
        } else {
          this.router.navigate(['/passenger']);
        }
      },
      error: (err: any) => {
        this.loading = false;
        console.error('Google login error:', err);
        this.message = err.error?.message || 'Failed to authenticate with Google. Please try again.';
      }
    });
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
          console.error(`Login error [HTTP ${err.status}] [URL: ${err.url || `${environment.apiUrl}/Auth/login`}]:`, err.statusText || err.message);
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
    if (!this.name || !this.regEmail || !this.regPassword) {
      this.message = 'Please provide Full Name, Email, and Password.';
      return;
    }
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(this.regEmail)) {
      this.message = 'Please enter a valid email address.';
      return;
    }
    if (this.regPassword.length < 8) {
      this.message = 'Password must be at least 8 characters';
      return;
    }

    this.loading = true;
    
    this.http.post(`${environment.apiUrl}/Auth/send-email-otp`, { 
      email: this.regEmail, 
      name: this.name 
    }).subscribe({
      next: (res: any) => {
        this.loading = false;
        this.step = 'otp';
        this.message = 'OTP verification code sent to ' + this.regEmail;
        this.startTimer();
      },
      error: (err: any) => {
        this.loading = false;
        this.message = err.error?.message || 'Failed to send OTP to email';
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
}
