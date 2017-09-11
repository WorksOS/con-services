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
    public partial class CompactionDesignProfileFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "CompactionDesignProfile.feature"
#line hidden
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitializeAttribute()]
        public static void FeatureSetup(Microsoft.VisualStudio.TestTools.UnitTesting.TestContext testContext)
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "CompactionDesignProfile", "I should be able to request Compaction Design Profile data.", ProgrammingLanguage.CSharp, ((string[])(null)));
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
                        && (TechTalk.SpecFlow.FeatureContext.Current.FeatureInfo.Title != "CompactionDesignProfile")))
            {
                ProductionDataSvc.AcceptanceTests.CompactionDesignProfileFeature.FeatureSetup(null);
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
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Compaction Get Slicer Design Profile")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "CompactionDesignProfile")]
        public virtual void CompactionGetSlicerDesignProfile()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Compaction Get Slicer Design Profile", ((string[])(null)));
#line 5
this.ScenarioSetup(scenarioInfo);
#line 6
 testRunner.Given("the Compaction Profile service URI \"/api/v2/profiles/design/slicer\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 7
  testRunner.And("a projectUid \"7925f179-013d-4aaf-aff4-7b9833bb06d6\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 8
  testRunner.And("a startLatDegrees \"36.207310\" and a startLonDegrees \"-115.019584\" and an endLatDe" +
                    "grees \"36.207322\" And an endLonDegrees \"-115.019574\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 9
  testRunner.And("a importedFileUid \"dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 10
  testRunner.And("a importedFileUid \"220e12e5-ce92-4645-8f01-1942a2d5a57f\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 11
 testRunner.When("I request a Compaction Design Profile", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 12
 testRunner.Then("the Compaction Design Profile should be", @"{
    ""gridDistanceBetweenProfilePoints"": 1.6069349835347946,
    ""results"": [
        {
            ""designFileUid"": ""dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff"",
            ""data"": [
                {
                    ""x"": 0,
                    ""y"": 597.4387
                },
                {
                    ""x"": 0.80197204271533173,
                    ""y"": 597.4356
                },
                {
                    ""x"": 1.6069349835347948,
                    ""y"": 597.434265
                }
            ]
        },
        {
            ""designFileUid"": ""220e12e5-ce92-4645-8f01-1942a2d5a57f"",
            ""data"": []
        }
    ],
    ""Code"": 0,
    ""Message"": ""success""
}", ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Compaction Get Slicer Empty Design Profile")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "CompactionDesignProfile")]
        public virtual void CompactionGetSlicerEmptyDesignProfile()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Compaction Get Slicer Empty Design Profile", ((string[])(null)));
#line 44
this.ScenarioSetup(scenarioInfo);
#line 45
 testRunner.Given("the Compaction Profile service URI \"/api/v2/profiles/design/slicer\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 46
  testRunner.And("a projectUid \"7925f179-013d-4aaf-aff4-7b9833bb06d6\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 47
  testRunner.And("a startLatDegrees \"36.209310\" and a startLonDegrees \"-115.019584\" and an endLatDe" +
                    "grees \"36.209322\" And an endLonDegrees \"-115.019574\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 48
  testRunner.And("a importedFileUid \"dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 49
 testRunner.When("I request a Compaction Design Profile", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 50
 testRunner.Then("the Compaction Design Profile should be", "{\n    \"gridDistanceBetweenProfilePoints\": 0,\n    \"results\": [\n        {\n         " +
                    "   \"designFileUid\": \"dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff\",\n            \"data\": " +
                    "[]\n        }\n    ],\n    \"Code\": 0,\n    \"Message\": \"success\"\n}", ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("Compaction Get Slicer Design Profile With Added Endpoints")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "CompactionDesignProfile")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.IgnoreAttribute()]
        public virtual void CompactionGetSlicerDesignProfileWithAddedEndpoints()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Compaction Get Slicer Design Profile With Added Endpoints", new string[] {
                        "ignore"});
#line 65
this.ScenarioSetup(scenarioInfo);
#line 66
 testRunner.Given("the Compaction Profile service URI \"/api/v2/profiles/design/slicer\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 67
  testRunner.And("a projectUid \"7925f179-013d-4aaf-aff4-7b9833bb06d6\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 68
  testRunner.And("a startLatDegrees \"36.207250\" and a startLonDegrees \"-115.019584\" and an endLatDe" +
                    "grees \"36.207322\" And an endLonDegrees \"-115.019574\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 69
  testRunner.And("a importedFileUid \"dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 70
 testRunner.When("I request a Compaction Design Profile", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 71
 testRunner.Then("the Compaction Design Profile should be", "{\n    \"gridDistanceBetweenProfilePoints\": 8.0405782488513378,\n    \"results\": [\n  " +
                    "      {\n            \"designFileUid\": \"dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff\",\n   " +
                    "         \"data\": [\n                {\n                    \"x\": 0,\n               " +
                    "     \"y\": \"NaN\"\n                },\n                {\n                    \"x\": 1." +
                    "4989359768016426,\n                    \"y\": 597.107849\n                },\n       " +
                    "         {\n                    \"x\": 2.4363884583427549,\n                    \"y\":" +
                    " 597.317444\n                },\n                {\n                    \"x\": 3.0398" +
                    "711084175734,\n                    \"y\": 597.4535\n                },\n             " +
                    "   {\n                    \"x\": 3.8040219479024642,\n                    \"y\": 597.4" +
                    "6875\n                },\n                {\n                    \"x\": 4.33875333089" +
                    "93378,\n                    \"y\": 597.4797\n                },\n                {\n  " +
                    "                  \"x\": 5.2303524427833983,\n                    \"y\": 597.466736\n " +
                    "               },\n                {\n                    \"x\": 5.5624914376821266," +
                    "\n                    \"y\": 597.4633\n                },\n                {\n        " +
                    "            \"x\": 6.7969372754612811,\n                    \"y\": 597.4468\n         " +
                    "       },\n                {\n                    \"x\": 7.7815432982108987,\n       " +
                    "             \"y\": 597.437439\n                },\n                {\n              " +
                    "      \"x\": 8.040578248851336,\n                    \"y\": 597.434265\n              " +
                    "  },\n                {\n                    \"x\": 8.0405782488513378,\n            " +
                    "        \"y\": \"NaN\"\n                }\n            ]\n        }\n    ],\n    \"Code\": " +
                    "0,\n    \"Message\": \"success\"\n}", ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
