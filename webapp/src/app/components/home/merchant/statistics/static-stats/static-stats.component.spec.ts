import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { StaticStatsComponent } from './static-stats.component';

describe('StaticStatsComponent', () => {
  let component: StaticStatsComponent;
  let fixture: ComponentFixture<StaticStatsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ StaticStatsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(StaticStatsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
