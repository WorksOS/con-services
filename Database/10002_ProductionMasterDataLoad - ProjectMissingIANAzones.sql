-- USE `VSS-ProjectMDM`;

/***
-- 109 ''
SELECT distinct ProjectTimeZone, ProjectID 
	FROM Project p
   WHERE LandfillTimeZone LIKE ''
 ORDER BY LandfillTimeZone, ProjectTimeZone;
 ***/
 
 
-- 'Coordinated Universal Time'          Etc/UTC  
UPDATE Project p
		SET LandfillTimeZone = 'Etc/UTC'
   WHERE LandfillTimeZone LIKE ''
		AND ProjectTimeZone LIKE 'Coordinated Universal Time';
 
-- 'E. Europe Standard Time'             Europe/Chisinau 
UPDATE Project p
		SET LandfillTimeZone = 'Europe/Chisinau'
   WHERE LandfillTimeZone LIKE ''
		AND ProjectTimeZone LIKE 'E. Europe Standard Time';
 
-- 'Jerusalem Standard Time'              Asia/Jerusalem
UPDATE Project p
		SET LandfillTimeZone = 'Asia/Jerusalem'
   WHERE LandfillTimeZone LIKE ''
		AND ProjectTimeZone LIKE 'Jerusalem Standard Time';
 
-- 'Malay Peninsula Standard Time'     Asia/Kuala_Lumpur
UPDATE Project p
		SET LandfillTimeZone = 'Asia/Kuala_Lumpur'
   WHERE LandfillTimeZone LIKE ''
		AND ProjectTimeZone LIKE 'Malay Peninsula Standard Time'; 
    
-- 'Russia TZ 6 Standard Time'            Asia/Krasnoyarsk
UPDATE Project p
		SET LandfillTimeZone = 'Asia/Krasnoyarsk'
   WHERE LandfillTimeZone LIKE ''
		AND ProjectTimeZone LIKE 'Russia TZ 6 Standard Time'; 
 
-- 'Russia TZ 9 Standard Time'            Asia/Vladivostok
UPDATE Project p
		SET LandfillTimeZone = 'Asia/Vladivostok'
   WHERE LandfillTimeZone LIKE ''
		AND ProjectTimeZone LIKE 'Russia TZ 9 Standard Time'; 
 
-- 'Vest-Europa (normaltid)'      W. Europe Standard Time
UPDATE Project p
		SET LandfillTimeZone = 'W. Europe Standard Time'
   WHERE LandfillTimeZone LIKE ''
		AND ProjectTimeZone LIKE 'Vest-Europa (normaltid)';  
    
-- 'West-Europa (standaardtijd)'      W. Europe Standard Time
UPDATE Project p
		SET LandfillTimeZone = 'W. Europe Standard Time'
   WHERE LandfillTimeZone LIKE ''
		AND ProjectTimeZone LIKE 'West-Europa (standaardtijd)';  

-- 'Hora estándar romance'      Europe/Madrid
-- the project site is actually in Arizona, looks like a test project - rubbish
UPDATE Project p
		SET LandfillTimeZone = 'Europe/Madrid'
   WHERE LandfillTimeZone LIKE ''
		AND ProjectTimeZone LIKE 'Hora estándar romance'
    AND ProjectID = 3465;  
    
-- 'Mitteleuropäische Zeit'               Europe/Sarajevo OR Europe/Skopje OR Europe/Zagreb OR Europe/Warsaw (Central European Time)  OR Europe/Berlin
UPDATE Project p
		SET LandfillTimeZone = 'Europe/Berlin'
   WHERE LandfillTimeZone LIKE ''
		AND ProjectTimeZone LIKE 'Mitteleuropäische Zeit'
    AND ProjectID IN (2403, 2392, 2391); 
    
-- 'Mitteleuropäische Zeit'               Europe/Sarajevo OR Europe/Skopje OR Europe/Zagreb OR Europe/Warsaw (Central European Time)  OR Europe/Berlin
UPDATE Project p
		SET LandfillTimeZone = 'Europe/Paris'
   WHERE LandfillTimeZone LIKE ''
		AND ProjectTimeZone LIKE 'Mitteleuropäische Zeit'
    AND ProjectID IN (2556);  
    
-- 'Mitteleuropäische Zeit'               Europe/Sarajevo OR Europe/Skopje OR Europe/Zagreb OR Europe/Warsaw (Central European Time)  OR Europe/Berlin
UPDATE Project p
		SET LandfillTimeZone = 'Europe/Zurich'
   WHERE LandfillTimeZone LIKE ''
		AND ProjectTimeZone LIKE 'Mitteleuropäische Zeit'
    AND ProjectID IN (2910, 2958, 3266);    
    
-- 'Mitteleuropäische Zeit'               Europe/Sarajevo OR Europe/Skopje OR Europe/Zagreb OR Europe/Warsaw (Central European Time)  OR Europe/Berlin
UPDATE Project p
		SET LandfillTimeZone = 'Europe/Rome'
   WHERE LandfillTimeZone LIKE ''
		AND ProjectTimeZone LIKE 'Mitteleuropäische Zeit'
    AND ProjectID IN (2956);      
    
-- 'Paris, Madrid'                        Europe/Madrid OR Europe/Paris
UPDATE Project p
		SET LandfillTimeZone = 'Europe/Berlin'
   WHERE LandfillTimeZone LIKE ''
		AND ProjectTimeZone LIKE 'Paris, Madrid'
    AND ProjectID IN (2641);     
    
-- 'Paris, Madrid'                        Europe/Madrid OR Europe/Paris
UPDATE Project p
		SET LandfillTimeZone = 'Europe/Paris'
   WHERE LandfillTimeZone LIKE ''
		AND ProjectTimeZone LIKE 'Paris, Madrid'
    AND ProjectID IN (2767);      
    
-- 'Russia TZ 2 Standard Time'            Europe/Moscow OR Europe/Volgograd
UPDATE Project p
		SET LandfillTimeZone = 'Europe/Moscow'
   WHERE LandfillTimeZone LIKE ''
		AND ProjectTimeZone LIKE 'Russia TZ 2 Standard Time'
    AND ProjectID IN (3692,3767,3783,3913,3929,4041,4204);   
 
-- 'Russia TZ 2 Standard Time'            Europe/Moscow OR Europe/Volgograd
UPDATE Project p
		SET LandfillTimeZone = 'Asia/Oral'
   WHERE LandfillTimeZone LIKE ''
		AND ProjectTimeZone LIKE 'Russia TZ 2 Standard Time'
    AND ProjectID IN (3106);    
    
-- 'Środkowoeuropejski czas stand.'      Africa/Monrovia OR Atlantic/Reykjavik (GMT)
UPDATE Project p
		SET LandfillTimeZone = 'Australia/Perth'
   WHERE LandfillTimeZone LIKE ''
		AND ProjectTimeZone LIKE 'Środkowoeuropejski czas stand.'
    AND ProjectID IN (3414);   

-- 'Västeuropa, normaltid'               }  Europe/Amsterdam OR Europe/Berlin OR Europe/Rome OR Europe/Stockholm OR Europe/Vienna   
UPDATE Project p
		SET LandfillTimeZone = 'Australia/Melbourne'
   WHERE LandfillTimeZone LIKE ''
		AND ProjectTimeZone LIKE 'Västeuropa, normaltid'
    AND ProjectID IN (2806, 2808);  
 
-- 'Västeuropa, normaltid'               }  Europe/Amsterdam OR Europe/Berlin OR Europe/Rome OR Europe/Stockholm OR Europe/Vienna   
UPDATE Project p
		SET LandfillTimeZone = 'Europe/Stockholm'
   WHERE LandfillTimeZone LIKE ''
		AND ProjectTimeZone LIKE 'Västeuropa, normaltid'
    AND ProjectID IN (2863); 
 
   




