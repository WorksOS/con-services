import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BrowserModule } from '@angular/platform-browser';
import { RouterModule } from '@angular/router';
import { FileDropModule } from 'ngx-file-drop';
import { AppComponent } from './app.component';
import { GridServiceDeployerComponent } from './grid-service-deployer/grid-service-deployer.component';
import { GridStatusComponent } from './grid-status/grid-status.component';
import { HomeComponent } from './home/home.component';
import { HttpErrorHandler } from './http-error-handler.service';
import { MessageService } from './message.service';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { UploadDataComponent } from './upload-data/upload-data.component';
import { SandboxComponent } from './sandbox/sandbox-component';
import { ProjectComponent } from './project/project-component';
import { SurveyedSurfacesComponent } from './surveyed-surfaces/surveyed-surfaces-component';
import { FetchDataComponent } from './fetch-data/fetch-data.component';

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    HomeComponent,
    UploadDataComponent,
    GridStatusComponent,
    GridServiceDeployerComponent,
    SandboxComponent,
    ProjectComponent,
    SurveyedSurfacesComponent,
    FetchDataComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    FileDropModule,
    RouterModule.forRoot([
      { path: '', component: HomeComponent, pathMatch: 'full' },
      { path: 'upload-data', component: UploadDataComponent },
      { path: 'grid-status', component: GridStatusComponent },
      { path: 'grid-service-deployer', component: GridServiceDeployerComponent },
      { path: 'project', component: ProjectComponent },
      { path: 'sandbox', component: SandboxComponent },
      { path: 'fetch-data', component: FetchDataComponent },
    ])
  ],
  providers: [
    HttpErrorHandler,
    MessageService
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
