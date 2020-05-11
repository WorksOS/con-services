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

    public projectMetadata: ISiteModelMetadata;

    public allProjectsMetadata: ISiteModelMetadata[] = [];


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

    public deleteProject(): void {
        this.deleteProjectService.deleteProject(this.projectUid);
    }

    public getAllProjectMetadata(): void {
        var result: ISiteModelMetadata[] = [];
        this.projectService.getAllProjectMetadata().subscribe(
            metadata => {
                metadata.forEach(data => result.push(data));
                this.allProjectsMetadata = result;
                this.projectMetadata = this.allProjectsMetadata[this.getIndexOfSelectedProjectMetadata()];
                this.projectUid = this.projectMetadata.id;
            });
    }

    public projectMetadataChanged(event: any): void {
        this.projectUid = this.projectMetadata.id;
    }

    public updateAllProjectsMetadata(): void {
        this.getAllProjectMetadata();
    }
}

