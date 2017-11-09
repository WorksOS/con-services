using System.Collections.Generic;

namespace VSS.Productivity3D.Scheduler.Common.Interfaces
{
  public interface IImportedFileRepo<T>
  {
    List<T> Read();
    long Create(T member);
    int Update(T member);
    int Delete(T member);
  }}
