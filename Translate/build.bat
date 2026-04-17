@echo off
setlocal

echo =======================================
echo   Gnomium Translation Core Builder
echo =======================================

:: Load configuration
if exist "config.bat" (
    call "config.bat"
) else (
    echo Error: config.bat not found. Please create one based on config.template.bat.
    pause
    exit /b 1
)

:: Locate compiler
set CSC="%MSBUILD_DIR%\csc.exe"
if not exist %CSC% (
    set CSC="C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
)
if not exist %CSC% (
    set CSC="C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe"
)

if not exist %CSC% (
    echo Error: csc.exe not found. Please configure MSBUILD_DIR in config.bat or install .NET Framework 4.0+.
    pause
    exit /b 1
)

:: Define paths
set "MANAGED_DIR=%GAME_DIR%\Gnomium_Data\Managed"
if not exist "%MANAGED_DIR%" (
    echo Error: Managed folder not found at %MANAGED_DIR%.
    echo Please configure the correct GAME_DIR in config.bat.
    pause
    exit /b 1
)

echo [1/2] Compiling GnomiumTranslationCore.dll...
%CSC% /nologo /nowarn:1701 /nostdlib+ /target:library /out:"GnomiumTranslationCore.dll" ^
    /reference:"%MANAGED_DIR%\netstandard.dll" ^
    /reference:"%MANAGED_DIR%\mscorlib.dll" ^
    /reference:"%MANAGED_DIR%\UnityEngine.dll" ^
    /reference:"%MANAGED_DIR%\UnityEngine.CoreModule.dll" ^
    /reference:"%MANAGED_DIR%\UnityEngine.UI.dll" ^
    /reference:"%MANAGED_DIR%\Unity.TextMeshPro.dll" ^
    /reference:"%MANAGED_DIR%\UnityEngine.TextRenderingModule.dll" ^
    /reference:"%MANAGED_DIR%\UnityEngine.TextCoreFontEngineModule.dll" ^
    /reference:"%MANAGED_DIR%\Newtonsoft.Json.dll" ^
    "GnomiumTranslationCore.cs"

if %errorlevel% neq 0 (
    echo Build failed for GnomiumTranslationCore.dll.
    pause
    exit /b %errorlevel%
)

echo [2/2] Compiling Patcher.exe...
%CSC% /nologo /out:"Patcher.exe" ^
    /reference:"Mono.Cecil.dll" ^
    "Patcher.cs"

if %errorlevel% neq 0 (
    echo Build failed for Patcher.exe.
    pause
    exit /b %errorlevel%
)

echo.
echo Build successful!
echo.
echo To install the translation, run install.bat or package_release.bat
echo.
if "%~1" neq "nopause" pause
