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
using VSS.VisionLink.Raptor.GridFabric.Arguments;
using VSS.VisionLink.Raptor.GridFabric.Requests;
using VSS.VisionLink.Raptor.Servers;
using VSS.VisionLink.Raptor.Servers.Client;
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
            // Get the relevant SiteModel. Use the generic application service server to instantiate the Ignite instance
            SiteModel siteModel = RaptorGenericApplicationServiceServer.PerformAction(() => SiteModels.Instance().GetSiteModel(ID, false));

            // Pull the sitemodel extents using the ProjectExtents executor which will use the Ignite instance created by the generic application service server above
            extents = ProjectExtents.ProductionDataOnly(ID);

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

                return  RaptorTileRenderingServer.NewInstance().RenderTile(new TileRenderRequestArgument
                (ID,
                 DisplayMode.Height,
                 extents,
                 true, // CoordsAreGrid
                 500, // PixelsX
                 500, // PixelsY
                 new CombinedFilter(siteModel) // Filter1
                 {
                     SpatialFilter = new CellSpatialFilter()
                     {
                         CoordsAreGrid = true,
                         IsSpatial = true,
                         Fence = new Fence(extents)
                     }
                 },
                 null // filter 2
                ));
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

        private void DoRender()
        {
            Bitmap bmp = PerformRender();

            if (bmp != null)
            {
                pictureBox1.Image = bmp;
                pictureBox1.Show();
            }
        }

        private void ViewPortChange(Action viewPortAction)
        {
            viewPortAction();
            DoRender();
        }

        private void ZoomAll_Click(object sender, EventArgs e)
        {
            // Get the project extent so we know where to render
            ViewPortChange(() => extents = ProjectExtents.ProductionDataOnly(ID));
        }

        private void btmZoomIn_Click(object sender, EventArgs e)
        {
            ViewPortChange(() => extents.ScalePlan(0.8));
        }

        private void btnZoomOut_Click(object sender, EventArgs e)
        {
            ViewPortChange(() => extents.ScalePlan(1.25));
        }

        private void btnTranslateNorth_Click(object sender, EventArgs e)
        {
            ViewPortChange(() => extents.Offset(0, 0.2 * extents.SizeX));
        }

        private void bntTranslateWest_Click(object sender, EventArgs e)
        {
            ViewPortChange(() => extents.Offset(-0.2 * extents.SizeX, 0));
        }

        private void bntTranslateEast_Click(object sender, EventArgs e)
        {
            ViewPortChange(() => extents.Offset(0.2 * extents.SizeX, 0));
        }

        private void bntTranslateSouth_Click(object sender, EventArgs e)
        {
            ViewPortChange(() => extents.Offset(0, -0.2 * extents.SizeX));
        }
    }
}
