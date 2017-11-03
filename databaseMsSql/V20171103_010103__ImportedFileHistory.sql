SET ANSI_NULLS, QUOTED_IDENTIFIER, ANSI_PADDING ON
GO

CREATE TABLE [dbo].[ImportedFileHistory](
	[fk_ImportedFileID] [bigint] NOT NULL,
	[InsertUTC] [datetime2](7) NOT NULL CONSTRAINT [DF_ImportedFileHistory_InsertUTC]  DEFAULT (getutcdate()),
	[ID] [bigint] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[fk_UserID] [bigint] NULL,
	[CreateUTC] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_ImportedFileHistory] PRIMARY KEY CLUSTERED 
(
	[fk_ImportedFileID] ASC,
	[InsertUTC] ASC,
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]

GO

