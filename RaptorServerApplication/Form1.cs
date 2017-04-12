using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VSS.VisionLink.Raptor.Servers;
using VSS.VisionLink.Raptor.Servers.Compute;

namespace RaptorServerApplication
{
    public partial class Form1 : Form
    {
        RaptorComputeServer server = null;

        public Form1()
        {
            server = new RaptorComputeServer();

            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }
    }
}
