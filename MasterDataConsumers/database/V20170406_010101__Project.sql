SET @s = (SELECT IF(
    (SELECT COUNT(*)
       FROM INFORMATION_SCHEMA.COLUMNS
        WHERE table_name = 'Project'
        AND table_schema = DATABASE()
        AND column_name = 'CoordinateSystemFileName'
    ) > 0,
    "SELECT 1",
    "ALTER TABLE `Project` ADD COLUMN `CoordinateSystemFileName` VARCHAR(256) NULL DEFAULT NULL AFTER `PolygonST`"
));  

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;   

SET @s = (SELECT IF(
    (SELECT COUNT(*)
       FROM INFORMATION_SCHEMA.COLUMNS
        WHERE table_name = 'Project'
        AND table_schema = DATABASE()
        AND column_name = 'CoordinateSystemLastActionedUTC'
    ) > 0,
    "SELECT 1",
    "ALTER TABLE `Project` ADD COLUMN `CoordinateSystemLastActionedUTC` DATETIME NULL DEFAULT NULL AFTER `LastActionedUTC`"
)); 

select  @s; 

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;   