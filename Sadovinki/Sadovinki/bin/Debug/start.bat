@echo off
set /a field=15
set /a client_count=8

start Sadovinki.exe server %field% %client_count%
for /l %%A in (1, 1, %client_count%) do (
	start Sadovinki.exe client
)

pause

taskkill /im Sadovinki.exe /f