@echo off
rem msbuild Tera.sln /t:rebuild /p:Configuration=Release /p:Platform="Any CPU" /fl /flp:logfile=ShinraMeter.log;verbosity=diagnostic
rem msbuild Tera.sln /p:Configuration=Release /p:Platform="Any CPU" /fl /flp:logfile=ShinraMeter.log;verbosity=normal
msbuild DamageMeter.UI/DamageMeter.UI.csproj /t:publish /p:Configuration=Release,TargetFramework=netcoreapp3.1,RuntimeIdentifer=win-x64 /p:PublishProfile=FolderProfile 
msbuild Publisher/Publisher.csproj /t:build /p:Configuration=Release,TargetFramework=netcoreapp3.1,RuntimeIdentifer=win-x64

set output=.\ShinraMeterV
set source=.
set variant=Release\publish
rmdir /Q /S "%output%"
md "%output%
md "%output%\resources"
md "%output%\resources\config"
md "%output%\lib"

xcopy "%source%\DamageMeter.UI\bin\%variant%" "%output%\" /E
xcopy "%source%\lib" "%output%\lib\" /E
copy "%source%\ReadmeUser.txt" "%output%\readme.txt"
copy "%source%\add_firewall_exception.bat" "%output%\add_firewall_exception.bat"
xcopy "%source%\resources" "%output%\resources\" /E /EXCLUDE:.\exclude.txt
copy "%source%\.git\modules\resources\data\refs\heads\master" "%output%\resources\head"
del "%output%\*.xml"
del "%output%\error.log"
del "%output%\*.vshost*"
del "%output%\*.pdb"
del "%output%\resources\data\hotdot\glyph*.tsv"
del "%output%\resources\data\hotdot\abnormal.tsv"
"%source%\Publisher\bin\release\netcoreapp3.1\Publisher.exe"
