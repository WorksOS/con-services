using System.Windows.Forms;
using VSS.TRex.Servers.Client;

namespace TRexServerApplication
{
    public partial class Form1 : Form
    {
        private ApplicationServiceServer server = new ApplicationServiceServer();

        public Form1()
        {
            InitializeComponent();
        }
    }
}
