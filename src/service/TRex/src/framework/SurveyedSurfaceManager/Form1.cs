using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using VSS.TRex.Designs;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.ExistenceMaps.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;

namespace SurveyedSurfaceManager
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private ISurveyedSurfaceManager surveyedSurfaceManager = DIContext.Obtain<ISurveyedSurfaceManager>();
        private IDesignManager designManager = DIContext.Obtain<IDesignManager>();
        private IExistenceMaps ExistenceMaps = DIContext.Obtain<IExistenceMaps>();

        private void button1_Click(object sender, EventArgs e)
        {
            // Get the site model ID
            if (!Guid.TryParse(txtSiteModelID.Text, out Guid ID))
            {
                MessageBox.Show(@"Invalid Site Model ID");
                return;
            }

            // Get the offset
            if (!double.TryParse(txtOffset.Text, out double offset))
            {
                MessageBox.Show(@"Invalid design offset");
                return;
            }

            // Invoke the service to add the surveyed surface
            try
            {
                // Load the file and extract its extents
                TTMDesign TTM = new TTMDesign(SubGridTreeConsts.DefaultCellSize);
                string fileName = Path.Combine(new [] { txtFilePath.Text, txtFileName.Text });
                DesignLoadResult result = TTM.LoadFromFile(fileName);
                if (result != DesignLoadResult.Success)
                {
                    MessageBox.Show($@"Unable to load '{fileName}, with error = {result}");
                    return;
                }

                BoundingWorldExtent3D extents = new BoundingWorldExtent3D();
                TTM.GetExtents(out extents.MinX, out extents.MinY, out extents.MaxX, out extents.MaxY);
                TTM.GetHeightRange(out extents.MinZ, out extents.MaxZ);

                ISurveyedSurface surveyedSurface = 
                  surveyedSurfaceManager.Add(ID, 
                                             new DesignDescriptor(Guid.NewGuid(), txtFilePath.Text, txtFileName.Text, offset),
                                             dateTimePicker.Value,
                                             extents);

                // Store the existence map for the surveyed surface for later use
                ExistenceMaps.SetExistenceMap(ID, VSS.TRex.ExistenceMaps.Interfaces.Consts.EXISTENCE_SURVEYED_SURFACE_DESCRIPTOR, surveyedSurface.ID, TTM.SubgridOverlayIndex());
            }
            catch (Exception E)
            {
                MessageBox.Show($@"Exception: {E}");
            }
        }
      
        private bool GetSiteModelID(out Guid ID)
        {
            if (Guid.TryParse(txtSiteModelID.Text, out ID))
            {
              return true;
            }
      
          MessageBox.Show(@"Invalid Site Model ID");
          ID = Guid.Empty;
          return false;
        }

        private void btnListSurveyedSurfacesClick(object sender, EventArgs e)
        {
            try
            {
              if (GetSiteModelID(out Guid SiteModelID))
              {
                ISurveyedSurfaces ss = surveyedSurfaceManager.List(SiteModelID);

                if (ss == null || ss.Count == 0)
                  MessageBox.Show(@"No surveyed surfaces");
                else
                  MessageBox.Show("Surveyed Surfaces:\n" + ss.Select(x => x.ToString() + "\n").Aggregate((s1, s2) => s1 + s2));
              }
            }
            catch (Exception E)
            {
                MessageBox.Show($@"Exception: {E}");
            }
        }

        private void btnRemoveSurveyedSurface_Click(object sender, EventArgs e)
        {
          if (GetSiteModelID(out Guid SiteModelID))
          {
            // Get the surveyd surface ID
            if (!Guid.TryParse(txtSurveyedSurfaceID.Text, out Guid SurveyedSurfaceID))
            {
              MessageBox.Show(@"Invalid Surveyed Surface ID");
              return;
            }

            // Invoke the service to remove the surveyed surface
            try
            {
              bool result = surveyedSurfaceManager.Remove(SiteModelID, SurveyedSurfaceID);
              MessageBox.Show($@"Result for removing surveyed surface ID {SurveyedSurfaceID} from Site Model {SiteModelID}: {result}");
            }
            catch (Exception E)
            {
              MessageBox.Show($@"Exception: {E}");
            }
          }
        }

        private void btnRemoveDesign_Click(object sender, EventArgs e)
        {
          if (GetSiteModelID(out Guid SiteModelID))
          {
            // Get the design ID
            if (!Guid.TryParse(txtDesignID.Text, out Guid DesignID))
            {
              MessageBox.Show(@"Invalid design ID");
              return;
            }

            // Invoke the service to remove the design
            try
            {
              bool result = designManager.Remove(SiteModelID, DesignID);

              MessageBox.Show($@"Result for removing design ID {DesignID} from Site Model {SiteModelID}: {result}");
            }
            catch (Exception E)
            {
              MessageBox.Show($@"Exception: {E}");
            }
          }
        }

        private void btnListDesigns_Click(object sender, EventArgs e)
        {
          try
          {
            if (GetSiteModelID(out Guid SiteModelID))
            {
              IDesigns designList = designManager.List(SiteModelID);

              if (designList == null || designList.Count == 0)
                MessageBox.Show(@"No designs");
              else
                MessageBox.Show("Designs:\n" + designList.Select(x => x.ToString() + "\n").Aggregate((s1, s2) => s1 + s2));
            }
          }
          catch (Exception E)
          {
            MessageBox.Show($@"Exception: {E}");
          }
        }

        private void btnAddAsNewDesign_Click(object sender, EventArgs e)
        {
          if (GetSiteModelID(out Guid SiteModelID))
          {
            // Get the offset
            if (!double.TryParse(txtOffset.Text, out double offset))
            {
              MessageBox.Show(@"Invalid design offset");
              return;
            }

            // Invoke the service to add the design
            try
            {
              // Load the file and extract its extents
              TTMDesign TTM = new TTMDesign(SubGridTreeConsts.DefaultCellSize);
              TTM.LoadFromFile(Path.Combine(new[] {txtFilePath.Text, txtFileName.Text}));

              BoundingWorldExtent3D extents = new BoundingWorldExtent3D();
              TTM.GetExtents(out extents.MinX, out extents.MinY, out extents.MaxX, out extents.MaxY);
              TTM.GetHeightRange(out extents.MinZ, out extents.MaxZ);

              // Create the new design for the site model
              IDesign design = designManager.Add(SiteModelID,
                new DesignDescriptor(Guid.NewGuid(), txtFilePath.Text, txtFileName.Text, offset),
                extents);

              // Store the existence map for the design for later use
              ExistenceMaps.SetExistenceMap(SiteModelID, Consts.EXISTENCE_MAP_DESIGN_DESCRIPTOR, design.ID, TTM.SubgridOverlayIndex());
            }
            catch (Exception E)
            {
              MessageBox.Show($@"Exception: {E}");
            }
          }
        }

        private void txtFilePath_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
