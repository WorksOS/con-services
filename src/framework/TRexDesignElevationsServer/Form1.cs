using System;
using System.Windows.Forms;
using VSS.TRex.Designs.Servers.Client;

namespace TRexDesignElevationsServer
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
