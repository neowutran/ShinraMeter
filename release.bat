@echo off
msbuild Tera.sln /p:Configuration=Release /p:Platform="Any CPU"
set output=.\ShinraMeterV
set source=.
set variant=Release
rmdir /Q /S "%output%"
md "%output%
md "%output%\resources"

copy "%source%\DamageMeter.Sniffing\bin\%variant%\*" "%output%\"
copy "%source%\DamageMeter.UI\bin\%variant%\*" "%output%\"
copy "%source%\ReadmeUser.txt" "%output%\readme.txt"
xcopy "%source%\resources" "%output%\resources\" /E
del "%output%\SharpPcap.xml"
del "%output%\PacketDotNet.xml"
del "%output%\*.vshost*"
del "%output%\*.pdb"
del "%output%\resources\config\*.xml"