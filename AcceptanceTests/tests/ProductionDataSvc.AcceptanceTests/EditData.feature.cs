﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:1.9.0.77
//      SpecFlow Generator Version:1.9.0.0
//      Runtime Version:4.0.30319.42000
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
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "1.9.0.77")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute()]
    public partial class EditDataFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "EditData.feature"
#line hidden
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitializeAttribute()]
        public static void FeatureSetup(Microsoft.VisualStudio.TestTools.UnitTesting.TestContext testContext)
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "EditData", "I should be able to do and undo machine design and layer edits.", ProgrammingLanguage.CSharp, ((string[])(null)));
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
            if (((TechTalk.SpecFlow.FeatureContext.Current != null) 
                        && (TechTalk.SpecFlow.FeatureContext.Current.FeatureInfo.Title != "EditData")))
            {
                ProductionDataSvc.AcceptanceTests.EditDataFeature.FeatureSetup(null);
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
        }
        
        public virtual void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        public virtual void FeatureBackground()
        {
#line 4
#line 5
 testRunner.Given("the edit data service URI \"/api/v1/productiondata/edit\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 6
  testRunner.And("the get edit data service URI \"/api/v1/productiondata/getedits\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 7
  testRunner.And("all data edits are cleared for project 1001285", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table1 = new TechTalk.SpecFlow.Table(new string[] {
                        "EditID",
                        "assetId",
                        "startUTC",
                        "endUTC",
                        "onMachineDesignName",
                        "liftNumber"});
            table1.AddRow(new string[] {
                        "0",
                        "1",
                        "2014-11-25T00:00:00.000",
                        "2014-11-25T00:00:01.000",
                        "VirtualDesign",
                        "null"});
            table1.AddRow(new string[] {
                        "1",
                        "1",
                        "2014-11-25T00:00:02.000",
                        "2014-11-25T00:00:03.000",
                        "null",
                        "100"});
            table1.AddRow(new string[] {
                        "2",
                        "2",
                        "2014-11-25T00:00:04.000",
                        "2014-11-25T00:01:05.000",
                        "VirtualDesign",
                        "null"});
            table1.AddRow(new string[] {
                        "3",
                        "2",
                        "2014-11-25T00:00:06.000",
                        "2014-11-25T00:00:07.000",
                        "null",
                        "100"});
            table1.AddRow(new string[] {
                        "4",
                        "2",
                        "2014-11-25T00:27:45.432",
                        "2014-11-25T00:27:45.434",
                        "null",
                        "100"});
            table1.AddRow(new string[] {
                        "5",
                        "2",
                        "2014-11-25T00:27:45.432",
                        "2014-11-25T00:27:45.434",
                        "VirtualDesign",
                        "null"});
            table1.AddRow(new string[] {
                        "6",
                        "2",
                        "2014-11-25T00:27:45.432",
                        "2014-11-25T00:27:45.434",
                        "VirtualDesign",
                        "100"});
            table1.AddRow(new string[] {
                        "7",
                        "2",
                        "2014-11-25T00:27:45.434",
                        "2014-11-25T00:27:45.436",
                        "VirtualDesign",
                        "200"});
            table1.AddRow(new string[] {
                        "8",
                        "2",
                        "2014-11-25T00:27:45.432",
                        "2014-11-25T00:27:45.434",
                        "Random",
                        "Random"});
            table1.AddRow(new string[] {
                        "9",
                        "2",
                        "2014-11-25T00:27:55.376",
                        "2014-11-25T00:38:45.559",
                        "VirtualDesign",
                        "100"});
#line 10
  testRunner.And("the following data edit details", ((string)(null)), table1, "And ");
#line hidden
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("EditData - Insert Design Edit")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "EditData")]
        public virtual void EditData_InsertDesignEdit()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("EditData - Insert Design Edit", ((string[])(null)));
#line 23
this.ScenarioSetup(scenarioInfo);
#line 4
this.FeatureBackground();
#line hidden
            TechTalk.SpecFlow.Table table2 = new TechTalk.SpecFlow.Table(new string[] {
                        "EditId"});
            table2.AddRow(new string[] {
                        "0"});
            table2.AddRow(new string[] {
                        "2"});
#line 24
 testRunner.Given("I submit the following data edits to project 1001285", ((string)(null)), table2, "Given ");
#line 28
 testRunner.When("I try to get all edits for project 1001285", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table3 = new TechTalk.SpecFlow.Table(new string[] {
                        "EditId"});
            table3.AddRow(new string[] {
                        "0"});
            table3.AddRow(new string[] {
                        "2"});
#line 29
 testRunner.Then("the result should contain the following data edits", ((string)(null)), table3, "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("EditData - Insert Lift Edit")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "EditData")]
        public virtual void EditData_InsertLiftEdit()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("EditData - Insert Lift Edit", ((string[])(null)));
#line 34
this.ScenarioSetup(scenarioInfo);
#line 4
this.FeatureBackground();
#line hidden
            TechTalk.SpecFlow.Table table4 = new TechTalk.SpecFlow.Table(new string[] {
                        "EditId"});
            table4.AddRow(new string[] {
                        "1"});
            table4.AddRow(new string[] {
                        "3"});
#line 35
 testRunner.Given("I submit the following data edits to project 1001285", ((string)(null)), table4, "Given ");
#line 39
 testRunner.When("I try to get all edits for project 1001285", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table5 = new TechTalk.SpecFlow.Table(new string[] {
                        "EditId"});
            table5.AddRow(new string[] {
                        "1"});
            table5.AddRow(new string[] {
                        "3"});
#line 40
 testRunner.Then("the result should contain the following data edits", ((string)(null)), table5, "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("EditData - Lift and Design Edits Consolidation")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "EditData")]
        public virtual void EditData_LiftAndDesignEditsConsolidation()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("EditData - Lift and Design Edits Consolidation", ((string[])(null)));
#line 45
this.ScenarioSetup(scenarioInfo);
#line 4
this.FeatureBackground();
#line hidden
            TechTalk.SpecFlow.Table table6 = new TechTalk.SpecFlow.Table(new string[] {
                        "EditId"});
            table6.AddRow(new string[] {
                        "4"});
            table6.AddRow(new string[] {
                        "5"});
#line 46
 testRunner.Given("I submit the following data edits to project 1001285", ((string)(null)), table6, "Given ");
#line 50
 testRunner.When("I try to get all edits for project 1001285", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table7 = new TechTalk.SpecFlow.Table(new string[] {
                        "EditId"});
            table7.AddRow(new string[] {
                        "6"});
#line 51
 testRunner.Then("the result should contain the following data edits", ((string)(null)), table7, "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("EditData - Insert Temporally Contiguous Edits")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "EditData")]
        public virtual void EditData_InsertTemporallyContiguousEdits()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("EditData - Insert Temporally Contiguous Edits", ((string[])(null)));
#line 55
this.ScenarioSetup(scenarioInfo);
#line 4
this.FeatureBackground();
#line hidden
            TechTalk.SpecFlow.Table table8 = new TechTalk.SpecFlow.Table(new string[] {
                        "EditId"});
            table8.AddRow(new string[] {
                        "6"});
            table8.AddRow(new string[] {
                        "7"});
#line 56
 testRunner.Given("I submit the following data edits to project 1001285", ((string)(null)), table8, "Given ");
#line 60
 testRunner.When("I try to get all edits for project 1001285", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table9 = new TechTalk.SpecFlow.Table(new string[] {
                        "EditId"});
            table9.AddRow(new string[] {
                        "6"});
            table9.AddRow(new string[] {
                        "7"});
#line 61
 testRunner.Then("the result should contain the following data edits", ((string)(null)), table9, "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("EditData - Insert Lift Edit Overlapping Real Lift Exactly")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "EditData")]
        public virtual void EditData_InsertLiftEditOverlappingRealLiftExactly()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("EditData - Insert Lift Edit Overlapping Real Lift Exactly", ((string[])(null)));
#line 66
this.ScenarioSetup(scenarioInfo);
#line 4
this.FeatureBackground();
#line hidden
            TechTalk.SpecFlow.Table table10 = new TechTalk.SpecFlow.Table(new string[] {
                        "EditId"});
            table10.AddRow(new string[] {
                        "9"});
#line 67
 testRunner.Given("I submit the following data edits to project 1001285", ((string)(null)), table10, "Given ");
#line 70
 testRunner.When("I read back all machine designs from \"/api/v1/projects/{0}/machinedesigns\" for pr" +
                    "oject 1001285", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 71
  testRunner.And("I read back all lifts from \"/api/v1/projects/{0}/liftids\" for project 1001285", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table11 = new TechTalk.SpecFlow.Table(new string[] {
                        "EditId"});
            table11.AddRow(new string[] {
                        "9"});
#line 72
 testRunner.Then("the lift list should contain the lift details in the following data edits", ((string)(null)), table11, "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("EditData - Read Back Edits")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "EditData")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.IgnoreAttribute()]
        public virtual void EditData_ReadBackEdits()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("EditData - Read Back Edits", new string[] {
                        "ignore"});
#line 76
this.ScenarioSetup(scenarioInfo);
#line 4
this.FeatureBackground();
#line hidden
            TechTalk.SpecFlow.Table table12 = new TechTalk.SpecFlow.Table(new string[] {
                        "EditId"});
            table12.AddRow(new string[] {
                        "8"});
#line 77
 testRunner.Given("I submit the following data edits to project 1001285", ((string)(null)), table12, "Given ");
#line 80
 testRunner.When("I read back all machine designs from \"/api/v1/projects/{0}/machinedesigns\" for pr" +
                    "oject 1001285", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 81
  testRunner.And("I read back all lifts from \"/api/v1/projects/{0}/liftids\" for project 1001285", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table13 = new TechTalk.SpecFlow.Table(new string[] {
                        "EditId"});
            table13.AddRow(new string[] {
                        "8"});
#line 82
 testRunner.Then("the machine design list should contain the design details in the following data e" +
                    "dits", ((string)(null)), table13, "Then ");
#line hidden
            TechTalk.SpecFlow.Table table14 = new TechTalk.SpecFlow.Table(new string[] {
                        "EditId"});
            table14.AddRow(new string[] {
                        "8"});
#line 85
  testRunner.And("the lift list should contain the lift details in the following data edits", ((string)(null)), table14, "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("EditData - Undo Single Edit")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "EditData")]
        public virtual void EditData_UndoSingleEdit()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("EditData - Undo Single Edit", ((string[])(null)));
#line 89
this.ScenarioSetup(scenarioInfo);
#line 4
this.FeatureBackground();
#line hidden
            TechTalk.SpecFlow.Table table15 = new TechTalk.SpecFlow.Table(new string[] {
                        "EditId"});
            table15.AddRow(new string[] {
                        "0"});
#line 90
 testRunner.Given("I submit the following data edits to project 1001285", ((string)(null)), table15, "Given ");
#line 93
 testRunner.When("I try to get all edits for project 1001285", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            TechTalk.SpecFlow.Table table16 = new TechTalk.SpecFlow.Table(new string[] {
                        "EditId"});
            table16.AddRow(new string[] {
                        "0"});
#line 94
  testRunner.And("the result matches the following data edits", ((string)(null)), table16, "And ");
#line hidden
            TechTalk.SpecFlow.Table table17 = new TechTalk.SpecFlow.Table(new string[] {
                        "EditId"});
            table17.AddRow(new string[] {
                        "0"});
#line 97
  testRunner.And("I try to undo the following edits for project 1001285", ((string)(null)), table17, "And ");
#line 100
  testRunner.And("I try to get all edits for project 1001285", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 101
 testRunner.Then("the result should be empty", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("EditData - Use Lift Edit in Filter")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "EditData")]
        public virtual void EditData_UseLiftEditInFilter()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("EditData - Use Lift Edit in Filter", ((string[])(null)));
#line 103
this.ScenarioSetup(scenarioInfo);
#line 4
this.FeatureBackground();
#line hidden
            TechTalk.SpecFlow.Table table18 = new TechTalk.SpecFlow.Table(new string[] {
                        "EditId"});
            table18.AddRow(new string[] {
                        "4"});
#line 104
 testRunner.Given("I submit the following data edits to project 1001285", ((string)(null)), table18, "Given ");
#line 107
 testRunner.When("I request \"Height\" from resource \"/api/v1/productiondata/cells/datum\" at Grid Poi" +
                    "nt (381447.523, 806857.580) for project 1001285 filtered by EditId 4", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 108
 testRunner.Then("the datum should be: displayMode = \"0\", returnCode = \"0\", value = \"38.07300186157" +
                    "2266\", timestamp = \"2014-11-25T00:27:45.433\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("EditData - Use Design Edit in Filter")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "EditData")]
        public virtual void EditData_UseDesignEditInFilter()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("EditData - Use Design Edit in Filter", ((string[])(null)));
#line 110
this.ScenarioSetup(scenarioInfo);
#line 4
this.FeatureBackground();
#line hidden
            TechTalk.SpecFlow.Table table19 = new TechTalk.SpecFlow.Table(new string[] {
                        "EditId"});
            table19.AddRow(new string[] {
                        "5"});
#line 111
 testRunner.Given("I submit the following data edits to project 1001285", ((string)(null)), table19, "Given ");
#line 114
 testRunner.When("I request \"Height\" from resource \"/api/v1/productiondata/cells/datum\" at Grid Poi" +
                    "nt (381447.523, 806857.580) for project 1001285 filtered by EditId 5", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 115
 testRunner.Then("the datum should be: displayMode = \"0\", returnCode = \"0\", value = \"38.07300186157" +
                    "2266\", timestamp = \"2014-11-25T00:27:45.433\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("EditData - Use Both Lift and Design Edits in Filter")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "EditData")]
        public virtual void EditData_UseBothLiftAndDesignEditsInFilter()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("EditData - Use Both Lift and Design Edits in Filter", ((string[])(null)));
#line 117
this.ScenarioSetup(scenarioInfo);
#line 4
this.FeatureBackground();
#line hidden
            TechTalk.SpecFlow.Table table20 = new TechTalk.SpecFlow.Table(new string[] {
                        "EditId"});
            table20.AddRow(new string[] {
                        "6"});
#line 118
 testRunner.Given("I submit the following data edits to project 1001285", ((string)(null)), table20, "Given ");
#line 121
 testRunner.When("I request \"Height\" from resource \"/api/v1/productiondata/cells/datum\" at Grid Poi" +
                    "nt (381447.523, 806857.580) for project 1001285 filtered by EditId 6", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 122
 testRunner.Then("the datum should be: displayMode = \"0\", returnCode = \"0\", value = \"38.07300186157" +
                    "2266\", timestamp = \"2014-11-25T00:27:45.433\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("EditData - Bad Request (Insert Overlapping Edits)")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "EditData")]
        public virtual void EditData_BadRequestInsertOverlappingEdits()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("EditData - Bad Request (Insert Overlapping Edits)", ((string[])(null)));
#line 124
this.ScenarioSetup(scenarioInfo);
#line 4
this.FeatureBackground();
#line hidden
            TechTalk.SpecFlow.Table table21 = new TechTalk.SpecFlow.Table(new string[] {
                        "EditId"});
            table21.AddRow(new string[] {
                        "0"});
#line 125
 testRunner.Given("I submit the following data edits to project 1001285", ((string)(null)), table21, "Given ");
#line 128
  testRunner.And("I submit data edit with EditId 0 to project 1001285 expecting HttpResponseCode 40" +
                    "0", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 129
 testRunner.Then("I should get Error Code -1 and Message \"Data edit overlaps\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
