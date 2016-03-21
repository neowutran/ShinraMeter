@echo off

goto check_Permissions

:check_Permissions
    echo Administrative permissions required. Detecting permissions...

    net session >nul 2>&1
    if %errorLevel% == 0 (
        for %%x in (in out) do (
			netsh advfirewall firewall add rule action=allow profile=any protocol=any enable=yes direction=%%x name=ShinraMeter program = "%~dp0%ShinraMeter.exe" > NUL
		)
		set /p delBuild=Rule added to the firewall: 
    ) else (
		set /p delBuild=You must run this program in admin mode: 
    )

    pause >nul
	
	

