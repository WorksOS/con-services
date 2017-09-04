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
        BoundingWorldExtent3D extents = BoundingWorldExtent3D.Inverted();

        RaptorGenericApplicationServiceServer genericApplicationServiceServer = new RaptorGenericApplicationServiceServer();
        RaptorTileRenderingServer tileRender = RaptorTileRenderingServer.NewInstance();

        /// <summary>
        /// Convert the Projecft ID in the text box into a number. It if is invalid return project ID 2 as a default
        /// </summary>
        /// <returns></returns>
        private long ID()
        {
            try
            {
                return Convert.ToInt64(editProjectID.Text);
            }
            catch
            {
                return -1;
            }
        }

        private Bitmap PerformRender()
        {
            // Get the relevant SiteModel. Use the generic application service server to instantiate the Ignite instance
            // SiteModel siteModel = RaptorGenericApplicationServiceServer.PerformAction(() => SiteModels.Instance().GetSiteModel(ID, false));
            SiteModel siteModel = SiteModels.Instance().GetSiteModel(ID(), false);

            try
            {
                // Modify extents to match the shape of the panel it is being displayed in
                if ((extents.SizeX / extents.SizeY) < (pictureBox1.Width / pictureBox1.Height))
                {
                    double pixelSize = extents.SizeX / pictureBox1.Width;
                    extents = new BoundingWorldExtent3D(extents.CenterX - (pictureBox1.Width / 2) * pixelSize,
                                                        extents.CenterY - (pictureBox1.Height / 2) * pixelSize,
                                                        extents.CenterX + (pictureBox1.Width / 2) * pixelSize,
                                                        extents.CenterY + (pictureBox1.Height / 2) * pixelSize);
                }
                else
                {
                    double pixelSize = extents.SizeY / pictureBox1.Height;
                    extents = new BoundingWorldExtent3D(extents.CenterX - (pictureBox1.Width / 2) * pixelSize,
                                                        extents.CenterY - (pictureBox1.Height / 2) * pixelSize,
                                                        extents.CenterX + (pictureBox1.Width / 2) * pixelSize,
                                                        extents.CenterY + (pictureBox1.Height / 2) * pixelSize);
                }
            
                CellPassAttributeFilter AttributeFilter = new CellPassAttributeFilter(siteModel)
                {
                    ReturnEarliestFilteredCellPass = chkSelectEarliestPass.Checked,
                    ElevationType = chkSelectEarliestPass.Checked ? ElevationType.First : ElevationType.Last
                };

                CellSpatialFilter SpatialFilter = new CellSpatialFilter()
                {
                    CoordsAreGrid = true,
                    IsSpatial = true,
                    Fence = new Fence(extents)
                };

                return tileRender.RenderTile(new TileRenderRequestArgument
                (ID(),
                 (DisplayMode)displayMode.SelectedIndex, //DisplayMode.Height,
                 extents,
                 true, // CoordsAreGrid
                 (ushort)pictureBox1.Width, // PixelsX
                 (ushort)pictureBox1.Height, // PixelsY
                 new CombinedFilter(AttributeFilter, SpatialFilter), // Filter1
                 null // filter 2
                ));
            }
            catch (Exception E)
            {
                MessageBox.Show(String.Format("Exception: {0}", E));
                return null;
            }
        }

        public Form1()
        {
            InitializeComponent();

            // Set the display modes in the combo box
            foreach (string mode in Enum.GetNames(typeof(DisplayMode)))
            {
                displayMode.Items.Add(mode);
            }

            displayMode.SelectedIndex = (int)DisplayMode.Height;
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

        private void DoUpdateLabels()
        {
            lblViewHeight.Text = String.Format("View height: {0:F3}m", extents.SizeY);
            lblViewWidth.Text = String.Format("View width: {0:F3}m", extents.SizeX);
            lblCellsPerPixel.Text = String.Format("Cells Per Pixel (X): {0:F3}", (extents.SizeX / pictureBox1.Width) / 0.34);
        }

        private void ViewPortChange(Action viewPortAction)
        {
            viewPortAction();
            DoUpdateLabels();
            DoRender();
        }

        private void ZoomAll_Click(object sender, EventArgs e)
        {
            // Get the project extent so we know where to render
            ViewPortChange(() => extents = ProjectExtents.ProductionDataOnly(ID()));
        }

        private void btmZoomIn_Click(object sender, EventArgs e)
        {
            ViewPortChange(() => extents.ScalePlan(0.8));
        }

        private void btnZoomOut_Click(object sender, EventArgs e)
        {
            ViewPortChange(() => extents.ScalePlan(1.25));
        }

        private double translationIncrement() => 0.2 * extents.SizeX > extents.SizeY ? extents.SizeY : extents.SizeX;

        private void btnTranslateNorth_Click(object sender, EventArgs e)
        {
            ViewPortChange(() => extents.Offset(0, translationIncrement()));
        }

        private void bntTranslateWest_Click(object sender, EventArgs e)
        {
            ViewPortChange(() => extents.Offset(-translationIncrement(), 0));
        }

        private void bntTranslateEast_Click(object sender, EventArgs e)
        {
            ViewPortChange(() => extents.Offset(translationIncrement(), 0));
        }

        private void bntTranslateSouth_Click(object sender, EventArgs e)
        {
            ViewPortChange(() => extents.Offset(0, -translationIncrement()));
        }

        private void btnRedraw_Click(object sender, EventArgs e)
        {
            ViewPortChange(() => { });
        }

        private void editProjectID_TextChanged(object sender, EventArgs e)
        {
            // Pull the sitemodel extents using the ProjectExtents executor which will use the Ignite instance created by the generic application service server above
            if (ID() != -1)
            {
                extents = ProjectExtents.ProductionDataOnly(ID());
                DoUpdateLabels();
            }
        }
    }
}
