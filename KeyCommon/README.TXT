Dec.18.2012  
-Hypnotron

About KeyCommon.Dll

KeyCommon.dll does not reference MTV3D65.dll or Keystone.dll

KeyCommon.Dll is designed to be referenced by client.exe, server.exe, keyscript.dll
and keystone.dll.  Ultimately I think I could merge KeyCommon and KeyMath since
neither reference Keystone.dll themselves but are both referenced by virtually all other assemblies.

It is because we do not want certain libraries to have access to the object types within
Keystone.DLL (keyscript.dll for example) that KeyCommon.DLL is used.

Game##.Dll is designed to be referenced by client.exe, server.exe and keyscript.dll but 
NOT keystone.dll.  Nor is it allowed to reference keystone.dll itself.  Game##.dll is entirely
agnostic to the scene and is only concerned with Simulation.  Thus it provides game specific
types to our scripts such as SensorContact, Waypoint, EmissionStrength, etc.

Ideally I want to move all that is not required in keyscript.dll and keycommon.dll to Game##.Dll.

May.15.2022 
I think KeyCommon.dll ideally should not be referenced by Keystone.Dll.  This was a mistake but i 
dont want to clean it up at this point.

