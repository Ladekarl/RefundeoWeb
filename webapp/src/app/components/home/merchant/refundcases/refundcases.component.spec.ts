import {async, ComponentFixture, TestBed} from '@angular/core/testing';

import {RefundCasesComponent} from './refundcases.component';

describe('RefundcasesComponent', () => {
    let component: RefundCasesComponent;
    let fixture: ComponentFixture<RefundCasesComponent>;

    beforeEach(async(() => {
        TestBed.configureTestingModule({
            declarations: [RefundCasesComponent]
        })
            .compileComponents();
    }));

    beforeEach(() => {
        fixture = TestBed.createComponent(RefundCasesComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });
});
