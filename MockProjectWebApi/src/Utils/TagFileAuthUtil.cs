using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MockProjectWebApi.Common;
using Newtonsoft.Json;


namespace MockProjectWebApi.Utils
{

  public static class TagFileUtilsHelper
  {


    public class GetProjectIDData
    {
      public Int64 assetId;
      public Int64 projectId;
      public string code;
      public string message;
    }

    public class GetAssetIDData
    {
      public Int64 projectId;
      public int machineLevel;
      public string radioSerial;
      public Int64 assetId;
      public string code;
      public string message;
    }

    static List<GetProjectIDData> listGP;
    static List<GetAssetIDData> listGA;
    public static string WorkingFilePath;
    
    public static void WriteMessage(string msg)
    {
     Console.WriteLine(msg);
    }

    public static void Init()
    {
      WorkingFilePath = "";
      if (listGA != null)
      {
        listGA.Clear();
      }
      else
      {
        listGA = new List<GetAssetIDData>();
      }

      if (listGP != null)
      {
        listGP.Clear();
      }
      else
      {
        listGP = new List<GetProjectIDData>();
      }

      LoadData();
    }

    public static void LoadData()
    {

      if (File.Exists("DataLocation.txt"))
      {
        // allows you to redirect input
        string locString = File.ReadAllText("DataLocation.txt");
        WorkingFilePath = locString;
      }

      if (File.Exists("c:\\temp\\DataLocation.txt"))
      {
        WorkingFilePath = "c:\\temp\\";
      }


      // GetProjectID List
      string loadFromPath = Path.Combine(WorkingFilePath, "GetProjectId.json");

      if (File.Exists(loadFromPath))
      {

        WriteMessage(string.Format("Loading GetProjectId.json from {0}", loadFromPath));

        string jsonString = File.ReadAllText(loadFromPath);
        if (listGP != null)
          listGP.Clear();

        listGP = JsonConvert.DeserializeObject<List<GetProjectIDData>>(jsonString);
        string output = JsonConvert.SerializeObject(listGP);
        WriteMessage(string.Format("Loaded Success"));
        WriteMessage(output);

      }
      else
      {
        WriteMessage(string.Format("Missing file {0}. Loading defaults", loadFromPath));
      }

      // GetAssetID List
      loadFromPath = Path.Combine(WorkingFilePath, "GetAssetId.json");

      if (File.Exists(loadFromPath))
      {

        WriteMessage(string.Format("Loading GetAssetId.json from {0}", loadFromPath));

        string jsonString = File.ReadAllText(loadFromPath);
        if (listGA != null)
          listGA.Clear();

        listGA = JsonConvert.DeserializeObject<List<GetAssetIDData>>(jsonString);
        string output2 = JsonConvert.SerializeObject(listGA);
        WriteMessage(string.Format("Loaded Success"));
        WriteMessage(output2);

      }
      else
      {
        WriteMessage(string.Format("Missing file {0}", loadFromPath));
      }


    }

    public static GetProjectIdResult LookupProjectId(long assetId, string tccOrgUid)
    {
      // default values
      long projectID = 9000001;
      bool myRes = true;

      foreach (GetProjectIDData aPrj in listGP)
      {
        if (aPrj.assetId == assetId)
        {
          projectID = aPrj.projectId;
          myRes = true;
          break;
        }
      }
      return GetProjectIdResult.CreateGetProjectIdResult(myRes,projectID);
    }

    public static GetAssetIdResult LookupAssetId(long projectId, string radioSerial)
    {
      long myAssetId = 1000001;
      int myMachineLevel = 16;
      bool myRes = true;
      foreach (GetAssetIDData myAss in listGA) //  haha :)
      {
        if (projectId == -1) // auto import match radioSerial
        {
            if (myAss.radioSerial == radioSerial)
            {
              myAssetId = myAss.assetId;
              myMachineLevel = myAss.machineLevel;
              myRes = true;
              break;
            }
        }
        else if (projectId == myAss.projectId)
        {
          myAssetId = myAss.assetId;
          myMachineLevel = myAss.machineLevel;
          myRes = true;
          break;
        }
      }

      return GetAssetIdResult.CreateGetAssetIdResult(myRes, myAssetId, myMachineLevel, 0, 0, "", "");
    }

    public static GetProjectBoundaryAtDateResult LookupBoundary(long projectId)
    {
      var myFence = new TWGS84FenceContainer();
      return GetProjectBoundaryAtDateResult.CreateGetProjectBoundaryAtDateResult(true, myFence, 0, 0, "", "");
    }

    public static GetProjectBoundariesAtDateResult LookupBoundaries(long assetId)
    {
      ProjectBoundaryPackage[] boundaries = new ProjectBoundaryPackage[0];
      return GetProjectBoundariesAtDateResult.CreateGetProjectBoundariesAtDateResult(true, boundaries, 0, 0, "", "");
    }

    public static TagFileProcessingErrorResult ReportError()
    {
      return TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(true, 0, 1, "", "");
    }

  }


}
