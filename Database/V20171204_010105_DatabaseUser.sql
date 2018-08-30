-- assume scope is on the appropriate database e.g. `VSS-Utilization`
-- USE `VSS-Landfill-Dev`;
 
 /*
SET @s = (SELECT IF(
    (SELECT COUNT(*)
        FROM mysql.user WHERE User LIKE 'vssLandfillUsr'
    ) > 0,
    "SELECT 1",
    "CREATE USER 'vssLandfillUsr' IDENTIFIED BY '(mRty94hER'" 
));

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

GRANT CREATE TEMPORARY TABLES, DELETE, EXECUTE, UPDATE, INSERT, SHOW VIEW, SELECT ON TABLE * TO 'vssLandfillUsr'; 
*/

 