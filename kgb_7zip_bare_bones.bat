ECHO Keystone Game Blocks Bare Bones Backup
ECHO -mhe switch = encrypting headers
ECHO -mmt switch = multithreading enabled

SET /P pass=password:

"c:\Program Files\7-zip\7z.exe" a -t7z F:\backups\%DATE:~-4%-%DATE:~4,2%-%DATE:~7,2%_KGB_BARE_BONES.7z "D:\dev\c#\KeystoneGameBlocks\" -mmt -p%pass%  -mhe -xr!_ReSharper\ -xr!Sims2Trek\ -xr!FPSCreator_Data\ -xr!fps_creator_packs\ -xr!log4net-1.2.10\ -xr!archive\ -xr!bin\ -xr!obj\ -xr!*.lib -xr!*.aqt -xr!*.ncb -xr!*.pch -xr!*.pdf -xr!*.pdb -xr!*.wmv -xr!*.avi -xr!*.wmf -xr!*.hfz -xr!*.rar -xr!*.gz -xr!*.zip -xr!*.dll -xr!*.exe -xr!*.Sims2* -xr!*.obj -xr!*.mtl -xr!*.max -xr!*.mdl -xr!*.3ds -xr!*.x -xr!*.tvm -xr!*.tva -xr!*.skp -xr!*.skb -xr!*.dds -xr!*.tga -xr!*.tva -xr!*.gif -xr!*.jpg -xr!*.bmp -xr!*.tif -xr!*.db -xr!*.png -xr!*.blend -xr!*.ogg -xr!*.mp3 -xr!*.terrain -xr!*.dat -xr!*.bin -xr!*.skp -xr!*.skb -xr!*.csv -xr!*.xls -xr!*.ppt -xr!*.fbx -xr!*.cur -xr!*.wav -xr!*.7z -xr!*.tmp -xr!meshes\ -xr!textures\ -xr!scifi_corridorCS98\

rem An example of what a batch file for creating an archive with a timestamp of the “month – day of month – year” is as follows:
REM .\location_of_7zip\7za.exe u -tzip “c:\location_of_archive\%DATE:~4,2%-%DATE:~7,2%-%DATE:~-2%.zip” “c:\location_of_files\*.txt”
 
 SET /P any =press any key to close...