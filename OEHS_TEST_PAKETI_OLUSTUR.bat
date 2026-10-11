@echo off
setlocal EnableExtensions
cd /d "%~dp0" || exit /b 1

rem Keep the original launcher name, but route all packaging through the
rem guarded script that runs tests and preserves the last successful ZIP.
set "UPDATED_SCRIPT=%~dp0OEHS_TEST_PAKETI_OLUSTUR_GUNCEL.bat"

if not exist "%UPDATED_SCRIPT%" (
    echo [HATA] Guncel paket olusturucu bulunamadi:
    echo        %UPDATED_SCRIPT%
    echo Depoyu GitHub'dan guncelleyin ve tekrar deneyin.
    pause
    exit /b 1
)

call "%UPDATED_SCRIPT%"
exit /b %errorlevel%
