using System;
using System.Collections.Generic;
using System.Text;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Scheduler.Common.Models
{
  public class NhOpImportedFile
  {
    public long LegacyProjectId; 

    public string ProjectUid { get; set; } // from joined table

    public long LegacyCustomerId { get; set; } 

    public string CustomerUid { get; set; } // from joined table

    public ImportedFileType ImportedFileType { get; set; } // CG and NG use same enums

    public int? DxfUnitsType { get; set; } // not included yet in NG, but very soon

    public string Name { get; set; } // CG includes SurveyedUtc

    public DateTime? SurveyedUtc { get; set; }

    public DateTime FileCreatedUtc { get; set; }

    public DateTime FileUpdatedUtc { get; set; }

    public string ImportedBy { get; set; }

    public DateTime LastActionedUtc { get; set; } 

   
    public override bool Equals(object obj)
    {
      NhOpImportedFile importedFile = obj as NhOpImportedFile;
      if(
          importedFile?.LegacyProjectId != LegacyProjectId
          || importedFile.ProjectUid != this.ProjectUid
          || importedFile.LegacyCustomerId != this.LegacyCustomerId
          || importedFile.CustomerUid != this.CustomerUid
          || importedFile.ImportedFileType != this.ImportedFileType
          || importedFile.DxfUnitsType != this.DxfUnitsType
          || importedFile.Name != this.Name 
          || importedFile.SurveyedUtc != this.SurveyedUtc
          || importedFile.FileCreatedUtc != this.FileCreatedUtc
          || importedFile.FileUpdatedUtc != this.FileUpdatedUtc
          || importedFile.ImportedBy != this.ImportedBy
          || importedFile.LastActionedUtc != this.LastActionedUtc
        )
        return false;
      return true;
    }

    public override int GetHashCode()
    {
      return 0;
    }
  }
}

/* CGens NH_OP..ImportedFiles
 * 
 --NG ProjectUid
 [fk_ProjectID] [bigint] NOT NULL,

  -- NG ImportedFileUid???

  -- NG ImportedFileId but different series 
  [ID] [bigint] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,

  -- NG legacyCustomerId, CustomerUid ??
	[fk_CustomerID] [bigint] NOT NULL,

  -- NG ImportedFileType
	[fk_ImportedFileTypeID] [int] NOT NULL,
  
  -- NG Name
  [Name] [nvarchar](100) NOT NULL,

  -- NG FileDescriptor?
	[SourcePath] [nvarchar](400) NULL,
	[SourceFilespaceID] [nvarchar](50) NULL,
  
  -- NG FileCreatedUtc/FileUpdatedUtc?

  -- NG ImportedBy (UserName)?

  -- NG SurveyedUtc
  [SurveyedUTC] [datetime2](7) NULL,
  
  -- NG IsDeleted (I think CG removes from this table and moves to ImportedFileHistory)

  -- NG IsActivated?
     
  -- NG LastActionedUtc
  [InsertUTC] [datetime2](7) NOT NULL CONSTRAINT [DF_ImportedFile_InsertUTC]  DEFAULT (getutcdate()),
	
  -- NG todo?
	[fk_DXFUnitsTypeID] [int] NULL,	

  -- NG not avail?
	[fk_ReferenceImportedFileID] [bigint] NULL,
	[Offset] [float] NULL CONSTRAINT [DF_ImportedFile_Offset]  DEFAULT ((0.0)),
	[fk_MassHaulPlanID] [bigint] NULL,
	[MinZoom] [int] NULL,
	[MaxZoom] [int] NULL,
	[MinLat] [float] NULL,
	[MinLon] [float] NULL,
	[MaxLat] [float] NULL,
	[MaxLon] [float] NULL,
	[IsNotifyUser] [bit] NOT NULL CONSTRAINT [DF_ImportedFile_IsNotifyUser]  DEFAULT ((0)),
 * */
