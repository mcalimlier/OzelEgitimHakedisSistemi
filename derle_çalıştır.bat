@echo off
setlocal
cd /d "%~dp0"

set "PROJECT=.\src\OzelEgitimHakedis.Wpf\OzelEgitimHakedis.Wpf.csproj"
set "BUILD_LOG=%~dp0OEHS_DERLEME_LOG.txt"
set "RUN_LOG=%~dp0OEHS_CALISMA_LOG.txt"
set "BIN_LOG=%~dp0OEHS_DERLEME_LOG.binlog"

echo.
echo ==========================================
echo   OEHS - DERLE VE CALISTIR
echo ==========================================
echo.

echo [%date% %time%] Derleme basliyor... > "%BUILD_LOG%"
dotnet build "%PROJECT%" --configuration Debug --no-restore -v:diagnostic -bl:"%BIN_LOG%" >> "%BUILD_LOG%" 2>&1

if errorlevel 1 (
    echo.
    echo [HATA] Derleme basarisiz.
    echo [INFO] Ayrinti: OEHS_DERLEME_LOG.txt
    echo.
    pause
    exit /b 1
)

echo [%date% %time%] Program baslatiliyor... > "%RUN_LOG%"
echo.
echo [OK] Derleme tamamlandi.
echo [OK] OEHS baslatiliyor...
echo.

dotnet run --project "%PROJECT%" --configuration Debug --no-build >> "%RUN_LOG%" 2>&1
set "EXIT_CODE=%errorlevel%"

echo. >> "%RUN_LOG%"
echo [%date% %time%] Program cikis kodu: %EXIT_CODE% >> "%RUN_LOG%"

echo.
echo ==========================================
echo   OEHS SONLANDI - CIKIS KODU: %EXIT_CODE%
echo ==========================================
echo.
echo [INFO] Calisma logu: OEHS_CALISMA_LOG.txt
echo.
pause
exit /b %EXIT_CODE%
