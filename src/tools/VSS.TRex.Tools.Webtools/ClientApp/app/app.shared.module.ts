import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpModule } from '@angular/http';
import { RouterModule } from '@angular/router';
import { FileUploadModule } from "ng2-file-upload/file-upload/file-upload.module";
import { AppComponent } from './components/app/app.component';
import { NavMenuComponent } from './components/navmenu/navmenu.component';
import { HomeComponent } from './components/home/home.component';
import { UploadDataComponent } from './components/upload-data/upload-data.component';
import { GridStatusComponent } from './components/gridStatus/gridStatus.component';

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    GridStatusComponent,
    UploadDataComponent,
    HomeComponent
  ],
  imports: [
    CommonModule,
    HttpModule,
    FileUploadModule,
    RouterModule.forRoot([
      { path: '', redirectTo: 'home', pathMatch: 'full' },
      { path: 'home', component: HomeComponent },
      { path: 'gridStatus', component: GridStatusComponent },
      { path: 'upload-data', component: UploadDataComponent },
      { path: '**', redirectTo: 'home' }
    ])
  ]
})
export class AppModuleShared {
}
