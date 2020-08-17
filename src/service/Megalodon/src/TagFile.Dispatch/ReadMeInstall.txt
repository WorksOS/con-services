Example install via powershell
  sc.exe create MegalodonSvc binpath= c:\MegalodonService\MegalodonSvc.dll start= auto displayname= MegalodonSvc
Remove service 
  sc.exe delete MegalodonSvc