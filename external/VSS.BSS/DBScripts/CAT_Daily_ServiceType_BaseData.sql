Print '************************************************'
Print '** Start ServiceType_BaseData **'
Print '************************************************'

--CAT Daily : New Subscription
	

USE [VSS-Store]
GO
	
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
SET NOCOUNT ON;
GO


IF NOT EXISTS (Select 1 From  ServiceType WHERE  Name  = 'CAT Daily' )
BEGIN
    INSERT INTO [dbo].[ServiceType]
       ([ID]
       ,[Name]
       ,[BSSPartNumber]
       ,[IsCore])
	VALUES
       (35
       ,'CAT Daily'
       ,'89500-10'
       ,0)
END
GO

Print '************************************************'
Print '** End ServiceType_BaseData **'
Print '************************************************'