﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:2.3.2.0
//      SpecFlow Generator Version:2.3.0.0
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace ProductionDataSvc.AcceptanceTests
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.3.2.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute()]
    public partial class GetMachineDesignsFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
        private Microsoft.VisualStudio.TestTools.UnitTesting.TestContext _testContext;
        
#line 1 "GetMachineDesigns.feature"
#line hidden
        
        public virtual Microsoft.VisualStudio.TestTools.UnitTesting.TestContext TestContext
        {
            get
            {
                return this._testContext;
            }
            set
            {
                this._testContext = value;
            }
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitializeAttribute()]
        public static void FeatureSetup(Microsoft.VisualStudio.TestTools.UnitTesting.TestContext testContext)
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner(null, 0);
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "GetMachineDesigns", "\tI should be able to get on-machine designs.", ProgrammingLanguage.CSharp, ((string[])(null)));
            testRunner.OnFeatureStart(featureInfo);
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.ClassCleanupAttribute()]
        public static void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute()]
        public virtual void TestInitialize()
        {
            if (((testRunner.FeatureContext != null) 
                        && (testRunner.FeatureContext.FeatureInfo.Title != "GetMachineDesigns")))
            {
                global::ProductionDataSvc.AcceptanceTests.GetMachineDesignsFeature.FeatureSetup(null);
            }
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute()]
        public virtual void ScenarioTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public virtual void ScenarioSetup(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioStart(scenarioInfo);
            testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs<Microsoft.VisualStudio.TestTools.UnitTesting.TestContext>(TestContext);
        }
        
        public virtual void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        public virtual void FeatureBackground()
        {
#line 4
#line 5
 testRunner.Given("the Machine Design service URI \"/api/v1/projects/{0}/machinedesigndetails\" and th" +
                    "e result file \"GetMachineDesignsResponse.json\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("GetMachineDesigns - Good Request")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "GetMachineDesigns")]
        public virtual void GetMachineDesigns_GoodRequest()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("GetMachineDesigns - Good Request", ((string[])(null)));
#line 7
this.ScenarioSetup(scenarioInfo);
#line 4
this.FeatureBackground();
#line 8
 testRunner.Given("a project Id 1001158", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 9
 testRunner.When("I request machine designs", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table1 = new TechTalk.SpecFlow.Table(new string[] {
                        "designId",
                        "designName"});
            table1.AddRow(new string[] {
                        "0",
                        "<No Design>"});
            table1.AddRow(new string[] {
                        "37",
                        "BC12"});
            table1.AddRow(new string[] {
                        "38",
                        "Building Pad"});
            table1.AddRow(new string[] {
                        "39",
                        "Building Pad_20121026_115902"});
            table1.AddRow(new string[] {
                        "10",
                        "Canal Design 2"});
            table1.AddRow(new string[] {
                        "6",
                        "Canal_DC"});
            table1.AddRow(new string[] {
                        "5",
                        "Canal_DC 02"});
            table1.AddRow(new string[] {
                        "4",
                        "Canal_DC 03"});
            table1.AddRow(new string[] {
                        "13",
                        "Canal_DC v3"});
            table1.AddRow(new string[] {
                        "11",
                        "Canal_DCv2"});
            table1.AddRow(new string[] {
                        "2",
                        "Canal_DTM"});
            table1.AddRow(new string[] {
                        "3",
                        "Canal_Road"});
            table1.AddRow(new string[] {
                        "7",
                        "Canal2-DC"});
            table1.AddRow(new string[] {
                        "48",
                        "Design"});
            table1.AddRow(new string[] {
                        "22",
                        "Design OGN"});
            table1.AddRow(new string[] {
                        "45",
                        "Design1BCD1"});
            table1.AddRow(new string[] {
                        "46",
                        "Dimensions Canal"});
            table1.AddRow(new string[] {
                        "14",
                        "Dimensions-Canal"});
            table1.AddRow(new string[] {
                        "15",
                        "Dimensions-Canal_20121105_105256"});
            table1.AddRow(new string[] {
                        "19",
                        "Ground"});
            table1.AddRow(new string[] {
                        "20",
                        "Ground Outside"});
            table1.AddRow(new string[] {
                        "21",
                        "Ground_sync"});
            table1.AddRow(new string[] {
                        "42",
                        "Large Sites Road"});
            table1.AddRow(new string[] {
                        "12",
                        "LEVEL 01"});
            table1.AddRow(new string[] {
                        "28",
                        "LEVEL 02"});
            table1.AddRow(new string[] {
                        "29",
                        "LEVEL 03"});
            table1.AddRow(new string[] {
                        "30",
                        "LEVEL 04"});
            table1.AddRow(new string[] {
                        "32",
                        "LEVEL 05"});
            table1.AddRow(new string[] {
                        "34",
                        "LEVEL 06"});
            table1.AddRow(new string[] {
                        "35",
                        "LEVEL 07"});
            table1.AddRow(new string[] {
                        "8",
                        "MAP 01"});
            table1.AddRow(new string[] {
                        "25",
                        "OGL"});
            table1.AddRow(new string[] {
                        "18",
                        "OGN"});
            table1.AddRow(new string[] {
                        "24",
                        "OGN_Ground"});
            table1.AddRow(new string[] {
                        "44",
                        "OriginalGround"});
            table1.AddRow(new string[] {
                        "23",
                        "Outside Ground"});
            table1.AddRow(new string[] {
                        "1",
                        "Pond1_2"});
            table1.AddRow(new string[] {
                        "41",
                        "Road2"});
            table1.AddRow(new string[] {
                        "40",
                        "SLOPE 01"});
            table1.AddRow(new string[] {
                        "31",
                        "SLOPE 02"});
            table1.AddRow(new string[] {
                        "33",
                        "SLOPE 03"});
            table1.AddRow(new string[] {
                        "36",
                        "SLOPE 04"});
            table1.AddRow(new string[] {
                        "27",
                        "Small Site Road 29 10 2012"});
            table1.AddRow(new string[] {
                        "43",
                        "Small Sites"});
            table1.AddRow(new string[] {
                        "16",
                        "Trimble Command Center"});
            table1.AddRow(new string[] {
                        "17",
                        "Trimble Command Center_20121030_141320"});
            table1.AddRow(new string[] {
                        "9",
                        "Trimble Dim Rd"});
            table1.AddRow(new string[] {
                        "26",
                        "Trimble Road 29 10 2012"});
            table1.AddRow(new string[] {
                        "47",
                        "Trimble Road with Ref Surfaces v2"});
            table1.AddRow(new string[] {
                        "49",
                        "we love u juarne"});
#line 10
 testRunner.Then("the following machine designs should be returned", ((string)(null)), table1, "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        public virtual void GetMachineDesignsForDateRange_GoodRequest(string requestName, string projectUID, string startUTC, string endUTC, string resultName, string[] exampleTags)
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("GetMachineDesigns For Date Range - Good Request", exampleTags);
#line 63
this.ScenarioSetup(scenarioInfo);
#line 4
this.FeatureBackground();
#line 64
 testRunner.Given(string.Format("a projectUid \"{0}\"", projectUID), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 65
  testRunner.And(string.Format("startUTC \"{0}\"", startUTC), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 66
  testRunner.And(string.Format("endUTC \"{0}\"", endUTC), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 67
 testRunner.When("I request machine designs", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 68
 testRunner.Then(string.Format("the result should match the \"{0}\" from the repository", resultName), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("GetMachineDesigns For Date Range - Good Request: NoDateRange")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "GetMachineDesigns")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "NoDateRange")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "NoDateRange")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectUID", "ff91dd40-1569-4765-a2bc-014321f76ace")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:startUTC", "")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:endUTC", "")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ResultName", "NoDateRange")]
        public virtual void GetMachineDesignsForDateRange_GoodRequest_NoDateRange()
        {
#line 63
this.GetMachineDesignsForDateRange_GoodRequest("NoDateRange", "ff91dd40-1569-4765-a2bc-014321f76ace", "", "", "NoDateRange", ((string[])(null)));
#line hidden
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("GetMachineDesigns For Date Range - Good Request: WithDateRange")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "GetMachineDesigns")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "WithDateRange")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "WithDateRange")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectUID", "ff91dd40-1569-4765-a2bc-014321f76ace")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:startUTC", "")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:endUTC", "")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ResultName", "WithDateRange")]
        public virtual void GetMachineDesignsForDateRange_GoodRequest_WithDateRange()
        {
#line 63
this.GetMachineDesignsForDateRange_GoodRequest("WithDateRange", "ff91dd40-1569-4765-a2bc-014321f76ace", "", "", "WithDateRange", ((string[])(null)));
#line hidden
        }
    }
}
#pragma warning restore
#endregion
