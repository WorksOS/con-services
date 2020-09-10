﻿using System.Collections.Generic;
using System.Linq;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.MasterData.Models.Models;

namespace VSS.Productivity3D.Filter.Abstractions.Models
{
  public class FilterListData : BaseDataResult, IMasterDataModel
  {
    /// <summary>
    /// Gets or sets the filter descriptors.
    /// </summary>
    /// <value>
    /// The filter descriptors.
    /// </value>
    public List<FilterDescriptor> filterDescriptors { get; set; }

    public List<string> GetIdentifiers() => filterDescriptors?
                                              .SelectMany(f => f.GetIdentifiers())
                                              .Distinct()
                                              .ToList()
                                            ?? new List<string>();
  }
}