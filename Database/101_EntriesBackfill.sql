/**** Landfill Entries Backfill ***/

SET SQL_SAFE_UPDATES=0;

UPDATE `Entries` etr, 
(SELECT `ProjectID`, `ProjectUID` FROM `Project`) prj 
SET etr.`ProjectUID` = prj.`ProjectUID`
WHERE etr.`ProjectID` = prj.`ProjectID`;

UPDATE `Entries` etr 
INNER JOIN `GeofenceUID` geo ON etr.`ProjectUID` = geo.`ProjectUID`
SET etr.`GeofenceUID` = geo.`GeofenceUID`
WHERE etr.`ProjectUID` = geo.`ProjectUID` AND etr.`GeofenceUID` IS NULL;

SET SQL_SAFE_UPDATES=1;


 