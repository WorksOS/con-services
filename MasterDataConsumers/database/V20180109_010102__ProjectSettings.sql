
SET @s = (SELECT IF(
    (SELECT COUNT(*)
       FROM INFORMATION_SCHEMA.COLUMNS
        WHERE table_name = 'ProjectSettings'
        AND table_schema = DATABASE()
        AND column_name = 'UserID'
    ) > 0,
    "SELECT 1",
    "ALTER TABLE `ProjectSettings` 
			ADD COLUMN `UserID` VARCHAR(36) NOT NULL AFTER `Settings`,
            ADD COLUMN `fk_ProjectSettingsTypeID` INT(10) NOT NULL DEFAULT 0 AFTER `fk_ProjectUID`"
));  

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt; 

     
SET @s = (SELECT IF(
    (SELECT COUNT(*)
			FROM  INFORMATION_SCHEMA.TABLE_CONSTRAINTS
			WHERE TABLE_SCHEMA = DATABASE()
			AND   TABLE_NAME   = 'ProjectSettings'
			AND   CONSTRAINT_NAME   = 'fk_ProjectUID'
		) > 0,
    "ALTER TABLE `ProjectSettings` DROP KEY `fk_ProjectUID`,
                                   ADD  KEY `IX_ProjectUID_UserID` (fk_ProjectUID, UserID),
								   DROP PRIMARY KEY,
		                           ADD  PRIMARY KEY (fk_ProjectUID, UserID, fk_ProjectSettingsTypeID);",
	"SELECT 1"
));  

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;