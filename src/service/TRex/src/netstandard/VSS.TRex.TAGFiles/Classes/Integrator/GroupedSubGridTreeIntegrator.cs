using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using VSS.TRex.Storage.Models;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Server;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.SubGridTrees.Types;

namespace VSS.TRex.TAGFiles.Classes.Integrator
{
  public class GroupedSubGridTreeIntegrator : IGroupedSubGridTreeIntegrator
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<GroupedSubGridTreeIntegrator>();

    public List<(IServerSubGridTree, DateTime, DateTime)> Trees { get; set; }

    public GroupedSubGridTreeIntegrator()
    {
    }

    public IServerSubGridTree IntegrateSubGridTreeGroup()
    {

      // Construct the overall existence map covering the aggregate of the supplied sub grid trees
      ISubGridTreeBitMask overallMap = new SubGridTreeSubGridExistenceBitMask();
      Trees.ForEach(x => x.Item1.ScanAllSubGrids(subGrid =>
        overallMap[subGrid.OriginX >> SubGridTreeConsts.SubGridIndexBitsPerLevel,
                   subGrid.OriginY >> SubGridTreeConsts.SubGridIndexBitsPerLevel] = true));

      // Sort the sub grid trees into time order
      Trees.Sort((x, y) => x.Item2.CompareTo(y.Item2));

      // Check (and warn) if any of the supplied models overlaps in time
      for (int i = 0; i < Trees.Count - 1; i++)
      {
        if (Trees[i].Item3 > Trees[i + 1].Item3)
        {
          Log.LogWarning($"Site models out of order in ${nameof(IntegrateSubGridTreeGroup)} in list containing {Trees.Count} models");
          break;
        }
      }

      // Iterate oer all sub grids in the composite existence map constructed a grouped sub grid from the combined models and
      // adding it to the result sub grid tree
      // Create the server sub grid tree to contain the result

      var subGridGrouper = new GroupedSubGridIntegrator
      {
        Trees = Trees
      };

      IServerSubGridTree result = new ServerSubGridTree(Guid.Empty, StorageMutability.Mutable);

      // Scan across all sub grids in the combined existence map and aggregate each spatial sub grid in turn
      overallMap.ScanAllSetBitsAsSubGridAddresses(address => subGridGrouper.IntegrateSubGridGroup(result.ConstructPathToCell(address.X, address.Y, SubGridPathConstructionType.CreateLeaf) as IServerLeafSubGrid));

      return result;
    }
  }
}
