Print '************************************************'
Print '**Start Deactivate MasterDataSync tasks **'
Print '************************************************'


USE [VSS-Store]
GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
SET NOCOUNT ON;
GO



IF EXISTS (Select 1 From  MasterDataSync WHERE  TaskName  = 'CustomerUser')
BEGIN
	Update MasterDataSync set IsActive = 0 where TaskName = 'CustomerUser'
END

IF EXISTS (Select 1 From  MasterDataSync WHERE  TaskName  = 'Group')
BEGIN
	Update MasterDataSync set IsActive = 0 where TaskName = 'Group'
END	 

IF EXISTS (Select 1 From  MasterDataSync WHERE  TaskName  = 'Geofence')
BEGIN
	Update MasterDataSync set IsActive = 0 where TaskName = 'Geofence'
END

IF EXISTS (Select 1 From  MasterDataSync WHERE  TaskName  = 'Preference')
BEGIN
	Update MasterDataSync set IsActive = 0 where TaskName = 'Preference'
END

IF EXISTS (Select 1 From  MasterDataSync WHERE  TaskName  = 'ECMInfo')
BEGIN
	Update MasterDataSync set IsActive = 0 where TaskName = 'ECMInfo'
END
GO

Print '************************************************'
Print '** End Deactivate MasterDataSync tasks **'
Print '************************************************'
