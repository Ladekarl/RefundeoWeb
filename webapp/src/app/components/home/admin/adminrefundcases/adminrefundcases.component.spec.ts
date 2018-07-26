import {async, ComponentFixture, TestBed} from '@angular/core/testing';

import {AdminRefundcasesComponent} from './adminrefundcases.component';

describe('RefundcasesComponent', () => {
    let component: AdminRefundcasesComponent;
    let fixture: ComponentFixture<AdminRefundcasesComponent>;

    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [AdminRefundcasesComponent]
        })
            .compileComponents();
    }));

    beforeEach(() => {
        fixture = TestBed.createComponent(AdminRefundcasesComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });
});
