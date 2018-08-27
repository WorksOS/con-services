using System.Windows.Forms;
using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.Servers.Client;

namespace TRexApplicationServer
{
    public partial class Form1 : Form
    {
        private ApplicationServiceServer server = new ApplicationServiceServer(new [] {
        ApplicationServiceServer.DEFAULT_ROLE,
        ServerRoles.ASNODE_PROFILER,
        ServerRoles.PATCH_REQUEST_ROLE,
        ServerRoles.TILE_RENDERING_NODE,
        ServerRoles.ANALYTICS_NODE,
      });


        public Form1()
        {
            InitializeComponent();
        }
    }
}
