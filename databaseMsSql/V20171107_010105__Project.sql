USE [NH_OP];

SET ANSI_NULLS, QUOTED_IDENTIFIER, ANSI_PADDING ON
GO

CREATE TABLE dbo.Project(
	[ID] [bigint] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[fk_CustomerID] [bigint] NOT NULL,
	[fk_SiteID] [bigint] NOT NULL,
	[Active] [bit] NOT NULL CONSTRAINT [DF_Project_Active]  DEFAULT ((1)),
	[InsertUTC] [datetime2](7) NOT NULL CONSTRAINT [DF_Project_InsertUTC]  DEFAULT (getutcdate()),
	[UpdateUTC] [datetime2](7) NOT NULL CONSTRAINT [DF_Project_UpdateUTC]  DEFAULT (getutcdate()),
	[fk_CoordinateSystemID] [bigint] NULL,
	[Restored] [bit] NOT NULL CONSTRAINT [DF_Project_Restored]  DEFAULT ((0)),
	[DefaultJobSite] [varchar](100) NULL,
	[StartKeyDate] [int] NOT NULL CONSTRAINT [DF_Project_StartKeyDate]  DEFAULT (dbo.fn_GetKeyDate(getutcdate())),
	[EndKeyDate] [int] NOT NULL CONSTRAINT [DF_Project_EndKeyDate]  DEFAULT (dbo.fn_GetKeyDate(getutcdate())),
	[TimezoneName] [nvarchar](50) NOT NULL,
	[fk_ProjectTypeID] [int] NOT NULL CONSTRAINT [DF_Project_fk_ProjectTypeID]  DEFAULT ((0)),
	[ProjectUID] [uniqueidentifier] ROWGUIDCOL  NOT NULL CONSTRAINT [DF_Project_ProjectUID]  DEFAULT (newsequentialid()),
 CONSTRAINT [PK_Project] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [UC_ProjectUID] UNIQUE NONCLUSTERED 
(
	[ProjectUID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO
