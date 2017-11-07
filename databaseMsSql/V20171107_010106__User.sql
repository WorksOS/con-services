USE [NH_OP];

SET ANSI_NULLS, QUOTED_IDENTIFIER, ANSI_PADDING ON
GO

CREATE TABLE dbo.[User](
	[ID] [bigint] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[fk_CustomerID] [bigint] NULL,
	[Name] [nvarchar](200) NOT NULL,
	[PasswordHash] [nvarchar](50) NOT NULL,
	[Salt] [nvarchar](50) NOT NULL,
	[TimezoneName] [nvarchar](50) NULL,
	[EmailContact] [nvarchar](1000) NOT NULL,
	[PwdExpiryUTC] [datetime] NULL,
	[UpdateUTC] [datetime] NOT NULL CONSTRAINT [DF_User_UpdateUTC]  DEFAULT (getutcdate()),
	[fk_LanguageID] [int] NOT NULL CONSTRAINT [DF_User_fk_LanguageID]  DEFAULT ((0)),
	[Units] [int] NULL,
	[LocationDisplayType] [tinyint] NULL,
	[GlobalID] [varchar](37) NOT NULL CONSTRAINT [DF_User_GlobalID]  DEFAULT (CONVERT([varchar](37),newid(),(0))),
	[AssetLabelPreferenceType] [tinyint] NULL CONSTRAINT [DF_User_AssetLabelPreferenceType]  DEFAULT ((1)),
	[FirstName] [nvarchar](100) NOT NULL,
	[LastName] [nvarchar](100) NOT NULL,
	[Address] [nvarchar](400) NULL,
	[PhoneNumber] [nvarchar](50) NULL,
	[JobTitle] [nvarchar](100) NULL,
	[Active] [bit] NOT NULL CONSTRAINT [DF_User_Active]  DEFAULT ((1)),
	[TermsofUseAcceptedUTC] [datetime2](7) NULL,
	[LogOnFailedCount] [int] NOT NULL CONSTRAINT [DF_User_LogOnFailedCount]  DEFAULT ((0)),
	[LogOnFirstFailedUTC] [datetime2](7) NULL,
	[LogOnLastFailedUTC] [datetime2](7) NULL,
	[MeterLabelPreferenceType] [int] NULL,
	[fk_TemperatureUnitID] [int] NOT NULL CONSTRAINT [DF_User_TemperatureUnit]  DEFAULT ((1)),
	[PasswordResetGUID] [varchar](37) NULL,
	[PasswordResetUTC] [datetime2](7) NULL,
	[InsertUTC] [datetime2](7) NULL,
	[fk_PressureUnitID] [int] NOT NULL CONSTRAINT [DF_User_PressureUnit]  DEFAULT ((1)),
	[LastLoginUTC] [datetime2](7) NULL,
	[IsEmailValidated] [bit] NOT NULL DEFAULT ((1)),
	[EmailVerificationUTC] [datetime2](7) NULL,
	[EmailVerificationGUID] [varchar](37) NULL,
	[Createdby] [varchar](50) NULL,
	[EmailModifiedCount] [int] NULL DEFAULT ((0)),
	[EmailVerificationTrackingUTC] [datetime2](7) NULL,
	[UserUID] [varchar](36) NULL,
	[IdentityMigrationUTC] [datetime2](7) NULL,
	[Domain] [varchar](100) NULL,
	[IsVLLoginID] [bit] NOT NULL CONSTRAINT [DF_User_IsVLLoginID]  DEFAULT ((0)),
 CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO


