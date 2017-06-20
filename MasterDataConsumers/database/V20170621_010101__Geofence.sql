SET @s = (SELECT IF(
    (SELECT COUNT(*)
       FROM INFORMATION_SCHEMA.COLUMNS
        WHERE table_name = 'Geofence'
        AND table_schema = DATABASE()
        AND column_name = 'Description'
        AND IS_NULLABLE = 'NO'
    ) > 0,    
    "ALTER TABLE `Geofence` MODIFY COLUMN `Description` nvarchar(2000) NULL DEFAULT NULL",
    "SELECT 1"
));  

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;  
