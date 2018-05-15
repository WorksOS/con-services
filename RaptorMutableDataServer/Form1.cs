using System.Windows.Forms;
using VSS.VisionLink.Raptor.Servers.Compute;

namespace RaptorMutableDataServer
{
    public partial class Form1 : Form
    {
        TagProcComputeServer server = null;

        public Form1()
        {
            server = new TagProcComputeServer();

            InitializeComponent();
        }
    }
}