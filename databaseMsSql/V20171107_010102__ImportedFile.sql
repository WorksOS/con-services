SET ANSI_NULLS, QUOTED_IDENTIFIER ON
GO

CREATE TABLE NH_OP..ImportedFile(
	[ID] [bigint] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[fk_CustomerID] [bigint] NOT NULL,
	[fk_ProjectID] [bigint] NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[InsertUTC] [datetime2](7) NOT NULL CONSTRAINT [DF_ImportedFile_InsertUTC]  DEFAULT (getutcdate()),
	[fk_ImportedFileTypeID] [int] NOT NULL,
	[fk_DXFUnitsTypeID] [int] NULL,
	[SurveyedUTC] [datetime2](7) NULL,
	[SourcePath] [nvarchar](400) NULL,
	[SourceFilespaceID] [nvarchar](50) NULL,
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
 CONSTRAINT [PK_ImportedFile] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
