@echo off
set output=.\ShinraMeterV
set source=.
set variant=Release
"%source%\Publisher\bin\%variant%\Publisher.exe" unpack
md "%output%\resources"
md "%output%\resources\config"
xcopy "%source%\resources" "%output%\resources\" /E /EXCLUDE:.\exclude.txt
copy "%source%\ShinraLauncher.exe" "%output%\"
copy "%source%\.git\modules\resources\data\refs\heads\master" "%output%\resources\head"
del "%output%\resources\data\hotdot\glyph*.tsv"
del "%output%\resources\data\hotdot\abnormal.tsv"
"%source%\Publisher\bin\%variant%\Publisher.exe"
