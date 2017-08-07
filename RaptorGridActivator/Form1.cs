using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VSS.VisionLink.Raptor.GridFabric.Grids;
using VSS.VisionLink.Raptor.Servers.Client;
using log4net;
using System.Reflection;

namespace RaptorGridActivator
{
    public partial class Form1 : Form
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                Log.Info("About to call ActivatePersistentGridServer.SetGridActive()");

                bool result = ActivatePersistentGridServer.SetGridActive(RaptorGrids.RaptorGridName());

                MessageBox.Show($"Activation result: {result}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Activation exception: {ex}");
            }
        }
    }
}
