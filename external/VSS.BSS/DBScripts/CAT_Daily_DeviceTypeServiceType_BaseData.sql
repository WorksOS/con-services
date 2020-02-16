Print '************************************************'
Print '** Start DeviceTypeServiceType_BaseData **'
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

DECLARE @ServiceTypeID INT
DECLARE @DeviceTypeID  INT

SELECT @ServiceTypeID = ID FROM ServiceType WHERE Name = 'CAT Daily'

SELECT @DeviceTypeID = ID FROM DeviceType WHERE Name = 'PL121'

IF NOT EXISTS (Select 1 From  DeviceTypeServiceType WHERE  fk_DeviceTypeID  = @DeviceTypeID AND fk_ServiceTypeID = @ServiceTypeID )
BEGIN
	INSERT INTO [dbo].[DeviceTypeServiceType]
           ([fk_DeviceTypeID]
           ,[fk_ServiceTypeID])
			VALUES
           (@DeviceTypeID
           ,@ServiceTypeID)
END

SELECT @DeviceTypeID = ID FROM DeviceType WHERE Name = 'PL321'

IF NOT EXISTS (Select 1 From  DeviceTypeServiceType WHERE  fk_DeviceTypeID  = @DeviceTypeID AND fk_ServiceTypeID = @ServiceTypeID )
BEGIN
	INSERT INTO [dbo].[DeviceTypeServiceType]
           ([fk_DeviceTypeID]
           ,[fk_ServiceTypeID])
			VALUES
           (@DeviceTypeID
           ,@ServiceTypeID)
END

SELECT @DeviceTypeID = ID FROM DeviceType WHERE Name = 'Series522'

IF NOT EXISTS (Select 1 From  DeviceTypeServiceType WHERE  fk_DeviceTypeID  = @DeviceTypeID AND fk_ServiceTypeID = @ServiceTypeID )
BEGIN
	INSERT INTO [dbo].[DeviceTypeServiceType]
           ([fk_DeviceTypeID]
           ,[fk_ServiceTypeID])
			VALUES
           (@DeviceTypeID
           ,@ServiceTypeID)
END

SELECT @DeviceTypeID = ID FROM DeviceType WHERE Name = 'Series523'

IF NOT EXISTS (Select 1 From  DeviceTypeServiceType WHERE  fk_DeviceTypeID  = @DeviceTypeID AND fk_ServiceTypeID = @ServiceTypeID )
BEGIN
	INSERT INTO [dbo].[DeviceTypeServiceType]
           ([fk_DeviceTypeID]
           ,[fk_ServiceTypeID])
			VALUES
           (@DeviceTypeID
           ,@ServiceTypeID)
END

SELECT @DeviceTypeID = ID FROM DeviceType WHERE Name = 'Series521'

IF NOT EXISTS (Select 1 From  DeviceTypeServiceType WHERE  fk_DeviceTypeID  = @DeviceTypeID AND fk_ServiceTypeID = @ServiceTypeID )
BEGIN
	INSERT INTO [dbo].[DeviceTypeServiceType]
           ([fk_DeviceTypeID]
           ,[fk_ServiceTypeID])
			VALUES
           (@DeviceTypeID
           ,@ServiceTypeID)
END

SELECT @DeviceTypeID = ID FROM DeviceType WHERE Name = 'PL420'

IF NOT EXISTS (Select 1 From  DeviceTypeServiceType WHERE  fk_DeviceTypeID  = @DeviceTypeID AND fk_ServiceTypeID = @ServiceTypeID )
BEGIN
	INSERT INTO [dbo].[DeviceTypeServiceType]
           ([fk_DeviceTypeID]
           ,[fk_ServiceTypeID])
			VALUES
           (@DeviceTypeID
           ,@ServiceTypeID)
END

SELECT @DeviceTypeID = ID FROM DeviceType WHERE Name = 'PL421'

IF NOT EXISTS (Select 1 From  DeviceTypeServiceType WHERE  fk_DeviceTypeID  = @DeviceTypeID AND fk_ServiceTypeID = @ServiceTypeID )
BEGIN
	INSERT INTO [dbo].[DeviceTypeServiceType]
           ([fk_DeviceTypeID]
           ,[fk_ServiceTypeID])
			VALUES
           (@DeviceTypeID
           ,@ServiceTypeID)
END		 
GO

Print '************************************************'
Print '** End DeviceTypeServiceType_BaseData **'
Print '************************************************'
