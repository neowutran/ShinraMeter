@echo off
set output=.\release
set source=.
set variant=Debug
rmdir /Q /S "%output%"
md "%output%
md "%output%\resources"
copy "%source%\Tera.Sniffing\bin\%variant%\*" "%output%\"
copy "%source%\Tera.DamageMeter\bin\%variant%\*" "%output%\"
copy "%source%\ReadmeUser.txt" "%output%\readme.txt"
copy "%source%\resources" "%output%\resources\"
del "%output%\SharpPcap.xml"
del "%output%\PacketDotNet.xml"
del "%output%\*.vshost*"
