using System;
using System.IO;
using System.Threading;
using log4net;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Productivity3D.TagFileHarvester;
using VSS.Productivity3D.TagFileHarvester.Implementation;
using VSS.Productivity3D.TagFileHarvester.Interfaces;
using VSS.Productivity3D.TagFileHarvester.Models;
using VSS.Productivity3D.TagFileHarvester.TaskQueues;
using VSS.Productivity3D.TagFileHarvesterTests.Mock;
using VSS.Productivity3D.TagFileHarvesterTests.MockRepositories;

namespace VSS.Productivity3D.TagFileHarvesterTests
{
  [TestClass]
  public class TasksTests
  {

    private static IUnityContainer unityContainer;
    private readonly MockFileRepository respositoryInstance = new MockFileRepository();
    private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType);

    [TestInitialize]
    public void Initialize()
    {
      unityContainer = new UnityContainer();
      unityContainer.RegisterInstance<IFileRepository>(respositoryInstance)
      .RegisterInstance<IHarvesterTasks>(new LimitedConcurrencyHarvesterTasks())
      .RegisterType<ITAGProcessorClient, MockRaptor>();
      unityContainer.RegisterInstance<ILog>(log);
      OrgsHandler.Clean();
      OrgsHandler.TagFileSubmitterTasksTimeout = TimeSpan.FromMinutes(5);
      respositoryInstance.Clean();
      OrgsHandler.Initialize(unityContainer);
    }

    [TestCleanup]
    public void TestCleanup()
    {
      unityContainer.RemoveAllExtensions();
    }

    [TestMethod]
    public void CanRunTagFileProcessTask()
    {
      /*
      var tagFileTask = new TagFileProcessTask(unityContainer);
      var result = tagFileTask.ProcessTagfile("test.tag", new Organization());
      Assert.AreEqual(TAGProcServiceDecls.TTAGProcServerProcessResult.tpsprOK,result);
       */
    }

    [TestMethod]
    public void CanRunProcessOrgTask()
    {
      /*
      var orgTask = new OrgProcessorTask(unityContainer, new Organization(), new CancellationToken());
      var result = orgTask.ProcessOrg(true);
      Assert.AreEqual(5,result.ErroneousFiles);
      Assert.AreEqual(75, result.ProcessedFiles);
       */
    }

    [TestMethod]
    [Ignore]
    public void CanHandleOrgProcessingAndCancel()
    {
      OrgsHandler.CheckAvailableOrgs(null);
      Thread.Sleep(TimeSpan.FromSeconds(2));

   //   OrgsHandler.OrgProcessingTasks.ForEach(t => t.Item3.Cancel());

    }
  }
}
