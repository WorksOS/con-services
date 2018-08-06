using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Filters;
using VSS.TRex.SiteModels.Interfaces;

namespace VSS.TRex.Gateway.Common.Converters
{
  public class DataConversionUtility
  {
    public static CombinedFilter ConvertFilter(FilterResult filter, ISiteModel siteModel)
    {
      if (filter == null)
        return new CombinedFilter();//TRex doesn't like null filter

      var combinedFilter = Mapper.Map<FilterResult, CombinedFilter>(filter);
      // TODO Map the excluded surveyed surfaces from the filter.SurveyedSurfaceExclusionList to the ones that are in the TRex database
      bool includeSurveyedSurfaces = filter.SurveyedSurfaceExclusionList.Count == 0;
      var excludedIds = siteModel.SurveyedSurfaces == null || includeSurveyedSurfaces ? new Guid[0] : siteModel.SurveyedSurfaces.Select(x => x.ID).ToArray();
      combinedFilter.AttributeFilter.SurveyedSurfaceExclusionList = excludedIds;
      return combinedFilter;
    }

  }
}
