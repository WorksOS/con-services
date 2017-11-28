SET ANSI_NULLS, QUOTED_IDENTIFIER, ANSI_PADDING ON
GO

CREATE TABLE NH_OP..Customer(
	[ID] [bigint] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[Name] [nvarchar](200) NOT NULL,
	[UpdateUTC] [datetime] NOT NULL CONSTRAINT [DF_Customer_UpdateUTC]  DEFAULT (getutcdate()),
	[fk_CustomerTypeID] [int] NOT NULL CONSTRAINT [DF_Customer_fk_CustomerTypeID]  DEFAULT ((0)),
	[BSSID] [varchar](50) NOT NULL,
	[fk_DealerNetworkID] [int] NOT NULL,
	[NetworkDealerCode] [varchar](50) NULL,
	[NetworkCustomerCode] [varchar](50) NULL,
	[DealerAccountCode] [varchar](50) NULL,
	[IsActivated] [bit] NOT NULL CONSTRAINT [DF_Customer_IsActivated]  DEFAULT ((1)),
	[CustomerUID] [uniqueidentifier] ROWGUIDCOL  NULL DEFAULT (newsequentialid()),
	[MapAPIProvider] [nvarchar](20) NULL,
	[TCCOrgID] [nvarchar](50) NULL,
	[PrimaryEmailContact] [nvarchar](1000) NULL,
	[FirstName] [nvarchar](100) NULL,
	[LastName] [nvarchar](100) NULL,
 CONSTRAINT [PK_Customer] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

SET ANSI_PADDING OFF
GO


