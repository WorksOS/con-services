using System;
using System.Collections.Generic;
using System.Linq;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Exports.CSV.GridFabric;
using VSS.TRex.SiteModels.Interfaces;

namespace VSS.TRex.Gateway.Common.Helpers
{
  public class CSVExportHelper
  {
    public static Tuple<DateTime, DateTime> GetDateRange(ISiteModel siteModel, FilterResult filter)
    {
      if (filter?.StartUtc == null || !filter.EndUtc.HasValue)
      {
        var startEndDate = siteModel.GetDateRange();

        var startUtc = filter?.StartUtc ?? startEndDate.Item1;
        var endUtc = filter?.EndUtc ?? startEndDate.Item2;
        return new Tuple<DateTime, DateTime>(startUtc, endUtc);
      }

      return new Tuple<DateTime, DateTime>(filter.StartUtc.Value, filter.EndUtc.Value);
    }

    //private static Tuple<DateTime, DateTime> GetDateRange(ISiteModel siteModel)
    //{
    //  DateTime minDate = DateTime.MaxValue;
    //  DateTime maxDate = DateTime.MinValue;

    //  foreach (var machine in siteModel.Machines)
    //  {
    //    var events = siteModel.MachinesTargetValues[machine.InternalSiteModelMachineIndex].StartEndRecordedDataEvents;
    //    if (events.Count() > 0)
    //    {
    //      events.GetStateAtIndex(0, out DateTime eventDateFirst, out _);
    //      if (minDate > eventDateFirst)
    //        minDate = eventDateFirst;
    //      if (maxDate < eventDateFirst)
    //        maxDate = eventDateFirst;

    //      if (events.Count() > 1)
    //      {
    //        var eventDateLast = events.LastStateDate();
    //        if (maxDate < eventDateLast)
    //          maxDate = eventDateLast;
    //      }
    //    }
    //  }

    //  return new Tuple<DateTime, DateTime>(minDate, maxDate);
    //}

    public static List<CSVExportMappedMachine> MapRequestedMachines(ISiteModel siteModel, string[] machineNames)
    {
      var result = new List<CSVExportMappedMachine>();
      if (machineNames == null || machineNames.Length == 0)
      {
        return result;
      }
      
      foreach (var name in machineNames)
      {
        var machine = siteModel.Machines.FirstOrDefault(x => string.Compare(x.Name, name, StringComparison.OrdinalIgnoreCase) == 0);
        if (machine != null)
        {
          result.Add(new CSVExportMappedMachine() { Uid = machine.ID, InternalSiteModelMachineIndex = machine.InternalSiteModelMachineIndex, Name = machine.Name });
        }
      }

      return result;
    }
  }
}
