import { TestBed } from '@angular/core/testing';

import { TransportAdminService } from './transport-admin.service';

describe('TransportAdminService', () => {
  let service: TransportAdminService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(TransportAdminService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
