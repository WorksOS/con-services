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
using VSS.VisionLink.Raptor.Servers.Client;

namespace RaptorServerApplication
{
    public partial class Form1 : Form
    {
        RaptorApplicationServiceServer server = null;

        public Form1()
        {
            server = new RaptorApplicationServiceServer();

            InitializeComponent();
        }
    }
}
