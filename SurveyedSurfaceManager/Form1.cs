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
using VSS.VisionLink.Raptor.Services.Surfaces;
using VSS.VisionLink.Raptor.Surfaces;

namespace SurveyedSurfaceManager
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private DeployAddSurveyedSurfaceService SurveyedSurfaceService = null;

        private bool CheckConnection()
        {
            if (SurveyedSurfaceService == null)
            {
                MessageBox.Show("Not connected to service");
            }

            return SurveyedSurfaceService != null;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!CheckConnection())
            {
                return;
            }

            // Get the site model ID
            if (!long.TryParse(txtSiteModelID.Text, out long ID))
            {
                MessageBox.Show("Invalid Site Model ID");
                return;
            }

            // Invoke the service to add the surveyed surface
            Exception E = SurveyedSurfaceService.Invoke_Add(ID, new DesignDescriptor(Guid.NewGuid().GetHashCode(), "", "", txtFilePath.Text, txtFileName.Text, 0), DateTime.Now);

            if (E != null)
            {
                MessageBox.Show($"Exception: {E}");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Deply the service as a cluster singleton
            SurveyedSurfaceService = new DeployAddSurveyedSurfaceService();

            Exception E = SurveyedSurfaceService.Deploy();

            if (E != null)
            {
                MessageBox.Show($"Exception: {E}");
            }
        }

        private void btnListSurveyedSurfacesClick(object sender, EventArgs e)
        {
            if (!CheckConnection())
            {
                return;
            }

            try
            {
                // Get the site model ID
                if (!long.TryParse(txtSiteModelID.Text, out long ID))
                {
                    MessageBox.Show("Invalid Site Model ID");
                    return;
                }

                SurveyedSurfaces ss = SurveyedSurfaceService.Invoke_List(ID);

                MessageBox.Show("Surveyed Surfaces:\n" + ss.Select(x => x.ToString() + "\n").Aggregate((s1, s2) => s1 + s2));
            }
            catch (Exception E)
            {
                MessageBox.Show($"Exception: {E}");
            }
        }
    }
}
