using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TagFileHarvester.Interfaces;
using TAGProcServiceDecls;

namespace TagFileHarvesterTests.Mock
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
