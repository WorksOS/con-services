import { Component } from '@angular/core';
 
import { ProjectExtents, DesignDescriptor, SurveyedSurface } from '../project/project-model';
import { ProjectService } from '../project/project-service';

@Component({
  selector: 'surveyed-surfaces',
  templateUrl: './surveyed-surfaces-component.html',
  providers: [ProjectService]
})

export class SurveyedSurfacesComponent {

  public projectUid: string = ""; 

  public newSurveyedSurfaceGuid: string = "";
  public surveyedSurfaces: SurveyedSurface[] = [];
        
  constructor(
    private projectService: ProjectService
  ) { }

  ngOnInit() {
  }

  public selectProject(): void {
    this.getSurveyedSurfaces();
  }

  public addADummySurveyedSurface(): void {
    var descriptor = new DesignDescriptor();
    descriptor.fileName = `C:/temp/${performance.now()}/SomeFile.ttm`;

    this.projectService.addSurveyedSurface(this.projectUid, descriptor, new Date(), new ProjectExtents(0, 0, 0, 0)).subscribe(
      uid => {
        this.newSurveyedSurfaceGuid = uid.id;
        this.getSurveyedSurfaces();
      });
  }

  public getSurveyedSurfaces(): void {
    var result: SurveyedSurface[] = [];
    this.projectService.getSurveyedSurfaces(this.projectUid).subscribe(
      surveyedsurfaces => {
        surveyedsurfaces.forEach(ss => result.push(ss));
        this.surveyedSurfaces = result;
      });  
  }

  public deleteSurveyedSurface(surveyedSurface : SurveyedSurface): void {
    this.projectService.deleteSurveyedSurface(this.projectUid, surveyedSurface.id).subscribe(x =>
      this.surveyedSurfaces.splice(this.surveyedSurfaces.indexOf(surveyedSurface), 1));
  }
}

