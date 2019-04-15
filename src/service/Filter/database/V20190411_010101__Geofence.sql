SET @s = (SELECT IF(
    (SELECT COUNT(*)
       FROM INFORMATION_SCHEMA.COLUMNS
        WHERE table_name = 'Geofence'
        AND table_schema = DATABASE()
        AND column_name = 'PolygonST'
    ) > 0 AND
    (SELECT COUNT(*)
       FROM INFORMATION_SCHEMA.COLUMNS
        WHERE table_name = 'Geofence'
        AND table_schema = DATABASE()
		AND column_name = 'GeometryWKT'
    ) > 0,
    "UPDATE `Geofence` SET `PolygonST` = ST_GeomFromText(`GeometryWKT`) WHERE `GeometryWKT` IS NOT NULL AND CHAR_LENGTH(`GeometryWKT`) < 4000",
	"SELECT 1"
));  

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt; 

SET @s = (SELECT IF(
    (SELECT COUNT(*)
       FROM INFORMATION_SCHEMA.COLUMNS
        WHERE table_name = 'Geofence'
        AND table_schema = DATABASE()
        AND column_name = 'GeometryWKT'
    ) > 0,
    "ALTER TABLE `Geofence` CHANGE COLUMN `GeometryWKT`  `OldGeometryWKT` varchar(4000) DEFAULT NULL;",
	"SELECT 1"
));  

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt; 



