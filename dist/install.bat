@echo off
REM Копирует комплект в C:\Tablo и создаёт ярлык (не требует админа, если C:\ доступен)
set SRC=%~dp0
set DST=C:\Tablo
if not exist "%DST%" mkdir "%DST%"
xcopy /Y /E "%SRC%*" "%DST%\" >nul
echo Комплект скопирован в %DST%
echo Запуск: %DST%\Tablo.exe
pause

