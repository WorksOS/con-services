use [vss-store]
GO


/****** Object:  UserDefinedTableType [dbo].[tbl_SalesModel]    Script Date: 11/8/2018 6:20:38 PM ******/
CREATE TYPE [dbo].[tbl_SalesModel] AS TABLE(
	[ModelCode] [varchar](128) NULL,
	[SerialNumberPrefix] [varchar](128) NULL,
	[StartRange] [bigint] NULL,
	[EndRange] [bigint] NULL,
	[Description] [varchar](256) NULL,
	[IconUID] [uniqueidentifier] NULL,
	[ProductFamilyUID] [uniqueidentifier] NULL,
	[SalesModelUID] [uniqueidentifier] NULL
)
GO

GRANT EXECUTE ON TYPE::dbo.tbl_SalesModel TO _NHOPSvc

GO

/****** Object:  StoredProcedure [dbo].[uspPub_SalesModel_Save]    Script Date: 11/8/2018 6:01:29 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

Create PROCEDURE [dbo].[uspPub_SalesModel_Save]  
  @upsertList  tbl_SalesModel READONLY,  
  @deleteList  tbl_SalesModel READONLY
AS  
BEGIN  
  SET NOCOUNT ON;
 

  MERGE ProductFamily AS destination  
USING  @upsertList AS source  
ON ( destination.ProductFamilyUID = source.ProductFamilyUID)	
  WHEN NOT MATCHED THEN  
    INSERT (ProductFamilyUID, Name, [Description], UpdateUTC)  
    VALUES( source.ProductFamilyUID, '-', '-', getutcdate());
 
 UPDATE A set A.Model = U.ModelCode, A.ProductFamilyName = P.[Description], A.UpdateUTC = getutcdate() from Asset A 
JOIN SalesModel S on A.Model = S.ModelCode 
AND A.SerialNumberVIN like concat(S.SerialNumberPrefix,'%') 
AND IsNumeric(substring(A.SerialNumberVin,len(S.SerialNumberPrefix)+1,len(A.SerialNumberVIN)))=1
AND substring(A.SerialNumberVin,len(S.SerialNumberPrefix)+1,len(A.SerialNumberVIN)) Between S.StartRange AND S.EndRange
JOIN @upsertList U on U.SalesModelUID = S.SalesModelUID 
JOIN ProductFamily P on U.[ProductFamilyUID] = P.ProductFamilyUID;

 MERGE salesmodel AS destination  
USING  @upsertList AS source  
ON ( destination.SalesModelUID = source.SalesModelUID)  
  WHEN MATCHED   THEN  
    UPDATE SET  ModelCode  = source.ModelCode,  
                SerialNumberPrefix = source.SerialNumberPrefix,  
                StartRange  = source.StartRange,  
                EndRange    = source.EndRange,
				[Description] = source.[Description],
				fk_productFamilyId = (select top 1 ID from ProductFamily where ProductFamilyUID = source.ProductFamilyUID),
				fk_IconId = (case when exists(select top 1 ID from Icon I where I.IconUID=source.IconUID)  then (
				select top 1 ID from Icon I where I.IconUID=source.IconUID) else 0 end), -- default Icon id
                UpdateUTC     = getutcdate()

				
                                            
  WHEN NOT MATCHED THEN  
    INSERT (SalesModelUID, ModelCode, [Description], SerialNumberPrefix,StartRange,EndRange,fk_productFamilyId, fk_IconId, UpdateUTC)  
    VALUES( source.SalesModelUID, source.ModelCode, source.[Description], source.SerialNumberPrefix,source.StartRange,source.EndRange,
	            (select top 1 ID from ProductFamily where ProductFamilyUID = source.ProductFamilyUID),
				(case when exists(select top 1 ID from Icon I where I.IconUID=source.IconUID)  then (
				select top 1 ID from Icon I where I.IconUID=source.IconUID) else 0 end), getutcdate());


	
 Delete SalesModel
 where SalesModelUID in (select SalesModelUID from @deleteList);

 END

GO

  
  
GO

GRANT EXECUTE ON uspPub_SalesModel_Save TO _NHOPSvc