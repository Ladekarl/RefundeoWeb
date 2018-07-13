import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ShoppersComponent } from './shoppers.component';

describe('ShoppersComponent', () => {
  let component: ShoppersComponent;
  let fixture: ComponentFixture<ShoppersComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ShoppersComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ShoppersComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
