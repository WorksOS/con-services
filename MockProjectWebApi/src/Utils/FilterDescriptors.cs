﻿using MockProjectWebApi.Json;
using VSS.MasterData.Models.Models;

namespace MockProjectWebApi.Utils
{
  public class FilterDescriptors
  {
    public class GoldenData
    { }

    public class Dimensions
    {
      public static FilterDescriptor DimensionsBoundaryCmv => new FilterDescriptor
      {
        FilterUid = "a37f3008-65e5-44a8-b406-9a078ec62ece",
        Name = "Dimensions boundary CMV",
        FilterJson = JsonResourceHelper.GetDimensionsFilterJson("DimensionsBoundaryCMV")
      };

      public static FilterDescriptor DimensionsBoundaryFilter => new FilterDescriptor
      {
        FilterUid = "154470b6-15ae-4cca-b281-eae8ac1efa6c",
        Name = "Dimensions boundary filter",
        FilterJson = JsonResourceHelper.GetFilterJson("DimensionsBoundaryFilter")
      };

      public static FilterDescriptor DimensionsBoundaryMdp => new FilterDescriptor
      {
        FilterUid = "3ef41e3c-d1f5-40cd-b012-99d11ff432ef",
        Name = "Dimensions boundary mdp",
        FilterJson = JsonResourceHelper.GetFilterJson("DimensionsBoundaryMDP")
      };

      public static FilterDescriptor DimensionsBoundaryFilterWithMachine => new FilterDescriptor
      {
        FilterUid = "9A39E490-88DF-43AF-A64B-C919E23081DA",
        Name = "Dimensions boundary filter with machine",
        FilterJson = JsonResourceHelper.GetFilterJson("DimensionsBoundaryFilterWithMachine")
      };

      public static FilterDescriptor ElevationRangeAndPaletteNoDataFilter => new FilterDescriptor
      {
        FilterUid = "200c7b47-b5e6-48ee-a731-7df6623412da",
        Name = "Elevation Range and Palette No Data Filter",
        FilterJson = JsonResourceHelper.GetFilterJson("ElevationRangeAndPaletteNoDataFilter")
      };

      public static FilterDescriptor SummaryVolumesBaseFilter => new FilterDescriptor
      {
        FilterUid = "F07ED071-F8A1-42C3-804A-1BDE7A78BE5B",
        FilterJson = JsonResourceHelper.GetFilterJson("SummaryVolumesBaseFilter")
      };

      public static FilterDescriptor SummaryVolumesTopFilter => new FilterDescriptor
      {
        FilterUid = "A40814AA-9CDB-4981-9A21-96EA30FFECDD",
        FilterJson = JsonResourceHelper.GetFilterJson("SummaryVolumesTopFilter")
      };

      public static FilterDescriptor SummaryVolumesFilterToday => new FilterDescriptor
      {
        FilterUid = "A54E5945-1AAA-4921-9CC1-C9D8C0A343D3",
        FilterJson = JsonResourceHelper.GetDimensionsFilterJson("SummaryVolumesFilterToday")
      };

      public static FilterDescriptor SummaryVolumesFilterYesterday => new FilterDescriptor
      {
        FilterUid = "A325F48A-3F3D-489A-976A-B4780EF84045",
        FilterJson = JsonResourceHelper.GetDimensionsFilterJson("SummaryVolumesFilterYesterday")
      };

      public static FilterDescriptor SummaryVolumesFilterNoLatLonToday => new FilterDescriptor
      {
        FilterUid = "F9D55290-27F2-4B70-BC63-9FD23218E6E6",
        FilterJson = JsonResourceHelper.GetDimensionsFilterJson("SummaryVolumesFilterNoLatLonToday")
      };
      public static FilterDescriptor SummaryVolumesFilterNoLatLonYesterday => new FilterDescriptor
      {
        FilterUid = "D6B254A0-C047-4805-9CCD-F847FAB05B14",
        FilterJson = JsonResourceHelper.GetDimensionsFilterJson("SummaryVolumesFilterNoLatLonYesterday")
      };
    }
  }
}