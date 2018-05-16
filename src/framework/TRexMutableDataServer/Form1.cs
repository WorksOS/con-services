using System.Windows.Forms;
using VSS.TRex.Servers.Compute;

namespace TRexMutableDataServer
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