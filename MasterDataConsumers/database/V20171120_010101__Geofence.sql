SET @s = (SELECT IF(
    (SELECT COUNT(*)
       FROM INFORMATION_SCHEMA.COLUMNS
        WHERE table_name = 'Geofence'
        AND table_schema = DATABASE()
        AND column_name = 'AreaSqMeters'
    ) > 0,
    "SELECT 1",
    "ALTER TABLE `Geofence` ADD COLUMN `AreaSqMeters` DECIMAL DEFAULT 0 AFTER Description"
));  

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt; 