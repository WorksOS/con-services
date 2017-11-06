using MockProjectWebApi.Json;
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
        FilterJson = JsonResourceHelper.GetFilterJson("DimensionsBoundaryCMV")
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
        FilterUid = "9c27697f-ea6d-478a-a168-ed20d6cd9a20",
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
        Name = "Summary volumes BaseFilter",
        FilterJson = JsonResourceHelper.GetFilterJson("SummaryVolumesBaseFilter")
      };

      public static FilterDescriptor SummaryVolumesTopFilter => new FilterDescriptor
      {
        FilterUid = "A40814AA-9CDB-4981-9A21-96EA30FFECDD",
        Name = "Summary volumes TopFilter",
        FilterJson = JsonResourceHelper.GetFilterJson("SummaryVolumesTopFilter")
      };

      public static FilterDescriptor SummaryVolumesBaseFilterToday => new FilterDescriptor
      {
        FilterUid = "3E2A21B2-D66E-44D4-A590-4F4B7C7FBA7B",
        Name = "Summary volumes BaseFilter Today",
        FilterJson = JsonResourceHelper.GetDimensionsFilterJson("SummaryVolumesBaseFilterToday")
      };

      public static FilterDescriptor SummaryVolumesTopFilterToday => new FilterDescriptor
      {
        FilterUid = "A54E5945-1AAA-4921-9CC1-C9D8C0A343D3",
        Name = "Summary volumes TopFilter Today",
        FilterJson = JsonResourceHelper.GetDimensionsFilterJson("SummaryVolumesTopFilterToday")
      };

      public static FilterDescriptor SummaryVolumesBaseFilterYesterday => new FilterDescriptor
      {
        FilterUid = "A325F48A-3F3D-489A-976A-B4780EF84045",
        Name = "Summary volumes BaseFilter Yesterday",
        FilterJson = JsonResourceHelper.GetDimensionsFilterJson("SummaryVolumesBaseFilterYesterday")
      };

      public static FilterDescriptor SummaryVolumesTopFilterYesterday => new FilterDescriptor
      {
        FilterUid = "A5FD6B6F-CA88-42F2-8AD8-F37E0635CF80",
        Name = "Summary volumes TopFilter Yesterday",
        FilterJson = JsonResourceHelper.GetDimensionsFilterJson("SummaryVolumesTopFilterYesterday")
      };
    }
  }
}