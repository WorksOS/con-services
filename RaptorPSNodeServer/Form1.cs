using System.Windows.Forms;
using VSS.TRex.Servers.Compute;

namespace TRexPSNodeServer
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
