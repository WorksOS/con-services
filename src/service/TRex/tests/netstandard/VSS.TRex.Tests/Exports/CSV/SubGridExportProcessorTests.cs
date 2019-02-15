using System;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Tests.TestFixtures;
using Xunit;
using FluentAssertions;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Exports.CSV.GridFabric;
using VSS.TRex.Gateway.Common.Converters;
using VSS.TRex.SiteModels;
using Formatter = VSS.TRex.Exports.CSV.Executors.Tasks.Formatter;
using VSS.TRex.Exports.CSV.Executors.Tasks;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;

namespace VSS.TRex.Tests.Exports.CSV
{
  public class SubGridExportProcessorTests : IClassFixture<DITagFileFixture>
  {
    [Fact(Skip = "not implemented")]
    public void FormatterInitializationDefault()
    {
      var siteModel = new SiteModel();
      var userPreferences = DefaultUserPreferences();
      var csvUserPreference = AutoMapperUtility.Automapper.Map<CSVExportUserPreferences>(userPreferences);
      OutputTypes outputType = OutputTypes.PassCountAllPasses;
      var requestArgument = new CSVExportRequestArgument();
      int runningRowCount = 0;

      var formatter = new Formatter(csvUserPreference, outputType, false);
      var subGridProcessor = new SubGridExportProcessor(formatter, requestArgument, siteModel, runningRowCount);

      IClientLeafSubGrid subGrid = null;
      var result = subGridProcessor.ProcessSubGrid(subGrid as ClientCellProfileLeafSubgrid);

      // todoJeannie
    }

    private UserPreferences DefaultUserPreferences()
    {
      return new UserPreferences(
        string.Empty,
        CSVExportUserPreferences.DefaultDateSeparator,
        CSVExportUserPreferences.DefaultTimeSeparator,
        CSVExportUserPreferences.DefaultThousandsSeparator,
        CSVExportUserPreferences.DefaultDecimalSeparator,
        0,
        0,
        (int)CSVExportUserPreferences.DefaultUnits,
        0,
        0,
        (int)CSVExportUserPreferences.DefaultTemperatureUnits,
        3); // is this used?
    }

  }
}

