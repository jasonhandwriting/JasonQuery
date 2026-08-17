@echo off
setlocal EnableExtensions

rem JasonQuery test x64 release package
set "REPOSITORY_ROOT=%~dp0"
if "%REPOSITORY_ROOT:~-1%"=="\" set "REPOSITORY_ROOT=%REPOSITORY_ROOT:~0,-1%"
set "SOURCE=%REPOSITORY_ROOT%\JasonQuery\bin\Release"
set "STAGING=%REPOSITORY_ROOT% x64"
set "LOCALIZATION_SOURCE=%SOURCE%\localization"
set "OUTPUT=%USERPROFILE%\Desktop\JasonQuery64Test.zip"
set "SEVENZIP=C:\Program Files\7-Zip\7z.exe"
set "REPOSITORY_VALIDATOR=%REPOSITORY_ROOT%\Build\Validate-Repository.ps1"
set "PACKAGE_VALIDATOR=%REPOSITORY_ROOT%\Build\Validate-Step3D-Release.ps1"
set "REPORT_DIRECTORY=%REPOSITORY_ROOT%\Build\ValidationReports"
set "REMOVE_INCOMPLETE_OUTPUT=0"
set "CURRENT_STEP=Initializing the test publish workflow"

for /F %%I in ('powershell.exe -NoProfile -Command "Get-Date -Format yyyyMMdd-HHmmss"') do set "RUN_ID=%%I"
if not defined RUN_ID (
    echo ERROR: Unable to create a validation report timestamp.
    pause
    exit /B 1
)

if not exist "%REPORT_DIRECTORY%" mkdir "%REPORT_DIRECTORY%"
if not exist "%REPORT_DIRECTORY%" (
    echo ERROR: Unable to create the validation report directory: %REPORT_DIRECTORY%
    pause
    exit /B 1
)

set "REPORT=%REPORT_DIRECTORY%\Publish-Test-%RUN_ID%.txt"

echo ============================================================
echo JasonQuery - Build test x64 release package
echo ============================================================

set "CURRENT_STEP=Checking prerequisites"
call :CheckPrerequisites
if errorlevel 1 goto :Failed

echo [1/7] Validating the repository...
set "CURRENT_STEP=Repository validation"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%REPOSITORY_VALIDATOR%" -RepositoryRoot "%REPOSITORY_ROOT%" -ReportPath "%REPORT%"
if errorlevel 1 (
    echo ERROR: Repository validation failed.
    goto :Failed
)

echo [2/7] Preparing a clean staging directory...
set "CURRENT_STEP=Preparing the staging directory"
call :MirrorRelease
if errorlevel 1 goto :Failed
call :CleanStaging
if errorlevel 1 goto :Failed

echo [3/7] Starting JasonQuery for the final manual smoke test...
echo Close JasonQuery after the test is complete.
set "CURRENT_STEP=Manual smoke test"
pushd "%STAGING%"
if errorlevel 1 (
    echo ERROR: Unable to use the staging directory as the working directory.
    goto :Failed
)

start "" /wait "%STAGING%\JasonQuery.exe"
set "SMOKE_TEST_EXIT_CODE=%ERRORLEVEL%"
popd

if not "%SMOKE_TEST_EXIT_CODE%"=="0" (
    echo ERROR: JasonQuery exited with code %SMOKE_TEST_EXIT_CODE%.
    goto :Failed
)

choice /C YN /N /M "Did JasonQuery start and run correctly? [Y/N]: "
if errorlevel 2 (
    echo ERROR: Publication cancelled because the smoke test was not confirmed.
    goto :Failed
)

>> "%REPORT%" echo.
>> "%REPORT%" echo Manual Validation
>> "%REPORT%" echo =================
>> "%REPORT%" echo [PASS] Manual smoke test - Maintainer confirmed that JasonQuery started and ran correctly.

echo [4/7] Restoring a clean staging directory after the smoke test...
set "CURRENT_STEP=Restoring the staging directory"
call :MirrorRelease
if errorlevel 1 goto :Failed

echo [5/7] Removing runtime and legacy files...
set "CURRENT_STEP=Cleaning the staging directory"
call :CleanStaging
if errorlevel 1 goto :Failed

echo [6/7] Creating %OUTPUT%...
set "CURRENT_STEP=Creating the test ZIP"
if exist "%OUTPUT%" del /F /Q "%OUTPUT%"
if exist "%OUTPUT%" (
    echo ERROR: The previous ZIP could not be removed: %OUTPUT%
    goto :Failed
)

set "REMOVE_INCOMPLETE_OUTPUT=1"
"%SEVENZIP%" a -tzip -mx=9 -y "%OUTPUT%" "%STAGING%\"
if errorlevel 1 (
    echo ERROR: 7-Zip failed to create the test package.
    goto :Failed
)

echo [7/7] Validating the Release directory and ZIP package...
set "CURRENT_STEP=Package validation"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%PACKAGE_VALIDATOR%" -ReleaseDirectory "%SOURCE%" -ReleaseZip "%OUTPUT%" -ReportPath "%REPORT%" -PackageKind "Test" -AppendReport
if errorlevel 1 (
    echo ERROR: Step 3D release validation failed.
    goto :Failed
)

>> "%REPORT%" echo.
>> "%REPORT%" echo Publish Workflow
>> "%REPORT%" echo ================
>> "%REPORT%" echo [PASS] Test publish workflow - Package created and validated.

echo.
echo SUCCESS: Test package created and validated.
echo Package: %OUTPUT%
echo Report:  %REPORT%
pause
exit /B 0

:CheckPrerequisites
if not exist "%SOURCE%\JasonQuery.exe" (
    echo ERROR: JasonQuery.exe was not found in: %SOURCE%
    exit /B 1
)

if not exist "%SOURCE%\JasonQuery.exe.config" (
    echo ERROR: JasonQuery.exe.config was not found in: %SOURCE%
    exit /B 1
)

if not exist "%SOURCE%\IconLibrary.dll" (
    echo ERROR: IconLibrary.dll was not found in: %SOURCE%
    exit /B 1
)

if not exist "%SOURCE%\IconLibrary.pdb" (
    echo ERROR: IconLibrary.pdb was not found in: %SOURCE%
    exit /B 1
)

if not exist "%LOCALIZATION_SOURCE%\english.xml" (
    echo ERROR: english.xml was not found in: %LOCALIZATION_SOURCE%
    exit /B 1
)

if not exist "%LOCALIZATION_SOURCE%\chinese-cht.xml" (
    echo ERROR: chinese-cht.xml was not found in: %LOCALIZATION_SOURCE%
    exit /B 1
)

if not exist "%LOCALIZATION_SOURCE%\chinese-chs.xml" (
    echo ERROR: chinese-chs.xml was not found in: %LOCALIZATION_SOURCE%
    exit /B 1
)

if not exist "%SEVENZIP%" (
    echo ERROR: 7-Zip was not found in: %SEVENZIP%
    exit /B 1
)

if not exist "%REPOSITORY_VALIDATOR%" (
    echo ERROR: Repository validator was not found in: %REPOSITORY_VALIDATOR%
    exit /B 1
)

if not exist "%PACKAGE_VALIDATOR%" (
    echo ERROR: Step 3D validator was not found in: %PACKAGE_VALIDATOR%
    exit /B 1
)

exit /B 0

:MirrorRelease
robocopy "%SOURCE%" "%STAGING%" /MIR /R:2 /W:1 /NFL /NDL /NJH /NJS /NP
if errorlevel 8 (
    echo ERROR: Robocopy failed while preparing the staging directory.
    exit /B 1
)

robocopy "%LOCALIZATION_SOURCE%" "%STAGING%\localization" /MIR /R:2 /W:1 /NFL /NDL /NJH /NJS /NP
if errorlevel 8 (
    echo ERROR: Robocopy failed while copying localization files.
    exit /B 1
)

exit /B 0

:CleanStaging
del /F /Q "%STAGING%\JasonQuery.db" >nul 2>&1
del /S /F /Q "%STAGING%\*.bak" >nul 2>&1
del /F /Q "%STAGING%\sqlite3.dll" >nul 2>&1
del /F /Q "%STAGING%\LinqBridge.dll" >nul 2>&1
del /F /Q "%STAGING%\*.xml" >nul 2>&1

for %%F in ("%STAGING%\*.pdb") do (
    if /I not "%%~nxF"=="JasonQuery.pdb" if /I not "%%~nxF"=="JasonLibrary.pdb" if /I not "%%~nxF"=="IconLibrary.pdb" if /I not "%%~nxF"=="MagicLibrary.pdb" if /I not "%%~nxF"=="ScintillaNET.pdb" if /I not "%%~nxF"=="Updater.pdb" del /F /Q "%%~fF" >nul 2>&1
)

for /D %%D in ("%STAGING%\*") do (
    if /I not "%%~nxD"=="localization" if /I not "%%~nxD"=="zh-Hans" if /I not "%%~nxD"=="zh-Hant" rd /S /Q "%%~fD"
)

if not exist "%STAGING%\localization\english.xml" (
    echo ERROR: The staging directory is missing localization\english.xml.
    exit /B 1
)

if not exist "%STAGING%\localization\chinese-cht.xml" (
    echo ERROR: The staging directory is missing localization\chinese-cht.xml.
    exit /B 1
)

if not exist "%STAGING%\localization\chinese-chs.xml" (
    echo ERROR: The staging directory is missing localization\chinese-chs.xml.
    exit /B 1
)

if not exist "%STAGING%\JasonQuery.exe.config" (
    echo ERROR: The staging directory is missing JasonQuery.exe.config.
    exit /B 1
)

where /R "%STAGING%" PoorMansTSqlFormatterLib.dll >nul 2>&1
if not errorlevel 1 (
    echo ERROR: PoorMansTSqlFormatterLib.dll still exists in the staging directory.
    exit /B 1
)

where /R "%STAGING%" LinqBridge.dll >nul 2>&1
if not errorlevel 1 (
    echo ERROR: LinqBridge.dll still exists in the staging directory.
    exit /B 1
)

exit /B 0

:Failed
if "%REMOVE_INCOMPLETE_OUTPUT%"=="1" del /F /Q "%OUTPUT%" >nul 2>&1
>> "%REPORT%" echo.
>> "%REPORT%" echo Publish Workflow
>> "%REPORT%" echo ================
>> "%REPORT%" echo [FAIL] Test publish workflow - %CURRENT_STEP%
echo.
echo FAILED: No publishable test package was produced.
echo Report: %REPORT%
pause
exit /B 1
