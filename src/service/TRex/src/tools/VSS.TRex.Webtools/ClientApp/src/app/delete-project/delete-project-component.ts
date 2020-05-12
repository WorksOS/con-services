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

    public projectUid: string;

    public candidateProjectUid: string;

    public projectMetadata: ISiteModelMetadata;

    public allProjectsMetadata: ISiteModelMetadata[] = [];

    public confirmationMessage: string = "";

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
            if (this.allProjectsMetadata[i].id === this.projectUid)
                return i;
        }
        return -1;
    }

    public selectProject(): void {
        this.projectUid = this.candidateProjectUid;

        if (this.projectUid === "" || this.projectUid === undefined) {
            this.confirmationMessage = "No project selected for deletion";
        } else {
            this.confirmationMessage = `Project ${this.projectUid} selected for deletion`;
        }
    }

    public deleteProject(): void {
        if (this.projectUid === "" || this.projectUid === undefined) {
            this.confirmationMessage = "No project selected for deletion";
        } else {
            if (this.confirmDeleteProject === false) {
                this.confirmationMessage = `Must confirm intent to delete project ${this.projectUid}`;
            } else {
                this.deleteProjectService.deleteProject(this.projectUid).subscribe(result => {
                    this.confirmationMessage = `Deletion response for project ${this.projectUid}: Result = ${result.deleteSiteModelResult} with ${result.numRemovedElements} removed elements.`;
                    this.confirmDeleteProject = false;
                    this.projectUid = "";
                    this.candidateProjectUid = "";
                    this.getAllProjectMetadata();
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
                this.candidateProjectUid = "";
            });
    }

    public projectMetadataChanged(event: any): void {
        this.candidateProjectUid = this.projectMetadata.id;
    }

    public updateAllProjectsMetadata(): void {
        this.getAllProjectMetadata();
    }
}

