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
using VSS.VisionLink.Raptor.GridFabric.Caches;
using VSS.VisionLink.Raptor.GridFabric.Grids;
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

        private DeployAddSurveyedSurfaceService DeployedSurveyedSurfaceService = null;
        private SurveyedSurfaceService SurveyedSurfaceService = null;

        private bool CheckConnection()
        {
            if (DeployedSurveyedSurfaceService == null && SurveyedSurfaceService == null)
            {
                MessageBox.Show("Not connected to service");
                return false;
            }
            else
            {
                return true;
            }
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
            try
            {
                if (DeployedSurveyedSurfaceService != null)
                {
                    DeployedSurveyedSurfaceService.Invoke_Add(ID, new DesignDescriptor(Guid.NewGuid().GetHashCode(), "", "", txtFilePath.Text, txtFileName.Text, 0), DateTime.Now);
                }
                else
                {
                    SurveyedSurfaceService.AddDirect(ID, new DesignDescriptor(Guid.NewGuid().GetHashCode(), "", "", txtFilePath.Text, txtFileName.Text, 0), DateTime.Now);
                }
            }
            catch (Exception E)
            {
                MessageBox.Show($"Exception: {E}");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Deploy the service as a cluster singleton
            DeployedSurveyedSurfaceService = new DeployAddSurveyedSurfaceService();

            try
            { 
                DeployedSurveyedSurfaceService.Deploy();
            }
            catch (Exception E)
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

                SurveyedSurfaces ss = DeployedSurveyedSurfaceService != null ? DeployedSurveyedSurfaceService.Invoke_List(ID) : SurveyedSurfaceService.ListDirect(ID);

                if (ss == null || ss.Count == 0)
                    MessageBox.Show("No surveyed surfaces");
                else
                    MessageBox.Show("Surveyed Surfaces:\n" + ss == null ? "None" : ss.Select(x => x.ToString() + "\n").Aggregate((s1, s2) => s1 + s2));
            }
            catch (Exception E)
            {
                MessageBox.Show($"Exception: {E}");
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            SurveyedSurfaceService = new SurveyedSurfaceService(RaptorGrids.RaptorGridName(), RaptorCaches.MutableNonSpatialCacheName());
            SurveyedSurfaceService.Init(null);
        }

        private void btnRemoveSurveyedSurface_Click(object sender, EventArgs e)
        {
            if (!CheckConnection())
            {
                return;
            }

            // Get the site model ID
            if (!long.TryParse(txtSiteModelID.Text, out long SiteModelID))
            {
                MessageBox.Show("Invalid Site Model ID");
                return;
            }

            // Get the site model ID
            if (!long.TryParse(txtSurveyedSurfaceID.Text, out long SurveydSurfaceID))
            {
                MessageBox.Show("Invalid Surveyed Surface ID");
                return;
            }

            // Invoke the service to remove the surveyed surface
            try
            {
                bool result = false;

                if (DeployedSurveyedSurfaceService != null)
                {
                    result = DeployedSurveyedSurfaceService.Invoke_Remove(SiteModelID, SurveydSurfaceID);
                }
                else
                {
                    result = SurveyedSurfaceService.RemoveDirect(SiteModelID, SurveydSurfaceID);
                }

                MessageBox.Show($"Result for removing surveyed surface ID {SurveydSurfaceID} from Site Model {SiteModelID}: {result}");
            }
            catch (Exception E)
            {
                MessageBox.Show($"Exception: {E}");
            }
        }
    }
}
