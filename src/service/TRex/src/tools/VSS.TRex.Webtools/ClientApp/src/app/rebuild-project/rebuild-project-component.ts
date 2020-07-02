import { Component } from '@angular/core';
import { ProjectService } from '../project/project-service';
import { ISiteModelMetadata } from '../project/project-model';
import { RebuildProjectService } from './rebuild-project-service';

@Component({
    selector: 'rebuildProject',
    templateUrl: './rebuild-project-component.html',
    providers: [ProjectService, RebuildProjectService]
})
export class RebuildProjectComponent {

    public rebuildingProjectUid: string;
    public candidateRebuildProjectUid: string;

    public projectMetadata: ISiteModelMetadata;

    public allProjectsMetadata: ISiteModelMetadata[] = [];

    public rebuildConfirmationMessage: string = "";
    public confirmRebuildProject: boolean = false;

    constructor(
        private rebuildProjectService: RebuildProjectService,
        private projectService: ProjectService
    ) {
    }

    ngOnInit() {
        this.getAllProjectMetadata();
    }

    private getIndexOfSelectedProjectMetadata(): number {
        for (var i = 0; i < this.allProjectsMetadata.length; i++) {
            if (this.allProjectsMetadata[i].id === this.rebuildingProjectUid)
                return i;
        }
        return -1;
    }

    public selectProjectForRebuilding(): void {
        this.rebuildingProjectUid = this.candidateRebuildProjectUid;

        if (this.rebuildingProjectUid === "" || this.rebuildingProjectUid === undefined) {
            this.rebuildConfirmationMessage = "No project selected for rebuilding";
        } else {
            this.rebuildConfirmationMessage = `Project ${this.rebuildingProjectUid} selected for rebuilding`;
        }
    }

    public rebuildProject(): void {
        var that = this;
        if (this.rebuildingProjectUid === "" || this.rebuildingProjectUid === undefined) {
            this.rebuildConfirmationMessage = "No project selected for rebuilding";
        } else {
            if (this.confirmRebuildProject === false) {
                this.rebuildConfirmationMessage = `Must confirm intent to rebuild project ${this.rebuildingProjectUid}`;
            } else {
                this.rebuildProjectService.rebuildProject(this.rebuildingProjectUid).subscribe(response => {
                    that.rebuildConfirmationMessage = `Rebuilding response for project ${that.rebuildingProjectUid}: Result = ${that.rebuildResultStatusAsString(response.rebuildResult)}.`;
                    that.confirmRebuildProject = false;
                    that.rebuildingProjectUid = "";
                    that.candidateRebuildProjectUid = "";
                    that.getAllProjectMetadata();
                });
            }
        }
    }

    public getAllProjectMetadata(): void {
        var result: ISiteModelMetadata[] = [];
        this.projectService.getAllProjectMetadata().subscribe(
            metadata => {
                metadata.forEach(data => result.push(data));
                this.allProjectsMetadata = result;
                this.projectMetadata = this.allProjectsMetadata[this.getIndexOfSelectedProjectMetadata()];
                this.candidateRebuildProjectUid = "";
            });
    }

    public projectMetadataChangedForRebuilding(event: any): void {
        this.candidateRebuildProjectUid = this.projectMetadata.id;
    }
    
    public updateAllProjectsMetadata(): void {
        this.getAllProjectMetadata();
    }

    private rebuildResultStatusAsString(result: number): string {

        switch (result) {
            case 0: return "UnknownError";
            case 1: return "OK";
            case 2: return "UnhandledException";
            case 3: return "UnableToLocateSiteModel";
            case 4: return "FailedToDeleteSiteModel";
            case 5: return "Pending";
            case 6: return "UnableToLocateTAGFileKeyCollection";
            case 7: return "Aborted";
            default:
                return `Unknown: ${result}`;
        }
    }
}

