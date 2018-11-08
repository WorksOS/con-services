SET @s = (SELECT IF(
    (SELECT COUNT(*)
			FROM  INFORMATION_SCHEMA.STATISTICS
			WHERE TABLE_SCHEMA = DATABASE()
			AND   TABLE_NAME   = 'Asset'
			AND INDEX_NAME     = 'IX_Asset_LegacyAssetID'
		) > 0,
    "SELECT 1",
    "ALTER TABLE `Asset` ADD KEY IX_Asset_LegacyAssetID (LegacyAssetID, IsDeleted)"
));  

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;