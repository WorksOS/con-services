import { Component } from '@angular/core';
import { ProjectService } from '../project/project-service';
import { DeleteProjectService } from './delete-project-service';
import { ISiteModelMetadata } from '../project/project-model';

@Component({
    selector: 'deleteProject',
    templateUrl: './delete-project-component.html',
    providers: [ProjectService, DeleteProjectService]
})
export class DeleteProjectComponent {

    public deletionProjectUid: string;
    public candidateDeleteProjectUid: string;

    public projectMetadata: ISiteModelMetadata;

    public allProjectsMetadata: ISiteModelMetadata[] = [];

    public deleteConfirmationMessage: string = "";
    public confirmDeleteProject: boolean = false;

    constructor(
        private deleteProjectService: DeleteProjectService,
        private projectService: ProjectService
    ) {
    }

    ngOnInit() {
        this.getAllProjectMetadata();
    }

    private getIndexOfSelectedProjectMetadata(): number {
        for (var i = 0; i < this.allProjectsMetadata.length; i++) {
            if (this.allProjectsMetadata[i].id === this.deletionProjectUid)
                return i;
        }
        return -1;
    }

    public selectProjectForDeletion(): void {
        this.deletionProjectUid = this.candidateDeleteProjectUid;

        if (this.deletionProjectUid === "" || this.deletionProjectUid === undefined) {
            this.deleteConfirmationMessage = "No project selected for deletion";
        } else {
            this.deleteConfirmationMessage = `Project ${this.deletionProjectUid} selected for deletion`;
        }
    }

    public deleteProject(): void {
        var that = this;
        if (this.deletionProjectUid === "" || this.deletionProjectUid === undefined) {
            this.deleteConfirmationMessage = "No project selected for deletion";
        } else {
            if (this.confirmDeleteProject === false) {
                this.deleteConfirmationMessage = `Must confirm intent to delete project ${this.deletionProjectUid}`;
            } else {
                this.deleteProjectService.deleteProject(this.deletionProjectUid).subscribe(response => {
                    that.deleteConfirmationMessage = `Deletion response for project ${that.deletionProjectUid}: Result = ${that.deleteResultStatusAsString(response.result)} with ${response.numRemovedElements} removed elements.`;
                    that.confirmDeleteProject = false;
                    that.deletionProjectUid = "";
                    that.candidateDeleteProjectUid = "";
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
                this.candidateDeleteProjectUid = "";
            });
    }

    public projectMetadataChangedForDeletion(event: any): void {
        this.candidateDeleteProjectUid = this.projectMetadata.id;
    }

    public updateAllProjectsMetadata(): void {
        this.getAllProjectMetadata();
    }

    private deleteResultStatusAsString(result: number): string {
        switch (result) {
            case 0: return "UnknownError";
            case 1: return "OK";
            case 2: return "UnhandledException";
            case 3: return "UnableToLocateSiteModel";
            case 4: return "FailedToRemoveSubGrids";
            case 5: return "FailedToRemoveProjectMetadata";
            case 6: return "FailedToCommitPrimaryElementRemoval";
            case 7: return "FailedToCommitExistenceMapRemoval";
            case 8: return "FailedToRemoveSiteDesigns";
            case 9: return "FailedToRemoveSurveyedSurfaces";
            case 10: return "FailedToRemoveAlignments";
            case 11: return "FailedToRemoveCSIB";
            default:
                return `Unknown: ${result}`;
        }
    }
}

