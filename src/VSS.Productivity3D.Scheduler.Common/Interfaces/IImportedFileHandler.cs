using System;
using System.Collections.Generic;

namespace VSS.Productivity3D.Scheduler.Common.Interfaces
{
  public interface IImportedFileHandler<T>
  {
    int ReadFromDb();
    void EmptyList();
    List<T> List();
   
    int WriteToDb();
  }
}
