@echo off
setlocal

echo =======================================
echo   Packaging Release Version
echo =======================================

set RELEASE_DIR=Release
if exist "%RELEASE_DIR%" rmdir /s /q "%RELEASE_DIR%"
mkdir "%RELEASE_DIR%"

echo [1/2] Building project...
call build.bat nopause
if %errorlevel% neq 0 (
    echo Build failed. Cannot package release.
    pause
    exit /b %errorlevel%
)

echo [2/2] Copying files to %RELEASE_DIR%...
copy /Y "GnomiumTranslationCore.dll" "%RELEASE_DIR%\"
copy /Y "Patcher.exe" "%RELEASE_DIR%\"
copy /Y "Mono.Cecil.dll" "%RELEASE_DIR%\"
copy /Y "dictionary.json" "%RELEASE_DIR%\"
copy /Y "*.ttf" "%RELEASE_DIR%\"

:: Create a release config file
echo :: Configuration file for Gnomium Translation > "%RELEASE_DIR%\config.bat"
echo :: Set the path to your game installation directory here. >> "%RELEASE_DIR%\config.bat"
echo set GAME_DIR=.. >> "%RELEASE_DIR%\config.bat"

:: Create a release install script
echo @echo off > "%RELEASE_DIR%\install.bat"
echo setlocal >> "%RELEASE_DIR%\install.bat"
echo echo ======================================= >> "%RELEASE_DIR%\install.bat"
echo echo   Gnomium Translation Installer >> "%RELEASE_DIR%\install.bat"
echo echo ======================================= >> "%RELEASE_DIR%\install.bat"
echo if exist "config.bat" ( >> "%RELEASE_DIR%\install.bat"
echo     call "config.bat" >> "%RELEASE_DIR%\install.bat"
echo ) else ( >> "%RELEASE_DIR%\install.bat"
echo     echo Error: config.bat not found. >> "%RELEASE_DIR%\install.bat"
echo     pause >> "%RELEASE_DIR%\install.bat"
echo     exit /b 1 >> "%RELEASE_DIR%\install.bat"
echo ) >> "%RELEASE_DIR%\install.bat"
echo set MANAGED_DIR=%%GAME_DIR%%\Gnomium_Data\Managed >> "%RELEASE_DIR%\install.bat"
echo if not exist "%%MANAGED_DIR%%" ( >> "%RELEASE_DIR%\install.bat"
echo     echo Error: Managed folder not found at %%MANAGED_DIR%%. >> "%RELEASE_DIR%\install.bat"
echo     pause >> "%RELEASE_DIR%\install.bat"
echo     exit /b 1 >> "%RELEASE_DIR%\install.bat"
echo ) >> "%RELEASE_DIR%\install.bat"
echo echo [1/5] Copying font to game managed folder... >> "%RELEASE_DIR%\install.bat"
echo copy /Y "%%~dp0*.ttf" "%%MANAGED_DIR%%\" >> "%RELEASE_DIR%\install.bat"
echo echo [2/5] Copying dictionary.json to game managed folder... >> "%RELEASE_DIR%\install.bat"
echo copy /Y "%%~dp0dictionary.json" "%%MANAGED_DIR%%\dictionary.json" >> "%RELEASE_DIR%\install.bat"
echo echo [3/5] Copying GnomiumTranslationCore.dll to game managed folder... >> "%RELEASE_DIR%\install.bat"
echo copy /Y "%%~dp0GnomiumTranslationCore.dll" "%%MANAGED_DIR%%\GnomiumTranslationCore.dll" >> "%RELEASE_DIR%\install.bat"
echo echo [4/5] Copying Mono.Cecil.dll to game managed folder... >> "%RELEASE_DIR%\install.bat"
echo copy /Y "%%~dp0Mono.Cecil.dll" "%%MANAGED_DIR%%\Mono.Cecil.dll" >> "%RELEASE_DIR%\install.bat"
echo echo [5/5] Running Patcher to inject translation code... >> "%RELEASE_DIR%\install.bat"
echo pushd "%%MANAGED_DIR%%" >> "%RELEASE_DIR%\install.bat"
echo "%%~dp0Patcher.exe" >> "%RELEASE_DIR%\install.bat"
echo popd >> "%RELEASE_DIR%\install.bat"
echo if %%errorlevel%% neq 0 ( >> "%RELEASE_DIR%\install.bat"
echo     echo Installation failed. >> "%RELEASE_DIR%\install.bat"
echo     pause >> "%RELEASE_DIR%\install.bat"
echo     exit /b %%errorlevel%% >> "%RELEASE_DIR%\install.bat"
echo ) >> "%RELEASE_DIR%\install.bat"
echo echo. >> "%RELEASE_DIR%\install.bat"
echo echo Translation installed successfully! You can now start the game. >> "%RELEASE_DIR%\install.bat"
echo echo. >> "%RELEASE_DIR%\install.bat"
echo pause >> "%RELEASE_DIR%\install.bat"

echo.
echo Packaging complete! Release files are in the %RELEASE_DIR% folder.
pause
