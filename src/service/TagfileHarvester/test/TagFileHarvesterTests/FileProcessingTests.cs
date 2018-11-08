using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using log4net;
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
  public class FileProcessingTests
  {

    private static IUnityContainer unityContainer;
    private readonly MockFileRepository respositoryInstance = new MockFileRepository();
    private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType);

    [TestInitialize]
    public void Initialize()
    {
      unityContainer = new UnityContainer();
      unityContainer.RegisterInstance<IFileRepository>(respositoryInstance);
      unityContainer.RegisterType<IHarvesterTasks, MockTaskScheduler>();
      unityContainer.RegisterInstance<ILog>(log);
      OrgsHandler.Clean();
      respositoryInstance.Clean();
      OrgsHandler.Initialize(unityContainer);
    }

    [TestCleanup]
    public void TestCleanup()
    {
      unityContainer.RemoveAllExtensions();
      
     }

    [TestMethod]
    public void CanRemoveAbsentOrgs()
    {
      OrgsHandler.CheckAvailableOrgs(null);
      //Save here two items from the list of created tasks
      var org1 = OrgsHandler.OrgProcessingTasks.First().Value;
      var org2 = OrgsHandler.OrgProcessingTasks.Last().Value;
      //Remove these orgs from the list of orgs
      respositoryInstance.DeleteOrgs(new Organization[]{org1.Item1, org2.Item1});
      OrgsHandler.CheckAvailableOrgs(null);
      Assert.AreEqual(MockFileRepository.OrgIncrement * 2 - 2, OrgsHandler.OrgProcessingTasks.Count);
    }

    [TestMethod]
    public void CanAddMoreOrgsToExisting()
    {
      OrgsHandler.CheckAvailableOrgs(null);
      //Add here more orgs here
      //And make sure that they are merged
      OrgsHandler.CheckAvailableOrgs(null);
      Assert.AreEqual(MockFileRepository.OrgIncrement*2,OrgsHandler.OrgProcessingTasks.Count);

    }

    [TestMethod]
    public void CanAddNewlyFoundOrgs()
    {
      OrgsHandler.CheckAvailableOrgs(null);
      Assert.AreEqual(MockFileRepository.OrgIncrement, OrgsHandler.OrgProcessingTasks.Count);
    }

    [TestMethod]
    public void CanListOrgs()
    {
      Assert.AreEqual(MockFileRepository.OrgIncrement, OrgsHandler.GetOrgs().Count);
    }
  }
}
