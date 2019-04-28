SET @s = (SELECT IF(
    (SELECT COUNT(*)
       FROM INFORMATION_SCHEMA.COLUMNS
        WHERE table_name = 'Geofence'
        AND table_schema = DATABASE()
        AND column_name = 'GeometryWKT'
    ) > 0,
    "ALTER TABLE `Geofence` CHANGE COLUMN `GeometryWKT`  `OldGeometryWKT` varchar(4000) DEFAULT NULL",
	"SELECT 1"
));  

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt; 
