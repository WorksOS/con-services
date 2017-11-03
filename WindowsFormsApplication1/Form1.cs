using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Query;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
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
using VSS.VisionLink.Raptor.GridFabric.Affinity;
using VSS.VisionLink.Raptor.GridFabric.Arguments;
using VSS.VisionLink.Raptor.GridFabric.Caches;
using VSS.VisionLink.Raptor.GridFabric.Grids;
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

        private Bitmap PerformRender(DisplayMode displayMode, int width, int height, bool returnEarliestFilteredCellPass, BoundingWorldExtent3D extents)
        {
            // Get the relevant SiteModel. Use the generic application service server to instantiate the Ignite instance
            // SiteModel siteModel = RaptorGenericApplicationServiceServer.PerformAction(() => SiteModels.Instance().GetSiteModel(ID, false));
            SiteModel siteModel = SiteModels.Instance().GetSiteModel(ID(), false);

            try
            {           
                CellPassAttributeFilter AttributeFilter = new CellPassAttributeFilter(siteModel)
                {
                    ReturnEarliestFilteredCellPass = returnEarliestFilteredCellPass,
                    ElevationType = returnEarliestFilteredCellPass ? ElevationType.First : ElevationType.Last
                };

                CellSpatialFilter SpatialFilter = new CellSpatialFilter()
                {
                    CoordsAreGrid = true,
                    IsSpatial = true,
                    Fence = new Fence(extents)
                };

                return tileRender.RenderTile(new TileRenderRequestArgument
                (ID(),
                 displayMode,
                 extents,
                 true, // CoordsAreGrid
                 (ushort)width, // PixelsX
                 (ushort)Height, // PixelsY
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

        private void fitExtentsToView(int width, int height)
        {
            // Modify extents to match the shape of the panel it is being displayed in
            if ((extents.SizeX / extents.SizeY) < (width / height))
            {
                double pixelSize = extents.SizeX / width;
                extents = new BoundingWorldExtent3D(extents.CenterX - (width / 2) * pixelSize,
                                                    extents.CenterY - (height / 2) * pixelSize,
                                                    extents.CenterX + (width / 2) * pixelSize,
                                                    extents.CenterY + (height / 2) * pixelSize);
            }
            else
            {
                double pixelSize = extents.SizeY / height;
                extents = new BoundingWorldExtent3D(extents.CenterX - (width / 2) * pixelSize,
                                                    extents.CenterY - (height / 2) * pixelSize,
                                                    extents.CenterX + (width / 2) * pixelSize,
                                                    extents.CenterY + (height / 2) * pixelSize);
            }
        }

        private void DoRender()
        {
            fitExtentsToView(pictureBox1.Width, pictureBox1.Height);

            Bitmap bmp = PerformRender((DisplayMode)displayMode.SelectedIndex, pictureBox1.Width, pictureBox1.Height, chkSelectEarliestPass.Checked, extents);

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

        private double translationIncrement() => 0.2 * (extents.SizeX > extents.SizeY ? extents.SizeY : extents.SizeX);

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

        private void btnMultiThreadTest_Click(object sender, EventArgs e)
        {
            int nImages = Convert.ToInt32(edtNumImages.Text);
            int nRuns = Convert.ToInt32(edtNumRuns.Text);

            DisplayMode displayMode = (DisplayMode)this.displayMode.SelectedIndex;
            int width = pictureBox1.Width;
            int height = pictureBox1.Height;
            bool selectEarliestPass = chkSelectEarliestPass.Checked;

//            Bitmap[] bitmaps = new Bitmap[nImages];

            fitExtentsToView(width, height);

            // Construct an array of identical bitmaps that are displayed on the form to see how well it multi-threads the requests

            StringBuilder results = new StringBuilder();

            for (int i = 0; i < nRuns; i++)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                
                // Parallel
                Parallel.For(0, nImages, x =>
                {
                     using (Bitmap b = PerformRender(displayMode, width, height, selectEarliestPass, extents))
                     {
                     }
                 });
                // Linear
                //for(int count = 0; count < nImages; count++)
                //{ 
                //    using (Bitmap b = PerformRender(displayMode, width, height, selectEarliestPass, extents))
                //   {
                //    }
                //};
                

                sw.Stop();

                results.Append(String.Format("Run {0}: Images:{1}, Time:{2}\n", i, nImages, sw.Elapsed));
            }

            MessageBox.Show(String.Format("Results:\n{0}", results.ToString()));
            //MessageBox.Show(String.Format("Images:{0}, Time:{1}", nImages, sw.Elapsed));
        }

        private void writeCacheMetrics(StreamWriter writer, ICacheMetrics metrics)
        {
            writer.WriteLine(String.Format("Number of items in cache: {0}", metrics.Size));
        }

        private void writeKeys(string title, StreamWriter writer, ICache<String, byte[]> cache)
        {
            int count = 0;

            writer.WriteLine(title);
            writer.WriteLine("###############");
            writer.WriteLine();

            if (cache == null)
            {
                return;
            }

//            writeCacheMetrics(writer, cache.GetMetrics());

            var scanQuery = new ScanQuery<String, byte[]>();
            IQueryCursor<ICacheEntry<String, byte[]>> queryCursor = cache.Query(scanQuery);
            scanQuery.PageSize = 1; // Restrict the number of keys requested in each page to reduce memory consumption

            foreach (ICacheEntry<String, byte[]> cacheEntry in queryCursor)
            {
                writer.WriteLine(String.Format("{0}:{1}", count++, cacheEntry.Key));
//                writeCacheMetrics(writer, cache.GetMetrics());
            }

            writer.WriteLine();
        }

        private void writeKeysSpatial(string title, StreamWriter writer, ICache<SubGridSpatialAffinityKey, byte[]> cache)
        {
            int count = 0;

            writer.WriteLine(title);
            writer.WriteLine("###############");
            writer.WriteLine();

            if (cache == null)
            {
                return;
            }

//            writeCacheMetrics(writer, cache.GetMetrics());

            var scanQuery = new ScanQuery<SubGridSpatialAffinityKey, byte[]>();
            scanQuery.PageSize = 1; // Restrict the number of keys requested in each page to reduce memory consumption

            IQueryCursor<ICacheEntry<SubGridSpatialAffinityKey, byte[]>> queryCursor = cache.Query(scanQuery);

            foreach (ICacheEntry<SubGridSpatialAffinityKey, byte[]> cacheEntry in queryCursor)
            {
                writer.WriteLine(String.Format("{0}:{1}", count++, cacheEntry.Key.ToString()));
//                writeCacheMetrics(writer, cache.GetMetrics());
            }

            writer.WriteLine();
        }

        private void retriveAllItems(string title, StreamWriter writer, ICache<SubGridSpatialAffinityKey, byte[]> cache)
        {
            var scanQuery = new ScanQuery<SubGridSpatialAffinityKey, byte[]>();
            scanQuery.PageSize = 1; // Restrict the number of keys requested in each page to reduce memory consumption

            IQueryCursor<ICacheEntry<SubGridSpatialAffinityKey, byte[]>> queryCursor = cache.Query(scanQuery);

            List<SubGridSpatialAffinityKey> items = new List<SubGridSpatialAffinityKey>();

            foreach (ICacheEntry<SubGridSpatialAffinityKey, byte[]> cacheEntry in queryCursor)
            {
                items.Add(cacheEntry.Key);
            }

            scanQuery = null;

            foreach (SubGridSpatialAffinityKey item in items)
            {
                byte[] Entry = cache.Get(item);
            }

            scanQuery = null;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Obtain all the keys and write them into the file "C:\Temp\AllRaptorIgniteCacheKeys.txt"

            try
            {
                using (var outFile = new FileStream(@"C:\Temp\AllRaptorIgniteCacheKeys.txt", FileMode.Create))
                {
                    using (var writer = new StreamWriter(outFile))
                    {
                        IIgnite ignite = Ignition.TryGetIgnite(RaptorGrids.RaptorGridName());

                        retriveAllItems(RaptorCaches.ImmutableNonSpatialCacheName(), writer, ignite.GetCache<SubGridSpatialAffinityKey, Byte[]>(RaptorCaches.ImmutableSpatialCacheName()));

/*
                        writeKeys(RaptorCaches.ImmutableNonSpatialCacheName(), writer, ignite.GetCache<String, Byte[]>(RaptorCaches.ImmutableNonSpatialCacheName()));
                        writeKeysSpatial(RaptorCaches.ImmutableSpatialCacheName(), writer, ignite.GetCache<SubGridSpatialAffinityKey, Byte[]>(RaptorCaches.ImmutableSpatialCacheName()));
                        writeKeys(RaptorCaches.MutableNonSpatialCacheName(), writer, ignite.GetCache<String, Byte[]>(RaptorCaches.MutableNonSpatialCacheName()));
                        writeKeysSpatial(RaptorCaches.MutableSpatialCacheName(), writer, ignite.GetCache<SubGridSpatialAffinityKey, Byte[]>(RaptorCaches.MutableSpatialCacheName()));
*/
                    }
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.ToString());
            }
        }
    }
}
