using System;
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
                Log.Info("About to call ActivatePersistentGridServer.Instance().SetGridActive() for Immutable Raptor grid");
                bool result1 = ActivatePersistentGridServer.Instance().SetGridActive(RaptorGrids.RaptorImmutableGridName());
                Log.Info($"Activation process completed: Immutable = {result1}");

                Log.Info("About to call ActivatePersistentGridServer.Instance().SetGridActive() for Mutable Raptor grid");
                bool result2 = ActivatePersistentGridServer.Instance().SetGridActive(RaptorGrids.RaptorMutableGridName());
                Log.Info($"Activation process completed: Mutable = {result2}");

                MessageBox.Show($"Activation process completed: Mutable = {result1}, Immutable = {result2}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Activation exception: {ex}");
            }
        }
    }
}
