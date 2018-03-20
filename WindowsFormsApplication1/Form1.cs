using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Query;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VSS.VisionLink.Analytics.Operations;
using VSS.VisionLink.Raptor.Analytics.GridFabric.Arguments;
using VSS.VisionLink.Raptor.Analytics.Models;
using VSS.VisionLink.Raptor.Designs.Storage;
using VSS.VisionLink.Raptor.Executors;
using VSS.VisionLink.Raptor.Filters;
using VSS.VisionLink.Raptor.Geometry;
using VSS.VisionLink.Raptor.GridFabric.Affinity;
using VSS.VisionLink.Raptor.GridFabric.Caches;
using VSS.VisionLink.Raptor.GridFabric.Events;
using VSS.VisionLink.Raptor.GridFabric.Grids;
using VSS.VisionLink.Raptor.GridFabric.Queues;
using VSS.VisionLink.Raptor.Rendering.Servers.Client;
using VSS.VisionLink.Raptor.Servers.Client;
using VSS.VisionLink.Raptor.Services.Designs;
using VSS.VisionLink.Raptor.Services.Surfaces;
using VSS.VisionLink.Raptor.SiteModels;
using VSS.VisionLink.Raptor.Storage;
using VSS.VisionLink.Raptor.Surfaces;
using VSS.VisionLink.Raptor.Types;
using VSS.VisionLink.Raptor.Volumes;
using VSS.VisionLink.Raptor.Volumes.GridFabric.Arguments;
using VSS.VisionLink.Raptor.Volumes.GridFabric.Responses;

namespace VSS.Raptor.IgnitePOC.TestApp
{
    public partial class Form1 : Form
    {
        BoundingWorldExtent3D extents = BoundingWorldExtent3D.Inverted();

        //        RaptorGenericApplicationServiceServer genericApplicationServiceServer = new RaptorGenericApplicationServiceServer();
        RaptorTileRenderingServer tileRenderServer = null;
        RaptorSimpleVolumesServer simpleVolumesServer = null;
        RaptorMutableClientServer mutableClient = null;

        SiteModelAttributesChangedEventListener SiteModelAttrubutesChanged = null;

        /// <summary>
        /// Convert the Project ID in the text box into a number. It if is invalid return project ID 2 as a default
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

            if (siteModel == null)
            {
                MessageBox.Show($"Site model {ID()} is unavailable");
                return null;
            }

            try
            {
                CellPassAttributeFilter AttributeFilter = new CellPassAttributeFilter(siteModel)
                {
                    ReturnEarliestFilteredCellPass = returnEarliestFilteredCellPass,
                    HasElevationTypeFilter = true,
                    ElevationType = returnEarliestFilteredCellPass ? ElevationType.First : ElevationType.Last,
                    SurveyedSurfaceExclusionList = GetSurveyedSurfaceExclusionList(siteModel)
                };

                CellSpatialFilter SpatialFilter = new CellSpatialFilter()
                {
                    CoordsAreGrid = true,
                    IsSpatial = true,
                    Fence = new Fence(extents)
                };

                //TODO Fix it with DI
                return null; /*tileRenderServer.RenderTile(new TileRenderRequestArgument
                (ID(),
                 displayMode,
                 extents,
                 true, // CoordsAreGrid
                 (ushort)width, // PixelsX
                 (ushort)height, // PixelsY
                 new CombinedFilter(AttributeFilter, SpatialFilter), // Filter1
                 null, // filter 2
                 (cmbDesigns.Items.Count == 0) ? long.MinValue : (cmbDesigns.SelectedValue as Design).ID// DesignDescriptor
                ));*/
            }
            catch (Exception E)
            {
                MessageBox.Show(String.Format("Exception: {0}", E));
                return null;
            }
        }

        private BoundingWorldExtent3D GetZoomAllExtents()
        {
            SiteModel siteModel = SiteModels.Instance().GetSiteModel(ID(), false);

            if (siteModel != null)
            {
                long[] SurveyedSurfaceExclusionList = (siteModel.SurveyedSurfaces == null || chkIncludeSurveyedSurfaces.Checked) ? new long[0] : siteModel.SurveyedSurfaces.Select(x => x.ID).ToArray();

                return ProjectExtents.ProductionDataAndSurveyedSurfaces(ID(), SurveyedSurfaceExclusionList);
            }
            else
            {
                return BoundingWorldExtent3D.Null();
            }
        }

        public Form1()
        {
            InitializeComponent();

            // Set the display modes in the combo box
            displayMode.Items.AddRange(Enum.GetNames(typeof(DisplayMode)));
            displayMode.SelectedIndex = (int)DisplayMode.Height;

            tileRenderServer = RaptorTileRenderingServer.NewInstance();
            simpleVolumesServer = RaptorSimpleVolumesServer.NewInstance();

            mutableClient = new RaptorMutableClientServer("TestApplication");

            // Instantiate a site model changed listener to catch changes to site model attributes
            SiteModelAttrubutesChanged = new SiteModelAttributesChangedEventListener(RaptorGrids.RaptorImmutableGridName());
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void fitExtentsToView(int width, int height)
        {
            double Aspect = (1.0 * height) / (1.0 * width);

            if ((extents.SizeX / extents.SizeY) > Aspect)
            {
                extents = new BoundingWorldExtent3D(extents.CenterX - (extents.SizeY / Aspect) / 2,
                                                    extents.CenterY - (extents.SizeY / 2),
                                                    extents.CenterX + (extents.SizeY / Aspect) / 2,
                                                    extents.CenterY + (extents.SizeY / 2),
                                                    extents.MinZ, extents.MaxZ);
            }
            else
            {
                extents = new BoundingWorldExtent3D(extents.CenterX - (extents.SizeX / 2),
                                                    extents.CenterY - (extents.SizeX * Aspect) / 2,
                                                    extents.CenterX + (extents.SizeX / 2),
                                                    extents.CenterY + (extents.SizeX * Aspect) / 2,
                                                    extents.MinZ, extents.MaxZ);
            }
            /*

                                    // Modify extents to match the shape of the panel it is being displayed in
                                    if ((extents.SizeX / extents.SizeY) < (width / height))
                                    {
                                        double pixelSize = extents.SizeX / width;
                            extents = new BoundingWorldExtent3D(extents.CenterX - (width / 2) * pixelSize,
                                                                extents.CenterY - (height / 2) * pixelSize,
                                                                extents.CenterX + (width / 2) * pixelSize,
                                                                extents.CenterY + (height / 2) * pixelSize);//,
            //                                                    extents.MinZ, extents.MaxZ);
                                    }
                                    else
                                    {
                                        double pixelSize = extents.SizeY / height;
                            extents = new BoundingWorldExtent3D(extents.CenterX - (width / 2) * pixelSize,
                                                                extents.CenterY - (height / 2) * pixelSize,
                                                                extents.CenterX + (width / 2) * pixelSize,
                                                                extents.CenterY + (height / 2) * pixelSize); //,                                                                
            //                                                    extents.MinZ, extents.MaxZ);
        }            
            */
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

        private void DoUpdateDesignsAndSurveyedSurfaces()
        {
            Designs designs = DesignsService.Instance().List(ID());

            if (designs != null)
            {
                cmbDesigns.DropDownStyle = ComboBoxStyle.DropDownList;

                //cmbDesigns.Items.Clear();

                cmbDesigns.DisplayMember = "Text";
                cmbDesigns.ValueMember = "Value";
                cmbDesigns.DataSource = designs.Select(x => new { Text = x.DesignDescriptor.FullPath, Value = x }).ToArray();
            }

            SurveyedSurfaceService surveyedSurfacesService = new SurveyedSurfaceService(VisionLink.Raptor.Storage.StorageMutability.Immutable);
            surveyedSurfacesService.Init(null);
            SurveyedSurfaces surveyedSurfaces = surveyedSurfacesService.List(ID());

            if (surveyedSurfaces != null)
            {
                cmbDesigns.DropDownStyle = ComboBoxStyle.DropDownList;

                //cmbSurveyedSurfaces.Items.Clear();

                cmbSurveyedSurfaces.DisplayMember = "Text";
                cmbSurveyedSurfaces.ValueMember = "Value";
                cmbSurveyedSurfaces.DataSource = surveyedSurfaces.Select(x => new { Text = x.DesignDescriptor.FullPath, Value = x }).ToArray();
            }
        }

        private void DoScreenUpdate()
        {
            DoUpdateLabels();
            DoRender();
        }

        private void ViewPortChange(Action viewPortAction)
        {
            viewPortAction();
            DoScreenUpdate();
        }

        private void ZoomAll_Click(object sender, EventArgs e)
        {
            // Get the project extent so we know where to render
            ViewPortChange(() => extents = GetZoomAllExtents());
            // GetAdjustedDataModelSpatialExtents
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
                extents = GetZoomAllExtents();
                DoUpdateLabels();
                DoUpdateDesignsAndSurveyedSurfaces();
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
                //for (int count = 0; count < nImages; count++)
                //{  
                //    using (Bitmap b = PerformRender(displayMode, width, height, selectEarliestPass, extents))
                //    {
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
                writer.WriteLine($"{count++}:{cacheEntry.Key}, size = {cacheEntry.Value.Length}");
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

            // writeCacheMetrics(writer, cache.GetMetrics());

            var scanQuery = new ScanQuery<SubGridSpatialAffinityKey, byte[]>
            {
                PageSize = 1 // Restrict the number of keys requested in each page to reduce memory consumption
            };

            IQueryCursor<ICacheEntry<SubGridSpatialAffinityKey, byte[]>> queryCursor = cache.Query(scanQuery);

            foreach (ICacheEntry<SubGridSpatialAffinityKey, byte[]> cacheEntry in queryCursor)
            {
                writer.WriteLine(String.Format("{0}:{1}", count++, cacheEntry.Key.ToString()));
                // writeCacheMetrics(writer, cache.GetMetrics());
            }

            writer.WriteLine();
        }

        private void retriveAllItems(string title, StreamWriter writer, ICache<SubGridSpatialAffinityKey, byte[]> cache)
        {
            var scanQuery = new ScanQuery<SubGridSpatialAffinityKey, byte[]>
            {
                PageSize = 1 // Restrict the number of keys requested in each page to reduce memory consumption
            };

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

        private void dumpKeysToFile(StorageMutability mutability, string fileName)
        {
            try
            {
                IIgnite ignite = RaptorGridFactory.Grid(RaptorGrids.RaptorGridName(mutability));

                if (ignite == null)
                {
                    MessageBox.Show($"No ignite reference for {RaptorGrids.RaptorGridName(mutability)} grid");
                    return;
                }

                using (var outFile = new FileStream(fileName, FileMode.Create))
                {
                    using (var writer = new StreamWriter(outFile))
                    {
                        if (mutability == StorageMutability.Immutable)
                        {
                            writeKeys(RaptorCaches.ImmutableNonSpatialCacheName(), writer, ignite.GetCache<string, Byte[]>(RaptorCaches.ImmutableNonSpatialCacheName()));
                            writeKeys(RaptorCaches.DesignTopologyExistenceMapsCacheName(), writer, ignite.GetCache<string, Byte[]>(RaptorCaches.DesignTopologyExistenceMapsCacheName()));
                            writeKeysSpatial(RaptorCaches.ImmutableSpatialCacheName(), writer, ignite.GetCache<SubGridSpatialAffinityKey, Byte[]>(RaptorCaches.ImmutableSpatialCacheName()));
                        }
                        if (mutability == StorageMutability.Mutable)
                        {
                            writeKeys(RaptorCaches.MutableNonSpatialCacheName(), writer, ignite.GetCache<string, Byte[]>(RaptorCaches.MutableNonSpatialCacheName()));
                            writeKeysSpatial(RaptorCaches.MutableSpatialCacheName(), writer, ignite.GetCache<SubGridSpatialAffinityKey, Byte[]>(RaptorCaches.MutableSpatialCacheName()));
                        }
                    }
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.ToString());
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Obtain all the keys and write them into files
            dumpKeysToFile(StorageMutability.Mutable, @"C:\Temp\AllRaptorIgniteCacheKeys = mutable.txt");
            dumpKeysToFile(StorageMutability.Immutable, @"C:\Temp\AllRaptorIgniteCacheKeys = immutable.txt");
        }

        private void chkSelectEarliestPass_CheckedChanged(object sender, EventArgs e)
        {
            DoScreenUpdate();
        }

        private void chkIncludeSurveyedSurfaces_CheckedChanged(object sender, EventArgs e)
        {
            DoScreenUpdate();
        }

        /// <summary>
        /// Calculate statistics across a single cache in the grid
        /// </summary>
        /// <param name="title"></param>
        /// <param name="cache"></param>
        /// <returns></returns>
        private string CalculateCacheStatistics(string title, ICache<Object, byte[]> cache)
        {

            if (cache == null)
            {
                return $"Cache {title} is null";
            }

            var scanQuery = new ScanQuery<Object, byte[]>
            {
                PageSize = 1 // Restrict the number of keys requested in each page to reduce memory consumption
            };

            long count = 0;
            long sumBytes = 0;
            long smallestBytes = long.MaxValue;
            long largestBytes = long.MinValue;

            IQueryCursor<ICacheEntry<Object, byte[]>> queryCursor = cache.Query(scanQuery);

            foreach (ICacheEntry<Object, byte[]> cacheEntry in queryCursor)
            {
                count++;
                sumBytes += cacheEntry.Value.Length;
                if (smallestBytes > cacheEntry.Value.Length) smallestBytes = cacheEntry.Value.Length;
                if (largestBytes < cacheEntry.Value.Length) largestBytes = cacheEntry.Value.Length;
            }

            if (count == 0) return $"No elements in cache {title}";

            return $"Cache {title}: {count} entries totalling {sumBytes} bytes. Average: {sumBytes / count} bytes, smallest: {smallestBytes} bytes, largest: {largestBytes} bytes";
        }

        /// <summary>
        /// Calculate statistics on the numbers and sizes of elements in the major Raptor caches
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("This may take some time...", "Confirmation", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
            {
                return;
            }

            try
            {
                IIgnite ignite = RaptorGridFactory.Grid(RaptorGrids.RaptorMutableGridName());

                if (ignite != null)
                {
                    string result = CalculateCacheStatistics(RaptorCaches.MutableNonSpatialCacheName(), ignite.GetCache<Object, Byte[]>(RaptorCaches.MutableNonSpatialCacheName())) + "\n" +
                                    CalculateCacheStatistics(RaptorCaches.MutableSpatialCacheName(), ignite.GetCache<Object, Byte[]>(RaptorCaches.MutableSpatialCacheName()));
                    MessageBox.Show(result, "Mutable Statistics");
                }
                else
                {
                    MessageBox.Show("No Ignite referece for mutable Statistics");
                }

                ignite = RaptorGridFactory.Grid(RaptorGrids.RaptorImmutableGridName());
                if (ignite != null)
                {
                    string result = CalculateCacheStatistics(RaptorCaches.ImmutableNonSpatialCacheName(), ignite.GetCache<Object, Byte[]>(RaptorCaches.ImmutableNonSpatialCacheName())) + "\n" +
                                    CalculateCacheStatistics(RaptorCaches.ImmutableSpatialCacheName(), ignite.GetCache<Object, Byte[]>(RaptorCaches.ImmutableSpatialCacheName()));
                    MessageBox.Show(result, "Immutable Statistics");
                }
                else
                {
                    MessageBox.Show("No Ignite referece for immutable Statistics");
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.ToString(), "An error occurred");
            }
        }

        /// <summary>
        /// Perform a simple volumes calc based on earliest to latest filters of the viewable screen area.
        /// </summary>
        /// <returns></returns>
        private SimpleVolumesResponse PerformVolume()
        {
            // Get the relevant SiteModel. Use the generic application service server to instantiate the Ignite instance
            // SiteModel siteModel = RaptorGenericApplicationServiceServer.PerformAction(() => SiteModels.Instance().GetSiteModel(ID, false));
            SiteModel siteModel = SiteModels.Instance().GetSiteModel(ID(), false);

            try
            {
                // Create the two filters
                CombinedFilter FromFilter = new CombinedFilter()
                {
                    AttributeFilter = new CellPassAttributeFilter(siteModel)
                    {
                        ReturnEarliestFilteredCellPass = true,
                        HasElevationTypeFilter = true,
                        ElevationType = ElevationType.First,
                        SurveyedSurfaceExclusionList = GetSurveyedSurfaceExclusionList(siteModel),
                    },

                    SpatialFilter = new CellSpatialFilter()
                    {
                        CoordsAreGrid = true,
                        IsSpatial = true,
                        Fence = new Fence(extents)
                    }
                };

                CombinedFilter ToFilter = new CombinedFilter()
                {
                    AttributeFilter = new CellPassAttributeFilter(siteModel)
                    {
                        ReturnEarliestFilteredCellPass = false,
                        HasElevationTypeFilter = true,
                        ElevationType = ElevationType.Last,
                        SurveyedSurfaceExclusionList = GetSurveyedSurfaceExclusionList(siteModel),
                    },

                    SpatialFilter = FromFilter.SpatialFilter
                };

                return simpleVolumesServer.ComputeSimpleVolues(new SimpleVolumesRequestArgument()
                {
                    SiteModelID = ID(),
                    BaseFilter = FromFilter,
                    TopFilter = ToFilter,
                    VolumeType = VolumeComputationType.Between2Filters
                });
            }
            catch (Exception E)
            {
                MessageBox.Show(String.Format("Exception: {0}", E));
                return null;
            }
        }

        /// <summary>
        /// Determine the list of survyeed surfaces that shoudl be excluded depeneding on the
        /// sitemodel and state of relevant UI components
        /// </summary>
        /// <param name="siteModel"></param>
        /// <returns></returns>
        private long[] GetSurveyedSurfaceExclusionList(SiteModel siteModel) => (siteModel.SurveyedSurfaces == null || chkIncludeSurveyedSurfaces.Checked) ? new long[0] : siteModel.SurveyedSurfaces.Select(x => x.ID).ToArray();

        private void btnCalculateVolumes_Click(object sender, EventArgs e)
        {
            // Calculate a simple volume based on a filter to filter, earliest to latest context

            SimpleVolumesResponse volume = PerformVolume();

            if (volume == null)
            {
                MessageBox.Show("Volume retuned no response");
                return;
            }

            MessageBox.Show($"Simple Volume Response:\n{volume}");
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Make a test queue object and see how it goes
            TestQueueHolder queue = new TestQueueHolder();

            IEnumerable<TestQueueItem> result = queue.Query(DateTime.Now);

            if (result?.Count() > 0)
            {
                MessageBox.Show($"Items: {result.Aggregate("", (accumulator, item) => accumulator + item.Value + " ")}");
            }
            else
            {
                MessageBox.Show("Query result is null or empty");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            // Test adding a stream for "5-ProductionDataModel.XML" to the mutable non-spatial cache 

            IIgnite ignite = Ignition.GetIgnite(RaptorGrids.RaptorMutableGridName());

            ICache<string, byte[]> cache = ignite.GetCache<string, byte[]>(RaptorCaches.MutableNonSpatialCacheName());

            byte[] bytes = new byte[100];

            string cacheKey = "50-ProductionDataModel.XML";

            try
            {
                cache.Put(cacheKey, bytes);

                byte[] readBytes = cache.Get(cacheKey);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception: {ex}");
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            // Calculate cut fill statistics from the latest elevations to the selected design

            var request = new CutFillOperation();
            var siteModel = SiteModels.Instance().GetSiteModel(ID(), false);
            var offsets = new double[7] { 0.5, 0.2, 0.1, 0, -0.1, -0.2, -0.5 };

            CutFillResult result = request.Execute(new CutFillStatisticsArgument()
            {
                DataModelID = siteModel.ID,
                Filter = new CombinedFilter()
                {
                    AttributeFilter = new CellPassAttributeFilter(siteModel),
                    SpatialFilter = new CellSpatialFilter()
                },
                DesignID = (cmbDesigns.Items.Count == 0) ? long.MinValue : (cmbDesigns.SelectedValue as Design).ID,
                Offsets = offsets
            });

            // Show the list of percentages calculated by the request
            MessageBox.Show($"Results [Cut/Fill:{offsets.Aggregate("", (a, v) => a + $"{ v.ToString("F1")}, ")}]: {result?.Percents?.Aggregate("", (a, v) => a + $"{v.ToString("F1")}% ")}");
        }
    }
}
