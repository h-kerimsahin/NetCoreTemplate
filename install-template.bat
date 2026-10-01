@echo off
REM ============================================================
REM  KrmShn Net Core Backend Project - Visual Studio Template Installer
REM  Kullanim: install-template.bat   (Administrator gerekmez)
REM ============================================================

setlocal enabledelayedexpansion

set "ZIP_FILE=%~dp0KrmShnNetCoreBackendProject.zip"
set "VS_VERSIONS=2022 2019"
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

for %%v in (%VS_VERSIONS%) do (
    set "TARGET_DIR=%USERPROFILE%\Documents\Visual Studio %%v\Templates\ProjectTemplates"
    if exist "%USERPROFILE%\Documents\Visual Studio %%v" (
        if not exist "!TARGET_DIR!" (
            mkdir "!TARGET_DIR!" >nul 2>&1
        )
        copy /Y "%ZIP_FILE%" "!TARGET_DIR!\" >nul
        if !errorlevel! equ 0 (
            echo   [OK] Visual Studio %%v  ->  !TARGET_DIR!
            set /a INSTALLED_COUNT+=1
        ) else (
            echo   [HATA] VS %%v icin kopyalama basarisiz.
        )
    ) else (
        echo   [ATLA] Visual Studio %%v kurulu degil.
    )
)

REM -- Preview / Newer VS versions fallback (2025, etc.) --
for %%d in ("%USERPROFILE%\Documents\Visual Studio *") do (
    set "D=%%~fd"
    set "TARGET_DIR=!D!\Templates\ProjectTemplates"
    if exist "!TARGET_DIR!" (
        if not exist "!TARGET_DIR!\KrmShnNetCoreBackendProject.zip" (
            copy /Y "%ZIP_FILE%" "!TARGET_DIR!\" >nul
            if !errorlevel! equ 0 (
                echo   [OK] Visual Studio (!D!) bulundu ve yuklendi.
                set /a INSTALLED_COUNT+=1
            )
        )
    )
)

echo.
echo ============================================================
if %INSTALLED_COUNT% gtr 0 (
    echo   BASARILI! Template %INSTALLED_COUNT% Visual Studio klasorune yuklendi.
    echo.
    echo   Kullanim:
    echo     1. Visual Studio'yu YENIDEN BASLAT (Onemli: Cache temizlenir)
    echo     2. Create a new project ^-> Search: "KrmShn"
    echo     3. "KrmShn Net Core Backend Project" sec, proje adi ver
    echo     4. Olusan cozum icinde:
    echo        - Presentation\*.Api\appsettings.json icindeki JWT Secret ve
    echo          ConnectionString degerlerini guncelle
    echo        - Developer Command Prompt: dotnet ef database update
) else (
    echo   [UYARI] Visual Studio kurulum klasoru bulunamadi.
    echo   Manuel kurulum: KrmShnNetCoreBackendProject.zip dosyasini
    echo   %%USERPROFILE%%\Documents\Visual Studio 2022\Templates\ProjectTemplates\
    echo   klasorune kopyalayin.
)
echo ============================================================
echo.
pause
endlocal
exit /b 0
