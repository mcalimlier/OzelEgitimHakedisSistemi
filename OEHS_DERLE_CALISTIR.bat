@echo off
setlocal
cd /d "%~dp0"

echo.
echo ==========================================
echo   OEHS - DERLE VE CALISTIR
echo ==========================================
echo.

dotnet build ".\src\OzelEgitimHakedis.Wpf\OzelEgitimHakedis.Wpf.csproj" --configuration Debug
if errorlevel 1 (
    echo.
    echo [HATA] Derleme basarisiz. Program baslatilmadi.
    echo.
    pause
    exit /b 1
)

echo.
echo [OK] Derleme tamamlandi.
echo [OK] OEHS baslatiliyor...
echo.

start "" ".\src\OzelEgitimHakedis.Wpf\bin\Debug\net8.0-windows\OEHS.exe"

exit /b 0
