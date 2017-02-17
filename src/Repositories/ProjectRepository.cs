using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using VSS.TagFileAuth.Service.Utils;
using Microsoft.Extensions.Logging;

namespace VSS.TagFileAuth.Service.Repositories
{
  public class ProjectRepository : RepositoryBase
  {
    private readonly ILogger log;

    public ProjectRepository(IConfigurationStore _connectionString, ILoggerFactory logger) : base(_connectionString)
    {
      log = logger.CreateLogger<ProjectRepository>();
    }
    
    /// <summary>
    /// There may be 0 or n subscriptions for this project. None/many may be current. 
    /// This method just gets ANY one of these or no subs (SubscriptionUID == null)
    /// We don't care, up to the calling code to decipher.
    /// </summary>
    /// <param name="projectUid"></param>
    ///// <returns></returns>
    //public async Task<Project> GetProject(string projectUid)
    //{
    //  await PerhapsOpenConnection();

    //  var project = (await Connection.QueryAsync<Project>
    //      (@"SELECT 
    //            p.ProjectUID, p.Name, p.LegacyProjectID, p.ProjectTimeZone, p.LandfillTimeZone,                     
    //            p.LastActionedUTC, p.IsDeleted, p.StartDate, p.EndDate, p.fk_ProjectTypeID as ProjectType,
    //            cp.fk_CustomerUID AS CustomerUID, cp.LegacyCustomerID, 
    //            ps.fk_SubscriptionUID AS SubscriptionUID, s.EndDate AS SubscriptionEndDate
    //          FROM Project p 
    //            JOIN CustomerProject cp ON cp.fk_ProjectUID = p.ProjectUID
    //            JOIN Customer c on c.CustomerUID = cp.fk_CustomerUID
    //            LEFT OUTER JOIN ProjectSubscription ps on ps.fk_ProjectUID = p.ProjectUID
    //            LEFT OUTER JOIN Subscription s on s.SubscriptionUID = ps.fk_SubscriptionUID 
    //          WHERE p.ProjectUID = @projectUid AND p.IsDeleted = 0",
    //        new { projectUid }
    //      )).FirstOrDefault();

    //  PerhapsCloseConnection();
    //  return project;
    //}

    ///// <summary>
    ///// gets only 1 row for a particular sub. only 1 projectUID and be associated with a sub
    ///// </summary>
    ///// <param name="subscriptionUid"></param>
    ///// <returns></returns>
    //public async Task<Project> GetProjectBySubcription(string subscriptionUid)
    //{
    //  await PerhapsOpenConnection();

    //  var projects = (await Connection.QueryAsync<Project>
    //      (@"SELECT 
    //            p.ProjectUID, p.Name, p.LegacyProjectID, p.ProjectTimeZone, p.LandfillTimeZone,                     
    //            p.LastActionedUTC, p.IsDeleted, p.StartDate, p.EndDate, p.fk_ProjectTypeID as ProjectType,
    //            cp.fk_CustomerUID AS CustomerUID, cp.LegacyCustomerID, 
    //            ps.fk_SubscriptionUID AS SubscriptionUID, s.EndDate AS SubscriptionEndDate
    //          FROM Project p 
    //            JOIN CustomerProject cp ON cp.fk_ProjectUID = p.ProjectUID
    //            JOIN Customer c on c.CustomerUID = cp.fk_CustomerUID
    //            JOIN ProjectSubscription ps on ps.fk_ProjectUID = p.ProjectUID
    //            JOIN Subscription s on s.SubscriptionUID = ps.fk_SubscriptionUID 
    //          WHERE ps.fk_SubscriptionUID = @subscriptionUid AND p.IsDeleted = 0",
    //          new { subscriptionUid }
    //      )).FirstOrDefault(); ;

    //  PerhapsCloseConnection();
    //  return projects;
    //}
    

  }
}