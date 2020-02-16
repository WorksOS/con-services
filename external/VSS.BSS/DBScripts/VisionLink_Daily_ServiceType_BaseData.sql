Print '************************************************'
Print '** Start ServiceType_BaseData **'
Print '************************************************'

--VisionLink Daily : New Subscription
USE [VSS-Store]
GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
SET NOCOUNT ON;
GO


IF NOT EXISTS (Select 1 From  ServiceType WHERE  Name  = 'VisionLink Daily' )
BEGIN
    INSERT INTO [dbo].[ServiceType]
       ([ID]
       ,[Name]
       ,[BSSPartNumber]
       ,[IsCore])
	VALUES
       (36
       ,'VisionLink Daily'
       ,'89500-15'
       ,1)
END
GO

Print '************************************************'
Print '** Start ServiceType_BaseData **'
Print '************************************************'