SET @s = (SELECT IF(
    (SELECT COUNT(*)
       FROM INFORMATION_SCHEMA.COLUMNS
        WHERE table_name = 'ImportedFile'
        AND table_schema = DATABASE()
        AND column_name = 'MinZoomLevel'
    ) > 0,
    "SELECT 1",
    "ALTER TABLE `ImportedFile` ADD COLUMN `MinZoomLevel` INT(11) DEFAULT 0 AFTER SurveyedUTC, ADD COLUMN `MaxZoomLevel` INT(11) DEFAULT 0 AFTER MinZoomLevel"
));  

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt; 