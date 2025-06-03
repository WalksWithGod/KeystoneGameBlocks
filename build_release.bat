rem to add this to VS.NET  Tools\  menu along with any other tools you want one click away to the following
rem Tools\External Tools
rem click Addd
rem title = Quick Build Release.bat
rem command = browse for this .bat file
rem check "use output window"
rem done!

"C:\Program Files\Microsoft Visual Studio 9.0\Common7\IDE\devenv" /out "E:\dev\c#\KeystoneGameBlocks\Editor\bin\Release\release_build.log" /build Release "E:\dev\c#\KeystoneGameBlocks\Keystone Game Blocks.sln"


