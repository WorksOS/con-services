using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Query;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VSS.TRex.Analytics.CMVStatistics;
using VSS.TRex.Analytics.CMVStatistics.GridFabric;
using VSS.TRex.Rendering.Implementations.Framework.GridFabric.Responses;
using VSS.TRex.TAGFiles.Classes.Queues;
using VSS.TRex.TAGFiles.GridFabric.Arguments;
using VSS.TRex.TAGFiles.GridFabric.Requests;
using VSS.TRex.DesignProfiling;
using VSS.TRex.Analytics.Operations;
using VSS.TRex.Analytics.GridFabric.Arguments;
using VSS.TRex.Analytics.MDPStatistics;
using VSS.TRex.Analytics.MDPStatistics.GridFabric;
using VSS.TRex.Analytics.Models;
using VSS.TRex.Analytics.PassCountStatistics;
using VSS.TRex.Analytics.PassCountStatistics.GridFabric;
using VSS.TRex.Analytics.SpeedStatistics;
using VSS.TRex.Analytics.SpeedStatistics.GridFabric;
using VSS.TRex.Analytics.TemperatureStatistics;
using VSS.TRex.Analytics.TemperatureStatistics.GridFabric;
using VSS.TRex.Designs;
using VSS.TRex.Designs.Storage;
using VSS.TRex.Executors;
using VSS.TRex.Exports.Patches;
using VSS.TRex.Exports.Patches.GridFabric;
using VSS.TRex.Filters;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.GridFabric.Caches;
using VSS.TRex.GridFabric.Events;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Queues;
using VSS.TRex.Profiling.GridFabric.Arguments;
using VSS.TRex.Profiling.GridFabric.Responses;
using VSS.TRex.Profiling.Servers.Client;
using VSS.TRex.Rendering.GridFabric.Arguments;
using VSS.TRex.Rendering.Servers.Client;
using VSS.TRex.Servers;
using VSS.TRex.Servers.Client;
using VSS.TRex.Services.Designs;
using VSS.TRex.Services.Surfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage;
using VSS.TRex.Surfaces;
using VSS.TRex.TAGFiles.Classes;
using VSS.TRex.TAGFiles.Classes.Validator;
using VSS.TRex.Types;
using VSS.TRex.Volumes;
using VSS.TRex.Volumes.GridFabric.Arguments;
using VSS.TRex.Volumes.GridFabric.Responses;

namespace VSS.TRex.IgnitePOC.TestApp
{
	public partial class Form1 : Form
	{
		BoundingWorldExtent3D extents = BoundingWorldExtent3D.Inverted();

		//GenericApplicationServiceServer genericApplicationServiceServer = new GenericApplicationServiceServer();
		TileRenderingServer tileRenderServer;
		SimpleVolumesServer simpleVolumesServer;
		MutableClientServer mutableClient;
	  ProfilingServer profilingServer;

		SiteModelAttributesChangedEventListener SiteModelAttrubutesChanged;

		/// <summary>
		/// Convert the Project ID in the text box into a number. It if is invalid return project ID 2 as a default
		/// </summary>
		/// <returns></returns>
		private Guid ID()
		{
			try
			{
					return Guid.Parse(editProjectID.Text);
			}
			catch
			{
					return Guid.Empty;
			}
		}

		private System.Drawing.Bitmap PerformRender(DisplayMode displayMode, int width, int height, bool returnEarliestFilteredCellPass, BoundingWorldExtent3D extents)
		{
				// Get the relevant SiteModel. Use the generic application service server to instantiate the Ignite instance
				// SiteModel siteModel = GenericApplicationServiceServer.PerformAction(() => SiteModels.Instance().GetSiteModel(ID, false));
        ISiteModel siteModel = SiteModels.SiteModels.Instance().GetSiteModel(ID(), false);

				if (siteModel == null)
				{
						MessageBox.Show($"Site model {ID()} is unavailable");
						return null;
				}

				try
				{
						CellPassAttributeFilter AttributeFilter = new CellPassAttributeFilter
						{
								ReturnEarliestFilteredCellPass = returnEarliestFilteredCellPass,
								HasElevationTypeFilter = true,
								ElevationType = returnEarliestFilteredCellPass ? ElevationType.First : ElevationType.Last,
								SurveyedSurfaceExclusionList = GetSurveyedSurfaceExclusionList(siteModel)
						};

						CellSpatialFilter SpatialFilter = new CellSpatialFilter
						{
								CoordsAreGrid = true,
								IsSpatial = true,
								Fence = new Fence(extents)
						};

						TileRenderResponse_Framework response = tileRenderServer.RenderTile(new TileRenderRequestArgument
						(ID(),
							displayMode,
							extents,
							true, // CoordsAreGrid
							(ushort)width, // PixelsX
							(ushort)height, // PixelsY
							new CombinedFilter(AttributeFilter, SpatialFilter), // Filter1
							null, // filter 2
							(cmbDesigns.Items.Count == 0) ? Guid.Empty : (cmbDesigns.SelectedValue as Design).ID// DesignDescriptor
						)) as TileRenderResponse_Framework;

        //TEST: compute profile first (reduces churn in other branches
				 // PerformProfile();

						return response?.TileBitmap;
				}
				catch (Exception E)
				{
						MessageBox.Show($"Exception: {E}");
						return null;
				}
		}

	  private void PerformProfile()
	  {
	    ProfileRequestArgument_ApplicationService arg = new ProfileRequestArgument_ApplicationService
	    {
	      ProjectID = ID(),
	      ProfileTypeRequired = GridDataType.Height,
	      PositionsAreGrid = true,
	      Filters = new FilterSet(new[] {new CombinedFilter()}),
	      CutFillDesignID = Guid.Empty,
	      StartPoint = new WGS84Point(lon: extents.MinX, lat: extents.MinY),
	      EndPoint = new WGS84Point(lon: extents.MaxX, lat: extents.MaxY),
	      ReturnAllPassesAndLayers = false,
	      DesignDescriptor = DesignDescriptor.Null()
	    };

      // Compute a profile from the bottom left of the screen extents to the top right 
      ProfileRequestResponse Response = profilingServer.ComputeProfile(arg);

	    if (Response == null)
	      MessageBox.Show("Profile response is null");
	    else
	    if (Response.ProfileCells == null)
	      MessageBox.Show("Profile response contains no profile cells");
	    else
	      MessageBox.Show($"Profile line returned a profile result of {Response?.ResultStatus} and {Response?.ProfileCells?.Count ?? 0} cells");
	  }

	  private BoundingWorldExtent3D GetZoomAllExtents()
		{
        ISiteModel siteModel = SiteModels.SiteModels.Instance().GetSiteModel(ID(), false);

				if (siteModel != null)
				{
						Guid[] SurveyedSurfaceExclusionList = (siteModel.SurveyedSurfaces == null || chkIncludeSurveyedSurfaces.Checked) ? new Guid[0] : siteModel.SurveyedSurfaces.Select(x => x.ID).ToArray();

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

				tileRenderServer = TileRenderingServer.NewInstance(new[] { ApplicationServiceServer.DEFAULT_ROLE_CLIENT, ServerRoles.TILE_RENDERING_NODE });
				simpleVolumesServer = SimpleVolumesServer.NewInstance(new [] { ApplicationServiceServer.DEFAULT_ROLE_CLIENT });
				mutableClient = new MutableClientServer("TestApplication");
        profilingServer = ProfilingServer.NewInstance(new[] { ApplicationServiceServer.DEFAULT_ROLE_CLIENT, ServerRoles.ASNODE_PROFILER});

				// Instantiate a site model changed listener to catch changes to site model attributes
				SiteModelAttrubutesChanged = new SiteModelAttributesChangedEventListener(TRexGrids.ImmutableGridName());
		}

		private void Form1_Load(object sender, EventArgs e)
		{
		}

		private void fitExtentsToView(int width, int height)
		{
				double Aspect = height / (double)width;

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

				System.Drawing.Bitmap bmp = PerformRender((DisplayMode)displayMode.SelectedIndex, pictureBox1.Width, pictureBox1.Height, chkSelectEarliestPass.Checked, extents);

				if (bmp != null)
				{
						bmp.Save(@"C:\temp\renderedtile.bmp", ImageFormat.Bmp);
						pictureBox1.Image = bmp;
						pictureBox1.Show();
				}
		}

		private void DoUpdateLabels()
		{
				lblViewHeight.Text = $"View height: {extents.SizeY:F3}m";
				lblViewWidth.Text = $"View width: {extents.SizeX:F3}m";
				lblCellsPerPixel.Text = $"Cells Per Pixel (X): {(extents.SizeX / pictureBox1.Width) / 0.34:F3}";
		}

		private void DoUpdateDesignsAndSurveyedSurfaces()
		{
				Designs.Storage.Designs designs = DesignsService.Instance().List(ID());

				if (designs != null)
				{
						cmbDesigns.DropDownStyle = ComboBoxStyle.DropDownList;

						//cmbDesigns.Items.Clear();

						cmbDesigns.DisplayMember = "Text";
						cmbDesigns.ValueMember = "Value";
						cmbDesigns.DataSource = designs.Select(x => new { Text = x.DesignDescriptor.FullPath, Value = x }).ToArray();
				}

				SurveyedSurfaceService surveyedSurfacesService = new SurveyedSurfaceService(StorageMutability.Immutable);
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
			Cursor.Current = Cursors.WaitCursor;
			viewPortAction();
			DoScreenUpdate();
			Cursor.Current = Cursors.Default;
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
				if (ID() != Guid.Empty)
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
								using (System.Drawing.Bitmap b = PerformRender(displayMode, width, height, selectEarliestPass, extents))
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

						results.Append($"Run {i}: Images:{nImages}, Time:{sw.Elapsed}\n");
				}

				MessageBox.Show($"Results:\n{results.ToString()}");
				//MessageBox.Show(String.Format("Images:{0}, Time:{1}", nImages, sw.Elapsed));
		}

		private void writeCacheMetrics(StreamWriter writer, ICacheMetrics metrics)
		{
				writer.WriteLine($"Number of items in cache: {metrics.Size}");
		}

		private void writeKeys(string title, StreamWriter writer, ICache<NonSpatialAffinityKey, byte[]> cache)
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

				var scanQuery = new ScanQuery<NonSpatialAffinityKey, byte[]>();
				IQueryCursor<ICacheEntry<NonSpatialAffinityKey, byte[]>> queryCursor = cache.Query(scanQuery);
				scanQuery.PageSize = 1; // Restrict the number of keys requested in each page to reduce memory consumption

				foreach (ICacheEntry<NonSpatialAffinityKey, byte[]> cacheEntry in queryCursor)
				{
						writer.WriteLine($"{count++}:{cacheEntry.Key}, size = {cacheEntry.Value.Length}");
						//                writeCacheMetrics(writer, cache.GetMetrics());
				}

				writer.WriteLine();
		}

		private void writeTAGFileBufferQueueKeys(string title, StreamWriter writer, ICache<TAGFileBufferQueueKey, TAGFileBufferQueueItem> cache)
		{
				int count = 0;

				writer.WriteLine(title);
				writer.WriteLine("#####################");
				writer.WriteLine();

				if (cache == null)
				{
						return;
				}

				var scanQuery = new ScanQuery<TAGFileBufferQueueKey, TAGFileBufferQueueItem>();
				IQueryCursor<ICacheEntry<TAGFileBufferQueueKey, TAGFileBufferQueueItem>> queryCursor = cache.Query(scanQuery);
				scanQuery.PageSize = 1; // Restrict the number of keys requested in each page to reduce memory consumption

				foreach (ICacheEntry<TAGFileBufferQueueKey, TAGFileBufferQueueItem> cacheEntry in queryCursor)
				{
						writer.WriteLine($"{count++}:{cacheEntry.Key}, size = {cacheEntry.Value.Content.Length}");
				}

				writer.WriteLine();
		}

		private void WriteKeysSpatial(string title, StreamWriter writer, ICache<SubGridSpatialAffinityKey, byte[]> cache)
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
						writer.WriteLine($"{count++}:{cacheEntry.Key}, size = {cacheEntry.Value.Length}");
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
						IIgnite ignite = TRexGridFactory.Grid(TRexGrids.GridName(mutability));

						if (ignite == null)
						{
								MessageBox.Show($"No ignite reference for {TRexGrids.GridName(mutability)} grid");
								return;
						}

						using (var outFile = new FileStream(fileName, FileMode.Create))
						{
								using (var writer = new StreamWriter(outFile))
								{
										if (mutability == StorageMutability.Immutable)
										{
												try
												{
														writeKeys(TRexCaches.ImmutableNonSpatialCacheName(), writer, ignite.GetCache<NonSpatialAffinityKey, byte[]>(TRexCaches.ImmutableNonSpatialCacheName()));
												}
												catch (Exception E)
												{
                            writer.WriteLine($"Exception occurred: {E.Message}");
												}
												try
												{
														writeKeys(TRexCaches.DesignTopologyExistenceMapsCacheName(), writer, ignite.GetCache<NonSpatialAffinityKey, byte[]>(TRexCaches.DesignTopologyExistenceMapsCacheName()));
												}
												catch (Exception E)
												{
												  writer.WriteLine($"Exception occurred: {E.Message}");
												}
                        try
												{
														WriteKeysSpatial(TRexCaches.ImmutableSpatialCacheName(), writer, ignite.GetCache<SubGridSpatialAffinityKey, byte[]>(TRexCaches.ImmutableSpatialCacheName()));
												}
												catch (Exception E)
												{
												  writer.WriteLine($"Exception occurred: {E.Message}");
												}
                    }
										if (mutability == StorageMutability.Mutable)
										{
												try
												{
														writeKeys(TRexCaches.MutableNonSpatialCacheName(), writer, ignite.GetCache<NonSpatialAffinityKey, byte[]>(TRexCaches.MutableNonSpatialCacheName()));
												}
												catch (Exception E)
												{
												  writer.WriteLine($"Exception occurred: {E.Message}");
												}
                        try
												{
														WriteKeysSpatial(TRexCaches.MutableSpatialCacheName(), writer, ignite.GetCache<SubGridSpatialAffinityKey, byte[]>(TRexCaches.MutableSpatialCacheName()));
												}
												catch (Exception E)
												{
												  writer.WriteLine($"Exception occurred: {E.Message}");
												}
                        try
												{
														writeTAGFileBufferQueueKeys(TRexCaches.TAGFileBufferQueueCacheName(), writer, ignite.GetCache<TAGFileBufferQueueKey, TAGFileBufferQueueItem>(TRexCaches.TAGFileBufferQueueCacheName()));
												}
												catch (Exception E)
												{
												  writer.WriteLine($"Exception occurred: {E.Message}");
												}
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
				dumpKeysToFile(StorageMutability.Mutable, @"C:\Temp\AllTRexIgniteCacheKeys = mutable.txt");
				dumpKeysToFile(StorageMutability.Immutable, @"C:\Temp\AllTRexIgniteCacheKeys = immutable.txt");
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
		private string CalculateCacheStatistics(string title, ICache<object, byte[]> cache)
		{

				if (cache == null)
				{
						return $"Cache {title} is null";
				}

				var scanQuery = new ScanQuery<object, byte[]>
				{
						PageSize = 1 // Restrict the number of keys requested in each page to reduce memory consumption
				};

				long count = 0;
				long sumBytes = 0;
				long smallestBytes = long.MaxValue;
				long largestBytes = long.MinValue;

				IQueryCursor<ICacheEntry<object, byte[]>> queryCursor = cache.Query(scanQuery);

				foreach (ICacheEntry<object, byte[]> cacheEntry in queryCursor)
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
		/// Calculate statistics on the numbers and sizes of elements in the major TRex caches
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
						IIgnite ignite = TRexGridFactory.Grid(TRexGrids.MutableGridName());

						if (ignite != null)
						{
								string result = CalculateCacheStatistics(TRexCaches.MutableNonSpatialCacheName(), ignite.GetCache<object, byte[]>(TRexCaches.MutableNonSpatialCacheName())) + "\n" +
																CalculateCacheStatistics(TRexCaches.MutableSpatialCacheName(), ignite.GetCache<object, byte[]>(TRexCaches.MutableSpatialCacheName()));
								MessageBox.Show(result, "Mutable Statistics");
						}
						else
						{
								MessageBox.Show("No Ignite referece for mutable Statistics");
						}

						ignite = TRexGridFactory.Grid(TRexGrids.ImmutableGridName());
						if (ignite != null)
						{
								string result = CalculateCacheStatistics(TRexCaches.ImmutableNonSpatialCacheName(), ignite.GetCache<object, byte[]>(TRexCaches.ImmutableNonSpatialCacheName())) + "\n" +
																CalculateCacheStatistics(TRexCaches.ImmutableSpatialCacheName(), ignite.GetCache<object, byte[]>(TRexCaches.ImmutableSpatialCacheName()));
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
		private SimpleVolumesResponse PerformVolume(bool useScreenExtents)
		{
				// Get the relevant SiteModel. Use the generic application service server to instantiate the Ignite instance
				// SiteModel siteModel = GenericApplicationServiceServer.PerformAction(() => SiteModels.Instance().GetSiteModel(ID, false));
            ISiteModel siteModel = SiteModels.SiteModels.Instance().GetSiteModel(ID(), false);

				try
				{
						// Create the two filters
						CombinedFilter FromFilter = new CombinedFilter()
						{
								AttributeFilter = new CellPassAttributeFilter(/*siteModel*/)
								{
										ReturnEarliestFilteredCellPass = true,
										HasElevationTypeFilter = true,
										ElevationType = ElevationType.First,
										SurveyedSurfaceExclusionList = GetSurveyedSurfaceExclusionList(siteModel),
								},

								SpatialFilter = !useScreenExtents 
								? new CellSpatialFilter()
								: new CellSpatialFilter
								{
										CoordsAreGrid = true,
										IsSpatial = true,
										Fence = new Fence(extents)
								}
						};

						CombinedFilter ToFilter = new CombinedFilter()
						{
								AttributeFilter = new CellPassAttributeFilter(/*siteModel*/)
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
						MessageBox.Show($@"Exception: {E}");
						return null;
				}
		}

		/// <summary>
		/// Determine the list of survyeed surfaces that shoudl be excluded depeneding on the
		/// sitemodel and state of relevant UI components
		/// </summary>
		/// <param name="siteModel"></param>
		/// <returns></returns>
        private Guid[] GetSurveyedSurfaceExclusionList(ISiteModel siteModel) => (siteModel.SurveyedSurfaces == null || chkIncludeSurveyedSurfaces.Checked) ? new Guid[0] : siteModel.SurveyedSurfaces.Select(x => x.ID).ToArray();

		private void btnCalculateVolumes_Click(object sender, EventArgs e)
		{
	// Calculate a simple volume based on a filter to filter, earliest to latest context, for the visible extents on the screen
				Cursor.Current = Cursors.WaitCursor;
				SimpleVolumesResponse volume = PerformVolume(true);

				if (volume == null)
				{
						MessageBox.Show("Volume retuned no response");
						return;
				}
			Cursor.Current = Cursors.Default;

			MessageBox.Show($"Simple Volume Response [Screen Extents]:\n{volume}");
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
				// Test adding a stream for "<NewID>-ProductionDataModel.XML" to the mutable non-spatial cache 

				IIgnite ignite = Ignition.GetIgnite(TRexGrids.MutableGridName());

				ICache<NonSpatialAffinityKey, byte[]> cache = ignite.GetCache<NonSpatialAffinityKey, byte[]>(TRexCaches.MutableNonSpatialCacheName());

				byte[] bytes = new byte[100];

				NonSpatialAffinityKey cacheKey = new NonSpatialAffinityKey(Guid.NewGuid(), "ProductionDataModel.XML");

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
			var siteModel = SiteModels.SiteModels.Instance().GetSiteModel(ID(), false);
			var offsets = new [] { 0.5, 0.2, 0.1, 0, -0.1, -0.2, -0.5 };

			Stopwatch sw = new Stopwatch();
			sw.Start();

			CutFillOperation operation = new CutFillOperation();
			CutFillResult result = operation.Execute(new CutFillStatisticsArgument()
			{
			    ProjectID = siteModel.ID,
					Filters = new FilterSet {Filters = new [] { new CombinedFilter() } },
					DesignID = (cmbDesigns.Items.Count == 0) ? Guid.Empty : (cmbDesigns.SelectedValue as Design).ID,
					Offsets = offsets
			});

			// Show the list of percentages calculated by the request
			MessageBox.Show($"Results (in {sw.Elapsed}) [Cut/Fill:{offsets.Aggregate("", (a, v) => a + $"{ v.ToString("F1")}, ")}]: {(result?.Percents == null ? "No Result" : result.Percents?.Aggregate("", (a, v) => a + $"{v.ToString("F1")}% "))}");
		}

		private void TemperatureSummaryButton_Click(object sender, EventArgs e)
		{
			var siteModel = SiteModels.SiteModels.Instance().GetSiteModel(ID(), false);

			Stopwatch sw = new Stopwatch();
			sw. Start();
			try
			{
				TemperatureOperation operation = new TemperatureOperation();
				TemperatureResult result = operation.Execute(
					new TemperatureStatisticsArgument()
					{
					  ProjectID = siteModel.ID, 
						Filters = new FilterSet() { Filters = new []{new CombinedFilter() }},
						OverrideTemperatureWarningLevels = true,
						OverridingTemperatureWarningLevels = new TemperatureWarningLevelsRecord(10, 150)
					}
				);

				if (result != null)
					MessageBox.Show($"Temperature Summary Results (in {sw.Elapsed}) :\n " +
					                $"Minimum Temperature: {result.MinimumTemperature} \n " +
					                $"Maximum Temperature: {result.MaximumTemperature} \n " +
													$"Above Temperature Percentage: {result.AboveTargetPercent} \n " +
													$"Within Temperature Percentage Range: {result.WithinTargetPercent} \n " +
													$"Below Temperature Percentage: {result.BelowTargetPercent} \n " +
													$"Total Area Covered in Sq Meters: {result.TotalAreaCoveredSqMeters} \n " +
													$"Is Target Temperature Constant: {result.IsTargetTemperatureConstant}");
			}
			finally
			{
				sw.Stop();
			}
		}

		private void button6_Click(object sender, EventArgs e)
		{
			// Tests the time taken to perform 10,000 full TTM patch requests for a test design, both locally, and by accessing a 
			// remote DesignElevation service

			TTMDesign design = new TTMDesign(SubGridTree.DefaultCellSize);
			design.LoadFromFile(@"C:\Temp\Bug36372.ttm");
			const int numPatches = 10000;

			float[,] Patch = new float[SubGridTree.SubGridTreeDimension, SubGridTree.SubGridTreeDimension];

			Stopwatch sw = new Stopwatch();
			sw.Start();

			for (int i = 0; i < numPatches; i++)
			{
					bool result = design.InterpolateHeights(Patch, 247500.0, 193350.0, SubGridTree.DefaultCellSize, 0);
			}

			MessageBox.Show($"{numPatches} patches requested in {sw.Elapsed}, {(numPatches * 1024.0) / (sw.ElapsedMilliseconds / 1000.0)} per second");

			Design ttmDesign = new Design(Guid.Empty, new DesignDescriptor(Guid.Empty, "", "", @"C:\Temp\", "Bug36372.ttm", 0.0), extents);
			sw.Reset();
			sw.Start();

			for (int i = 0; i < numPatches; i++)
			{
					bool result = design.InterpolateHeights(Patch, 247500.0, 193350.0, SubGridTree.DefaultCellSize, 0);
			}

			MessageBox.Show($"{numPatches} patches requested in {sw.Elapsed}, {(numPatches * 1024.0) / (sw.ElapsedMilliseconds / 1000.0)} per second");
		}

		private void btnRedraw_Click_1(object sender, EventArgs e)
		{

		}

		private void btnCalcAll_Click(object sender, EventArgs e)
		{
				// Calculate a simple volume based on a filter to filter, earliest to latest context for the entire model
				Cursor.Current = Cursors.WaitCursor;
				SimpleVolumesResponse volume = PerformVolume(false);

				if (volume == null)
				{
						MessageBox.Show("Volume retuned no response");
						return;
				}
				Cursor.Current = Cursors.Default;

				MessageBox.Show($"Simple Volume Response [Model Extents]:\n{volume}");
		}

		private void btnFileOpen_Click(object sender, EventArgs e)
		{
				if (openFileDialog1.ShowDialog() == DialogResult.OK)
				{
						this.edtTagfile.Text = openFileDialog1.FileName;
				}
		}

		private void btnSubmitTagFile_Click(object sender, EventArgs e)
		{
				//  Machine machine = new Machine(null, "TestName", "TestHardwareID", 0, 0, Guid.NewGuid(), 0, false);

				if (this.edtTagfile.Text == string.Empty)
				{
						MessageBox.Show("Missing tagfile");
						return;
				}
				try
				{
						SubmitTAGFileRequest request = new SubmitTAGFileRequest();
						SubmitTAGFileRequestArgument arg = null;
						string fileName = this.edtTagfile.Text;
						//  "J:\\PP\\Construction\\Office software\\SiteVision Office\\Test Files\\VisionLink Data\\Southern Motorway\\TAYLORS COMP\\IgniteTestData\\0201J004SV--TAYLORS COMP--110504215856.tag";


						Guid TheProject = (this.edtProjectID.Text == String.Empty) ? Guid.Empty : Guid.Parse(this.edtProjectID.Text);
						Guid TheAsset = (this.edtAssetID.Text == String.Empty) ? Guid.Empty : Guid.Parse(this.edtAssetID.Text);
						string TheFileName = Path.GetFileName(fileName);
						string tccOrgID = this.edtTCCOrgID.Text; // maybe it could have been guid

						using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
						{
								byte[] bytes = new byte[fs.Length];
								fs.Read(bytes, 0, bytes.Length);

								arg = new SubmitTAGFileRequestArgument()
											{
															ProjectID = TheProject,//ID(),
															AssetID = TheAsset,
															TAGFileName = TheFileName,
															TagFileContent = bytes,
															TCCOrgID = tccOrgID

											};

						}

						var res = request.Execute(arg);
						MessageBox.Show(String.Format("Submission Result:{0}, File:{1}, ErrorMessage:{2}", res.Success,res.FileName, res.Exception));

				}
				catch (Exception exception)
				{
						MessageBox.Show(exception.Message);
						throw;
				}


		}

		private void btnGenGUID_Click(object sender, EventArgs e)
		{
				this.edtAssetID.Text = Guid.NewGuid().ToString();
		}

		private void btnGenGuid2_Click(object sender, EventArgs e)
		{
				this.edtTCCOrgID.Text = Guid.NewGuid().ToString();

		}

		private void btnGenGuid3_Click(object sender, EventArgs e)
		{
				this.edtProjectID.Text = Guid.NewGuid().ToString();
		}

		private void btnCopyGuid_Click(object sender, EventArgs e)
		{
				this.edtProjectID.Text = this.editProjectID.Text;
		}

		private void btnCustom_Click(object sender, EventArgs e)
		{
				// create some empty guids to customise
				this.edtAssetID.Text = new Guid().ToString();
				this.edtTCCOrgID.Text = new Guid().ToString();
				this.edtProjectID.Text = new Guid().ToString();
		}

        private void btnGetMetaData_Click(object sender, EventArgs e)
        {


            if (this.edtTagfile.Text == string.Empty)
            {
                MessageBox.Show("Missing tagfile");
                return;
            }
            try
            {
                string fileName = this.edtTagfile.Text;
                Guid TheProject = (this.edtProjectID.Text == String.Empty) ? Guid.Empty : Guid.Parse(this.edtProjectID.Text);
                Guid TheAsset = (this.edtAssetID.Text == String.Empty) ? Guid.Empty : Guid.Parse(this.edtAssetID.Text);
                string TheFileName = Path.GetFileName(fileName);

                TagFileDetail td = new TagFileDetail()
                                   {
                                           projectId = TheProject,
                                           assetId = TheAsset,
                                           tagFileName = TheFileName,
                                           tccOrgId = "",
                                           tagFileContent = new byte[0]
                                  };

                td = TagFileRepository.GetTagfile(td);
                MessageBox.Show(String.Format("ProjectID:{0}, Asset:{1}, TCCOrg:{2},IsJohnDoe:{3}, FileLenght:{4}",
                        td.projectId, td.assetId,td.tccOrgId,td.IsJohnDoe,td.tagFileContent.Length));

            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
                throw;
            }


        }

      /// <summary>
      /// Test a request for patches.
      /// 1. Make the first request to get the total number of pageas as well as the first 10 seubgrids
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
    private void button7_Click(object sender, EventArgs e)
    {
      PatchRequestServer server = new PatchRequestServer();

      PatchResult result = server.Execute(new PatchRequestArgument()
      {
        ProjectID = ID(),
        DataPatchNumber = 0,
        DataPatchSize = 10,
        Mode = DisplayMode.Height,
        Filters = new FilterSet(new [] {new CombinedFilter() })
      });

      MessageBox.Show($"Patch response: Total pages required: {result.TotalNumberOfPagesToCoverFilteredData}, PageSize: {result.MaxPatchSize}, Page number {result.PatchNumber}, Number of subgrids in patch: {result.Patch.Length}");
	}

    private void btnKill_Click(object sender, EventArgs e)
    {
      foreach (var process in Process.GetProcessesByName("TRexTileServer"))
      {
        process.Kill();
      }

      foreach (var process in Process.GetProcessesByName("TRexGridActivator"))
      {
        process.Kill();
      }
      foreach (var process in Process.GetProcessesByName("TRexDesignElevationsServer"))
      {
        process.Kill();
      }
      foreach (var process in Process.GetProcessesByName("TRexImmutableDataServer"))
      {
        process.Kill();
      }
      foreach (var process in Process.GetProcessesByName("TRexMutableDataServer"))
      {
        process.Kill();
      }


    }

    private void btnEmpty_Click(object sender, EventArgs e)
    {
      editProjectID.Text = new Guid().ToString();
    }

    private void SpeedSummaryButton_Click(object sender, EventArgs e)
    {
      var siteModel = SiteModels.SiteModels.Instance().GetSiteModel(ID(), false);

      Stopwatch sw = new Stopwatch();
      sw.Start();
      try
      {
        SpeedOperation operation = new SpeedOperation();
        SpeedResult result = operation.Execute(
          new SpeedStatisticsArgument()
          {
            ProjectID = siteModel.ID,
            Filters = new FilterSet() { Filters = new[] { new CombinedFilter() } },
            TargetMachineSpeed = new MachineSpeedExtendedRecord(5, 50)
          }
        );

        if (result != null)
          MessageBox.Show($"Machine Speed Summary Results (in {sw.Elapsed}) :\n " +
                          $"Above Machine Speed Percentage: {result.AboveTargetPercent} \n " +
                          $"Within Machine Speed Percentage Range: {result.WithinTargetPercent} \n " +
                          $"Below Machine Speed Percentage: {result.BelowTargetPercent} \n " +
                          $"Total Area Covered in Sq Meters: {result.TotalAreaCoveredSqMeters}");
      }
      finally
      {
        sw.Stop();
      }
    }

    private void CMVSummaryButton_Click(object sender, EventArgs e)
    {
      var siteModel = SiteModels.SiteModels.Instance().GetSiteModel(ID(), false);

      Stopwatch sw = new Stopwatch();
      sw.Start();
      try
      {
        CMVOperation operation = new CMVOperation();
        CMVResult result = operation.Execute(
          new CMVStatisticsArgument()
          {
            ProjectID = siteModel.ID,
            Filters = new FilterSet() { Filters = new[] { new CombinedFilter() } },
            CMVPercentageRange = new CMVRangePercentageRecord(80, 120),
            OverrideMachineCMV = false,
            OverridingMachineCMV = 50
          }
        );

        if (result != null)
          MessageBox.Show($"CMV Summary Results (in {sw.Elapsed}) :\n " +
                          $"Above CMV Percentage: {result.AboveTargetPercent} \n " +
                          $"Within CMV Percentage Range: {result.WithinTargetPercent} \n " +
                          $"Below CMV Percentage: {result.BelowTargetPercent} \n " +
                          $"Total Area Covered in Sq Meters: {result.TotalAreaCoveredSqMeters}");
      }
      finally
      {
        sw.Stop();
      }
    }

    private void MDPSummaryButton_Click(object sender, EventArgs e)
    {
      var siteModel = SiteModels.SiteModels.Instance().GetSiteModel(ID(), false);

      Stopwatch sw = new Stopwatch();
      sw.Start();
      try
      {
        MDPOperation operation = new MDPOperation();
        MDPResult result = operation.Execute(
          new MDPStatisticsArgument()
          {
            ProjectID = siteModel.ID,
            Filters = new FilterSet() { Filters = new[] { new CombinedFilter() } },
            MDPPercentageRange = new MDPRangePercentageRecord(80, 120),
            OverrideMachineMDP = false,
            OverridingMachineMDP = 1000
          }
        );

        if (result != null)
          MessageBox.Show($"MDP Summary Results (in {sw.Elapsed}) :\n " +
                          $"Above MDP Percentage: {result.AboveTargetPercent} \n " +
                          $"Within MDP Percentage Range: {result.WithinTargetPercent} \n " +
                          $"Below MDP Percentage: {result.BelowTargetPercent} \n " +
                          $"Total Area Covered in Sq Meters: {result.TotalAreaCoveredSqMeters}");
      }
      finally
      {
        sw.Stop();
      }
    }

    private void PassCountSummaryButton_Click(object sender, EventArgs e)
    {
      var siteModel = SiteModels.SiteModels.Instance().GetSiteModel(ID(), false);

      Stopwatch sw = new Stopwatch();
      sw.Start();
      try
      {
        PassCountOperation operation = new PassCountOperation();
        PassCountResult result = operation.Execute(
          new PassCountStatisticsArgument()
          {
            ProjectID = siteModel.ID,
            Filters = new FilterSet() { Filters = new[] { new CombinedFilter() } },
            OverridingTargetPassCountRange = new PassCountRangeRecord(3, 10),
            OverrideTargetPassCount = false
          }
        );

        if (result != null)
          MessageBox.Show($"Pass Count Summary Results (in {sw.Elapsed}) :\n " +
                          $"Above Pass Count Percentage: {result.AboveTargetPercent} \n " +
                          $"Within Pass Count Percentage Range: {result.WithinTargetPercent} \n " +
                          $"Below Pass Count Percentage: {result.BelowTargetPercent} \n " +
                          $"Total Area Covered in Sq Meters: {result.TotalAreaCoveredSqMeters}");
      }
      finally
      {
        sw.Stop();
      }
    }
  }
}
