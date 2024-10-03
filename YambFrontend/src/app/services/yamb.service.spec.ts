import { TestBed } from '@angular/core/testing';

import { YambService } from './yamb.service';

describe('YambService', () => {
  let service: YambService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(YambService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
