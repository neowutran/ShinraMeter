@echo off
set output=.\debug
set source=.
set variant=Debug
rmdir /Q /S "%output%"
md "%output%
md "%output%\resources"

copy "%source%\Tera.Sniffing\bin\%variant%\*" "%output%\"
copy "%source%\Tera.DamageMeter.UI.WPF\bin\%variant%\*" "%output%\"
copy "%source%\ReadmeUser.txt" "%output%\readme.txt"
xcopy "%source%\resources" "%output%\resources\" /E
del "%output%\SharpPcap.xml"
del "%output%\PacketDotNet.xml"
del "%output%\*.vshost*"
