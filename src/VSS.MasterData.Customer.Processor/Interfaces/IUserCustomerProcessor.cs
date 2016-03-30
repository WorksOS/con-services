using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.Customer.Processor.Interfaces
{
  interface IUserCustomerProcessor
  {
    void Process();
    void Stop();
  }
}
