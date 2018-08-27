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

    private bool ActivateMutable()
    {
      Log.LogInformation("About to call ActivatePersistentGridServer.Instance().SetGridActive() for Mutable TRex grid");
      bool result = ActivatePersistentGridServer.Instance().SetGridActive(TRexGrids.MutableGridName());
      Log.LogInformation($"Activation process completed: Mutable = {result}");

      return result;
    }

    private bool ActivateImmutable()
    {
      Log.LogInformation("About to call ActivatePersistentGridServer.Instance().SetGridActive() for Immutable TRex grid");
      bool result = ActivatePersistentGridServer.Instance().SetGridActive(TRexGrids.ImmutableGridName());
      Log.LogInformation($"Activation process completed: Immutable = {result}");

      return result;
    }

    private void button1_Click(object sender, EventArgs e)
    {
      try
      {
        MessageBox.Show($"Activation process completed: Mutable = {ActivateImmutable()}, Immutable = {ActivateMutable()}");
      }
      catch (Exception ex)
      {
        MessageBox.Show($"Activation exception: {ex}");
      }
    }

    private void button2_Click(object sender, EventArgs e)
    {
      try
      {
        MessageBox.Show($"Activation process completed: Immutable = {ActivateImmutable()}");
      }
      catch (Exception ex)
      {
        MessageBox.Show($"Activation exception: {ex}");
      }
    }

    private void button3_Click(object sender, EventArgs e)
    {
      try
      {
        MessageBox.Show($"Activation process completed: Mutable = {ActivateMutable()}");
      }
      catch (Exception ex)
      {
        MessageBox.Show($"Activation exception: {ex}");
      }
    }
  }
}
