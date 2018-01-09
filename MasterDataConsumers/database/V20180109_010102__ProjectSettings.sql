
SET @s = (SELECT IF(
    (SELECT COUNT(*)
       FROM INFORMATION_SCHEMA.COLUMNS
        WHERE table_name = 'ProjectSettings'
        AND table_schema = DATABASE()
        AND column_name = 'UserID'
    ) > 0,
    "SELECT 1",
    "ALTER TABLE `ProjectSettings` 
			ADD COLUMN `UserID` VARCHAR(36) NULL AFTER `Settings`,
            ADD COLUMN `fk_ProjectSettingsTypeID` INT(10) NULL DEFAULT 0 AFTER `fk_ProjectUID`"
));  

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt; 
