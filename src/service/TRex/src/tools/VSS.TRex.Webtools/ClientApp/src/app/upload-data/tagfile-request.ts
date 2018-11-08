export interface ITagFileRequest {
  //Data is base 64 encoded string, projectuid and org ids are guids
  fileName: string;
  data: string;
  ProjectUid: string;
  OrgId: string;
}
