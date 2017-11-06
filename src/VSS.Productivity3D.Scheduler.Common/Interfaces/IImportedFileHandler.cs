using System;
using System.Collections.Generic;

namespace VSS.Productivity3D.Scheduler.Common.Interfaces
{
  public interface IImportedFileHandler<T>
  {
    int Read();

    void EmptyList();
    List<T> List();
   
    int Create(List<T> memberList );
    int Update(List<T> memberList);
    int Delete(List<T> memberList);
  }
}
