using System;
using System.Collections.Generic;
using System.Linq;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.SiteModels.Interfaces;

namespace VSS.TRex.Gateway.Common.Helpers
{
  public class CSVExportHelper
  {
    public static Tuple<DateTime, DateTime> GetDateRange(ISiteModel siteModel, FilterResult filter)
    {
      if (filter?.StartUtc == null || !filter.EndUtc.HasValue)
      {
        var startEndDate = GetDateRange(siteModel);

        var startUtc = filter?.StartUtc ?? startEndDate.Item1;
        var endUtc = filter?.EndUtc ?? startEndDate.Item2;
        return new Tuple<DateTime, DateTime>(startUtc, endUtc);
      }

      return new Tuple<DateTime, DateTime>(filter.StartUtc.Value, filter.EndUtc.Value);
    }

    public static Tuple<DateTime, DateTime> GetDateRange(ISiteModel siteModel)
    {
      DateTime minDate = DateTime.MaxValue;
      DateTime maxDate = DateTime.MinValue;

      // todoJeannie for veta, should this be limited to those machines in the machinesList - might save some processing time?
      foreach (var machine in siteModel.Machines)
      {
        var events = siteModel.MachinesTargetValues[machine.InternalSiteModelMachineIndex].StartEndRecordedDataEvents;
        if (events.Count() > 0)
        {
          events.GetStateAtIndex(0, out DateTime eventDateFirst, out _);
          if (minDate > eventDateFirst)
            minDate = eventDateFirst;
          if (events.Count() > 1)
          {
            events.GetStateAtIndex(events.Count()-1, out DateTime eventDateLast, out _);
            if (maxDate < eventDateLast)
              maxDate = eventDateLast;
          }
        }
      }

      return new Tuple<DateTime, DateTime>(minDate, maxDate);
    }

    public static Guid[] GetRequestedMachines(ISiteModel siteModel, string[] machineNames)
    {
      if (machineNames == null || machineNames.Length == 0)
      {
        return null;
      }

      var result = new List<Guid>();

      foreach (var name in machineNames)
      {
        var machine = siteModel.Machines.First(x => string.Compare(x.Name, name, StringComparison.OrdinalIgnoreCase) == 0);
        if (machine != null)
        {
          result.Add(machine.ID);
        }
      }

      return result.ToArray();
    }
  }
}
