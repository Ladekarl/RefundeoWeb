import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { PurchaseChartComponent } from './purchase-chart.component';

describe('PurchaseChartComponent', () => {
  let component: PurchaseChartComponent;
  let fixture: ComponentFixture<PurchaseChartComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ PurchaseChartComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(PurchaseChartComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
