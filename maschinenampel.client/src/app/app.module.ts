import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { UpdateDashboardComponent } from './update-dashboard/update-dashboard.component';
import { AddDashboardComponent } from './add-dashboard/add-dashboard.component';
import { DisplayDashboardComponent } from './display-dashboard/display-dashboard.component';

@NgModule({
  declarations: [
    AppComponent,
    UpdateDashboardComponent,
    AddDashboardComponent,
    DisplayDashboardComponent
  ],
  imports: [
    BrowserModule, HttpClientModule,
    AppRoutingModule, FormsModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
