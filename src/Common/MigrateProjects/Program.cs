using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ThreeDAPIs.ProjectMasterData;
using VSS.Hosted.VLCommon;

namespace MigrateProjects
{
  class Program
  {
    static void Main(string[] args)
    {
      //Get all projects
      Console.WriteLine("Loading projects");
      NH_OPDataSet opDataset = new NH_OPDataSet();
      NH_OPDataSetTableAdapters.DataTable1TableAdapter table = new NH_OPDataSetTableAdapters.DataTable1TableAdapter();
      NH_OPDataSetTableAdapters.CoordinateSystemTableAdapter coordtable = new NH_OPDataSetTableAdapters.CoordinateSystemTableAdapter();
      Console.WriteLine("Filling projects");
      table.Fill(opDataset.DataTable1);
      Console.WriteLine("Filling CSs");
      coordtable.Fill(opDataset.CoordinateSystem);

      var projectsToSend = opDataset.DataTable1.Where(p=>1==1);
      var synchronizer = new ProjectSynchronizer();
      string result = String.Empty;

      //First process all projects including archived\deleted
       foreach (var project in projectsToSend)
       {
         string coorIdName="";
         try
         {
           coorIdName = opDataset.CoordinateSystem.Where(s => s.ID == project.fk_CoordinateSystemID)
             .Select(s => s.CoordinateSystemFileName).First();
         }
         catch
         {
         }
         Console.WriteLine("Create Project {0}", project.Name);

         //Create project
         /*synchronizer.SyncCreateProject(
           (int)project.ID, 
           project.ProjectUID, 
           project.StartKeyDate, 
           project.EndKeyDate,
           project.Name,
           project.TimezoneName, 
           (VSS.Hosted.VLCommon.ProjectTypeEnum) project.fk_ProjectTypeID,
           GetWicketFromPoints(XmlToPoints(project.Polygon)),
           project.CustomerUID,
           project.fk_CustomerID,
           coorIdName,
           DateTime.UtcNow);*/

         //Associate geofence with the project
         /*Console.WriteLine("Assign Site {0}", project.Name);
         synchronizer.SyncAssignSiteToProject(project.ProjectUID, project.SiteUID, DateTime.UtcNow);

         //Associate project to the customer
         Console.WriteLine("Assign Customer {0}", project.CustomerUID);
         synchronizer.SyncAssignProjectToCustomer(project.ProjectUID, project.CustomerUID, project.fk_CustomerID,DateTime.UtcNow);*/
       }


      
      //Now messup with deleted projects
     /* var archPrj = projectsToSend.Where(p => p.Active == false).ToList();
      foreach (var projectRow in  archPrj)
      {
        synchronizer.SyncDeleteProject(projectRow.ProjectUID, DateTime.UtcNow);

      }
      Task.WaitAll(synchronizer.tasks.ToArray());*/

    }


    private static List<Point> XmlToPoints(string polygonXml)
    {
      if (!string.IsNullOrEmpty(polygonXml))
      {
        XElement doc = XElement.Load(new StringReader(polygonXml));
        return (from xml in doc.Elements("Point")
                select new Point()
                {
                  x = double.Parse(xml.Element("x").Value),
                  y = double.Parse(xml.Element("y").Value)
                }).ToList<Point>();
      }
      return null;
    }

    private static string GetWicketFromPoints(List<Point> points)
    {
      if (points.Count == 0)
        return "";

      var polygonWkt = new StringBuilder("POLYGON((");
      foreach (var point in points)
      {
        polygonWkt.Append(String.Format("{0} {1},", point.x, point.y));
      }
      polygonWkt.Append(String.Format("{0} {1}))", points[0].x, points[0].y));
      return polygonWkt.ToString();
    }

  }
}
