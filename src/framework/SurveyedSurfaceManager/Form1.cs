using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using VSS.TRex.Designs;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.ExistenceMaps;
using VSS.TRex.Geometry;
using VSS.TRex.Services.Designs;
using VSS.TRex.Services.SurveyedSurfaces;
using VSS.TRex.Storage.Models;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SurveyedSurfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;

namespace SurveyedSurfaceManager
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private SurveyedSurfaceServiceProxy DeployedSurveyedSurfaceService = null;
        private SurveyedSurfaceService SurveyedSurfaceService = null;

        private bool CheckConnection()
        {
            if ((DeployedSurveyedSurfaceService == null && SurveyedSurfaceService == null) ||
                (DIContext.Obtain<IDesignsService>() == null))
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
            if (!Guid.TryParse(txtSiteModelID.Text, out Guid ID))
            {
                MessageBox.Show("Invalid Site Model ID");
                return;
            }

            // Get the offset
            if (!double.TryParse(txtOffset.Text, out double offset))
            {
                MessageBox.Show("Invalid design offset");
                return;
            }

            // Invoke the service to add the surveyed surface
            try
            {
                // Load the file and extract its extents
                TTMDesign TTM = new TTMDesign(SubGridTreeConsts.DefaultCellSize);
                string fileName = Path.Combine(new string[] { txtFilePath.Text, txtFileName.Text });
                DesignLoadResult result = TTM.LoadFromFile(fileName);
                if (result != DesignLoadResult.Success)
                {
                    MessageBox.Show($"Unable to load '{fileName}, with error = {result}");
                    return;
                }

                BoundingWorldExtent3D extents = new BoundingWorldExtent3D();
                TTM.GetExtents(out extents.MinX, out extents.MinY, out extents.MaxX, out extents.MaxY);
                TTM.GetHeightRange(out extents.MinZ, out extents.MaxZ);

                if (DeployedSurveyedSurfaceService != null)
                {
                    DeployedSurveyedSurfaceService.Invoke_Add(ID, 
                                                              new DesignDescriptor(Guid.NewGuid(), "", "", txtFilePath.Text, txtFileName.Text, offset),                                                          
                                                              dateTimePicker.Value,
                                                              extents);

                    throw new NotImplementedException("Existence map not set via Ignite service invocation to add a surveyes surface or design");
                }
                else
                {
                    SurveyedSurfaceService.AddDirect(ID, 
                                                     new DesignDescriptor(Guid.NewGuid(), "", "", txtFilePath.Text, txtFileName.Text, offset),
                                                     dateTimePicker.Value,
                                                     extents,
                                                     out Guid SurveyedSurfaceID);

                    // Store the existence map for the surveyd surface for later use
                    ExistenceMaps.SetExistenceMap(ID, Consts.EXISTANCE_SURVEYED_SURFACE_DESCRIPTOR, SurveyedSurfaceID, TTM.SubgridOverlayIndex());
                }
            }
            catch (Exception E)
            {
                MessageBox.Show($"Exception: {E}");
            }
        }

        /// <summary>
        /// Register services...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            // Deploy the service as a cluster singleton
            DeployedSurveyedSurfaceService = new SurveyedSurfaceServiceProxy();

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
                if (!Guid.TryParse(txtSiteModelID.Text, out Guid ID))
                {
                    MessageBox.Show("Invalid Site Model ID");
                    return;
                }

                ISurveyedSurfaces ss = DeployedSurveyedSurfaceService != null ? DeployedSurveyedSurfaceService.Invoke_List(ID) : SurveyedSurfaceService.ListDirect(ID);

                if (ss == null || ss.Count == 0)
                    MessageBox.Show("No surveyed surfaces");
                else
                    MessageBox.Show("Surveyed Surfaces:\n" + ss.Select(x => x.ToString() + "\n").Aggregate((s1, s2) => s1 + s2));
            }
            catch (Exception E)
            {
                MessageBox.Show($"Exception: {E}");
            }
        }

        /// <summary>
        /// Create direct access
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click_1(object sender, EventArgs e)
        {
            SurveyedSurfaceService = new SurveyedSurfaceService(StorageMutability.Immutable);
            SurveyedSurfaceService.Init(null);
        }

        private void btnRemoveSurveyedSurface_Click(object sender, EventArgs e)
        {
            if (!CheckConnection())
            {
                return;
            }

            // Get the site model ID
            if (!Guid.TryParse(txtSiteModelID.Text, out Guid SiteModelID))
            {
                MessageBox.Show("Invalid Site Model ID");
                return;
            }

            // Get the site model ID
            if (!Guid.TryParse(txtSurveyedSurfaceID.Text, out Guid SurveydSurfaceID))
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

        private void btnRemoveDesign_Click(object sender, EventArgs e)
        {
            if (!CheckConnection())
            {
                return;
            }

            // Get the site model ID
            if (!Guid.TryParse(txtSiteModelID.Text, out Guid SiteModelID))
            {
                MessageBox.Show("Invalid Site Model ID");
                return;
            }

            // Get the design ID
            if (!Guid.TryParse(txtDesignID.Text, out Guid DesignID))
            {
                MessageBox.Show("Invalid design ID");
                return;
            }

            // Invoke the service to remove the design
            try
            {
                bool result = DIContext.Obtain<IDesignsService>().RemoveDirect(SiteModelID, DesignID);

                MessageBox.Show($"Result for removing design ID {DesignID} from Site Model {SiteModelID}: {result}");
            }
            catch (Exception E)
            {
                MessageBox.Show($"Exception: {E}");
            }
        }

        private void btnListDesigns_Click(object sender, EventArgs e)
        {
            if (!CheckConnection())
            {
                return;
            }

            try
            {
                // Get the site model ID
                if (!Guid.TryParse(txtSiteModelID.Text, out Guid ID))
                {
                    MessageBox.Show("Invalid Site Model ID");
                    return;
                }

                IDesigns designList = DIContext.Obtain<IDesignsService>().ListDirect(ID);

                if (designList == null || designList.Count == 0)
                    MessageBox.Show("No designs");
                else
                    MessageBox.Show("Designs:\n" + designList.Select(x => x.ToString() + "\n").Aggregate((s1, s2) => s1 + s2));
            }
            catch (Exception E)
            {
                MessageBox.Show($"Exception: {E}");
            }
        }

        private void btnAddAsNewDesign_Click(object sender, EventArgs e)
        {
            if (!CheckConnection())
            {
                return;
            }

            // Get the site model ID
            if (!Guid.TryParse(txtSiteModelID.Text, out Guid ID))
            {
                MessageBox.Show("Invalid Site Model ID");
                return;
            }

            // Get the offset
            if (!double.TryParse(txtOffset.Text, out double offset))
            {
                MessageBox.Show("Invalid design offset");
                return;
            }

            // Invoke the service to add the design
            try
            {
                // Load the file and extract its extents
                TTMDesign TTM = new TTMDesign(SubGridTreeConsts.DefaultCellSize);
                TTM.LoadFromFile(Path.Combine(new [] { txtFilePath.Text, txtFileName.Text }));

                BoundingWorldExtent3D extents = new BoundingWorldExtent3D();
                TTM.GetExtents(out extents.MinX, out extents.MinY, out extents.MaxX, out extents.MaxY);
                TTM.GetHeightRange(out extents.MinZ, out extents.MaxZ);

                // Create the new design for the site model
                DIContext.Obtain<IDesignsService>().AddDirect(ID,
                                         new DesignDescriptor(Guid.NewGuid(), "", "", txtFilePath.Text, txtFileName.Text, offset),
                                         extents,
                                         out Guid DesignID);

                // Store the existence map for the design for later use
                ExistenceMaps.SetExistenceMap(ID, Consts.EXISTANCE_MAP_DESIGN_DESCRIPTOR, DesignID, TTM.SubgridOverlayIndex());
            }
            catch (Exception E)
            {
                MessageBox.Show($"Exception: {E}");
            }
        }

        private void txtFilePath_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
