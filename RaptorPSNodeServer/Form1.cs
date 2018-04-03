using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VSS.VisionLink.Raptor;
using VSS.VisionLink.Raptor.Servers.Compute;

namespace RaptorPSNodeServer
{
    public partial class Form1 : Form
    {
        RaptorSubGridProcessingServer server = null;

        public Form1()
        {
            server = new RaptorSubGridProcessingServer();

            InitializeComponent();

            Text = string.Format("{0}: Spatial Division {1}", Text, RaptorServerConfig.Instance().SpatialSubdivisionDescriptor);
        }
    }
}
