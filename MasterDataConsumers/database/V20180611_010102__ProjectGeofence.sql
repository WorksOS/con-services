 
SET @s = (SELECT IF(
    (SELECT COUNT(*)
			FROM  INFORMATION_SCHEMA.STATISTICS
			WHERE TABLE_SCHEMA = DATABASE()
			AND   TABLE_NAME   = 'ProjectGeofence'
            AND INDEX_NAME = 'IX_ProjectGeofence_GeofenceUID'
		) > 0,
    "SELECT 1",
    "ALTER TABLE `ProjectGeofence` ADD KEY IX_ProjectGeofence_GeofenceUID (fk_GeofenceUID)"
));  

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;