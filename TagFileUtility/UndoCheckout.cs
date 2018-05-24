using System;
using System.Linq;
using System.Windows.Forms;


namespace TagFileUtility
{
  public partial class UndoCheckout : Form
  {
    public UndoCheckout()
    {
      InitializeComponent();
    }

    private void btnGo_Click(object sender, EventArgs e)
    {
      ClearOutput();
      if (ValidInput())
      {
        var fileRepo = new FileRepository(txtUserName.Text, txtOrg.Text, txtPassword.Text);
        var org = fileRepo.ListOrganizations().FirstOrDefault(f => f.shortName.Equals(txtOrg.Text, StringComparison.OrdinalIgnoreCase));
        OutputText($"Found org {org.shortName}");
        var folders = fileRepo.ListFolders(org);
        OutputText($"Found {folders.Count} folders for org {org.shortName}");
        foreach (var folder in folders)
        {
          var files = fileRepo.ListFiles(org, folder);
          OutputText($"Found {files.Count} files for org {org.shortName} in folder {folder}");
          foreach (var f in files)
          {
            if (f.fullName.EndsWith("-EditCopy.tag"))
            {
              OutputText($"Ignoring {f.fullName}");
            }
            else
            {
              OutputText($"Processing {f.fullName}");
              var success = fileRepo.CancelCheckout(org, f.fullName);
            }
          }
        }
        OutputText("Finished");
      }
    }

    private bool ValidInput()
    {
      bool ok = true;
      if (string.IsNullOrEmpty(txtUserName.Text))
      {
        OutputText("Enter TCC user name");
        ok = false;
      }
      if (string.IsNullOrEmpty(txtOrg.Text))
      {
        OutputText("Enter TCC organization short name");
        ok = false;
      }
      if (string.IsNullOrEmpty(txtPassword.Text))
      {
        OutputText("Enter TCC password");
        ok = false;
      }
      return ok;
    }

    private void OutputText(string text)
    {
      txtOutput.AppendText(text + Environment.NewLine);
    }

    private void ClearOutput()
    {
      txtOutput.Text = string.Empty;
    }
  }
}
