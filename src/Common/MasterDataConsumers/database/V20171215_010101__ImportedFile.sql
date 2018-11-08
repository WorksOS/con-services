SET @s = (SELECT IF(
    (SELECT COUNT(*)
       FROM INFORMATION_SCHEMA.STATISTICS
        WHERE table_name = 'ImportedFile'
        AND table_schema = DATABASE()
        AND index_name = 'UIX_ImportedFile_ProjectUID_Name_SurveyedUTC'
    ) > 0,
    "ALTER TABLE `ImportedFile` DROP index `UIX_ImportedFile_ProjectUID_Name_SurveyedUTC`",
    "SELECT 1"
));  

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt; 
