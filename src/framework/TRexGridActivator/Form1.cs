using System;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using System.Reflection;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.Logging;
using VSS.TRex.Servers.Client;

namespace TRexGridActivator
{
    public partial class Form1 : Form
    {
        private static readonly ILogger Log = Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                Log.LogInformation("About to call ActivatePersistentGridServer.Instance().SetGridActive() for Immutable TRex grid");
                bool result1 = ActivatePersistentGridServer.Instance().SetGridActive(TRexGrids.ImmutableGridName());
                Log.LogInformation($"Activation process completed: Immutable = {result1}");

                Log.LogInformation("About to call ActivatePersistentGridServer.Instance().SetGridActive() for Mutable TRex grid");
                bool result2 = ActivatePersistentGridServer.Instance().SetGridActive(TRexGrids.MutableGridName());
                Log.LogInformation($"Activation process completed: Mutable = {result2}");

                MessageBox.Show($"Activation process completed: Mutable = {result1}, Immutable = {result2}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Activation exception: {ex}");
            }
        }
    }
}
