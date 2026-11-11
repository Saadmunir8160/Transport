import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PassengerDashboardComponent } from './passenger-dashboard';

describe('PassengerDashboardComponent', () => {
  let component: PassengerDashboardComponent;
  let fixture: ComponentFixture<PassengerDashboardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PassengerDashboardComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PassengerDashboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
