@echo off
setlocal EnableExtensions
cd /d "%~dp0"

set "BUILD_LOG=%~dp0OEHS_DERLEME_LOG.txt"
set "RUN_LOG=%~dp0OEHS_CALISMA_LOG.txt"
set "BIN_LOG=%~dp0OEHS_DERLEME_LOG.binlog"

echo.
echo ==========================================
echo   OEHS - DERLE, LOG KAYDET VE CALISTIR
echo ==========================================
echo.
echo Log dosyalari:
echo   %BUILD_LOG%
echo   %RUN_LOG%
echo   %BIN_LOG%
echo.

echo [%date% %time%] DERLEME BASLADI > "%BUILD_LOG%"
echo ========================================== >> "%BUILD_LOG%"
echo OEHS WPF DEBUG BUILD >> "%BUILD_LOG%"
echo ========================================== >> "%BUILD_LOG%"
echo. >> "%BUILD_LOG%"

dotnet build ".\src\OzelEgitimHakedis.Wpf\OzelEgitimHakedis.Wpf.csproj" --configuration Debug --no-restore -v:diagnostic -bl:"%BIN_LOG%" >> "%BUILD_LOG%" 2>&1
set "BUILD_EXIT=%errorlevel%"

echo. >> "%BUILD_LOG%"
echo ========================================== >> "%BUILD_LOG%"
echo [%date% %time%] DERLEME BITTI - EXIT CODE: %BUILD_EXIT% >> "%BUILD_LOG%"
echo ========================================== >> "%BUILD_LOG%"

if not "%BUILD_EXIT%"=="0" (
    echo.
    echo [HATA] Derleme basarisiz.
    echo [HATA] Ayrintili log:
    echo %BUILD_LOG%
    echo.
    echo Bu dosyayi bana yukle.
    echo.
    pause
    exit /b %BUILD_EXIT%
)

echo.
echo [OK] Derleme tamamlandi.
echo.

echo [%date% %time%] PROGRAM BASLATILIYOR > "%RUN_LOG%"
echo ========================================== >> "%RUN_LOG%"
echo OEHS WPF DEBUG CALISMA LOGU >> "%RUN_LOG%"
echo ========================================== >> "%RUN_LOG%"
echo. >> "%RUN_LOG%"

echo [OK] OEHS baslatiliyor...
echo [NOT] Program kapanirsa hata/istisna bilgisi calisma loguna yazilacak.
echo.

dotnet run --project ".\src\OzelEgitimHakedis.Wpf\OzelEgitimHakedis.Wpf.csproj" --configuration Debug --no-build >> "%RUN_LOG%" 2>&1
set "RUN_EXIT=%errorlevel%"

echo. >> "%RUN_LOG%"
echo ========================================== >> "%RUN_LOG%"
echo [%date% %time%] PROGRAM SONLANDI - EXIT CODE: %RUN_EXIT% >> "%RUN_LOG%"
echo ========================================== >> "%RUN_LOG%"

echo.
echo ==========================================
echo   OEHS PROGRAM SONLANDI
echo ==========================================
echo.
echo Calisma sonucu: %RUN_EXIT%
echo.
echo Log dosyalari:
echo   DERLEME : %BUILD_LOG%
echo   CALISMA : %RUN_LOG%
echo   BINLOG  : %BIN_LOG%
echo.
echo Bu iki TXT dosyasini bana yukleyebilirsin.
echo Program acilip hemen kapandiysa ozellikle
echo OEHS_CALISMA_LOG.txt dosyasini yukle.
echo.
pause
exit /b %RUN_EXIT%
