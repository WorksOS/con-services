using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtility.Model.TestEvents;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using ProjectWebApi.Models;
using Repositories.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;


namespace TestUtility
{
  public class TestSupport
  {
    #region Public Properties

    public string AssetUid { get; set; }
    public DateTime FirstEventDate { get; set; }
    public DateTime LastEventDate { get; set; }
    // public AssetConfigData AssetConfig { get; set; }
    public Customer MockCustomer { get; set; }
    public Subscription MockSubscription { get; set; }
    public ProjectSubscription MockProjectSubscription { get; set; }
    public Guid ProjectUid { get; set; }
    public Guid CustomerUid { get; set; }
    public Guid GeofenceUid { get; set; }
    public Guid SubscriptionUid { get; set; }

    public CreateProjectEvent CreateProjectEvt { get; set; }
    public UpdateProjectEvent UpdateProjectEvt { get; set; }
    public DeleteProjectEvent DeleteProjectEvt { get; set; }
    public AssociateProjectCustomer AssociateCustomerProjectEvt { get; set; }
    public DissociateProjectCustomer DissociateCustomerProjectEvt { get; set; }
    public AssociateProjectGeofence AssociateProjectGeofenceEvt { get; set; }
    #endregion

    #region Private Properties

    private readonly Random rndNumber = new Random();
    private readonly object syncLock = new object();
    private const char SEPARATOR = '|';
    private readonly TestConfig appConfig = new TestConfig();
    private readonly Msg msg = new Msg();

    #endregion

    #region Public Methods

    public TestSupport()
    {
      SetFirstEventDate();
      SetLastEventDate();
      SetAssetUid();
      SetProjectUid();
      SetCustomerUid();
      SetGeofenceUid();
      SetSubscriptionUid();
    }

    /// <summary>
    /// Set up the first event date for the events to go in. Also used as project start date for project tests.
    /// </summary>
    public void SetFirstEventDate()
    {
      FirstEventDate = DateTime.SpecifyKind(DateTime.Today.AddDays(-RandomNumber(10, 360)), DateTimeKind.Unspecified);
    }

    /// <summary>
    /// Set up the last event date for the events to go in. Also used as project end date for project tests.
    /// </summary>
    public void SetLastEventDate()
    {
      LastEventDate = FirstEventDate.AddYears(2);
    }

    /// <summary>
    /// Set the asset UID to a random GUID
    /// </summary>
    public void SetAssetUid()
    {
      AssetUid = Guid.NewGuid().ToString();
    }

    /// <summary>
    /// Set the project UID to a random GUID
    /// </summary>
    public void SetProjectUid()
    {
      ProjectUid = Guid.NewGuid();
    }

    /// <summary>
    /// Set the customer UID to a random GUID
    /// </summary>
    public void SetCustomerUid()
    {
      CustomerUid = Guid.NewGuid();
    }

    /// <summary>
    /// Set the geofence UID to a random GUID
    /// </summary>
    public void SetGeofenceUid()
    {
      GeofenceUid = Guid.NewGuid();
    }

    /// <summary>
    /// Set the subscription UID to a random GUID
    /// </summary>
    public void SetSubscriptionUid()
    {
      SubscriptionUid = Guid.NewGuid();
    }

    /// <summary>
    /// Inject all events from the test into kafka
    /// </summary>
    /// <param name="eventArray">all of the events</param>
    public void InjectEventsIntoKafka(string[] eventArray)
    {
      msg.DisplayEventsToConsole(eventArray);
      var allEvents = ConvertArrayToList<EventTable>(eventArray);
      WriteAListOfProjectEventsToKafka(allEvents);
      WaitForTimeBasedOnNumberOfRecords(allEvents.Count);
    }

    /// <summary>
    /// Inject events into database for existing asset
    /// </summary>
    /// <param name="eventArray">all of the events</param>
    /// <param name="isSameAsset">Set to false to create a new asset</param>
    public void InjectEventsIntoMySqlDatabase(string[] eventArray, bool isSameAsset = true)
    {
      msg.DisplayEventsForDbInjectToConsole(eventArray);
      var allEvents = ConvertArrayToList<EventTable>(eventArray);
      WriteAListOfMachineEventsToMySqlDatabase(allEvents, isSameAsset);
    }

    /// <summary>
    /// Create the project via the web api. 
    /// </summary>
    /// <param name="projectUid">project UID</param>
    /// <param name="projectId">legacy project id</param>
    /// <param name="name">project name</param>
    /// <param name="startDate">project start date</param>
    /// <param name="endDate">project end date</param>
    /// <param name="projectType">project type</param>
    /// <param name="timezone">project time zone</param>
    /// <param name="actionUtc">timestamp of the event</param>
    /// <param name="statusCode">expected status code from web api call</param>
    public void CreateProjectViaWebApi(Guid projectUid, int projectId, string name, DateTime startDate, DateTime endDate,
      string timezone, ProjectType projectType, DateTime actionUtc, string boundary, HttpStatusCode statusCode)
    {
      CreateProjectEvt = new CreateProjectEvent
      {
        ProjectID = projectId,
        ProjectUID = projectUid,
        ProjectName = name,
        ProjectType = projectType,
        ProjectBoundary = boundary,
        ProjectStartDate = startDate,
        ProjectEndDate = endDate,
        ProjectTimezone = timezone,
        ActionUTC = actionUtc
      };
      CallProjectWebApi(CreateProjectEvt, string.Empty, statusCode, "Create");
    }

    /// <summary>
    /// Update the project via the web api. 
    /// </summary>
    /// <param name="projectUid">project UID</param>
    /// <param name="name">project name</param>
    /// <param name="endDate">project end date</param>
    /// <param name="timezone">project time zone</param>
    /// <param name="actionUtc">timestamp of the event</param>
    /// <param name="statusCode">expected status code from web api call</param>
    public void UpdateProjectViaWebApi(Guid projectUid, string name, DateTime endDate, string timezone, DateTime actionUtc, HttpStatusCode statusCode, ProjectType projectType = ProjectType.Standard)
    {
      UpdateProjectEvt = new UpdateProjectEvent
      {
        ProjectUID = projectUid,
        ProjectName = name,
        ProjectType = projectType,
        ProjectEndDate = endDate,
        ProjectTimezone = timezone,
        ActionUTC = actionUtc
      };
      CallProjectWebApi(UpdateProjectEvt, string.Empty, statusCode, "Update", "PUT");
    }

    /// <summary>
    /// Delete the project via the web api. 
    /// </summary>
    /// <param name="projectUid">project UID</param>
    /// <param name="actionUtc">timestamp of the event</param>
    /// <param name="statusCode">expected status code from web api call</param>
    public void DeleteProjectViaWebApi(Guid projectUid, DateTime actionUtc, HttpStatusCode statusCode)
    {
      DeleteProjectEvt = new DeleteProjectEvent
      {
        ProjectUID = projectUid,
        ActionUTC = actionUtc
      };
      CallProjectWebApi(DeleteProjectEvt, string.Empty, statusCode, "Delete", "DELETE");
    }

    /// <summary>
    /// Associate a customer and project via the web api. 
    /// </summary>
    /// <param name="projectUid">project UID</param>
    /// <param name="customerUid">customer UID</param>
    /// <param name="customerId">legacy customer ID</param>
    /// <param name="actionUtc">timestamp of the event</param>
    /// <param name="statusCode">expected status code from web api call</param>
    public void AssociateCustomerProjectViaWebApi(Guid projectUid, Guid customerUid, int customerId, DateTime actionUtc, HttpStatusCode statusCode)
    {
      AssociateCustomerProjectEvt = new AssociateProjectCustomer
      {
        ProjectUID = projectUid,
        CustomerUID = customerUid,
        LegacyCustomerID = customerId,
        RelationType = RelationType.Customer,
        ActionUTC = actionUtc
      };
      CallProjectWebApi(AssociateCustomerProjectEvt, "AssociateCustomer", statusCode, "Associate customer");
    }

    /// <summary>
    /// Dissociate a customer and project via the web api. 
    /// </summary>
    /// <param name="projectUid">project UID</param>
    /// <param name="customerUid">customer UID</param>
    /// <param name="actionUtc">timestamp of the event</param>
    /// <param name="statusCode">expected status code from web api call</param>
    public void DissociateProjectViaWebApi(Guid projectUid, Guid customerUid, DateTime actionUtc, HttpStatusCode statusCode)
    {
      DissociateCustomerProjectEvt = new DissociateProjectCustomer
      {
        ProjectUID = projectUid,
        CustomerUID = customerUid,
        ActionUTC = actionUtc
      };
      CallProjectWebApi(DissociateCustomerProjectEvt, "DissociateCustomer", statusCode, "Dissociate customer");
    }

    /// <summary>
    /// Associate a geofence and project via the web api. 
    /// </summary>
    /// <param name="projectUid">project UID</param>
    /// <param name="geofenceUid">geofence UID</param>
    /// <param name="actionUtc">timestamp of the event</param>
    /// <param name="statusCode">expected status code from web api call</param>
    public void AssociateGeofenceProjectViaWebApi(Guid projectUid, Guid geofenceUid, DateTime actionUtc, HttpStatusCode statusCode)
    {
      AssociateProjectGeofenceEvt = new AssociateProjectGeofence
      {
        ProjectUID = projectUid,
        GeofenceUID = geofenceUid,
        ActionUTC = actionUtc
      };
      CallProjectWebApi(AssociateProjectGeofenceEvt, "AssociateGeofence", statusCode, "Associate geofence");
    }

    public void GetProjectsViaWebApiAndCompareActualWithExpected(HttpStatusCode statusCode, Guid customerUid, string[] expectedResultsArray)
    {
      var response = CallProjectWebApi(null, null, statusCode, "Get", "GET", customerUid == Guid.Empty ? null : customerUid.ToString());
      if (statusCode == HttpStatusCode.OK)
      {
        var actualProjects = JsonConvert.DeserializeObject<List<ProjectDescriptor>>(response).OrderBy(p => p.ProjectUid).ToList();
        var expectedProjects =
          ConvertArrayToList<ProjectDescriptor>(expectedResultsArray).OrderBy(p => p.ProjectUid).ToList();
        msg.DisplayResults("Expected projects :" + JsonConvert.SerializeObject(expectedProjects),
          "Actual from WebApi: " + response);
        CollectionAssert.AreEqual(expectedProjects, actualProjects);
      }
    }

    /// <summary>
    /// Call the project web api
    /// </summary>
    /// <param name="evt">THe project event containing the data</param>
    /// <param name="routeSuffix">suffix to add to base uri if required</param>
    /// <param name="statusCode">expected return code of the web api call</param>
    /// <param name="what">name of the api being called for logging</param>
    /// <param name="method">http method</param>
    /// <param name="customerUid">Customer UID to add to http headers</param>
    /// <returns>The web api response</returns>
    private string CallProjectWebApi(IProjectEvent evt, string routeSuffix, HttpStatusCode statusCode, string what, string method = "POST", string customerUid = null)
    {
      var configJson = evt == null ? null : JsonConvert.SerializeObject(evt, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
      var restClient = new RestClientUtil();
      var response = restClient.DoHttpRequest(GetBaseUri() + routeSuffix, method, "application/json", configJson, statusCode, customerUid);
      Console.WriteLine(what + " project response:" + response);
      return response;
    }

    /// <summary>
    /// Inject the MockCustomer
    /// </summary>
    /// <param name="customerUid">Customer UID</param>
    /// <param name="name">Customer name</param>
    /// <param name="type">Customer type</param>
    public void CreateMockCustomer(Guid customerUid, string name, CustomerType type)
    {
      MockCustomer = new Customer
      {
        CustomerUID = customerUid.ToString(),
        Name = name,
        CustomerType = type,
        IsDeleted = false,
        LastActionedUTC = DateTime.UtcNow
      };
      var customerTypeId = (int)MockCustomer.CustomerType;
      var deleted = MockCustomer.IsDeleted ? 1 : 0;
      var query = $@"INSERT INTO `{appConfig.dbSchema}`.{"Customer"} 
                            (CustomerUID,Name,fk_CustomerTypeID,IsDeleted,LastActionedUTC) VALUES
                            ('{MockCustomer.CustomerUID}','{MockCustomer.Name}',{customerTypeId},{deleted},'{MockCustomer.LastActionedUTC:yyyy-MM-dd HH\:mm\:ss.fffffff}');";
      var mysqlHelper = new MySqlHelper();
      mysqlHelper.ExecuteMySqlInsert(appConfig.dbConnectionString, query);
    }

    /// <summary>
    /// Inject the MockSubscription and MockProjectSubscription
    /// </summary>
    /// <param name="projectUid">Project UID</param>
    /// <param name="subscriptionUid">Subscription UID</param>
    /// <param name="customerUid">Customer UID</param>
    /// <param name="startDate">Start date of the subscription</param>
    /// <param name="endDate">End date of the subscription</param>
    /// <param name="effectiveDate">Date at which the subscripton takes effect for the project</param>
    public void CreateMockProjectSubscription(string projectUid, string subscriptionUid, string customerUid, DateTime startDate, DateTime endDate, DateTime effectiveDate)
    {
      MockSubscription = new Subscription
      {
        SubscriptionUID = subscriptionUid,
        CustomerUID = customerUid,
        ServiceTypeID = 20,//19=Landfill, 20=Project Monitoring
        StartDate = startDate,
        EndDate = endDate,
        LastActionedUTC = DateTime.UtcNow
      };
      var query = $@"INSERT INTO `{appConfig.dbSchema}`.{"Subscription"} 
                            (SubscriptionUID,fk_CustomerUID,fk_ServiceTypeID,StartDate,EndDate,LastActionedUTC) VALUES
                            ('{MockSubscription.SubscriptionUID}','{MockSubscription.CustomerUID}',{MockSubscription.ServiceTypeID},'{MockSubscription.StartDate:yyyy-MM-dd HH}','{MockSubscription.EndDate:yyyy-MM-dd}','{MockSubscription.LastActionedUTC:yyyy-MM-dd HH\:mm\:ss.fffffff}');";
      var mysqlHelper = new MySqlHelper();
      mysqlHelper.ExecuteMySqlInsert(appConfig.dbConnectionString, query);

      MockProjectSubscription = new ProjectSubscription
      {
        SubscriptionUID = subscriptionUid,
        ProjectUID = projectUid,
        EffectiveDate = effectiveDate,
        LastActionedUTC = DateTime.UtcNow
      };
      query = $@"INSERT INTO `{appConfig.dbSchema}`.{"ProjectSubscription"} 
                            (fk_SubscriptionUID,fk_ProjectUID,EffectiveDate,LastActionedUTC) VALUES
                            ('{MockProjectSubscription.SubscriptionUID}','{MockProjectSubscription.ProjectUID}','{MockProjectSubscription.EffectiveDate:yyyy-MM-dd}','{MockProjectSubscription.LastActionedUTC:yyyy-MM-dd HH\:mm\:ss.fffffff}');";
      mysqlHelper.ExecuteMySqlInsert(appConfig.dbConnectionString, query);
    }

    /// <summary>
    /// Check if the test is being debugged in VS. Set to different endpoind
    /// </summary>
    /// <returns></returns>
    public string GetBaseUri()
    {
      var baseUri = appConfig.webApiUri;
      if (Debugger.IsAttached)
      {
        baseUri = appConfig.debugWebApiUri;
      }
      return baseUri;
    }

    #endregion

    #region Private Methods
    /// <summary>
    /// Publish event to kafka
    /// </summary>
    /// <param name="allEvents">List of events in a inputEvent class format</param>
    private void WriteAListOfProjectEventsToKafka(List<EventTable> allEvents)
    {
      var kafkaDriver = new RdKafkaDriver();
      foreach (var singleEvent in allEvents)
      {
        // var dayOffSet = Convert.ToInt32(singleEvent.DayOffset);
        //var eventDate = FirstEventDate.AddDays(dayOffSet) + DateTime.ParseExact(singleEvent.Timestamp, "HH:mm:ss", CultureInfo.InvariantCulture).TimeOfDay;
        var eventDate = DateTime.Parse(singleEvent.EventDate, CultureInfo.InvariantCulture);
        var eventUtc = eventDate;    // eventUtc is eventdate without offset applied
        var deviceTime = singleEvent.UtcOffsetHours == "nullOffset" ? null : DateTimeExtensions.ToIso8601DateTime(eventUtc, Convert.ToDouble(singleEvent.UtcOffsetHours));
        LastEventDate = eventDate;   // Always set the event date to be the last one. Assume the go in sequential order. 
        var jsonSettings = new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified };
        var topicName = appConfig.masterDataTopic + singleEvent.EventType + appConfig.kafkaTopicSuffix;

        switch (singleEvent.EventType)
        {
          #region Customer Events
          case "CreateCustomerEvent":
            topicName = appConfig.masterDataTopic + "ICustomerEvent" + appConfig.kafkaTopicSuffix;
            var createCustomerEvent = new CreateCustomerEvent()
            {
              ActionUTC = eventUtc,
              ReceivedUTC = eventUtc,
              CustomerName = singleEvent.CustomerName,
              CustomerType = singleEvent.CustomerType,
              CustomerUID = new Guid(singleEvent.CustomerUID)
            };
            kafkaDriver.SendKafkaMessage(topicName, JsonConvert.SerializeObject(new { CreateCustomerEvent = createCustomerEvent }, jsonSettings));
            break;
          case "UpdateCustomerEvent":
            topicName = appConfig.masterDataTopic + "ICustomerEvent" + appConfig.kafkaTopicSuffix;
            var updateCustomerEvent = new UpdateCustomerEvent()
            {
              ActionUTC = eventUtc,
              ReceivedUTC = eventUtc,
              CustomerName = singleEvent.CustomerName,
              CustomerUID = new Guid(singleEvent.CustomerUID)
            };
            kafkaDriver.SendKafkaMessage(topicName, JsonConvert.SerializeObject(new { UpdateCustomerEvent = updateCustomerEvent }, jsonSettings));
            break;
          case "DeleteCustomerEvent":
            topicName = appConfig.masterDataTopic + "ICustomerEvent" + appConfig.kafkaTopicSuffix;
            var deleteCustomerEvent = new DeleteCustomerEvent()
            {
              ActionUTC = eventUtc,
              ReceivedUTC = eventUtc,
              CustomerUID = new Guid(singleEvent.CustomerUID)
            };
            kafkaDriver.SendKafkaMessage(topicName, JsonConvert.SerializeObject(new { DeleteCustomerEvent = deleteCustomerEvent }, jsonSettings));
            break;
          case "AssociateCustomerUserEvent":
            topicName = appConfig.masterDataTopic + "ICustomerEvent" + appConfig.kafkaTopicSuffix;
            var associateCustomerUserEvent = new AssociateCustomerUserEvent()
            {
              ActionUTC = eventUtc,
              ReceivedUTC = eventUtc,
              CustomerUID = new Guid(singleEvent.CustomerUID),
              UserUID = new Guid(singleEvent.UserUID)
            };
            kafkaDriver.SendKafkaMessage(topicName, JsonConvert.SerializeObject(new { AssociateCustomerUserEvent = associateCustomerUserEvent }, jsonSettings));
            break;
          case "DissociateCustomerUserEvent":
            topicName = appConfig.masterDataTopic + "ICustomerEvent" + appConfig.kafkaTopicSuffix;
            var dissociateCustomerUserEvent = new DissociateCustomerUserEvent()
            {
              ActionUTC = eventUtc,
              ReceivedUTC = eventUtc,
              CustomerUID = new Guid(singleEvent.CustomerUID),
              UserUID = new Guid(singleEvent.UserUID)
            };
            kafkaDriver.SendKafkaMessage(topicName, JsonConvert.SerializeObject(new { DissociateCustomerUserEvent = dissociateCustomerUserEvent }, jsonSettings));
            break;
          #endregion

          #region Subscription events
          case "CreateProjectSubscriptionEvent":
            topicName = appConfig.masterDataTopic + "ISubscriptionEvent" + appConfig.kafkaTopicSuffix;
            var createProjectSubscriptionEvent = new CreateProjectSubscriptionEvent()
            {
              ActionUTC = eventUtc,
              ReceivedUTC = eventUtc,
              StartDate = DateTime.Parse(singleEvent.StartDate), //, CultureInfo.InvariantCulture),
              EndDate = DateTime.Parse(singleEvent.EndDate), //CultureInfo.InvariantCulture),
              SubscriptionType = singleEvent.SubscriptionType,
              SubscriptionUID = new Guid(singleEvent.SubscriptionUID)
            };
            kafkaDriver.SendKafkaMessage(topicName, JsonConvert.SerializeObject(new { CreateProjectSubscriptionEvent = createProjectSubscriptionEvent }, jsonSettings));
            break;
          case "UpdateProjectSubscriptionEvent":
            topicName = appConfig.masterDataTopic + "ISubscriptionEvent" + appConfig.kafkaTopicSuffix;
            var updateProjectSubscriptionEvent = new UpdateProjectSubscriptionEvent()
            {
              ActionUTC = eventUtc,
              ReceivedUTC = eventUtc,
              StartDate = DateTime.Parse(singleEvent.StartDate), // CultureInfo.InvariantCulture),
              EndDate = DateTime.Parse(singleEvent.EndDate), //CultureInfo.InvariantCulture),
              SubscriptionType = singleEvent.SubscriptionType,
              SubscriptionUID = new Guid(singleEvent.SubscriptionUID)
            };
            kafkaDriver.SendKafkaMessage(topicName, JsonConvert.SerializeObject(new { UpdateProjectSubscriptionEvent = updateProjectSubscriptionEvent }, jsonSettings));
            break;
          case "AssociateProjectSubscriptionEvent":
            topicName = appConfig.masterDataTopic + "ISubscriptionEvent" + appConfig.kafkaTopicSuffix;
            var associateProjectSubscriptionEvent = new AssociateProjectSubscriptionEvent()
            {
              ActionUTC = eventUtc,
              ReceivedUTC = eventUtc,
              EffectiveDate = DateTime.Parse(singleEvent.EffectiveDate, CultureInfo.InvariantCulture),
              ProjectUID = new Guid(singleEvent.ProjectUID),
              SubscriptionUID = new Guid(singleEvent.SubscriptionUID)
            };
            kafkaDriver.SendKafkaMessage(topicName, JsonConvert.SerializeObject(new { AssociateProjectSubscriptionEvent = associateProjectSubscriptionEvent }, jsonSettings));
            break;
          #endregion

          #region Project events
          case "CreateProjectEvent":
            topicName = appConfig.masterDataTopic + "IProjectEvent" + appConfig.kafkaTopicSuffix;
            var createProjectEvent = new CreateProjectEvent()
            {
              ActionUTC = eventUtc,
              ReceivedUTC = eventUtc,
              ProjectEndDate = DateTime.Parse(singleEvent.ProjectEndDate),
              ProjectID = Int32.Parse(singleEvent.ProjectID),
              ProjectName = singleEvent.ProjectName,
              ProjectStartDate = DateTime.Parse(singleEvent.ProjectStartDate),
              ProjectTimezone = singleEvent.ProjectTimezone,
              ProjectType = (ProjectType)Enum.Parse(typeof(ProjectType), singleEvent.ProjectType),
              ProjectUID = new Guid(singleEvent.ProjectUID)
            };
            kafkaDriver.SendKafkaMessage(topicName, JsonConvert.SerializeObject(new { CreateProjectEvent = createProjectEvent }, jsonSettings));
            break;

          case "UpdateProjectEvent":
            topicName = appConfig.masterDataTopic + "IProjectEvent" + appConfig.kafkaTopicSuffix;
            var updateProjectEvent = new UpdateProjectEvent()
            {
              ActionUTC = eventUtc,
              ReceivedUTC = eventUtc,
              ProjectEndDate = DateTime.Parse(singleEvent.ProjectEndDate),
              ProjectName = singleEvent.ProjectName,
              ProjectTimezone = singleEvent.ProjectTimezone,
              ProjectType = (ProjectType)Enum.Parse(typeof(ProjectType), singleEvent.ProjectType),
              ProjectUID = new Guid(singleEvent.ProjectUID)
            };
            kafkaDriver.SendKafkaMessage(topicName, JsonConvert.SerializeObject(new { UpdateProjectEvent = updateProjectEvent }, jsonSettings));
            break;

          case "DeleteProjectEvent":
            topicName = appConfig.masterDataTopic + "IProjectEvent" + appConfig.kafkaTopicSuffix;
            var deleteProjectEvent = new DeleteProjectEvent()
            {
              ActionUTC = eventUtc,
              ReceivedUTC = eventUtc,
              ProjectUID = new Guid(singleEvent.ProjectUID)
            };
            kafkaDriver.SendKafkaMessage(topicName, JsonConvert.SerializeObject(new { DeleteProjectEvent = deleteProjectEvent }, jsonSettings));
            break;

          case "AssociateProjectCustomer":
            topicName = appConfig.masterDataTopic + "IProjectEvent" + appConfig.kafkaTopicSuffix;
            var associateCustomerProject = new AssociateProjectCustomer()
            {
              ActionUTC = eventUtc,
              ReceivedUTC = eventUtc,
              ProjectUID = new Guid(singleEvent.ProjectUID),
              CustomerUID = new Guid(singleEvent.CustomerUID)
            };
            kafkaDriver.SendKafkaMessage(topicName, JsonConvert.SerializeObject(new { AssociateProjectCustomer = associateCustomerProject }, jsonSettings));
            break;

          case "AssociateProjectGeofence":
            topicName = appConfig.masterDataTopic + "IProjectEvent" + appConfig.kafkaTopicSuffix;
            var associateProjectGeofence = new AssociateProjectGeofence()
            {
              ActionUTC = eventUtc,
              ReceivedUTC = eventUtc,
              ProjectUID = new Guid(singleEvent.ProjectUID),
              GeofenceUID = new Guid(singleEvent.GeofenceUID)
            };
            kafkaDriver.SendKafkaMessage(topicName, JsonConvert.SerializeObject(new { AssociateProjectGeofence = associateProjectGeofence }, jsonSettings));
            break;
          #endregion

          #region Geofence Events

          case "CreateGeofenceEvent":
            topicName = appConfig.masterDataTopic + "IGeofenceEvent" + appConfig.kafkaTopicSuffix;
            var createGeofenceEvent = new CreateGeofenceEvent()
            {
              ActionUTC = eventUtc,
              ReceivedUTC = eventUtc,
              GeofenceUID = new Guid(singleEvent.GeofenceUID),
              CustomerUID = new Guid(singleEvent.CustomerUID),
              Description = singleEvent.Description,
              FillColor = Int32.Parse(singleEvent.FillColor),
              GeofenceName = singleEvent.GeofenceName,
              GeofenceType = singleEvent.GeofenceType,
              GeometryWKT = singleEvent.GeometryWKT,
              IsTransparent = Boolean.Parse(singleEvent.IsTransparent),
              UserUID = new Guid(singleEvent.UserUID)
            };
            kafkaDriver.SendKafkaMessage(topicName, JsonConvert.SerializeObject(new { CreateGeofenceEvent = createGeofenceEvent }, jsonSettings));
            break;



            #endregion
        }
      }
    }

    /// <summary>
    /// Inset the events into the database 
    /// </summary>
    /// <param name="allEvents">List of all the events</param>
    /// <param name="isSameAsset">Is all the same asset</param>
    private void WriteAListOfMachineEventsToMySqlDatabase(List<EventTable> allEvents, bool isSameAsset = true)
    {
      foreach (var singleEvent in allEvents)
      {
        // This is used to set multiple asset events in the database
        if (!isSameAsset)
        {
          SetAssetUid();
          SetFirstEventDate();
          // CreateMockAssetConfigLoadOnly(1, true);
        }

        // Set the dates. Note: Timestamp from the test scenarios is always eventUtc. The device time is then calculated from eventUtc and local offset or location event offset applied.
        var dayOffSet = Convert.ToInt32(singleEvent.DayOffset);
        var eventDate = FirstEventDate.AddDays(dayOffSet) + DateTime.ParseExact(singleEvent.Timestamp, "HH:mm:ss", CultureInfo.InvariantCulture).TimeOfDay;
        var eventUtc = eventDate;
        //var deviceTime = singleEvent.UtcOffsetHours == "null" ? null : DateTimeExtensions.ToIso8601DateTime(eventUtc, Convert.ToDouble(singleEvent.UtcOffsetHours));
        var eventKeyDate = KeyDate(eventDate);
        LastEventDate = eventDate; // Always set the event date to be the last one. Assume the go in sequential order. 
        var mysqlHelper = new MySqlHelper();
        switch (singleEvent.EventType)
        {
          //#region SwitchStateEvent
          //case "SwitchStateEvent":
          //    var eventType = GetTimeStampedEventTypeFromAssetConfig(singleEvent);
          //    var switchStatequery = $@"INSERT INTO `{appConfig.dbSchema}`.{"TimeStampedEvent"} 
          //                        (AssetUID,fk_EventTypeID,EventUTC,EventDeviceTime,EventKeyDate) VALUES
          //                        ('{AssetUid}',{eventType},'{eventUtc:yyyy-MM-dd HH\:mm\:ss.fffffff}','{eventDate:yyyy-MM-dd HH\:mm\:ss.fffffff}',{eventKeyDate});";
          //    mysqlHelper.ExecuteMySqlInsert(appConfig.dbConnectionString, switchStatequery);
          //    break;
          //#endregion

          //#region OdometerEvent
          //case "OdometerEvent":
          //    var odometerMeter = singleEvent.OdometerKilometers;
          //    var odometerEventQuery = $@"INSERT INTO `{appConfig.dbSchema}`.{"OdometerMeterEvent"} 
          //                        (AssetUID,OdometerMeter,EventUTC,EventDeviceTime,EventKeyDate) VALUES
          //                        ('{AssetUid}',{odometerMeter},'{eventUtc:yyyy-MM-dd HH\:mm\:ss.fffffff}','{eventDate:yyyy-MM-dd HH\:mm\:ss.fffffff}',{eventKeyDate});";
          //    mysqlHelper.ExecuteMySqlInsert(appConfig.dbConnectionString, odometerEventQuery);
          //    break;
          //#endregion

          //#region LocationEvent
          //case "LocationEvent":

          //    break;
          //#endregion

          #region CreateAssetEvent
          case "CreateAssetEvent":
            var createAssetEventQuery = $@"INSERT INTO `{appConfig.dbSchema}`.{"Asset"} 
                                            (AssetUID,Name,MakeCode,SerialNumber,Model,IconKey,AssetType,LastActionedUTC) VALUES
                                            ('{AssetUid}','{singleEvent.AssetName}','{singleEvent.Make}','{singleEvent.SerialNumber}','{singleEvent.Model}',{singleEvent.IconId},'{singleEvent.AssetType}','{eventUtc:yyyy-MM-dd HH\:mm\:ss.fffffff}');";
            mysqlHelper.ExecuteMySqlInsert(appConfig.dbConnectionString, createAssetEventQuery);
            break;
            #endregion
        }
      }
    }

    /// <summary>
    /// Convert a string array of the type used for tests to a list of objects.
    /// </summary>
    /// <param name="objectDetailsArray">Array containing the details for each object, first row is header, subsequent rows are objects one object per row</param>
    /// <returns>List of objects</returns>
    private List<T> ConvertArrayToList<T>(IEnumerable<string> objectDetailsArray)
    {
      var allObjects = new List<T>();
      var allColumnNames = string.Empty;
      var rowCnt = 0;
      foreach (var row in objectDetailsArray)
      {
        rowCnt++;
        var rowValues = row.Split(SEPARATOR);
        if (rowCnt == 1)
        {
          allColumnNames = row;
          continue;
        }
        var obj = Activator.CreateInstance<T>();
        SetObjectPropertyValues(obj, rowValues, allColumnNames);
        allObjects.Add(obj);
      }
      return allObjects;
    }


    /// <summary>
    /// Convert a string array of the type used for tests to an object.
    /// </summary>
    /// <param name="objectDetailsArray">Two elment array, first row is header second is data.</param>
    /// <returns>object</returns>
    public T ConvertArrayToObject<T>(IEnumerable<string> objectDetailsArray)
    {
      var obj = Activator.CreateInstance<T>();

      if (objectDetailsArray.Count() == 2)
      {
        SetObjectPropertyValues(obj, objectDetailsArray.ElementAt(1).Split(SEPARATOR), objectDetailsArray.ElementAt(0));
      }

      return obj;
    }


    /// <summary>
    /// Set properties of an object with the values from the corresponding column of the string array
    /// </summary>
    /// <param name="obj">object we are wanting to set the properties of </param>
    /// <param name="dataRow">row of data</param>
    /// <param name="header">column headers</param>
    private void SetObjectPropertyValues<T>(T obj, string[] dataRow, string header)
    {
      var properties = typeof(T).GetProperties();
      foreach (var property in properties)
      {
        int idx;
        idx = GetColumnIndex(header, property.Name.Trim());
        obj = SetPropertyValue(obj, property, dataRow, idx);
      }
    }


    /// <summary>
    /// Set the property value for a specific field with the value from the string array
    /// </summary>
    /// <param name="obj"> class. The class we are setting the properties for </param>
    /// <param name="property">One field in PropertyInfo format</param>
    /// <param name="dataRow">One row of data from the string array</param>
    /// <param name="idx">index of where the field is in the string array</param>
    /// <returns>cycle with values in the properties</returns>
    private T SetPropertyValue<T>(T obj, PropertyInfo property, string[] dataRow, int idx)
    {
      if (idx == -1 || dataRow[idx].Trim() == "na")
      {
        return obj;
      }
      else if (dataRow[idx].Trim() == "null" || dataRow[idx].Trim() == "")
      {
        property.SetValue(obj, null);
      }
      else if (Nullable.GetUnderlyingType(property.PropertyType) == typeof(DateTime))
      {
        //Check to see if this is one of our special datetimes with offset included
        if (Regex.IsMatch(dataRow[idx], @"^\s*\d+d\+\d+"))
        {
          //This is a date column and its offset is defined as the number prior to the 'd+' in the string
          DateTime dateTime = ConvertVSSDateString(dataRow[idx]);
          property.SetValue(obj, dateTime);
        }
        else //treat as normal date
        {
          DateTime dateTime = DateTime.Parse(dataRow[idx].Trim());
          property.SetValue(obj, dateTime);
        }
      }
      else if (property.PropertyType == typeof(string) && Regex.IsMatch(dataRow[idx], @"^\s*\d+d\+\d+"))
      {
        //This is a date column and its offset is defined as the number prior to the 'd+' in the string
        String[] components = Regex.Split(dataRow[idx], @"d+\+");
        var offset = Double.Parse(components[0].Trim());
        DateTime dateTime = DateTime.Parse(FirstEventDate.AddDays(offset).ToString("yyyy-MM-dd") + " " + components[1].Trim());
        property.SetValue(obj, dateTime.ToString(CultureInfo.InvariantCulture));
      }
      else if (property.PropertyType.GetTypeInfo().IsEnum)
      {
        var val = Enum.Parse(property.PropertyType, dataRow[idx].Trim());
        property.SetValue(obj, val);
      }
      else //Work out the type and set the value.
      {
        Type t = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
        object value = (dataRow[idx].Trim() == null) ? null : Convert.ChangeType(dataRow[idx].Trim(), t);
        property.SetValue(obj, value, null);
      }
      return obj;
    }


    /// <summary>
    /// Get the index of where the column header is in the array
    /// </summary>
    /// <param name="header">string with all the column headers in it</param>
    /// <param name="columnName">specific column name we are trying to locate</param>
    /// <returns>index in the string array this is</returns>
    private int GetColumnIndex(string header, string columnName)
    {
      string[] headerValues = header.Replace(" ", "").Split(SEPARATOR);
      var idx = Array.FindIndex(headerValues, s => s.Equals(columnName));
      return idx;
    }

    /// <summary>
    /// Generate a random number. This is use for the number of days in the past to get a start date from.
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    private int RandomNumber(int min, int max)
    {
      lock (syncLock)
      {
        return rndNumber.Next(min, max);
      }
    }

    /// <summary>
    /// Converts a special date string eg 2d+12:00:00 which signifies a two date and 12 hour offset
    /// to a normal date time based on the first event date.
    /// </summary>
    /// <param name="vssDateString"></param>
    /// <returns></returns>
    public DateTime ConvertVSSDateString(string vssDateString)
    {
      String[] components = Regex.Split(vssDateString, @"d+\+");
      var offset = Double.Parse(components[0].Trim());
      return DateTime.Parse(FirstEventDate.AddDays(offset).ToString("yyyy-MM-dd") + " " + components[1].Trim());
    }

    /// <summary>
    /// Get the keydate from the datetime
    /// </summary>
    /// <param name="eventDate"></param>
    /// <returns></returns>
    public int KeyDate(DateTime eventDate)
    {
      return (eventDate.Year * 10000) + (eventDate.Month * 100) + (eventDate.Day);
    }

    /// <summary>
    /// Wait for time to let the data feed process
    /// </summary>
    /// <param name="count"></param>
    private void WaitForTimeBasedOnNumberOfRecords(int count)
    {
      if (count < 7)
      { Thread.Sleep(1000); }
      if (count > 6)
      { Thread.Sleep(1000); }
      if (count > 13)
      { Thread.Sleep(1000); }
    }

    #endregion
  }
}
