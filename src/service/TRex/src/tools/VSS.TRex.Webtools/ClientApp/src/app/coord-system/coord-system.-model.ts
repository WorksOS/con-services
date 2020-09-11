export class CoordinateSystemFile {
  ProjectUid: any;
  CSFileContent: string;
  CSFileName: string;

  public constructor(projectUid: any, csFileContent: string, csFileName: string) {
    this.ProjectUid = projectUid;
    this.CSFileContent = csFileContent;
    this.CSFileName = csFileName;
  }
}