using System.Collections.Generic;

namespace VSS.Productivity3D.Scheduler.Common.Interfaces
{
  public interface IImportedFileRepo<T>
  {
    List<T> Read(bool processSurveyedSurfaceType);
    bool ProjectAndCustomerExist(string customerUid, string projectUid);
    long Create(T member);
    int Update(T member);
    int Delete(T member);
  }}
