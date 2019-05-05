using System;
using FluentAssertions;
using VSS.TRex.DI;
using VSS.TRex.SiteModels.GridFabric.Events;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.SiteModels
{
  public class SiteModelChangeNotificationTests : IClassFixture<DITAGFileAndSubGridRequestsFixture>
  {
    private void TestIt(Func<ISiteModel, bool> loadedFunc, 
      Func<ISiteModel, object> loadAction, 
      Action<SiteModelAttributesChangedEvent, bool> modifyAction,
      bool modified,
      bool finalState)
    {
      var siteModels = DIContext.Obtain<ISiteModels>();

      // Create the new site model
      var guid = Guid.NewGuid();
      var siteModel = siteModels.GetSiteModel(guid, true);

      // Force the site model to be reloaded in a virgin state so the 'loaded' state is as expected
      // as metadata registration sets all the loaded flags to 'true'
      var _evt = new SiteModelAttributesChangedEvent
      {
        SiteModelID = guid,
        AlignmentsModified = true,
        CsibModified = true,
        DesignsModified = true,
        ExistenceMapModified = true,
        MachineDesignsModified = true,
        MachineTargetValuesModified = true,
        MachinesModified = true,
        ProofingRunsModified = true,
        SurveyedSurfacesModified = true
      };

      siteModels.SiteModelAttributesHaveChanged(_evt);
      siteModel = siteModels.GetSiteModel(guid, false);

      loadedFunc.Invoke(siteModel).Should().BeFalse();

      var _ = loadAction.Invoke(siteModel);
      loadedFunc.Invoke(siteModel).Should().BeTrue();

      var evt = new SiteModelAttributesChangedEvent
      {
        SiteModelID = guid
      };
      modifyAction.Invoke(evt, modified);

      siteModels.SiteModelAttributesHaveChanged(evt);

      var siteModel2 = siteModels.GetSiteModel(guid, false);
      siteModel2.Should().NotBeNull();
      siteModel2.Should().NotBe(siteModel);
      loadedFunc.Invoke(siteModel2).Should().Be(finalState);
    }

    /// <summary>
    /// </summary>
    /// <param name="loadedFunc">Determines the 'loaded' status of the element being tested</param>
    /// <param name="loadAction">Performs the action to load the state of the element being tested</param>
    /// <param name="modifyAction">Performs the action to set the modified state of the change notification related to the element being tested</param>
    private void TestModAndUnModded(Func<ISiteModel, bool> loadedFunc,
      Func<ISiteModel, object> loadAction,
      Action<SiteModelAttributesChangedEvent, bool> modifyAction)
    {
      TestIt(loadedFunc, loadAction, modifyAction, true, false);
      TestIt(loadedFunc, loadAction, modifyAction, false, true);
    }

    [Fact]
    public void Test_SiteModel_ChangeNotification_CSIB()
    {
      TestModAndUnModded(siteModel => siteModel.CSIBLoaded, x => x.CSIB(), (evt, state) => evt.CsibModified = state);
    }

    [Fact]
    public void Test_SiteModel_ChangeNotification_Alignments()
    {
      TestModAndUnModded(siteModel => siteModel.AlignmentsLoaded, x => x.Alignments, (evt, state) => evt.AlignmentsModified = state);
    }

    [Fact]
    public void Test_SiteModel_ChangeNotification_Designs()
    {
      TestModAndUnModded(siteModel => siteModel.DesignsLoaded, x => x.Designs, (evt, state) => evt.DesignsModified = state);
    }

    [Fact]
    public void Test_SiteModel_ChangeNotification_ExistenceMap()
    {
      TestModAndUnModded(siteModel => siteModel.ExistenceMapLoaded, x => x.ExistenceMap, (evt, state) => evt.ExistenceMapModified = state);
    }

    [Fact]
    public void Test_SiteModel_ChangeNotification_MachinesTargetValues()
    {
      TestModAndUnModded(siteModel => siteModel.MachineTargetValuesLoaded, x => x.MachinesTargetValues, (evt, state) => evt.MachineTargetValuesModified = state);
    }

    [Fact]
    public void Test_SiteModel_ChangeNotification_Machines()
    {
      TestModAndUnModded(siteModel => siteModel.MachinesLoaded, x => x.Machines, (evt, state) => evt.MachinesModified = state);
    }

    [Fact]
    public void Test_SiteModel_ChangeNotification_SiteModelDesigns()
    {
      TestModAndUnModded(siteModel => siteModel.SiteModelDesignsLoaded, x => x.SiteModelDesigns, (evt, state) => evt.MachineDesignsModified = state);
    }

    [Fact]
    public void Test_SiteModel_ChangeNotification_SiteModelMachineDesigns()
    {
      TestModAndUnModded(siteModel => siteModel.SiteModelMachineDesignsLoaded, x => x.SiteModelMachineDesigns, (evt, state) => evt.MachineDesignsModified = state);
    }

    [Fact]
    public void Test_SiteModel_ChangeNotification_ProofingRUns()
    {
      TestModAndUnModded(siteModel => siteModel.SiteProofingRunsLoaded, x => x.SiteProofingRuns, (evt, state) => evt.ProofingRunsModified = state);
    }

    [Fact]
    public void Test_SiteModel_ChangeNotification_SurveyedSurfaces()
    {
      TestModAndUnModded(siteModel => siteModel.SurveyedSurfacesLoaded, x => x.SurveyedSurfaces, (evt, state) => evt.SurveyedSurfacesModified = state);
    }
  }
}
