using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtility;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace WebApiTests
{
  [TestClass]
  public class ProjectTests
  {
    [TestMethod]
    public void Create_Project_All_Ok()
    {
      var msg = new Msg();
      msg.Title("projects 1", "Create a project");
      var mysql = new MySqlHelper();

      var testSupport = new TestSupport();
      testSupport.CreateProjectViaWebApi("project 1", ProjectType.Standard, testSupport.FirstEventDate, testSupport.FirstEventDate.AddMonths(3), "New Zealand Standard Time", DateTime.UtcNow);
      mysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, testSupport.ProjectUid);             // Verify the result in the database      
      //TODO:
      //Need a kafka consumer
      //Add method to TestSupport to consume events similat to WriteAListOfMachineEventsToKafka
      //Add method to TestSupport like InjectEventsIntoKafka for waiting and consuming
      //Wrap all this in a method to verify expected events from kafka
      //UPDATE of TODO: full integration will use project kafka consumer to write to another DB which can then be checked for record
    }

    [TestMethod]
    public void Create_Project_Twice()
    {


    }

    [TestMethod]
    public void Create_Project_Bad_Data()
    {


    }

    [TestMethod]
    public void Update_Project_After_Create()
    {

    }

    [TestMethod]
    public void Update_Project_Before_Create()
    {

    }


    [TestMethod]
    public void Update_Project_Bad_Data()
    {

    }

    [TestMethod]
    public void Delete_Project_After_Create()
    {

    }

    [TestMethod]
    public void Delete_Project_Before_Create()
    {

    }


    [TestMethod]
    public void Delete_Project_Bad_Data()
    {

    }

    [TestMethod]
    public void Associate_Customer_Project_After_Create()
    {

    }

    [TestMethod]
    public void Associate_Customer_Project_Before_Create()
    {

    }


    [TestMethod]
    public void Associate_Customer_Project_Bad_Data()
    {

    }

    [TestMethod]
    public void Dissociate_Customer_Project_After_Associate()
    {

    }

    [TestMethod]
    public void Dissociate_Customer_Project_Before_Associate()
    {
      //project exists but not associated
    }


    [TestMethod]
    public void Dissociate_Customer_Project_Bad_Data()
    {
      //invalid guids
      //project doesn't exist
    }

    [TestMethod]
    public void Associate_Geofence_Project_After_Create()
    {

    }

    [TestMethod]
    public void Associate_Geofence_Project_Before_Create()
    {

    }


    [TestMethod]
    public void Associate_Geofence_Project_Bad_Data()
    {

    }

  }
}
