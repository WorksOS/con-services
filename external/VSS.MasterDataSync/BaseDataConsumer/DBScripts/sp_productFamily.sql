USE [VSS-Store]
GO

CREATE TYPE [dbo].[tbl_ProductFamily] AS TABLE(
 [Name] [varchar](128) NOT NULL,
 [Description] [varchar](256) NULL,
 [ProductFamilyUID] [uniqueidentifier] NULL
)
GO

GRANT EXECUTE ON TYPE::dbo.tbl_ProductFamily TO _NHOPSvc

GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[uspPub_ProductFamily_Save]
 @upsertList as tbl_ProductFamily READONLY
 AS
 BEGIN
 SET NOCOUNT ON
UPDATE A set A.ProductFamilyName = U.[Description], A.UpdateUTC = GETUTCDATE() from Asset A
JOIN ProductFamily P on A.ProductFamilyName= P.[Description]
JOIN @upsertList U on U.ProductFamilyUID=P.ProductFamilyUID
WHERE A.ProductFamilyName != U.[Description];

MERGE ProductFamily as destination
USING @upsertList as source
ON (destination.ProductFamilyUID = source.ProductFamilyUID)
 WHEN MATCHED THEN
  UPDATE SET Name = source.Name,
			 [Description] = source.[Description],
			 UpdateUTC = GETUTCDATE()

 WHEN NOT MATCHED THEN
  INSERT(Name, [Description],ProductFamilyUID, UpdateUTC)
  VALUES(source.Name, source.[Description], source.ProductFamilyUID, GETUTCDATE());

END

GO

GO

GRANT EXECUTE ON uspPub_ProductFamily_Save TO _NHOPSvc