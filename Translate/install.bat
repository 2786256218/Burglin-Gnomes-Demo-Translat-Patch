@echo off
setlocal

echo =======================================
echo   Gnomium Translation Installer
echo =======================================

:: Load configuration
if exist "config.bat" (
    call "config.bat"
) else (
    echo Error: config.bat not found. Please create one based on config.template.bat.
    pause
    exit /b 1
)

set "MANAGED_DIR=%GAME_DIR%\Gnomium_Data\Managed"
if not exist "%MANAGED_DIR%" (
    echo Error: Managed folder not found at %MANAGED_DIR%.
    echo Please configure the correct GAME_DIR in config.bat.
    pause
    exit /b 1
)

if not exist "GnomiumTranslationCore.dll" (
    echo Error: GnomiumTranslationCore.dll not found. Please run build.bat first.
    pause
    exit /b 1
)

if not exist "Patcher.exe" (
    echo Error: Patcher.exe not found. Please run build.bat first.
    pause
    exit /b 1
)

echo [1/5] Copying font to game managed folder...
copy /Y "*.ttf" "%MANAGED_DIR%\"

echo [2/5] Copying dictionary.json to game managed folder...
copy /Y "dictionary.json" "%MANAGED_DIR%\dictionary.json"

echo [3/5] Copying GnomiumTranslationCore.dll to game managed folder...
copy /Y "GnomiumTranslationCore.dll" "%MANAGED_DIR%\GnomiumTranslationCore.dll"

echo [4/5] Copying Mono.Cecil.dll to game managed folder...
copy /Y "Mono.Cecil.dll" "%MANAGED_DIR%\Mono.Cecil.dll"

echo [5/5] Running Patcher to inject translation code...
pushd "%MANAGED_DIR%"
"%~dp0Patcher.exe"
popd

if %errorlevel% neq 0 (
    echo Installation failed.
    pause
    exit /b %errorlevel%
)

echo.
echo Translation installed successfully! You can now start the game.
echo.
if "%~1" neq "nopause" pause
