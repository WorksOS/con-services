using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using VSS.MasterData.Models.Handlers;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Filter.Abstractions.Models
{
  public class FilterRequest
  {
    /// <summary>
    /// The filterUid whose filter is to be udpated, empty for create
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public string FilterUid { get; set; } = string.Empty;

    /// <summary>
    /// The name to be upserted, it may be empty for transient filters
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The filter containing the Json string representing a seialised filter to be created.
    /// May be empty if all defaults
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    public string FilterJson { get; set; } = string.Empty;

    /// <summary>
    /// The list of filterUids from which to create a combined filter.
    /// May be empty if no pre-exisitng filters are to be combined.
    /// Will be ignired if the content of FilterJson is non-null
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public List<HierarchicFilterElement> HierarchicFilterUids { get; set; } = null;

    /// <summary>
    /// The type of filter. If not specified defaults to Transient.
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public FilterType FilterType { get; set; } = FilterType.Transient;

    /// <summary>
    /// Returns a new instance of <see cref="FilterRequest"/> using the provided inputs.
    /// </summary>
    public static FilterRequest Create(string filterUid, string name, string filterJson, FilterType filterType)
    {
      return new FilterRequest
      {
        FilterUid = filterUid,
        Name = name,
        FilterJson = filterJson,
        FilterType = filterType
      };
    }

    /// <summary>
    /// Create a Filter Request based on a filter model
    /// </summary>
    /// <param name="filter">The filter model to be used</param>
    /// <param name="filterUid">Optional Filter UID, if null, a new filter will be created otherwise the filter will be updated</param>
    /// <param name="name">Optional filter name</param>
    /// <param name="filterType">Filter Type, defaults to Transient</param>
    public static FilterRequest Create(Filter filter, string filterUid = null, string name = null,
      FilterType filterType = FilterType.Transient)
    {
      return new FilterRequest
      {
        FilterJson = JsonConvert.SerializeObject(filter),
        FilterUid = filterUid,
        Name = name,
        FilterType = filterType
      };
    }

    public virtual void Validate(IServiceExceptionHandler serviceExceptionHandler, bool onlyFilterUid = false)
    {
      if (FilterUid == null || (FilterUid != string.Empty && Guid.TryParse(FilterUid, out _) == false) || HierarchicFilterUids?.Count == 0)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 2);
      }

      if (HierarchicFilterUids != null)
      {
        HierarchicFilterUids.ForEach(x =>
        {
          if (x == null || (x.FilterUid != string.Empty && Guid.TryParse(x.FilterUid, out _) == false))
          {
            serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 79);
          }

          if (x.Role != FilterCombinationRole.WidgetFilter &&
              x.Role != FilterCombinationRole.Undefined &&
              x.Role != FilterCombinationRole.MasterFilter &&
              x.Role != FilterCombinationRole.VolumesFilter)
          {
            serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 80);
          }
        });

        // There must be at least a master filter present, 0 or 1 dashbaord filters present and 0 or 1 volume filters present
        if (HierarchicFilterUids.Sum(x => x.Role == FilterCombinationRole.MasterFilter ? 1 : 0) != 1)
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 81);
        }

        if (HierarchicFilterUids.Sum(x => x.Role == FilterCombinationRole.WidgetFilter ? 1 : 0) > 1)
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 82);
        }

        if (HierarchicFilterUids.Sum(x => x.Role == FilterCombinationRole.VolumesFilter ? 1 : 0) > 1)
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 83);
        }

        if (HierarchicFilterUids.Sum(x => x.Role == FilterCombinationRole.Undefined ? 1 : 0) > 0)
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 84);
        }
      }

      //Only filterUid needs validating for get filter otherwise everything needs validating
      if (!onlyFilterUid)
      {
        //Must have a name for persistent filters
        if (FilterType == FilterType.Persistent && string.IsNullOrEmpty(Name))
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 3);
        }

        if (FilterJson == null)
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 4);
        }

        if (FilterJson == string.Empty)
        {
          // Newtonsoft.JSON treats empty strings as invalid JSON but for our purposes it is valid.
          return;
        }

        // Validate filterJson... 
        FilterModel(serviceExceptionHandler)?.Validate(serviceExceptionHandler);
      }
    }

    /// <summary>
    /// Validate the object as a transient filter.
    /// </summary>
    public void ValidateTransientFilter(IServiceExceptionHandler serviceExceptionHandler)
    {
      if (FilterType != FilterType.Transient)
      {
        return;
      }

      if (string.IsNullOrEmpty(FilterJson))
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 8, "Missing filter");
      }
    }

    public Abstractions.Models.Filter FilterModel(IServiceExceptionHandler serviceExceptionHandler)
    {
      try
      {
        return string.IsNullOrEmpty(FilterJson) ? null : JsonConvert.DeserializeObject<Abstractions.Models.Filter>(FilterJson);
      }
      catch (JsonReaderException exception)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 42, exception.Message);
      }
      return null;
    }
  }
}
