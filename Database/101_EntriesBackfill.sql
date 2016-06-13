/**** Landfill Entries Backfill ***/

SET SQL_SAFE_UPDATES=0;

UPDATE `Entries` etr, 
(SELECT `ProjectID`, `ProjectUID` FROM `Project`) prj 
SET etr.`ProjectUID` = prj.`ProjectUID`
WHERE etr.`ProjectID` = prj.`ProjectID`;

SET SQL_SAFE_UPDATES=1;

 