using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VSS.TRex.DesignProfiling.Servers.Client;

namespace RaptorDesignElevationsServer
{
    public partial class Form1 : Form
    {
        CalculateDesignElevationsServer server = null;

        public Form1()
        {
            server = new CalculateDesignElevationsServer();

            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
