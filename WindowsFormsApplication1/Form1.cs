using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VSS.VisionLink.Raptor;
using VSS.VisionLink.Raptor.Executors;
using VSS.VisionLink.Raptor.Filters;
using VSS.VisionLink.Raptor.Geometry;
using VSS.VisionLink.Raptor.SiteModels;
using VSS.VisionLink.Raptor.Types;

namespace VSS.Raptor.IgnitePOC.TestApp
{
    public partial class Form1 : Form
    {
        long ID = 1;
        BoundingWorldExtent3D extents = BoundingWorldExtent3D.Inverted();

        private Bitmap PerformRender()
        {
            SiteModel siteModel = SiteModels.Instance().GetSiteModel(ID, false);

            if (!extents.IsValidPlanExtent)
            {
                return null;
            }

            try
            {
                // Modify extents to be a square area with the data to be rendered centered on it
                if (extents.SizeX > extents.SizeY)
                {
                    double Delta = (extents.SizeX - extents.SizeY) / 2;
                    extents.MinY -= Delta;
                    extents.MaxY += Delta;
                }
                else
                {
                    double Delta = (extents.SizeY - extents.SizeX) / 2;
                    extents.MinX -= Delta;
                    extents.MaxX += Delta;
                }

                double tileSize = extents.SizeX;

                RenderOverlayTile render = new RenderOverlayTile
                    (ID,
                     DisplayMode.Height,
                     new XYZ(extents.MinX, extents.MinY),
                     new XYZ(extents.MaxX, extents.MaxY),
                     true, // CoordsAreGrid
                     500, // PixelsX
                     500, // PixelsY
                     new CombinedFilter(siteModel) // Filter1
                         {
                         SpatialFilter = new CellSpatialFilter()
                         {
                             CoordsAreGrid = true,
                             IsSpatial = true,
                             Fence = new Fence(extents.MinX, extents.MinY, extents.MaxX, extents.MaxY)
                         }
                     },
                     null); // Filter2

                Bitmap bmp = render.Execute();

                return bmp;
            }
            catch (Exception E)
            {
                return null;
            }
        }
    

    public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void DoRender()
        {
            Bitmap bmp = PerformRender();

            if (bmp != null)
            {
                pictureBox1.Image = bmp;
                pictureBox1.Show();
            }
        }

        private void ZoomAll_Click(object sender, EventArgs e)
        {
            // Get the project extent so we know where to render
            extents = ProjectExtents.ProductionDataOnly(ID);
            DoRender();
        }

        private void btmZoomIn_Click(object sender, EventArgs e)
        {
            extents.ScalePlan(0.8);
            DoRender();
        }

        private void btnZoomOut_Click(object sender, EventArgs e)
        {
            extents.ScalePlan(1.25);
            DoRender();
        }

        private void btnTranslateNorth_Click(object sender, EventArgs e)
        {
            extents.Offset(0, 0.2 * extents.SizeX);
            DoRender();
        }

        private void bntTranslateWest_Click(object sender, EventArgs e)
        {
            extents.Offset(-0.2 * extents.SizeX, 0);
            DoRender();
        }

        private void bntTranslateEast_Click(object sender, EventArgs e)
        {
            extents.Offset(0.2 * extents.SizeX, 0);
            DoRender();
        }

        private void bntTranslateSouth_Click(object sender, EventArgs e)
        {
            extents.Offset(0, -0.2 * extents.SizeX);
            DoRender();
        }
    }
}
