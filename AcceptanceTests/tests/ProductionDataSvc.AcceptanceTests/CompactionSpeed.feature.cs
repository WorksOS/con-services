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
    public partial class CompactionSpeedFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "CompactionSpeed.feature"
#line hidden
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitializeAttribute()]
        public static void FeatureSetup(Microsoft.VisualStudio.TestTools.UnitTesting.TestContext testContext)
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "CompactionSpeed", "I should be able to request compaction speed data", ProgrammingLanguage.CSharp, ((string[])(null)));
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
                        && (TechTalk.SpecFlow.FeatureContext.Current.FeatureInfo.Title != "CompactionSpeed")))
            {
                ProductionDataSvc.AcceptanceTests.CompactionSpeedFeature.FeatureSetup(null);
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
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Compaction Get Speed Summary")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "CompactionSpeed")]
        public virtual void CompactionGetSpeedSummary()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Compaction Get Speed Summary", ((string[])(null)));
#line 4
this.ScenarioSetup(scenarioInfo);
#line 5
testRunner.Given("the Compaction Speed Summary service URI \"/api/v2/compaction/speed/summary\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 6
testRunner.And("a projectUid \"ff91dd40-1569-4765-a2bc-014321f76ace\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 7
testRunner.When("I request Speed summary", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 8
testRunner.Then("the Speed result should be", "{\n\"speedSummaryData\": {\n    \"percentEqualsTarget\": 36.8,\n    \"percentGreaterThanT" +
                    "arget\": 39.1,\n    \"percentLessThanTarget\": 24.1,\n    \"totalAreaCoveredSqMeters\":" +
                    " 10636.7028,\n    \"minTarget\": 5.0,\n    \"maxTarget\": 10.0\n},\n\"Code\": 0,\n\"Message\"" +
                    ": \"success\"\n}", ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Compaction Get Speed Summary with summary settings")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "CompactionSpeed")]
        public virtual void CompactionGetSpeedSummaryWithSummarySettings()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Compaction Get Speed Summary with summary settings", ((string[])(null)));
#line 24
this.ScenarioSetup(scenarioInfo);
#line 25
testRunner.Given("the Compaction Speed Summary service URI \"/api/v2/compaction/speed/summary\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 26
testRunner.And("a projectUid \"3335311a-f0e2-4dbe-8acd-f21135bafee4\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 27
testRunner.When("I request Speed summary", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 28
testRunner.Then("the Speed result should be", @"{
  ""speedSummaryData"": {
    ""percentEqualsTarget"": 25.1,
    ""percentGreaterThanTarget"": 34.8,
    ""percentLessThanTarget"": 40.1,
    ""totalAreaCoveredSqMeters"": 10636.7028,
    ""minTarget"": 7.0,
    ""maxTarget"": 11.0
  },
  ""Code"": 0,
  ""Message"": ""success""
}", ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
