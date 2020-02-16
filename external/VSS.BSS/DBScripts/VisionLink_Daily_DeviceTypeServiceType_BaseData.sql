Print '************************************************'
Print '** Start DeviceTypeServiceType_BaseData **'
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

DECLARE @ServiceTypeID INT
DECLARE @DeviceTypeID  INT

SELECT @ServiceTypeID = ID FROM ServiceType WHERE Name = 'VisionLink Daily'

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


SELECT @DeviceTypeID = ID FROM DeviceType WHERE Name = 'SNM940'

IF NOT EXISTS (Select 1 From  DeviceTypeServiceType WHERE  fk_DeviceTypeID  = @DeviceTypeID AND fk_ServiceTypeID = @ServiceTypeID )
BEGIN
	INSERT INTO [dbo].[DeviceTypeServiceType]
           ([fk_DeviceTypeID]
           ,[fk_ServiceTypeID])
			VALUES
           (@DeviceTypeID
           ,@ServiceTypeID)
END

SELECT @DeviceTypeID = ID FROM DeviceType WHERE Name = 'CrossCheck'

IF NOT EXISTS (Select 1 From  DeviceTypeServiceType WHERE  fk_DeviceTypeID  = @DeviceTypeID AND fk_ServiceTypeID = @ServiceTypeID )
BEGIN
	INSERT INTO [dbo].[DeviceTypeServiceType]
           ([fk_DeviceTypeID]
           ,[fk_ServiceTypeID])
			VALUES
           (@DeviceTypeID
           ,@ServiceTypeID)
END

SELECT @DeviceTypeID = ID FROM DeviceType WHERE Name = 'TrimTrac'

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

SELECT @DeviceTypeID = ID FROM DeviceType WHERE Name = 'TM3000'

IF NOT EXISTS (Select 1 From  DeviceTypeServiceType WHERE  fk_DeviceTypeID  = @DeviceTypeID AND fk_ServiceTypeID = @ServiceTypeID )
BEGIN
	INSERT INTO [dbo].[DeviceTypeServiceType]
           ([fk_DeviceTypeID]
           ,[fk_ServiceTypeID])
			VALUES
           (@DeviceTypeID
           ,@ServiceTypeID)
END

SELECT @DeviceTypeID = ID FROM DeviceType WHERE Name = 'TAP66'

IF NOT EXISTS (Select 1 From  DeviceTypeServiceType WHERE  fk_DeviceTypeID  = @DeviceTypeID AND fk_ServiceTypeID = @ServiceTypeID )
BEGIN
	INSERT INTO [dbo].[DeviceTypeServiceType]
           ([fk_DeviceTypeID]
           ,[fk_ServiceTypeID])
			VALUES
           (@DeviceTypeID
           ,@ServiceTypeID)
END

SELECT @DeviceTypeID = ID FROM DeviceType WHERE Name = 'SNM451'

IF NOT EXISTS (Select 1 From  DeviceTypeServiceType WHERE  fk_DeviceTypeID  = @DeviceTypeID AND fk_ServiceTypeID = @ServiceTypeID )
BEGIN
	INSERT INTO [dbo].[DeviceTypeServiceType]
           ([fk_DeviceTypeID]
           ,[fk_ServiceTypeID])
			VALUES
           (@DeviceTypeID
           ,@ServiceTypeID)
END

SELECT @DeviceTypeID = ID FROM DeviceType WHERE Name = 'PL431'

IF NOT EXISTS (Select 1 From  DeviceTypeServiceType WHERE  fk_DeviceTypeID  = @DeviceTypeID AND fk_ServiceTypeID = @ServiceTypeID )
BEGIN
	INSERT INTO [dbo].[DeviceTypeServiceType]
           ([fk_DeviceTypeID]
           ,[fk_ServiceTypeID])
			VALUES
           (@DeviceTypeID
           ,@ServiceTypeID)
END

SELECT @DeviceTypeID = ID FROM DeviceType WHERE Name = 'DCM300'

IF NOT EXISTS (Select 1 From  DeviceTypeServiceType WHERE  fk_DeviceTypeID  = @DeviceTypeID AND fk_ServiceTypeID = @ServiceTypeID )
BEGIN
	INSERT INTO [dbo].[DeviceTypeServiceType]
           ([fk_DeviceTypeID]
           ,[fk_ServiceTypeID])
			VALUES
           (@DeviceTypeID
           ,@ServiceTypeID)
END

SELECT @DeviceTypeID = ID FROM DeviceType WHERE Name = 'PL641'

IF NOT EXISTS (Select 1 From  DeviceTypeServiceType WHERE  fk_DeviceTypeID  = @DeviceTypeID AND fk_ServiceTypeID = @ServiceTypeID )
BEGIN
	INSERT INTO [dbo].[DeviceTypeServiceType]
           ([fk_DeviceTypeID]
           ,[fk_ServiceTypeID])
			VALUES
           (@DeviceTypeID
           ,@ServiceTypeID)
END

SELECT @DeviceTypeID = ID FROM DeviceType WHERE Name = 'PLE641'

IF NOT EXISTS (Select 1 From  DeviceTypeServiceType WHERE  fk_DeviceTypeID  = @DeviceTypeID AND fk_ServiceTypeID = @ServiceTypeID )
BEGIN
	INSERT INTO [dbo].[DeviceTypeServiceType]
           ([fk_DeviceTypeID]
           ,[fk_ServiceTypeID])
			VALUES
           (@DeviceTypeID
           ,@ServiceTypeID)
END

SELECT @DeviceTypeID = ID FROM DeviceType WHERE Name = 'PLE641PLUSPL631'

IF NOT EXISTS (Select 1 From  DeviceTypeServiceType WHERE  fk_DeviceTypeID  = @DeviceTypeID AND fk_ServiceTypeID = @ServiceTypeID )
BEGIN
	INSERT INTO [dbo].[DeviceTypeServiceType]
           ([fk_DeviceTypeID]
           ,[fk_ServiceTypeID])
			VALUES
           (@DeviceTypeID
           ,@ServiceTypeID)
END

SELECT @DeviceTypeID = ID FROM DeviceType WHERE Name = 'PLE631'

IF NOT EXISTS (Select 1 From  DeviceTypeServiceType WHERE  fk_DeviceTypeID  = @DeviceTypeID AND fk_ServiceTypeID = @ServiceTypeID )
BEGIN
	INSERT INTO [dbo].[DeviceTypeServiceType]
           ([fk_DeviceTypeID]
           ,[fk_ServiceTypeID])
			VALUES
           (@DeviceTypeID
           ,@ServiceTypeID)
END

SELECT @DeviceTypeID = ID FROM DeviceType WHERE Name = 'PL631'

IF NOT EXISTS (Select 1 From  DeviceTypeServiceType WHERE  fk_DeviceTypeID  = @DeviceTypeID AND fk_ServiceTypeID = @ServiceTypeID )
BEGIN
	INSERT INTO [dbo].[DeviceTypeServiceType]
           ([fk_DeviceTypeID]
           ,[fk_ServiceTypeID])
			VALUES
           (@DeviceTypeID
           ,@ServiceTypeID)
END

SELECT @DeviceTypeID = ID FROM DeviceType WHERE Name = 'PL241'

IF NOT EXISTS (Select 1 From  DeviceTypeServiceType WHERE  fk_DeviceTypeID  = @DeviceTypeID AND fk_ServiceTypeID = @ServiceTypeID )
BEGIN
	INSERT INTO [dbo].[DeviceTypeServiceType]
           ([fk_DeviceTypeID]
           ,[fk_ServiceTypeID])
			VALUES
           (@DeviceTypeID
           ,@ServiceTypeID)
END

SELECT @DeviceTypeID = ID FROM DeviceType WHERE Name = 'PL231'

IF NOT EXISTS (Select 1 From  DeviceTypeServiceType WHERE  fk_DeviceTypeID  = @DeviceTypeID AND fk_ServiceTypeID = @ServiceTypeID )
BEGIN
	INSERT INTO [dbo].[DeviceTypeServiceType]
           ([fk_DeviceTypeID]
           ,[fk_ServiceTypeID])
			VALUES
           (@DeviceTypeID
           ,@ServiceTypeID)
END

SELECT @DeviceTypeID = ID FROM DeviceType WHERE Name = 'BasicVirtualDevice'

IF NOT EXISTS (Select 1 From  DeviceTypeServiceType WHERE  fk_DeviceTypeID  = @DeviceTypeID AND fk_ServiceTypeID = @ServiceTypeID )
BEGIN
	INSERT INTO [dbo].[DeviceTypeServiceType]
           ([fk_DeviceTypeID]
           ,[fk_ServiceTypeID])
			VALUES
           (@DeviceTypeID
           ,@ServiceTypeID)
END

SELECT @DeviceTypeID = ID FROM DeviceType WHERE Name = 'MTHYPHEN10'

IF NOT EXISTS (Select 1 From  DeviceTypeServiceType WHERE  fk_DeviceTypeID  = @DeviceTypeID AND fk_ServiceTypeID = @ServiceTypeID )
BEGIN
	INSERT INTO [dbo].[DeviceTypeServiceType]
           ([fk_DeviceTypeID]
           ,[fk_ServiceTypeID])
			VALUES
           (@DeviceTypeID
           ,@ServiceTypeID)
END

SELECT @DeviceTypeID = ID FROM DeviceType WHERE Name = 'XT5060'

IF NOT EXISTS (Select 1 From  DeviceTypeServiceType WHERE  fk_DeviceTypeID  = @DeviceTypeID AND fk_ServiceTypeID = @ServiceTypeID )
BEGIN
	INSERT INTO [dbo].[DeviceTypeServiceType]
           ([fk_DeviceTypeID]
           ,[fk_ServiceTypeID])
			VALUES
           (@DeviceTypeID
           ,@ServiceTypeID)
END

SELECT @DeviceTypeID = ID FROM DeviceType WHERE Name = 'XT4860'

IF NOT EXISTS (Select 1 From  DeviceTypeServiceType WHERE  fk_DeviceTypeID  = @DeviceTypeID AND fk_ServiceTypeID = @ServiceTypeID )
BEGIN
	INSERT INTO [dbo].[DeviceTypeServiceType]
           ([fk_DeviceTypeID]
           ,[fk_ServiceTypeID])
			VALUES
           (@DeviceTypeID
           ,@ServiceTypeID)
END

SELECT @DeviceTypeID = ID FROM DeviceType WHERE Name = 'TTUSeries'

IF NOT EXISTS (Select 1 From  DeviceTypeServiceType WHERE  fk_DeviceTypeID  = @DeviceTypeID AND fk_ServiceTypeID = @ServiceTypeID )
BEGIN
	INSERT INTO [dbo].[DeviceTypeServiceType]
           ([fk_DeviceTypeID]
           ,[fk_ServiceTypeID])
			VALUES
           (@DeviceTypeID
           ,@ServiceTypeID)
END

SELECT @DeviceTypeID = ID FROM DeviceType WHERE Name = 'XT2000'

IF NOT EXISTS (Select 1 From  DeviceTypeServiceType WHERE  fk_DeviceTypeID  = @DeviceTypeID AND fk_ServiceTypeID = @ServiceTypeID )
BEGIN
	INSERT INTO [dbo].[DeviceTypeServiceType]
           ([fk_DeviceTypeID]
           ,[fk_ServiceTypeID])
			VALUES
           (@DeviceTypeID
           ,@ServiceTypeID)
END

SELECT @DeviceTypeID = ID FROM DeviceType WHERE Name = 'MTGModularGatewayHYPHENMotorEngine'

IF NOT EXISTS (Select 1 From  DeviceTypeServiceType WHERE  fk_DeviceTypeID  = @DeviceTypeID AND fk_ServiceTypeID = @ServiceTypeID )
BEGIN
	INSERT INTO [dbo].[DeviceTypeServiceType]
           ([fk_DeviceTypeID]
           ,[fk_ServiceTypeID])
			VALUES
           (@DeviceTypeID
           ,@ServiceTypeID)
END

SELECT @DeviceTypeID = ID FROM DeviceType WHERE Name = 'MTGModularGatewayHYPHENElectricEngine'

IF NOT EXISTS (Select 1 From  DeviceTypeServiceType WHERE  fk_DeviceTypeID  = @DeviceTypeID AND fk_ServiceTypeID = @ServiceTypeID )
BEGIN
	INSERT INTO [dbo].[DeviceTypeServiceType]
           ([fk_DeviceTypeID]
           ,[fk_ServiceTypeID])
			VALUES
           (@DeviceTypeID
           ,@ServiceTypeID)
END

SELECT @DeviceTypeID = ID FROM DeviceType WHERE Name = 'MCHYPHEN3'

IF NOT EXISTS (Select 1 From  DeviceTypeServiceType WHERE  fk_DeviceTypeID  = @DeviceTypeID AND fk_ServiceTypeID = @ServiceTypeID )
BEGIN
	INSERT INTO [dbo].[DeviceTypeServiceType]
           ([fk_DeviceTypeID]
           ,[fk_ServiceTypeID])
			VALUES
           (@DeviceTypeID
           ,@ServiceTypeID)
END

SELECT @DeviceTypeID = ID FROM DeviceType WHERE Name = 'XT6540'

IF NOT EXISTS (Select 1 From  DeviceTypeServiceType WHERE  fk_DeviceTypeID  = @DeviceTypeID AND fk_ServiceTypeID = @ServiceTypeID )
BEGIN
	INSERT INTO [dbo].[DeviceTypeServiceType]
           ([fk_DeviceTypeID]
           ,[fk_ServiceTypeID])
			VALUES
           (@DeviceTypeID
           ,@ServiceTypeID)
END

SELECT @DeviceTypeID = ID FROM DeviceType WHERE Name = 'XT65401'

IF NOT EXISTS (Select 1 From  DeviceTypeServiceType WHERE  fk_DeviceTypeID  = @DeviceTypeID AND fk_ServiceTypeID = @ServiceTypeID )
BEGIN
	INSERT INTO [dbo].[DeviceTypeServiceType]
           ([fk_DeviceTypeID]
           ,[fk_ServiceTypeID])
			VALUES
           (@DeviceTypeID
           ,@ServiceTypeID)
END

SELECT @DeviceTypeID = ID FROM DeviceType WHERE Name = 'XT65402'

IF NOT EXISTS (Select 1 From  DeviceTypeServiceType WHERE  fk_DeviceTypeID  = @DeviceTypeID AND fk_ServiceTypeID = @ServiceTypeID )
BEGIN
	INSERT INTO [dbo].[DeviceTypeServiceType]
           ([fk_DeviceTypeID]
           ,[fk_ServiceTypeID])
			VALUES
           (@DeviceTypeID
           ,@ServiceTypeID)
END

SELECT @DeviceTypeID = ID FROM DeviceType WHERE Name = 'THREEPDATA'

IF NOT EXISTS (Select 1 From  DeviceTypeServiceType WHERE  fk_DeviceTypeID  = @DeviceTypeID AND fk_ServiceTypeID = @ServiceTypeID )
BEGIN
	INSERT INTO [dbo].[DeviceTypeServiceType]
           ([fk_DeviceTypeID]
           ,[fk_ServiceTypeID])
			VALUES
           (@DeviceTypeID
           ,@ServiceTypeID)
END

SELECT @DeviceTypeID = ID FROM DeviceType WHERE Name = 'PL131'

IF NOT EXISTS (Select 1 From  DeviceTypeServiceType WHERE  fk_DeviceTypeID  = @DeviceTypeID AND fk_ServiceTypeID = @ServiceTypeID )
BEGIN
	INSERT INTO [dbo].[DeviceTypeServiceType]
           ([fk_DeviceTypeID]
           ,[fk_ServiceTypeID])
			VALUES
           (@DeviceTypeID
           ,@ServiceTypeID)
END

SELECT @DeviceTypeID = ID FROM DeviceType WHERE Name = 'PL141'

IF NOT EXISTS (Select 1 From  DeviceTypeServiceType WHERE  fk_DeviceTypeID  = @DeviceTypeID AND fk_ServiceTypeID = @ServiceTypeID )
BEGIN
	INSERT INTO [dbo].[DeviceTypeServiceType]
           ([fk_DeviceTypeID]
           ,[fk_ServiceTypeID])
			VALUES
           (@DeviceTypeID
           ,@ServiceTypeID)
END

SELECT @DeviceTypeID = ID FROM DeviceType WHERE Name = 'PL440'

IF NOT EXISTS (Select 1 From  DeviceTypeServiceType WHERE  fk_DeviceTypeID  = @DeviceTypeID AND fk_ServiceTypeID = @ServiceTypeID )
BEGIN
	INSERT INTO [dbo].[DeviceTypeServiceType]
           ([fk_DeviceTypeID]
           ,[fk_ServiceTypeID])
			VALUES
           (@DeviceTypeID
           ,@ServiceTypeID)
END

SELECT @DeviceTypeID = ID FROM DeviceType WHERE Name = 'PLE601'

IF NOT EXISTS (Select 1 From  DeviceTypeServiceType WHERE  fk_DeviceTypeID  = @DeviceTypeID AND fk_ServiceTypeID = @ServiceTypeID )
BEGIN
	INSERT INTO [dbo].[DeviceTypeServiceType]
           ([fk_DeviceTypeID]
           ,[fk_ServiceTypeID])
			VALUES
           (@DeviceTypeID
           ,@ServiceTypeID)
END

SELECT @DeviceTypeID = ID FROM DeviceType WHERE Name = 'PL161'

IF NOT EXISTS (Select 1 From  DeviceTypeServiceType WHERE  fk_DeviceTypeID  = @DeviceTypeID AND fk_ServiceTypeID = @ServiceTypeID )
BEGIN
	INSERT INTO [dbo].[DeviceTypeServiceType]
           ([fk_DeviceTypeID]
           ,[fk_ServiceTypeID])
			VALUES
           (@DeviceTypeID
           ,@ServiceTypeID)
END

SELECT @DeviceTypeID = ID FROM DeviceType WHERE Name = 'PL240'

IF NOT EXISTS (Select 1 From  DeviceTypeServiceType WHERE  fk_DeviceTypeID  = @DeviceTypeID AND fk_ServiceTypeID = @ServiceTypeID )
BEGIN
	INSERT INTO [dbo].[DeviceTypeServiceType]
           ([fk_DeviceTypeID]
           ,[fk_ServiceTypeID])
			VALUES
           (@DeviceTypeID
           ,@ServiceTypeID)
END

SELECT @DeviceTypeID = ID FROM DeviceType WHERE Name = 'PL542'

IF NOT EXISTS (Select 1 From  DeviceTypeServiceType WHERE  fk_DeviceTypeID  = @DeviceTypeID AND fk_ServiceTypeID = @ServiceTypeID )
BEGIN
	INSERT INTO [dbo].[DeviceTypeServiceType]
           ([fk_DeviceTypeID]
           ,[fk_ServiceTypeID])
			VALUES
           (@DeviceTypeID
           ,@ServiceTypeID)
END

SELECT @DeviceTypeID = ID FROM DeviceType WHERE Name = 'PLE642'

IF NOT EXISTS (Select 1 From  DeviceTypeServiceType WHERE  fk_DeviceTypeID  = @DeviceTypeID AND fk_ServiceTypeID = @ServiceTypeID )
BEGIN
	INSERT INTO [dbo].[DeviceTypeServiceType]
           ([fk_DeviceTypeID]
           ,[fk_ServiceTypeID])
			VALUES
           (@DeviceTypeID
           ,@ServiceTypeID)
END

SELECT @DeviceTypeID = ID FROM DeviceType WHERE Name = 'PLE742'

IF NOT EXISTS (Select 1 From  DeviceTypeServiceType WHERE  fk_DeviceTypeID  = @DeviceTypeID AND fk_ServiceTypeID = @ServiceTypeID )
BEGIN
	INSERT INTO [dbo].[DeviceTypeServiceType]
           ([fk_DeviceTypeID]
           ,[fk_ServiceTypeID])
			VALUES
           (@DeviceTypeID
           ,@ServiceTypeID)
END

SELECT @DeviceTypeID = ID FROM DeviceType WHERE Name = 'SNM941'

IF NOT EXISTS (Select 1 From  DeviceTypeServiceType WHERE  fk_DeviceTypeID  = @DeviceTypeID AND fk_ServiceTypeID = @ServiceTypeID )
BEGIN
	INSERT INTO [dbo].[DeviceTypeServiceType]
           ([fk_DeviceTypeID]
           ,[fk_ServiceTypeID])
			VALUES
           (@DeviceTypeID
           ,@ServiceTypeID)
END

SELECT @DeviceTypeID = ID FROM DeviceType WHERE Name = 'PL240B'

IF NOT EXISTS (Select 1 From  DeviceTypeServiceType WHERE  fk_DeviceTypeID  = @DeviceTypeID AND fk_ServiceTypeID = @ServiceTypeID )
BEGIN
	INSERT INTO [dbo].[DeviceTypeServiceType]
           ([fk_DeviceTypeID]
           ,[fk_ServiceTypeID])
			VALUES
           (@DeviceTypeID
           ,@ServiceTypeID)
END

SELECT @DeviceTypeID = ID FROM DeviceType WHERE Name = 'TAP76'

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
Print '** Start DeviceTypeServiceType_BaseData **'
Print '************************************************'