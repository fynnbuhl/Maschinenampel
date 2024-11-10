import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UpdateDashboardComponent } from './update-dashboard.component';

describe('UpdateDashboardComponent', () => {
  let component: UpdateDashboardComponent;
  let fixture: ComponentFixture<UpdateDashboardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [UpdateDashboardComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(UpdateDashboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
