ECHO Keystone Game Blocks Full Backup ETA ~20 minutes across network to W:\Backups, 1.5GB, 28% CPU Utilization
ECHO -mhe switch = encrypting headers
ECHO -mmt switch = multithreading enabled
SET /P pass=password:

"c:\Program Files\7-zip\7z.exe" a -t7z F:\Backups\%DATE:~-4%-%DATE:~4,2%-%DATE:~7,2%_KGB_FULL.7z "D:\dev\c#\KeystoneGameBlocks\" -mmt -p%pass% -mhe -xr!pool\

 SET /P any =press any key to close...