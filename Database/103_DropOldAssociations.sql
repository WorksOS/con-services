/*** Drop columns which are now in link tables. Script to be run after backfill. ***/

/* Drop CustomerUID column from Project Table if there */
SET @s = (SELECT IF(
    (SELECT COUNT(*)
       FROM INFORMATION_SCHEMA.COLUMNS
        WHERE table_name = 'Project'
        AND table_schema = DATABASE()
        AND column_name = 'CustomerUID'
    ) = 0,
    "SELECT 1",
    "ALTER TABLE `Project` DROP COLUMN `CustomerUID`"
));

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

/* Drop SubscriptionUID column from Project Table if there */
SET @s = (SELECT IF(
    (SELECT COUNT(*)
       FROM INFORMATION_SCHEMA.COLUMNS
        WHERE table_name = 'Project'
        AND table_schema = DATABASE()
        AND column_name = 'SubscriptionUID'
    ) = 0,
    "SELECT 1",
    "ALTER TABLE `Project` DROP COLUMN `SubscriptionUID`"
));

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

/* Drop ProjectUID column from Geofence Table if there */
SET @s = (SELECT IF(
    (SELECT COUNT(*)
       FROM INFORMATION_SCHEMA.COLUMNS
        WHERE table_name = 'Geofence'
        AND table_schema = DATABASE()
        AND column_name = 'ProjectUID'
    ) = 0,
    "SELECT 1",
    "ALTER TABLE `Geofence` DROP COLUMN `ProjectUID`"
));

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

