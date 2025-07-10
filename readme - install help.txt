DirectX 12 
	- d3dx_42.dll needs to be installed.  Install DX12 and if any problems, find it and place it
          in the libs\TV3D65\ dll where mtv3d65.dll is located

msvcr71.dll is an old dll from .net 1.1 days and there is no official redistributable still available for it.
	- mtv3d65.dll requires this to be placed in C:\Windows\SysWOW64
          I have included a copy in Libs\TV3D65\

	- note: i did not have to regsrv32 it.

in CoreCliet.Initialize() on line _audio.Initialize(graphics.Handle); i have the same filenotfoundexception.  
im not sure if its vorbisdotnet or directsound.  just commenting out _audio.Initialize(graphics.Handle); allows the app to run.

Windows Defender "Controlled Folder Acess" & File Attributes = Read Only
	- if for some reason you have issues with .css scripts being copied from mods\\caesar\\scripts to \\bin\\...\\
          make sure the folder and file attributes are NOT read only.
	- Make sure if Windows Defender has "Controlled Folder Access" enabled and the relevant path is affected, 
          add KeyEdit.exe as an "Allowed App"

--------------------------------------
If any strange issues loading any other external DLL that fails to load, try
Dependency Walker (binaries available)
https://github.com/lucasg/Dependencies





