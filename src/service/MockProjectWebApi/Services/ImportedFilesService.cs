using System;
using System.Collections.Generic;
using MockProjectWebApi.Utils;
using VSS.MasterData.Models.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace MockProjectWebApi.Services
{
  public class ImportedFilesService : IImportedFilesService
  {
    public Dictionary<string, List<FileData>> ImportedFiles;

    public ImportedFilesService()
    {
      CreateTestData();
    }

    private void CreateTestData()
    {
      ImportedFiles = new Dictionary<string, List<FileData>>();

      var dimensionsImportedFiles = new List<FileData>
      {
        new FileData
          {
            Name = "CERA.bg.dxf",
            ProjectUid = ConstantsUtil.DIMENSIONS_PROJECT_UID,
            CustomerUid = "DxfTileAcceptanceTest",
            ImportedFileType = ImportedFileType.Linework,
            ImportedFileUid = "cfcd4c01-6fc8-45d5-872f-513a0f619f03",
            LegacyFileId = 1,
            IsActivated = true,
            MinZoomLevel = 15,
            MaxZoomLevel = 18
          },
          new FileData
          {
            Name = "Marylands_Metric.dxf",
            ProjectUid = ConstantsUtil.DIMENSIONS_PROJECT_UID,
            CustomerUid = "DxfTileAcceptanceTest",
            ImportedFileType = ImportedFileType.Linework,
            ImportedFileUid = "ea89be4b-0efb-4b8f-ba33-03f0973bfc7b",
            LegacyFileId = 2,
            IsActivated = true,
            MinZoomLevel = 18,
            MaxZoomLevel = 19
          },
          new FileData
          {
            Name = "Large Sites Road - Trimble Road.TTM",
            ProjectUid = ConstantsUtil.DIMENSIONS_PROJECT_UID,
            CustomerUid = "CutFillAcceptanceTest",
            ImportedFileType = ImportedFileType.DesignSurface,
            ImportedFileUid = "dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff",
            LegacyFileId = 3,
            IsActivated = true,
            MinZoomLevel = 15,
            MaxZoomLevel = 20
          },
          new FileData
          {
            Name = "Large Sites Road - Trimble Road.TTM",
            ProjectUid = ConstantsUtil.DIMENSIONS_PROJECT_UID,
            CustomerUid = "CutFillAcceptanceTest",
            ImportedFileType = ImportedFileType.DesignSurface,
            ImportedFileUid = "dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff",
            LegacyFileId = 111,
            IsActivated = true,
            MinZoomLevel = 15,
            MaxZoomLevel = 20
          },
          new FileData
          {
            Name = "Large Sites Road.svl",
            ProjectUid = ConstantsUtil.DIMENSIONS_PROJECT_UID,
            CustomerUid = "StationOffsetReportTest",
            ImportedFileType = ImportedFileType.Alignment,
            ImportedFileUid = "6ece671b-7959-4a14-86fa-6bfe6ef4dd62",
            LegacyFileId = 112,
            IsActivated = true,
            MinZoomLevel = 15,
            MaxZoomLevel = 17
          },
          new FileData
          {
            Name = "Topcon Road - Topcon Phil.svl",
            ProjectUid = ConstantsUtil.DIMENSIONS_PROJECT_UID,
            CustomerUid = "StationOffsetReportTest",
            ImportedFileType = ImportedFileType.Alignment,
            ImportedFileUid = "c6662be1-0f94-4897-b9af-28aeeabcd09b",
            LegacyFileId = 113,
            IsActivated = true,
            MinZoomLevel = 16,
            MaxZoomLevel = 18
          },
          new FileData
          {
            Name = "Milling - Milling.svl",
            ProjectUid = ConstantsUtil.DIMENSIONS_PROJECT_UID,
            CustomerUid = "StationOffsetReportTest",
            ImportedFileType = ImportedFileType.Alignment,
            ImportedFileUid = "3ead0c55-1e1f-4d30-aaf8-873526a2ab82",
            LegacyFileId = 114,
            IsActivated = true,
            MinZoomLevel = 15,
            MaxZoomLevel = 19
          },
          new FileData
          {
            Name = "Section 1 IFC Rev J.ttm",
            ProjectUid = ConstantsUtil.DIMENSIONS_PROJECT_UID,
            CustomerUid = "ImportFileProxyTest",
            ImportedFileType = ImportedFileType.DesignSurface,
            ImportedFileUid = "eb798b46-c927-4fdd-b998-b11011ee7365",
            LegacyFileId = 115,
            IsActivated = true,
            MinZoomLevel = 16,
            MaxZoomLevel = 19
          }
      };

      ImportedFiles.Add(ConstantsUtil.DIMENSIONS_PROJECT_UID, dimensionsImportedFiles);

      var importedFilesGoldenData1 = new List<FileData>();
      importedFilesGoldenData1.AddRange(surveyedSurfacesFileListIsActivated);
      importedFilesGoldenData1.AddRange(goldenDataDesignSurfaceFileList);

      ImportedFiles.Add(ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1, importedFilesGoldenData1);

      ImportedFiles.Add(ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_2, surveyedSurfacesFileList);
    }

    private readonly List<FileData> surveyedSurfacesFileListIsActivated = new List<FileData>
      {
        new FileData
        {
          Name = "Original Ground Survey - Dimensions 2012_2016-05-13T000202Z.TTM",
          ProjectUid = ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1,
          CustomerUid = "SurveyedSurfaceAcceptanceTest",
          ImportedFileType = ImportedFileType.SurveyedSurface,
          ImportedFileUid = "ff323224-f2ab-4af6-b4bc-95dd0903c003",
          LegacyFileId = 14177,
          IsActivated = true,
          MinZoomLevel = 0,
          MaxZoomLevel = 0,
          SurveyedUtc = DateTime.Parse("2012-05-13T00:02:02")
        },
        new FileData
        {
          Name = "Large Sites Road - Trimble Road_2016-05-13T000000Z.TTM",
          ProjectUid = ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1,
          CustomerUid = "SurveyedSurfaceAcceptanceTest",
          ImportedFileType = ImportedFileType.SurveyedSurface,
          ImportedFileUid = "4f9bebe8-812b-4552-9af6-1ddfb2f813ed",
          LegacyFileId = 14176,
          IsActivated = true,
          MinZoomLevel = 0,
          MaxZoomLevel = 0
        },
        new FileData
        {
          Name = "Milling - Milling_2016-05-08T234647Z.TTM",
          ProjectUid = ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1,
          CustomerUid = "SurveyedSurfaceAcceptanceTest",
          ImportedFileType = ImportedFileType.SurveyedSurface,
          ImportedFileUid = "dcb41fbd-7d43-4b36-a144-e22bbccc24a8",
          LegacyFileId = 14175,
          IsActivated = true,
          MinZoomLevel = 0,
          MaxZoomLevel = 0,
          SurveyedUtc = DateTime.Parse("2016-05-08T23:46:47")
        },
        new FileData
        {
          Name = "Large Sites Road - Trimble Road_2016-05-08T234455Z.TTM",
          ProjectUid = ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1,
          CustomerUid = "SurveyedSurfaceAcceptanceTest",
          ImportedFileType = ImportedFileType.SurveyedSurface,
          ImportedFileUid = "0372718b-534a-430f-bb71-dc71acb9bd5b",
          LegacyFileId = 14174,
          IsActivated = true,
          MinZoomLevel = 0,
          MaxZoomLevel = 0
        },
        new FileData
        {
          Name = "Large Sites Road - Trimble Road_2012-06-01T015500Z.TTM",
          ProjectUid = ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1,
          CustomerUid = "SurveyedSurfaceAcceptanceTest",
          ImportedFileType = ImportedFileType.SurveyedSurface,
          ImportedFileUid = "0db110ed-8dc2-487a-901c-0ea5de6fd8dd",
          LegacyFileId = 14222,
          IsActivated = true,
          MinZoomLevel = 0,
          MaxZoomLevel = 0
        }
      };

    private readonly List<FileData> surveyedSurfacesFileList = new List<FileData>
      {
        new FileData
        {
          Name = "Original Ground Survey - Dimensions 2012_2016-05-13T000202Z.TTM",
          ProjectUid = ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_2.ToString(),
          CustomerUid = "SurveyedSurfaceAcceptanceTest",
          ImportedFileType = ImportedFileType.SurveyedSurface,
          ImportedFileUid = "ff323224-f2ab-4af6-b4bc-95dd0903c003",
          LegacyFileId = 14177,
          IsActivated = false,
          MinZoomLevel = 0,
          MaxZoomLevel = 0,
          SurveyedUtc = DateTime.Parse("2012-05-13T00:02:02")
        },
        new FileData
        {
          Name = "Large Sites Road - Trimble Road_2016-05-13T000000Z.TTM",
          ProjectUid = ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_2.ToString(),
          CustomerUid = "SurveyedSurfaceAcceptanceTest",
          ImportedFileType = ImportedFileType.SurveyedSurface,
          ImportedFileUid = "4f9bebe8-812b-4552-9af6-1ddfb2f813ed",
          LegacyFileId = 14176,
          IsActivated = false,
          MinZoomLevel = 0,
          MaxZoomLevel = 0
        },
        new FileData
        {
          Name = "Milling - Milling_2016-05-08T234647Z.TTM",
          ProjectUid = ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_2.ToString(),
          CustomerUid = "SurveyedSurfaceAcceptanceTest",
          ImportedFileType = ImportedFileType.SurveyedSurface,
          ImportedFileUid = "dcb41fbd-7d43-4b36-a144-e22bbccc24a8",
          LegacyFileId = 14175,
          IsActivated = false,
          MinZoomLevel = 0,
          MaxZoomLevel = 0,
          SurveyedUtc = DateTime.Parse("2016-05-08T23:46:47")
        },
        new FileData
        {
          Name = "Large Sites Road - Trimble Road_2016-05-08T234455Z.TTM",
          ProjectUid = ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_2.ToString(),
          CustomerUid = "SurveyedSurfaceAcceptanceTest",
          ImportedFileType = ImportedFileType.SurveyedSurface,
          ImportedFileUid = "0372718b-534a-430f-bb71-dc71acb9bd5b",
          LegacyFileId = 14174,
          IsActivated = false,
          MinZoomLevel = 0,
          MaxZoomLevel = 0
        },
        new FileData
        {
          Name = "Large Sites Road - Trimble Road_2012-06-01T015500Z.TTM",
          ProjectUid = ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_2.ToString(),
          CustomerUid = "SurveyedSurfaceAcceptanceTest",
          ImportedFileType = ImportedFileType.SurveyedSurface,
          ImportedFileUid = "0db110ed-8dc2-487a-901c-0ea5de6fd8dd",
          LegacyFileId = 14222,
          IsActivated = false,
          MinZoomLevel = 0,
          MaxZoomLevel = 0
        }
      };

    private readonly List<FileData> goldenDataDesignSurfaceFileList = new List<FileData>
      {
        new FileData
        {
          Name = "Original Ground Survey - Dimensions 2012.TTM",
          ProjectUid = ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1,
          CustomerUid = "DesignSurfaceAcceptanceTest",
          ImportedFileType = ImportedFileType.DesignSurface,
          ImportedFileUid = "3d255208-8aa2-4172-9046-f97a36eff896",
          LegacyFileId = 15177,
          IsActivated = true,
          MinZoomLevel = 15,
          MaxZoomLevel = 19
        },
        new FileData
        {
          Name = "Large Sites Road - Trimble Road.TTM",
          ProjectUid = ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1,
          CustomerUid = "DesignSurfaceAcceptanceTest",
          ImportedFileType = ImportedFileType.DesignSurface,
          ImportedFileUid = "dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff",
          LegacyFileId = 15176,
          IsActivated = true,
          MinZoomLevel = 15,
          MaxZoomLevel = 18
        },
        new FileData
        {
          Name = "Milling - Milling.TTM",
          ProjectUid = ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1,
          CustomerUid = "DesignSurfaceAcceptanceTest",
          ImportedFileType = ImportedFileType.DesignSurface,
          ImportedFileUid = "220e12e5-ce92-4645-8f01-1942a2d5a57f",
          LegacyFileId = 15175,
          IsActivated = true,
          MinZoomLevel = 16,
          MaxZoomLevel = 17
        },
        new FileData
        {
          Name = "Topcon Road - Topcon.TTM",
          ProjectUid = ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1,
          CustomerUid = "DesignSurfaceAcceptanceTest",
          ImportedFileType = ImportedFileType.DesignSurface,
          ImportedFileUid = "ea97efb9-c0c4-4a7f-9eee-e2b0ef0b0916",
          LegacyFileId = 15174,
          IsActivated = true,
          MinZoomLevel = 16,
          MaxZoomLevel = 18
        },
        new FileData
        {
          Name = "Trimble Command Centre.TTM",
          ProjectUid = ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1,
          CustomerUid = "DesignSurfaceAcceptanceTest",
          ImportedFileType = ImportedFileType.DesignSurface,
          ImportedFileUid = Guid.NewGuid().ToString(),
          LegacyFileId = 15222,
          IsActivated = true,
          MinZoomLevel = 19,
          MaxZoomLevel = 20
        }
      };
  }
}
