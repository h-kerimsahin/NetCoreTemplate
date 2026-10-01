@echo off
REM ============================================================
REM  KrmShn Net Core Backend Project - Visual Studio Template Installer
REM  Kullanim: install-template.bat   (Administrator gerekmez)
REM ============================================================

setlocal enabledelayedexpansion

set "ZIP_FILE=%~dp0KrmShnNetCoreBackendProject.zip"
set "VS_DOC_VERSIONS=2022 2019"
set "VS_ROAMING_VERSIONS=18.0 17.0 16.0"
set "INSTALLED_COUNT=0"

if not exist "%ZIP_FILE%" (
    echo [HATA] Template ZIP bulunamadi: %ZIP_FILE%
    echo Bu dosyayi KrmShnNetCoreBackendProject.zip ile ayni klasore koyun.
    pause
    exit /b 1
)

echo ============================================================
echo   KrmShn Net Core Backend Project Template Yukleniyor...
echo ============================================================
echo.

REM ---- Yontem 1: Documents\Visual Studio YY klasorleri ----
echo [1/3] Documents\Visual Studio klasorleri kontrol ediliyor...
for %%v in (%VS_DOC_VERSIONS%) do (
    set "TARGET_DIR=%USERPROFILE%\Documents\Visual Studio %%v\Templates\ProjectTemplates"
    if exist "%USERPROFILE%\Documents\Visual Studio %%v" (
        if not exist "!TARGET_DIR!" (
            mkdir "!TARGET_DIR!" >nul 2>&1
        )
        copy /Y "%ZIP_FILE%" "!TARGET_DIR!\" >nul
        if !errorlevel! equ 0 (
            echo   [OK] Visual Studio %%v (Documents)  ->  !TARGET_DIR!
            set /a INSTALLED_COUNT+=1
        ) else (
            echo   [HATA] VS %%v icin kopyalama basarisiz.
        )
    ) else (
        echo   [ATLA] Visual Studio %%v Documents klasoru bulunamadi.
    )
)

REM ---- Fallback: Documents\Visual Studio (version siz - yeni surumler) ----
set "TARGET_DIR=%USERPROFILE%\Documents\Visual Studio\Templates\ProjectTemplates"
if exist "%USERPROFILE%\Documents\Visual Studio" (
    if not exist "!TARGET_DIR!" mkdir "!TARGET_DIR!" >nul 2>&1
    copy /Y "%ZIP_FILE%" "!TARGET_DIR!\" >nul
    if !errorlevel! equ 0 (
        echo   [OK] Visual Studio (Documents\Visual Studio)  ->  !TARGET_DIR!
        set /a INSTALLED_COUNT+=1
    )
) else (
    echo   [ATLA] Documents\Visual Studio klasoru bulunamadi.
)

echo.
echo [2/3] AppData Visual Studio cache/installation klasorleri kontrol ediliyor...

REM ---- Yontem 2: AppData\Local\Microsoft\VisualStudio ProjectTemplates ----
for %%v in (%VS_ROAMING_VERSIONS%) do (
    set "VS_INST=%LOCALAPPDATA%\Microsoft\VisualStudio\%%v*"
    for /d %%d in ("!VS_INST!") do (
        set "INST_DIR=%%~fd"
        set "TARGET_DIR=!INST_DIR!\ProjectTemplates"
        if exist "!INST_DIR!" (
            if not exist "!TARGET_DIR!" mkdir "!TARGET_DIR!" >nul 2>&1
            copy /Y "%ZIP_FILE%" "!TARGET_DIR!\" >nul
            if !errorlevel! equ 0 (
                echo   [OK] VS Local (!INST_DIR!)  ->  !TARGET_DIR!
                set /a INSTALLED_COUNT+=1
            )

            REM ---- Cache temizle: ProjectTemplatesCache icinde eski kalanlari sil ----
            set "CACHE_DIR=!INST_DIR!\ProjectTemplatesCache"
            if exist "!CACHE_DIR!\KrmShnNetCoreBackendProject*" (
                echo   [TEMIZLE] Eski cache siliniyor: !CACHE_DIR!\KrmShnNetCoreBackendProject*
                rmdir /s /q "!CACHE_DIR!\KrmShnNetCoreBackendProject" >nul 2>&1
                del /q "!CACHE_DIR!\KrmShnNetCoreBackendProject.zip" >nul 2>&1
            )
        )
    )
)

REM ---- Yontem 3: AppData\Roaming ProjectTemplatesCache ----
for %%v in (%VS_ROAMING_VERSIONS%) do (
    set "VS_ROAM=%APPDATA%\Microsoft\VisualStudio\%%v*"
    for /d %%d in ("!VS_ROAM!") do (
        set "ROAM_DIR=%%~fd"
        set "CACHE_DIR=!ROAM_DIR!\ProjectTemplatesCache"
        if exist "!CACHE_DIR!" (
            REM ---- Eski cache temizle ----
            if exist "!CACHE_DIR!\KrmShnNetCoreBackendProject*" (
                echo   [TEMIZLE] Eski roaming cache siliniyor: !CACHE_DIR!\KrmShnNetCoreBackendProject*
                rmdir /s /q "!CACHE_DIR!\KrmShnNetCoreBackendProject" >nul 2>&1
                del /q "!CACHE_DIR!\KrmShnNetCoreBackendProject.zip" >nul 2>&1
            )
            REM ---- ZIP'i cache icine de kopyala (bazi VS surumleri buradan okur) ----
            copy /Y "%ZIP_FILE%" "!CACHE_DIR!\" >nul
            if !errorlevel! equ 0 (
                echo   [OK] VS Roaming (!ROAM_DIR!) cache icine kopyalandi.
                set /a INSTALLED_COUNT+=1
            )
        )
    )
)

echo.
echo [3/3] DevEnv template yenileme deneniyor...
REM ---- devenv /installvstemplates calistir (varsa) ----
set "DEVENV_FOUND=0"
for %%e in (
    "%ProgramFiles%\Microsoft Visual Studio\2022\*\Common7\IDE\devenv.exe"
    "%ProgramFiles(x86)%\Microsoft Visual Studio\2022\*\Common7\IDE\devenv.exe"
    "%ProgramFiles%\Microsoft Visual Studio\2026\*\Common7\IDE\devenv.exe"
    "%ProgramFiles(x86)%\Microsoft Visual Studio\2026\*\Common7\IDE\devenv.exe"
) do (
    for %%f in (%%e) do (
        if exist "%%~ff" (
            echo   [BULUNDU] %%~ff
            echo   [CALISTIRILIYOR] %%~ff /installvstemplates  (1-2 dk surebilir)
            start "" /wait "%%~ff" /installvstemplates /nosplash
            set "DEVENV_FOUND=1"
        )
    )
)
if "!DEVENV_FOUND!"=="0" (
    echo   [BILGI] DevEnv bulunamadi; manuel olarak Visual Studio'yu YENIDEN BASLATIN.
)

echo.
echo ============================================================
if %INSTALLED_COUNT% gtr 0 (
    echo   BASARILI! Template %INSTALLED_COUNT% konuma yuklendi.
    echo.
    echo   ONEMLI ADIMLAR (UYGULAMANIZ GEREKIYOR):
    echo     1. Tum Visual Studio pencerelerini KAPATIN
    echo     2. Visual Studio'yu YENIDEN BASLATIN  (cache temizlenir)
    echo     3. Create a new project ^-> Search: "KrmShn" yazin
    echo     4. "KrmShn Net Core Backend Project" sec, proje adi verin
    echo     5. Olusan cozum icinde:
    echo        - Presentation\*.Api\appsettings.json icindeki JWT Secret ve
    echo          ConnectionString degerlerini guncelleyin
    echo        - Developer Command Prompt: dotnet ef database update
    echo.
    echo   EGER HATA DEVAM EDERSE:
    echo   - Asagidaki KLASORLERI elle silip tekrar deneyin:
    echo     %%LOCALAPPDATA%%\Microsoft\VisualStudio\18.0_*\ProjectTemplatesCache\
    echo     %%APPDATA%%\Microsoft\VisualStudio\18.0_*\ProjectTemplatesCache\
    echo   - Sonra bu .bat dosyasini tekrar calistirin
) else (
    echo   [UYARI] Gecerli bir Visual Studio kurulum klasoru bulunamadi.
    echo.
    echo   MANUEL KURULUM (Onerilen):
    echo   1. KrmShnNetCoreBackendProject.zip dosyasini acmayin
    echo   2. Asagidaki klasorlerden birine kopyalayin:
    echo      a) %%USERPROFILE%%\Documents\Visual Studio 2022\Templates\ProjectTemplates\
    echo      b) %%USERPROFILE%%\Documents\Visual Studio\Templates\ProjectTemplates\
    echo   3. Visual Studio'yu YENIDEN BASLATIN
    echo.
    echo   2. YOL: Cache icine kopyalayin:
    echo      %%LOCALAPPDATA%%\Microsoft\VisualStudio\18.0_XXXXXXXX\ProjectTemplates\
    echo      (18.0_XXXXXXXX kismini kendi klasorunuza gore duzenleyin)
)
echo ============================================================
echo.
pause
endlocal
exit /b 0
