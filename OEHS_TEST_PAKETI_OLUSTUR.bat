@echo off
setlocal EnableExtensions
cd /d "%~dp0"

set "PROJECT=.\src\OzelEgitimHakedis.Wpf\OzelEgitimHakedis.Wpf.csproj"
set "DIST=.\dist"
set "OUT=%DIST%\OEHS-Test-Paketi"
set "ZIP=%DIST%\OEHS-Test-Paketi.zip"

echo.
echo ==========================================
echo   OEHS - ARKADASA TEST PAKETI OLUSTUR
echo ==========================================
echo.

if exist "%OUT%" rmdir /s /q "%OUT%"
if exist "%ZIP%" del /q "%ZIP%"
if not exist "%DIST%" mkdir "%DIST%"

echo [1/2] Windows x64 test surumu hazirlaniyor...
dotnet publish "%PROJECT%" --configuration Release --runtime win-x64 --self-contained true ^
  --output "%OUT%" ^
  /p:PublishSingleFile=true ^
  /p:IncludeNativeLibrariesForSelfExtract=true ^
  /p:DebugSymbols=false ^
  /p:DebugType=None

if errorlevel 1 (
    echo.
    echo [HATA] Paket olusturulamadi.
    pause
    exit /b 1
)

if not exist "%OUT%\OEHS.exe" (
    echo.
    echo [HATA] OEHS.exe bulunamadi.
    pause
    exit /b 1
)

echo.
echo [2/2] ZIP paketi olusturuluyor...
powershell -NoProfile -ExecutionPolicy Bypass -Command "Compress-Archive -Path '%OUT%\OEHS.exe' -DestinationPath '%ZIP%' -Force"

if errorlevel 1 (
    echo.
    echo [HATA] ZIP olusturulamadi.
    pause
    exit /b 1
)

echo.
echo [OK] Test paketi hazir:
echo      %ZIP%
echo.
echo Arkadasiniza sadece bu ZIP dosyasini gonderebilirsiniz.
echo Kaynak kod, GitHub deposu veya gelistirme klasoru gonderilmez.
echo.
pause
exit /b 0
