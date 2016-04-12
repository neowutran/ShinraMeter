@echo off

net session >nul 2>&1
if %errorLevel% == 0 (
	for %%x in (in out) do (
		netsh advfirewall firewall add rule name="TeraServer" direction=%%x action=allow protocol=tcp profile=any enable=yes remoteip=79.110.94.211,79.110.94.212,79.110.94.213,79.110.94.214,79.110.94.215,79.110.94.216,79.110.94.217,79.110.94.218,79.110.94.219,79.110.94.218,208.67.49.28,208.67.49.52,208.67.49.68,208.67.49.84,208.67.49.92,208.67.49.100,208.67.49.60,91.225.237.5,91.225.237.6,91.225.237.7,91.225.237.8,91.225.237.9,91.225.237.11,91.225.237.12,220.228.174.67,220.228.174.68,203.141.240.215,203.141.240.216,203.141.240.217,183.110.3.7,183.110.3.8,183.110.3.6,183.110.3.10,183.110.3.9
	)
	echo Rule added to the firewall
) else (
	echo You must run this program in admin mode 
)

pause >nul
