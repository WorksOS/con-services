using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagFileHarvester.Interfaces
{
  public interface ITAGProcessorClient
  {
    TAGProcServiceDecls.TTAGProcServerProcessResult SubmitTAGFileToTAGFileProcessor(string orgId, string TagFilename, Stream File);
  }
}
