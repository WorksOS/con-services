using VSS.TRex.Types;

namespace VSS.TRex.SubGridTrees.Client
{
  public static class Utilities
  {
    /// <summary>
    /// Determine the type of intermediary data to be assembled to provide the correct response to the request
    /// </summary>
    public static GridDataType IntermediaryICGridDataTypeForDataType(GridDataType dataType, bool includeSurveyedSurfacesInResult)
    {
      return dataType switch
      {
        GridDataType.Height => includeSurveyedSurfacesInResult ? GridDataType.HeightAndTime : GridDataType.Height,

        // SimpleVolumeOverlay not supported yet in TRex
        // GridDataType.SimpleVolumeOverlay => IncludeSurveyedSurfacesInResult ? GridDataType.HeightAndTime : GridDataType.Height;

        GridDataType.CutFill => includeSurveyedSurfacesInResult ? GridDataType.HeightAndTime : GridDataType.Height,

        _ => dataType
      };
    }

    /// <summary>
    /// Determines valid relationships between the data type of a possibly derived client grid and the underlying
    /// grid data type a sub grid retriever is tasked with retrieving
    /// </summary>
    public static bool DerivedGridDataTypesAreCompatible(GridDataType baseType, GridDataType derivedType)
    {
      return baseType == derivedType ||
             baseType == GridDataType.Height && derivedType == GridDataType.HeightAndTime ||
             baseType == GridDataType.CutFill && (derivedType == GridDataType.HeightAndTime || derivedType == GridDataType.Height);
    }
  }
}
