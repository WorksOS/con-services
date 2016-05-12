using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.Landfill.Common.Processor
{
  public interface IProcessor
  {
    void Process();
    void Stop();
  }
}
