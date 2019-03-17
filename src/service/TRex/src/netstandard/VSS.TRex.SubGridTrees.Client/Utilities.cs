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
            var result = DataType;

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (DataType)
            {
                case GridDataType.Height:
                  result = IncludeSurveyedSurfacesInResult ? GridDataType.HeightAndTime : GridDataType.Height;
                  break;

                // SimpleVolumeOverlay not supported yet in TRex
                // case GridDataType.SimpleVolumeOverlay:
                //   result = IncludeSurveyedSurfacesInResult ? GridDataType.HeightAndTime : GridDataType.Height;
                //   break;

                case GridDataType.CutFill:
                  result = GridDataType.Height;
                  break;
            }

            return result;
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
