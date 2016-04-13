@echo off

net session >nul 2>&1
if %errorLevel% == 0 (
	for %%x in (in out) do (
 		netsh advfirewall firewall add rule action=allow profile=any protocol=any enable=yes direction=%%x name=ShinraMeter program = "%~dp0%ShinraMeter.exe" > NUL 
	)
	echo Rule added to the firewall
) else (
	echo You must run this program in admin mode 
)

pause >nul
