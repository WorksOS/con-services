using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using VSS.Project.Service.Utils;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using VSS.Project.Data;
using VSS.Project.Data.Models;
using VSS.Customer.Data;

namespace RepositoryTests
{
  [TestClass]
  public class SubscriptionRepositoryTests
  {
    [TestInitialize]
    public void Init()
    {
      var serviceCollection = new ServiceCollection();
      serviceCollection.AddSingleton<ILoggerFactory>((new LoggerFactory()).AddDebug());
      new DependencyInjectionProvider(serviceCollection.BuildServiceProvider());
    }

    #region Subscription

    /// <summary>
    /// Create Subscription - Happy path i.e. 
    ///   customer, project, CustomerProject relationship exists
    ///   project doesn't exist already.
    /// </summary>
    [TestMethod]
    public void CreateSubscriptionWithProject_HappyPath()
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Create Subscription - Happy path out of order
    ///  same as above but out of order
    /// </summary>
    [TestMethod]
    public void CreateSubscriptionWithProject_HappyPathButOutOfOrder()
    {
      throw new NotImplementedException();
    }


    /// <summary>
    /// Create Subscription - Happy path RelationShips not setup i.e. 
    ///   customer and CustomerProject relationship NOT added
    ///   Subscription doesn't exist already.
    /// </summary>
    [TestMethod]
    public void CreateSubscriptionWithProject_HappyPath_NoProject()
    {
      throw new NotImplementedException();
    }


    /// <summary>
    /// Create Subscription - Subscription already exists
    ///   customer and CustomerProject relationship also added
    ///   Subscription exists but is different.
    /// </summary>
    [TestMethod]
    public void CreateSubscriptionWithProject_SubscriptionExists()
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Update Subscription - Happy path i.e. 
    ///   customer and CustomerProject relationship also added
    ///   Subscription exists and New ActionUTC is later than its LastActionUTC.
    /// </summary>
    [TestMethod]
    public void UpdateSubscription_HappyPath()
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Update Subscription - earlier ActionUTC 
    ///   customer and CustomerProject relationship also added
    ///   Subscription exists and New ActionUTC is earlier than its LastActionUTC.
    /// </summary>
    [TestMethod]
    public void UpdateSubscription_OldUpdate()
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Create Customer Subscription - not supported?
    /// </summary>
    [TestMethod]
    public void CreateCustomerSubscription_NotSupported()
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Update Customer Subscription - not supported?
    /// </summary>
    [TestMethod]
    public void UpdateCustomerSubscription_NotSupported()
    {
      throw new NotImplementedException();
    }


    /// <summary>
    /// Associate Customer Subscription - not supported?
    /// </summary>
    [TestMethod]
    public void AssociateCustomerSubscription_NotSupported()
    {
      throw new NotImplementedException();
    }
    #endregion


    #region associateWithProject

    /// <summary>
    /// AssociateProjectSubscriptionEvent - Happy Path
    ///   project and sub added.
    /// </summary>
    [TestMethod]
    public void AssociateProjectSubscriptionEvent_HappyPath()
    {      
      throw new NotImplementedException();
    }

    /// <summary>
    /// AssociateProjectSubscriptionEvent - already exists
    /// </summary>
    [TestMethod]
    public void AssociateProjectSubscriptionEvent_AlreadyExists()
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Dissociate ProjectSubscription - not needed?
    /// </summary>
    [TestMethod]
    public void DissociateProjectSubscription_NotSupported()
    {
      throw new NotImplementedException();
    }
    #endregion

    #region private
    private CreateProjectEvent CopyModel(Project project)
    {
      return new CreateProjectEvent()
      {
        ProjectUID = Guid.Parse(project.ProjectUID),
        ProjectID = project.LegacyProjectID,
        ProjectName = project.Name,
        ProjectType = project.ProjectType,
        ProjectTimezone = project.ProjectTimeZone,

        ProjectStartDate = project.StartDate,
        ProjectEndDate = project.EndDate,
        ActionUTC = project.LastActionedUTC
      };
    }

    private Project CopyModel(CreateProjectEvent kafkaProjectEvent)
    {
      return new Project()
      {
        ProjectUID = kafkaProjectEvent.ProjectUID.ToString(),
        LegacyProjectID = kafkaProjectEvent.ProjectID,
        Name = kafkaProjectEvent.ProjectName,
        ProjectType = kafkaProjectEvent.ProjectType,
        // IsDeleted =  N/A

        ProjectTimeZone = kafkaProjectEvent.ProjectTimezone,
        LandfillTimeZone = TimeZone.WindowsToIana(kafkaProjectEvent.ProjectTimezone),

        LastActionedUTC = kafkaProjectEvent.ActionUTC,
        StartDate = kafkaProjectEvent.ProjectStartDate,
        EndDate = kafkaProjectEvent.ProjectEndDate
      };
    }
    #endregion

  }
}
 
 