using System;
using System.Windows.Forms;
using VSS.VisionLink.Raptor.Servers.Client;

namespace RaptorServerApplication
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
