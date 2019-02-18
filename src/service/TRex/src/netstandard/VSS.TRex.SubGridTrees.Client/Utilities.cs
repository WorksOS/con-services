using VSS.TRex.Types;

namespace VSS.TRex.SubGridTrees.Client
{
    public static class Utilities
    {
        /// <summary>
        /// Determine the type of intermediary data to be assembled to provide the correct response to the request
        /// </summary>
        /// <param name="DataType"></param>
        /// <param name="IncludeSurveyedSurfacesInResult"></param>
        /// <returns></returns>
        public static GridDataType IntermediaryICGridDataTypeForDataType(GridDataType DataType, bool IncludeSurveyedSurfacesInResult)
        {
            switch (DataType)
            {
                case GridDataType.Height: return IncludeSurveyedSurfacesInResult ? GridDataType.HeightAndTime : GridDataType.Height;
                case GridDataType.SimpleVolumeOverlay: return IncludeSurveyedSurfacesInResult ? GridDataType.HeightAndTime : GridDataType.Height;
                case GridDataType.CutFill: return GridDataType.Height;
                default:
                    return DataType;
            }
        }

      /// <summary>
      /// Determines valid relationships between the data type of a possibly derived client grid and the underlying
      /// grid data type a sub grid retriever is tasked with retrieving
      /// </summary>
      /// <param name="baseType"></param>
      /// <param name="derivedType"></param>
      public static bool DerivedGridDataTypesAreCompatible(GridDataType baseType, GridDataType derivedType)
      {
        return baseType == derivedType ||
               baseType == GridDataType.Height && derivedType == GridDataType.HeightAndTime ||
               baseType == GridDataType.CutFill && derivedType == GridDataType.Height;
      }
    }
}
