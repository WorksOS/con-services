using System;
using System.Collections.Generic;
using System.Text;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Scheduler.Common.Models
{
  public class NhOpImportedFile
  {
    public long LegacyProjectId; // CG=fk_ProjectID; NG=legacyProjectId

    // NA in CG table . 
    // how do we obtain this for syncing join with Project and Customer?
    // todo Will user be able to create Projects in NG which don't exist in CG, 
    //    so that the Project needs syncing to NH_OP also?
    public string ProjectUid { get; set; }

    // public long ImportedFileId { get; set; } // CG=ID ; ImportedFileId NG. (just a unique code but DIFFERENT in both places)

    // NA in CG table . ditto to ProjectUid todo
    //public string ImportedFileUid { get; set; }

    // public long LegacyCustomerId { get; set; } // CG=fk_CustomerID; NG=legacyCustomerId

    // NA in CG table . ditto to ProjectUid todo join Customer
    public string CustomerUid { get; set; }

    // luckliy CG and NG use same enums
    public ImportedFileType ImportedFileType { get; set; } // CG = fk_ImportedFileTypeID

    public string Name { get; set; } // is this the same between CG and NG?

    public string FileDescriptor { get; set; } // CG = SourcePath/SourceFilespaceID?

    // CG N/A 
    //public DateTime FileCreatedUtc { get; set; }

    // CG N/A 
    //public DateTime FileUpdatedUtc { get; set; }

    // CG N/A
    //public string ImportedBy { get; set; }

    public DateTime? SurveyedUtc { get; set; }

    // CG N/A does actual delete (or rather moves to history)
    //public bool IsDeleted { get; set; }

    // CG N/A 
    //public bool IsActivated { get; set; }

    public DateTime LastActionedUtc { get; set; } // CG =InsertUTC

    // other columns avail in CG but not avail in NG e.g. fk_ReferenceImportedFileID; Offset; minZoom

    public override bool Equals(object obj)
    {
      NhOpImportedFile importedFile = obj as NhOpImportedFile;
      if (importedFile == null)
        return false;
      if(
          importedFile.ProjectUid != this.ProjectUid
          || importedFile.ImportedFileType != this.ImportedFileType
          || importedFile.CustomerUid != this.CustomerUid
          || importedFile.Name != this.Name 
        )
        return false;
      //DateTime? surveyedUtc1 = importedFile.SurveyedUtc;
      //DateTime? surveyedUtc2 = this.SurveyedUtc;
      //if ((surveyedUtc1.HasValue == surveyedUtc2.HasValue ? (surveyedUtc1.HasValue ? (surveyedUtc1.GetValueOrDefault() == surveyedUtc2.GetValueOrDefault() ? 1 : 0) : 1) : 0) != 0)
      //  return importedFile.LastActionedUtc == this.LastActionedUtc;
      //return false;
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
