using System;
using System.Windows.Forms;

namespace ServiceDiscoveryConfigurator
{
  public partial class FormCustomServer : Form
  {
    private FormCustomServer()
    {
      InitializeComponent();
    }

    public string Result { get; set; }

    private void btnOk_Click(object sender, EventArgs e)
    {
      Result = txtValue.Text;
      DialogResult = DialogResult.OK;
      Close();
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
      Result = null;
      DialogResult = DialogResult.Cancel;
      Close();
    }
    
    public static string GetSetting(string key, string currentValue)
    {
      var form = new FormCustomServer
      {
        txtValue = {Text = currentValue}, 
        Text = $"Custom Value for Configuration Key: {key}"
      };

      return form.ShowDialog() == DialogResult.OK 
        ? form.Result 
        : null;
    }
  }
}
