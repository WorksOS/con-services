/*
 in Dec 2017 the VSS kafka Que version upgrade is being upgraded.
 The existing Landfill custom kafka consumers don't support it.
 So we changed these custom masterData (customer/project/Goefence/Subscription)
     consuemrs to using the Merino sandbox Consumers.alter
     
  The old database tables needed to be upgraded a little to support the
     new consumers. This script achieves that, and can be used to upgrade an existing 
     database installation as on production.alter
  If you are creating a brand new database then use all scripts
      from MerinoSandbox\MasterDataConsumers\database\. 
      BTW You could install all tables but not necessary.
      
  Customer; CustomerType; CustomerUser
  Project; ProjectType; CustomerProject
  Subscription; ServiceTypeFamiily; ServiceType; ProjectSubscription; 
        AssetSubscription (required by consumers, not landfill)
  Geofence; GeofenceTypeEnum; ProjectGeofence
*/
 
/*

-- Customer
SET @s = (SELECT IF(
    (SELECT COUNT(*)
       FROM INFORMATION_SCHEMA.COLUMNS
        WHERE table_name = 'Customer'
        AND table_schema = DATABASE()
        AND column_name = 'CustomerName'
    ) > 0,
    "ALTER TABLE `Customer` CHANGE COLUMN `CustomerName` `Name` VARCHAR(200) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_unicode_ci' NOT NULL",
    "SELECT 1"
));

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- CustomerUser
SET @s = (SELECT IF(
    (SELECT COUNT(*)
       FROM INFORMATION_SCHEMA.COLUMNS
        WHERE table_name = 'CustomerUser'
        AND table_schema = DATABASE()
        AND column_name = 'fk_UserUID'
    ) > 0,
    "ALTER TABLE `CustomerUser` CHANGE COLUMN `fk_UserUID` `UserUID` VARCHAR(64) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_unicode_ci' NOT NULL",
    "SELECT 1"
));

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Project

SET @s = (SELECT IF(
    (SELECT COUNT(*)
       FROM INFORMATION_SCHEMA.COLUMNS
        WHERE table_name = 'Project'
        AND table_schema = DATABASE()
        AND column_name = 'ProjectID'
    ) > 0,
    "ALTER TABLE `Project` CHANGE COLUMN `ProjectID` `LegacyProjectID` INT(10) UNSIGNED NOT NULL",
    "SELECT 1"
));

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Geofence


SET @s = (SELECT IF(
    (SELECT COUNT(*)
       FROM INFORMATION_SCHEMA.COLUMNS
        WHERE table_name = 'Geofence'
        AND table_schema = DATABASE()
        AND column_name = 'CustomerUID'
    ) > 0,
    "ALTER TABLE `Geofence` CHANGE COLUMN `CustomerUID` `fk_CustomerUID` VARCHAR(36) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_unicode_ci' NOT NULL",
    "SELECT 1"
));

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Subscription

SET @s = (SELECT IF(
    (SELECT COUNT(*)
       FROM INFORMATION_SCHEMA.COLUMNS
        WHERE table_name = 'Subscription'
        AND table_schema = DATABASE()
        AND column_name = 'CustomerUID'
    ) > 0,
    "ALTER TABLE `Subscription` CHANGE COLUMN `CustomerUID` `fk_CustomerUID` VARCHAR(36) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_unicode_ci' NOT NULL",
    "SELECT 1"
));

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

*/

 