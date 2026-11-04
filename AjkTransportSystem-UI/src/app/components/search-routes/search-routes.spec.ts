import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SearchRoutes } from './search-routes';

describe('SearchRoutes', () => {
  let component: SearchRoutes;
  let fixture: ComponentFixture<SearchRoutes>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SearchRoutes]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SearchRoutes);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
