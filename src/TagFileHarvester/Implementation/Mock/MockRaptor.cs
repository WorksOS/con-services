using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VSS.Productivity3D.TagFileHarvester.Interfaces;
using TAGProcServiceDecls;

namespace VSS.Productivity3D.TagFileHarvesterTests.Mock
{
  public class MockRaptor : ITAGProcessorClient
  {
    public TTAGProcServerProcessResult SubmitTAGFileToTAGFileProcessor(string orgId, string TagFilename, Stream File)
    {
     // Thread.Sleep(TimeSpan.FromSeconds(2));
      return TTAGProcServerProcessResult.tpsprOK;
    }
  }
}
