            
SET @s = (SELECT IF(
    (SELECT COUNT(*)
			FROM  INFORMATION_SCHEMA.STATISTICS
			WHERE TABLE_SCHEMA = DATABASE()
			AND   TABLE_NAME   = 'Geofence'
            AND INDEX_NAME = 'IX_Geofence_CustomerUID'
		) > 0,
    "SELECT 1",
    "ALTER TABLE `Geofence` ADD KEY IX_Geofence_CustomerUID (fk_CustomerUID)"
));  

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;