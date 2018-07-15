using System.Windows.Forms;
using VSS.TRex.Servers;
using VSS.TRex.Servers.Client;

namespace TRexApplicationServer
{
    public partial class Form1 : Form
    {
        private ApplicationServiceServer server = 
          new ApplicationServiceServer(new [] { ApplicationServiceServer.DEFAULT_ROLE, ServerRoles.TILE_RENDERING_NODE, ServerRoles.ASNODE_PROFILER });

        public Form1()
        {
            InitializeComponent();
        }
    }
}
