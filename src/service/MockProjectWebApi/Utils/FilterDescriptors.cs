using MockProjectWebApi.Json;
using VSS.MasterData.Models.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace MockProjectWebApi.Utils
{
  public class FilterDescriptors
  {
    public class GoldenDimensions
    {
      public static FilterDescriptor ProjectExtentsFilter => new FilterDescriptor
      {
        FilterUid = "5e089924-98cb-49a6-8323-19537dc6d665",
        Name = "Golden Dimensions Project Extents Filter",
        FilterJson = JsonResourceHelper.GetGoldenDimensionsFilterJson("ProjectExtentsFilter")
      };

      public static FilterDescriptor ProjectExtentsFilterElevationTypeFirst => new FilterDescriptor
      {
        FilterUid = "f4e9b4dd-e8c4-4edb-b9aa-59a209c17de7",
        Name = "Golden Dimensions Project Extents Filter",
        FilterJson = JsonResourceHelper.GetGoldenDimensionsFilterJson("ProjectExtentsFilterElevationTypeFirst")
      };

      public static FilterDescriptor ProjectExtentsFilterElevationTypeLast => new FilterDescriptor
      {
        FilterUid = "7730ea54-6c6f-4450-ae94-1933471d7961",
        Name = "Golden Dimensions Project Extents Filter",
        FilterJson = JsonResourceHelper.GetGoldenDimensionsFilterJson("ProjectExtentsFilterElevationTypeLast")
      };
      public static FilterDescriptor VolumesFilterWithPassCountRangeEarliest => new FilterDescriptor
      {
        FilterUid = "3507b523-9390-4e11-90e9-7a1263bb5cd9",
        Name = "Golden Dimensions Volumes Filter With Pass Count Range Earliest",
        FilterJson = JsonResourceHelper.GetGoldenDimensionsFilterJson("VolumesWithPassCountRangeEarliest")
      };

      public static FilterDescriptor VolumesFilterWithPassCountRangeLatest => new FilterDescriptor
      {
        FilterUid = "3f91916b-7cfc-4c98-9e68-0e5307ffaba5",
        Name = "Golden Dimensions Volumes Filter With Pass Count Range Latest",
        FilterJson = JsonResourceHelper.GetGoldenDimensionsFilterJson("VolumesWithPassCountRangeLatest")
      };

      #region Invalid date range filters (first -> last)

      public static FilterDescriptor InvalidDateFilterElevationTypeFirst => new FilterDescriptor
      {
        FilterUid = "f92100c6-5397-4574-9688-be375d40625e",
        Name = "Golden Dimensions Project Extents Filter",
        FilterJson = JsonResourceHelper.GetGoldenDimensionsFilterJson("InvalidDateFilterElevationTypeFirst")
      };

      public static FilterDescriptor InvalidDateFilterElevationTypeLast => new FilterDescriptor
      {
        FilterUid = "7bc0bfa5-b0e9-463d-9f17-bdc8b18c0b8f",
        Name = "Golden Dimensions Project Extents Filter",
        FilterJson = JsonResourceHelper.GetGoldenDimensionsFilterJson("InvalidDateFilterElevationTypeLast")
      };

      #endregion

      #region No Data date range filters (first -> last)

      public static FilterDescriptor NoDataFilterElevationTypeFirst => new FilterDescriptor
      {
        FilterUid = "ce4497d9-76d0-4477-aa23-2ee1acd8c4f0",
        Name = "Golden Dimensions Project Extents Filter",
        FilterJson = JsonResourceHelper.GetGoldenDimensionsFilterJson("NoDataFilterElevationTypeFirst")
      };

      public static FilterDescriptor NoDataFilterElevationTypeLast => new FilterDescriptor
      {
        FilterUid = "fe6065a7-21fe-4db0-8f47-3ea6c320dac7",
        Name = "Golden Dimensions Project Extents Filter",
        FilterJson = JsonResourceHelper.GetGoldenDimensionsFilterJson("NoDataFilterElevationTypeLast")
      };

      #endregion

      public static FilterDescriptor SummaryVolumesBaseFilter20170305 => new FilterDescriptor
      {
        FilterUid = "abd72636-43d3-4b04-9c3b-6383743659e4",
        Name = "GD Dimensions Base Filter 5/3/2017",
        FilterJson = JsonResourceHelper.GetGoldenDimensionsFilterJson("SummaryVolumesBaseFilter20170305")
      };

      public static FilterDescriptor SummaryVolumesTopFilter20170621 => new FilterDescriptor
      {
        FilterUid = "f0e02abf-995c-44ef-bd89-1936d2564e57",
        Name = "GD Dimensions Base Filter 21/6/2017",
        FilterJson = JsonResourceHelper.GetGoldenDimensionsFilterJson("SummaryVolumesTopFilter20170621")
      };
    }

    public class Dimensions
    {
      public static FilterDescriptor DimensionsAsAtCustom => new FilterDescriptor
      {
        FilterUid = "a8405aca-71f1-463d-8821-c2415d67e78c",
        Name = "Dimensions as at Custom 5-11-2012 4:59:59",
        FilterType = FilterType.Persistent,
        FilterJson = JsonResourceHelper.GetFilterJson("DimensionsAsAtCustom")
      };

      public static FilterDescriptor DimensionsBoundaryCmv => new FilterDescriptor
      {
        FilterUid = "a37f3008-65e5-44a8-b406-9a078ec62ece",
        Name = "Dimensions boundary CMV",
        FilterType = FilterType.Persistent,
        FilterJson = JsonResourceHelper.GetFilterJson("DimensionsBoundaryCMV")
      };

      public static FilterDescriptor DimensionsBoundaryCmvPassCountRange => new FilterDescriptor
      {
        FilterUid = "026cabf4-f1b2-4211-a3df-8a314e365e80",
        Name = "Dimensions boundary CMV with pass count range",
        FilterType = FilterType.Persistent,
        FilterJson = JsonResourceHelper.GetFilterJson("DimensionsBoundaryCMVPassCountRange")
      };

      public static FilterDescriptor DimensionsBoundaryCmvAsAtToday => new FilterDescriptor
      {
        FilterUid = "c638018c-5026-44be-af0b-006ecad65462",
        Name = "Dimensions boundary CMV as at Today",
        FilterType = FilterType.Persistent,
        FilterJson = JsonResourceHelper.GetFilterJson("DimensionsBoundaryCMVAsAtToday")
      };

      public static FilterDescriptor DimensionsBoundaryFilter => new FilterDescriptor
      {
        FilterUid = "154470b6-15ae-4cca-b281-eae8ac1efa6c",
        Name = "Dimensions boundary filter",
        FilterType = FilterType.Persistent,
        FilterJson = JsonResourceHelper.GetFilterJson("DimensionsBoundaryFilter")
      };

      public static FilterDescriptor DimensionsBoundaryFilterAsAtToday => new FilterDescriptor
      {
        FilterUid = "3c836562-bcd5-4a35-99a5-cb5655572be7",
        Name = "Dimensions boundary filter as at today",
        FilterType = FilterType.Persistent,
        FilterJson = JsonResourceHelper.GetFilterJson("DimensionsBoundaryFilterAsAtToday")
      };

      public static FilterDescriptor DimensionsBoundaryMdp => new FilterDescriptor
      {
        FilterUid = "3ef41e3c-d1f5-40cd-b012-99d11ff432ef",
        Name = "Dimensions boundary MDP",
        FilterType = FilterType.Persistent,
        FilterJson = JsonResourceHelper.GetFilterJson("DimensionsBoundaryMDP")
      };

      public static FilterDescriptor DimensionsBoundaryMdpPassCountRange => new FilterDescriptor
      {
        FilterUid = "bc29dd86-015f-4e84-a29f-cbc0a2add277",
        Name = "Dimensions boundary MDP with pass count range",
        FilterType = FilterType.Persistent,
        FilterJson = JsonResourceHelper.GetFilterJson("DimensionsBoundaryMDPPassCountRange")
      };

      public static FilterDescriptor DimensionsBoundaryMdpAsAtToday => new FilterDescriptor
      {
        FilterUid = "cefd0bda-53e4-45bf-a2b9-ca0cf6f6907a",
        Name = "Dimensions boundary mdp as at today",
        FilterType = FilterType.Persistent,
        FilterJson = JsonResourceHelper.GetFilterJson("DimensionsBoundaryMDPAsAtToday")
      };

      public static FilterDescriptor DimensionsBoundaryFilterWithMachine => new FilterDescriptor
      {
        FilterUid = "9c27697f-ea6d-478a-a168-ed20d6cd9a20",
        Name = "Dimensions boundary filter with machine",
        FilterType = FilterType.Persistent,
        FilterJson = JsonResourceHelper.GetFilterJson("DimensionsBoundaryFilterWithMachine")
      };

      public static FilterDescriptor ElevationRangeAndPaletteNoDataFilter => new FilterDescriptor
      {
        FilterUid = "200c7b47-b5e6-48ee-a731-7df6623412da",
        Name = "Elevation Range and Palette No Data Filter",
        FilterType = FilterType.Persistent,
        FilterJson = JsonResourceHelper.GetFilterJson("ElevationRangeAndPaletteNoDataFilter")
      };

      public static FilterDescriptor SummaryVolumesBaseFilter => new FilterDescriptor
      {
        FilterUid = "F07ED071-F8A1-42C3-804A-1BDE7A78BE5B",
        Name = "Summary Volumes Base Filter",
        FilterType = FilterType.Persistent,
        FilterJson = JsonResourceHelper.GetFilterJson("SummaryVolumesBaseFilter")
      };

      public static FilterDescriptor SummaryVolumesTopFilter => new FilterDescriptor
      {
        FilterUid = "A40814AA-9CDB-4981-9A21-96EA30FFECDD",
        Name = "Summary Volumes Top Filter",
        FilterType = FilterType.Persistent,
        FilterJson = JsonResourceHelper.GetFilterJson("SummaryVolumesTopFilter")
      };

      public static FilterDescriptor SummaryVolumesFilterNoDates => new FilterDescriptor
      {
        FilterUid = "98f03939-e559-442b-b376-4dd25f86349e",
        Name = "Summary Volumes Filter No Dates",
        FilterType = FilterType.Persistent,
        FilterJson = JsonResourceHelper.GetDimensionsFilterJson("SummaryVolumesFilterNoDates")
      };

      public static FilterDescriptor SummaryVolumesFilterToday => new FilterDescriptor
      {
        FilterUid = "A54E5945-1AAA-4921-9CC1-C9D8C0A343D3",
        Name = "Summary Volumes Filter Today",
        FilterType = FilterType.Persistent,
        FilterJson = JsonResourceHelper.GetDimensionsFilterJson("SummaryVolumesFilterToday")
      };

      public static FilterDescriptor SummaryVolumesFilterYesterday => new FilterDescriptor
      {
        FilterUid = "A325F48A-3F3D-489A-976A-B4780EF84045",
        Name = "Summary Volumes Filter Yesterday",
        FilterType = FilterType.Persistent,
        FilterJson = JsonResourceHelper.GetDimensionsFilterJson("SummaryVolumesFilterYesterday")
      };

      public static FilterDescriptor SummaryVolumesFilterNoLatLonToday => new FilterDescriptor
      {
        FilterUid = "F9D55290-27F2-4B70-BC63-9FD23218E6E6",
        Name = "Summary Volumes Filter No Lat Lon Today",
        FilterType = FilterType.Persistent,
        FilterJson = JsonResourceHelper.GetDimensionsFilterJson("SummaryVolumesFilterNoLatLonToday")
      };
      public static FilterDescriptor SummaryVolumesFilterNoLatLonYesterday => new FilterDescriptor
      {
        FilterUid = "D6B254A0-C047-4805-9CCD-F847FAB05B14",
        Name = "Summary Volumes Filter No Lat Lon Yesterday",
        FilterType = FilterType.Persistent,
        FilterJson = JsonResourceHelper.GetDimensionsFilterJson("SummaryVolumesFilterNoLatLonYesterday")
      };

      public static FilterDescriptor SummaryVolumesFilterProjectExtents => new FilterDescriptor
      {
        FilterUid = "03914de8-dce7-403b-8790-1e07773db5e1",
        Name = "Summary Volumes Filter Project Extents",
        FilterType = FilterType.Persistent,
        FilterJson = JsonResourceHelper.GetDimensionsFilterJson("SummaryVolumesFilterProjectExtents")
      };

      public static FilterDescriptor SummaryVolumesFilterCustom20121101First => new FilterDescriptor
      {
        FilterUid = "9244d3f1-af2b-41ed-aa16-5a776278b6eb",
        Name = "Summary Volumes Filter Custom 2012-11-01 First",
        FilterType = FilterType.Persistent,
        FilterJson = JsonResourceHelper.GetDimensionsFilterJson("SummaryVolumesFilterCustom20121101First")
      };

      public static FilterDescriptor SummaryVolumesFilterExtentsEarliest => new FilterDescriptor
      {
        FilterUid = "9c27697f-ea6d-478a-a168-ed20d6cd9a22",
        Name = "Summary Volumes Filter Extents Earliest",
        FilterType = FilterType.Persistent,
        FilterJson = JsonResourceHelper.GetDimensionsFilterJson("SummaryVolumesFilterProjectExtentsEarliest")
      };
      public static FilterDescriptor SummaryVolumesFilterExtentsEarliestWithPassCountRange => new FilterDescriptor
      {
        FilterUid = "5a130d7c-a79b-433d-a04a-094b07cfc1dd",
        Name = "Summary Volumes Filter Extents Earliest With Pass Count Range",
        FilterType = FilterType.Persistent,
        FilterJson = JsonResourceHelper.GetDimensionsFilterJson("SummaryVolumesFilterProjectExtentsEarliestWithPassCountRange")
      };

      public static FilterDescriptor SummaryVolumesFilterExtentsLatest => new FilterDescriptor
      {
        FilterUid = "9c27697f-ea6d-478a-a168-ed20d6cd9a21",
        Name = "Summary Volumes Filter Extents Latest",
        FilterType = FilterType.Persistent,
        FilterJson = JsonResourceHelper.GetDimensionsFilterJson("SummaryVolumesFilterProjectExtentsLatest")
      };
      public static FilterDescriptor SummaryVolumesFilterExtentsLatestWithPassCountRange => new FilterDescriptor
      {
        FilterUid = "b06996e4-4944-4d84-b2c7-e1808dd7d7d7",
        Name = "Summary Volumes Filter Extents Latest With Pass Count Range",
        FilterType = FilterType.Persistent,
        FilterJson = JsonResourceHelper.GetDimensionsFilterJson("SummaryVolumesFilterProjectExtentsLatestWithPassCountRange")
      };

      public static FilterDescriptor SummaryVolumesFilterCustom20121101Last => new FilterDescriptor
      {
        FilterUid = "279ed62b-06a2-4184-ab14-dd7462dcc8c1",
        Name = "Summary Volumes Filter Custom 2012-11-01 Last",
        FilterType = FilterType.Persistent,
        FilterJson = JsonResourceHelper.GetDimensionsFilterJson("SummaryVolumesFilterCustom20121101Last")
      };

      public static FilterDescriptor SummaryVolumesFilterNull => new FilterDescriptor
      {
        FilterUid = "e2c7381d-1a2e-4dc7-8c0e-45df2f92ba0e",
        Name = "Summary Volumes Filter Null",
        FilterType = FilterType.Persistent,
        FilterJson = JsonResourceHelper.GetDimensionsFilterJson("SummaryVolumesFilterNull20121101")
      };

      public static FilterDescriptor SummaryVolumesTemperature => new FilterDescriptor
      {
        FilterUid = "601afff6-844e-448d-a16c-bd40a5dc9124",
        Name = "Summary Volumes Temperature",
        FilterType = FilterType.Persistent,
        FilterJson = JsonResourceHelper.GetDimensionsFilterJson("SummaryVolumesTemperature")
      };

      public static FilterDescriptor ReportDxfTile => new FilterDescriptor
      {
        FilterUid = "7b2bd262-8355-44ba-938a-d50f9712dafc",
        Name = "Report Dxf Tile",
        FilterType = FilterType.Persistent,
        FilterJson = JsonResourceHelper.GetFilterJson("ReportDxfTile")
      };
      public static FilterDescriptor DimensionsAlignmentFilter0to200 => new FilterDescriptor
      {
        FilterUid = "2811c7c3-d270-4d63-97e2-fc3340bf6c7a",
        Name = "Dimensions Alignment Filter 0-200",
        FilterType = FilterType.Persistent,
        FilterJson = JsonResourceHelper.GetFilterJson("DimensionsAlignmentFilter0to200")
      };
      public static FilterDescriptor DimensionsAlignmentFilter100to200 => new FilterDescriptor
      {
        FilterUid = "2811c7c3-d270-4d63-97e2-fc3340bf6c6b",
        Name = "Dimensions Alignment Filter 100t-200",
        FilterType = FilterType.Persistent,
        FilterJson = JsonResourceHelper.GetFilterJson("DimensionsAlignmentFilter100to200")
      };
      public static FilterDescriptor DimensionsTemperatureRangeFilter => new FilterDescriptor
      {
        FilterUid = "1980fc8b-c892-4f9f-b673-bc09827bf2b5",
        Name = "Dimensions Temperature Range Filter",
        FilterType = FilterType.Persistent,
        FilterJson = JsonResourceHelper.GetFilterJson("DimensionsTemperatureRange")
      };
      public static FilterDescriptor DimensionsTempRangeBoundaryFilter => new FilterDescriptor
      {
        FilterUid = "3c0b76b6-8e35-4729-ab83-f976732d999b",
        Name = "Dimensions Temperature Range Filter With Boundary",
        FilterType = FilterType.Persistent,
        FilterJson = JsonResourceHelper.GetFilterJson("DimensionsTempRangeBoundary")
      };
      public static FilterDescriptor DimensionsPassCountRangeFilter => new FilterDescriptor
      {
        FilterUid = "c5590172-a1bb-440a-bc7d-6c35ecc75724",
        Name = "Dimensions Pass Count Range Filter",
        FilterType = FilterType.Persistent,
        FilterJson = JsonResourceHelper.GetFilterJson("DimensionsPassCountRange")
      };
      public static FilterDescriptor DimensionsAutomaticsFilter => new FilterDescriptor
      {
        FilterUid = "887f90a6-56b9-4266-9d62-ff99e7d346f0",
        Name = "Dimensions Automatics Filter With Boundary",
        FilterType = FilterType.Persistent,
        FilterJson = JsonResourceHelper.GetFilterJson("DimensionsAutomatics")
      };
    }
  }
}
