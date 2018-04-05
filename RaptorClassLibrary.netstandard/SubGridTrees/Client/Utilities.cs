using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.SubGridTrees.Client
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
    }
}
