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
    public partial class ExportReportToSurfaceFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "ExportReportToSurface.feature"
#line hidden
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitializeAttribute()]
        public static void FeatureSetup(Microsoft.VisualStudio.TestTools.UnitTesting.TestContext testContext)
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "ExportReportToSurface", "I should be able to request production data to surface export report.", ProgrammingLanguage.CSharp, ((string[])(null)));
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
                        && (TechTalk.SpecFlow.FeatureContext.Current.FeatureInfo.Title != "ExportReportToSurface")))
            {
                ProductionDataSvc.AcceptanceTests.ExportReportToSurfaceFeature.FeatureSetup(null);
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
 testRunner.Given("the Export Report To Surface service URI \"/api/v2/export/surface\" and the result " +
                    "file \"ExportReportToSurfaceResponse.json\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
        }
        
        public virtual void ExportReportToSurface_GoodRequest(string requestName, string projectUID, string tolerance, string fileName, string resultName, string[] exampleTags)
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("ExportReportToSurface - Good Request", exampleTags);
#line 7
this.ScenarioSetup(scenarioInfo);
#line 4
this.FeatureBackground();
#line 8
  testRunner.And(string.Format("projectUid \"{0}\"", projectUID), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 9
 testRunner.And(string.Format("fileName is \"{0}\"", fileName), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 10
 testRunner.And(string.Format("tolerance \"{0}\"", tolerance), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 11
 testRunner.When("I request an Export Report To Surface", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 12
 testRunner.Then("the export result should successful", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("ExportReportToSurface - Good Request")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "ExportReportToSurface")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("VariantName", "With Tolerance")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "With Tolerance")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectUID", "7925f179-013d-4aaf-aff4-7b9833bb06d6")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Tolerance", "1.50")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:FileName", "SurfaceWithTolerance")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ResultName", "WithTolerance")]
        public virtual void ExportReportToSurface_GoodRequest_WithTolerance()
        {
            this.ExportReportToSurface_GoodRequest("With Tolerance", "7925f179-013d-4aaf-aff4-7b9833bb06d6", "1.50", "SurfaceWithTolerance", "WithTolerance", ((string[])(null)));
        }
        
        public virtual void ExportReportToSurface_GoodRequest_NoTolerance(string requestName, string projectUID, string fileName, string resultName, string[] exampleTags)
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("ExportReportToSurface - Good Request - No Tolerance", exampleTags);
#line 17
this.ScenarioSetup(scenarioInfo);
#line 4
this.FeatureBackground();
#line 18
  testRunner.And(string.Format("projectUid \"{0}\"", projectUID), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 19
 testRunner.And(string.Format("fileName is \"{0}\"", fileName), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 20
 testRunner.When("I request an Export Report To Surface", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 21
 testRunner.Then("the export result should successful", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("ExportReportToSurface - Good Request - No Tolerance")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "ExportReportToSurface")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectUID", "7925f179-013d-4aaf-aff4-7b9833bb06d6")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:FileName", "SurfaceNoTolerance")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ResultName", "NoTolerance")]
        public virtual void ExportReportToSurface_GoodRequest_NoTolerance_()
        {
            this.ExportReportToSurface_GoodRequest_NoTolerance("", "7925f179-013d-4aaf-aff4-7b9833bb06d6", "SurfaceNoTolerance", "NoTolerance", ((string[])(null)));
        }
        
        public virtual void ExportReportToSurface_BadRequest_NoProjectUID(string requestName, string tolerance, string fileName, string errorCode, string errorMessage, string[] exampleTags)
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("ExportReportToSurface - Bad Request - NoProjectUID", exampleTags);
#line 26
this.ScenarioSetup(scenarioInfo);
#line 4
this.FeatureBackground();
#line 27
 testRunner.And(string.Format("fileName is \"{0}\"", fileName), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 28
 testRunner.And(string.Format("tolerance \"{0}\"", tolerance), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 29
 testRunner.When("I request an Export Report To Surface expecting Unauthorized", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 30
 testRunner.Then(string.Format("the report result should contain error code {0} and error message \"{1}\"", errorCode, errorMessage), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("ExportReportToSurface - Bad Request - NoProjectUID")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "ExportReportToSurface")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Tolerance", "0.05")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:FileName", "Test")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ErrorCode", "-5")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ErrorMessage", "Missing Project or project does not belong to specified customer or don\'t have ac" +
            "cess to the project")]
        public virtual void ExportReportToSurface_BadRequest_NoProjectUID_()
        {
            this.ExportReportToSurface_BadRequest_NoProjectUID("", "0.05", "Test", "-5", "Missing Project or project does not belong to specified customer or don\'t have ac" +
                    "cess to the project", ((string[])(null)));
        }
        
        public virtual void ExportReportToSurface_NoContent_NoFileName(string requestName, string projectUID, string tolerance, string[] exampleTags)
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("ExportReportToSurface - No Content - NoFileName", exampleTags);
#line 35
this.ScenarioSetup(scenarioInfo);
#line 4
this.FeatureBackground();
#line 36
 testRunner.And(string.Format("projectUid \"{0}\"", projectUID), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 37
 testRunner.And(string.Format("tolerance \"{0}\"", tolerance), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 38
 testRunner.When("I request an Export Report To Surface expecting NoContent", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("ExportReportToSurface - No Content - NoFileName")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "ExportReportToSurface")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectUID", "7925f179-013d-4aaf-aff4-7b9833bb06d6")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Tolerance", "0.05")]
        public virtual void ExportReportToSurface_NoContent_NoFileName_()
        {
            this.ExportReportToSurface_NoContent_NoFileName("", "7925f179-013d-4aaf-aff4-7b9833bb06d6", "0.05", ((string[])(null)));
        }
        
        public virtual void ExportReportToSurface_GoodRequestWithFilter(string requestName, string projectUID, string filterUID, string tolerance, string fileName, string[] exampleTags)
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("ExportReportToSurface - Good Request with Filter", exampleTags);
#line 43
this.ScenarioSetup(scenarioInfo);
#line 4
this.FeatureBackground();
#line 44
  testRunner.And(string.Format("projectUid \"{0}\"", projectUID), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 45
  testRunner.And(string.Format("tolerance \"{0}\"", tolerance), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 46
 testRunner.And(string.Format("filterUid \"{0}\"", filterUID), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 47
  testRunner.And(string.Format("fileName is \"{0}\"", fileName), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 48
 testRunner.When("I request an Export Report To Surface", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 49
 testRunner.Then("the export result should successful", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute()]
        [Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute("ExportReportToSurface - Good Request with Filter")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("FeatureTitle", "ExportReportToSurface")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:RequestName", "")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:ProjectUID", "7925f179-013d-4aaf-aff4-7b9833bb06d6")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:FilterUID", "d7cb424d-b012-4618-b3bc-e526ca84bbd6")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:Tolerance", "0.05")]
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute("Parameter:FileName", "SurfanceWithTolerance")]
        public virtual void ExportReportToSurface_GoodRequestWithFilter_()
        {
            this.ExportReportToSurface_GoodRequestWithFilter("", "7925f179-013d-4aaf-aff4-7b9833bb06d6", "d7cb424d-b012-4618-b3bc-e526ca84bbd6", "0.05", "SurfanceWithTolerance", ((string[])(null)));
        }
    }
}
#pragma warning restore
#endregion
