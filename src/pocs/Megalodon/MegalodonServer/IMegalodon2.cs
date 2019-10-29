using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MegalodonServer
{
  public interface IMegalodon2
  {
    void StartProcess(string ip, int port);
    void EndProcess();
  }
}
