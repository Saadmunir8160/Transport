import os
import re

html_path = r'd:\fyp\AjkTransportSystem-UI\src\app\components\driver-dashboard\driver-dashboard.html'
html = open(html_path, 'r', encoding='utf-8').read()

report_issue_html = '''
      <!-- REPORT ISSUE TAB -->
      <div *ngIf="activeTab === 'report_issue'" class="dashboard-content enterprise-tab">
        <div class="enterprise-header">
          <h2 class="welcome-title">Report an Issue</h2>
          <p class="date-subtitle">Log vehicle problems, road hazards, or trip delays. Works offline.</p>
        </div>

        <div class="enterprise-grid">
          <div class="enterprise-card">
            <div class="card-header border-bottom">
              <h3 class="card-title">Submit New Issue</h3>
            </div>
            <div class="card-body" style="padding: 1.5rem;">
              <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; margin-bottom: 1rem;">
                <div>
                  <label style="display: block; font-weight: 600; margin-bottom: 0.5rem;">Category</label>
                  <select [(ngModel)]="newIssue.category" class="form-control" style="width: 100%; padding: 0.75rem; border: 1px solid #e2e8f0; border-radius: 6px;">
                    <option value="">Select Category</option>
                    <option value="Breakdown">Vehicle Breakdown</option>
                    <option value="RoadBlocked">Road Blocked / Hazard</option>
                    <option value="Accident">Accident</option>
                    <option value="Delay">Trip Delay</option>
                    <option value="Other">Other</option>
                  </select>
                </div>
                <div>
                  <label style="display: block; font-weight: 600; margin-bottom: 0.5rem;">Severity</label>
                  <select [(ngModel)]="newIssue.severity" class="form-control" style="width: 100%; padding: 0.75rem; border: 1px solid #e2e8f0; border-radius: 6px;">
                    <option value="Low">Low</option>
                    <option value="Medium">Medium</option>
                    <option value="High">High</option>
                    <option value="Critical">Critical</option>
                  </select>
                </div>
              </div>
              
              <div style="margin-bottom: 1rem;">
                <label style="display: block; font-weight: 600; margin-bottom: 0.5rem;">Description</label>
                <textarea [(ngModel)]="newIssue.description" class="form-control" rows="4" style="width: 100%; padding: 0.75rem; border: 1px solid #e2e8f0; border-radius: 6px; resize: vertical;" placeholder="Describe the issue in detail..."></textarea>
              </div>

              <div style="margin-bottom: 1.5rem;">
                <label style="display: block; font-weight: 600; margin-bottom: 0.5rem;">Photo Evidence (Optional)</label>
                <input type="file" accept="image/*" (change)="onIssuePhotoSelected($event)" class="form-control" style="padding: 0.5rem; border: 1px solid #e2e8f0; border-radius: 6px; width: 100%;" />
                <div *ngIf="newIssue.photoUrl" style="margin-top: 1rem;">
                  <img [src]="newIssue.photoUrl" style="max-height: 200px; border-radius: 6px;" />
                </div>
              </div>

              <button class="btn btn-primary" style="width: 100%; padding: 1rem; font-size: 1.1rem;" (click)="submitIssue()">Submit Report</button>
            </div>
          </div>

          <div class="enterprise-card">
            <div class="card-header border-bottom">
              <h3 class="card-title">My Open Issues</h3>
            </div>
            <div class="card-body">
              <div *ngIf="openIssues.length === 0" style="padding: 2rem; text-align: center; color: #64748b;">
                You have no open issues.
              </div>
              <ul style="list-style: none; padding: 0; margin: 0;" *ngIf="openIssues.length > 0">
                <li *ngFor="let issue of openIssues" style="border-bottom: 1px solid #f1f5f9; padding: 1rem;">
                  <div style="display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 0.5rem;">
                    <strong style="color: #0f172a;">{{ issue.category }}</strong>
                    <span class="status-badge" [ngClass]="{'badge-danger': issue.severity === 'Critical' || issue.severity === 'High', 'badge-warning': issue.severity === 'Medium'}">
                      {{ issue.severity }}
                    </span>
                  </div>
                  <p style="font-size: 0.9rem; color: #475569; margin: 0 0 0.5rem 0;">{{ issue.description }}</p>
                  <div style="display: flex; justify-content: space-between; font-size: 0.8rem;">
                    <span style="color: #64748b;">{{ issue.submittedAt | date:'medium' }}</span>
                    <span style="color: #059669; font-weight: 600;">{{ issue.status }}</span>
                  </div>
                  <div *ngIf="issue.adminResponse" style="margin-top: 0.75rem; padding: 0.75rem; background-color: #f8fafc; border-left: 3px solid #0ea5e9; border-radius: 0 4px 4px 0;">
                    <strong style="font-size: 0.8rem; color: #0284c7;">Admin Response:</strong>
                    <p style="margin: 0; font-size: 0.85rem; color: #334155;">{{ issue.adminResponse }}</p>
                  </div>
                </li>
              </ul>
            </div>
          </div>
        </div>
      </div>
'''

my_profile_html = '''
      <!-- MY PROFILE TAB -->
      <div *ngIf="activeTab === 'my_profile'" class="dashboard-content enterprise-tab">
        <div class="enterprise-header">
          <h2 class="welcome-title">My Profile</h2>
          <p class="date-subtitle">Manage your account settings and view your credentials.</p>
        </div>

        <div *ngIf="!profileData" style="padding: 2rem; text-align: center; color: #64748b;">
          Loading profile...
        </div>

        <div *ngIf="profileData" class="enterprise-grid">
          
          <div class="alert-box alert-danger" *ngIf="isLicenceExpiringSoon()" style="grid-column: 1 / -1;">
            <div class="alert-icon-wrapper">⚠️</div>
            <div class="alert-content">
              <strong>Action Required:</strong> Your driving licence will expire on {{ profileData.licenseExpiry | date }}. Please contact admin immediately to renew.
            </div>
          </div>

          <div class="enterprise-card">
            <div class="card-header border-bottom">
              <h3 class="card-title">Driver Information</h3>
            </div>
            <div class="card-body" style="padding: 1.5rem;">
              <div style="display: flex; gap: 2rem; align-items: flex-start;">
                <div style="width: 100px; height: 100px; border-radius: 50%; background-color: #e2e8f0; display: flex; align-items: center; justify-content: center; font-size: 2.5rem; font-weight: bold; color: #64748b; flex-shrink: 0;">
                  {{ profileData.name ? profileData.name.charAt(0).toUpperCase() : 'D' }}
                </div>
                <div style="flex: 1; display: grid; grid-template-columns: 1fr 1fr; gap: 1rem;">
                  <div>
                    <div style="font-size: 0.85rem; color: #64748b;">Full Name</div>
                    <div style="font-weight: 600; color: #0f172a;">{{ profileData.name }}</div>
                  </div>
                  <div>
                    <div style="font-size: 0.85rem; color: #64748b;">Employee ID</div>
                    <div style="font-weight: 600; color: #0f172a;">{{ profileData.employeeId || 'N/A' }}</div>
                  </div>
                  <div>
                    <div style="font-size: 0.85rem; color: #64748b;">Phone</div>
                    <div style="font-weight: 600; color: #0f172a;">{{ profileData.phone }}</div>
                  </div>
                  <div>
                    <div style="font-size: 0.85rem; color: #64748b;">Email</div>
                    <div style="font-weight: 600; color: #0f172a;">{{ profileData.email }}</div>
                  </div>
                  <div>
                    <div style="font-size: 0.85rem; color: #64748b;">Licence Number</div>
                    <div style="font-weight: 600; color: #0f172a;">{{ profileData.licenseNumber || 'N/A' }}</div>
                  </div>
                  <div>
                    <div style="font-size: 0.85rem; color: #64748b;">Licence Expiry</div>
                    <div style="font-weight: 600;" [ngStyle]="{'color': isLicenceExpiringSoon() ? '#ef4444' : '#0f172a'}">{{ profileData.licenseExpiry ? (profileData.licenseExpiry | date) : 'N/A' }}</div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <div class="enterprise-card">
            <div class="card-header border-bottom">
              <h3 class="card-title">Account Settings</h3>
            </div>
            <div class="card-body" style="padding: 1.5rem;">
              <div style="margin-bottom: 1.5rem;">
                <label style="display: block; font-weight: 600; margin-bottom: 0.5rem;">App Language</label>
                <select [(ngModel)]="profileSettings.languagePreference" class="form-control" style="width: 100%; padding: 0.75rem; border: 1px solid #e2e8f0; border-radius: 6px;">
                  <option value="English">English</option>
                  <option value="Urdu">Urdu</option>
                  <option value="Hindi">Hindi</option>
                </select>
              </div>

              <div style="margin-bottom: 1.5rem;">
                <label style="display: block; font-weight: 600; margin-bottom: 0.5rem;">Change Password</label>
                <input type="password" [(ngModel)]="profileSettings.currentPassword" placeholder="Current Password" class="form-control" style="width: 100%; padding: 0.75rem; border: 1px solid #e2e8f0; border-radius: 6px; margin-bottom: 0.5rem;" />
                <input type="password" [(ngModel)]="profileSettings.newPassword" placeholder="New Password" class="form-control" style="width: 100%; padding: 0.75rem; border: 1px solid #e2e8f0; border-radius: 6px;" />
              </div>

              <button class="btn btn-primary" style="width: 100%; padding: 1rem;" (click)="updateProfileSettings()">Save Changes</button>
            </div>
          </div>

        </div>
      </div>
'''

if "activeTab === 'report_issue'" not in html:
    # Use regex to replace the final closing tags
    # The structure is: </div> (content-area) </div> (main-content) </div> (admin-wrapper)
    pattern = r'(</div>\s*</div>\s*</div>\s*</div>\s*)$'
    match = re.search(pattern, html)
    if match:
        new_html = html[:match.start()] + report_issue_html + my_profile_html + match.group(1)
        open(html_path, 'w', encoding='utf-8').write(new_html)
        print('Injected HTML tabs.')
    else:
        print('Could not find injection point.')
