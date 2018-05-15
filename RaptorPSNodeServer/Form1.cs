using System.Windows.Forms;
using VSS.VisionLink.Raptor.Servers.Compute;

namespace RaptorPSNodeServer
{
    public partial class Form1 : Form
    {
        SubGridProcessingServer server = null;

        public Form1()
        {
            server = new SubGridProcessingServer();

            InitializeComponent();
        }
    }
}
