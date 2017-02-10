@echo off
set output=.\ShinraMeterV
set source=.
set variant=Release
"%source%\Publisher\bin\%variant%\Publisher.exe" unpack
md "%output%\resources"
md "%output%\resources\config"
xcopy "%source%\resources" "%output%\resources\" /E /EXCLUDE:.\exclude.txt
del "%output%\resources\data\hotdot\glyph*.tsv"
del "%output%\resources\data\hotdot\abnormal.tsv"
"%source%\Publisher\bin\%variant%\Publisher.exe"
