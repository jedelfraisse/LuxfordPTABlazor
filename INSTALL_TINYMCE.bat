@echo off
REM TinyMCE Download Helper Script
REM This script helps you set up TinyMCE locally

echo.
echo ===============================================
echo TinyMCE Local Installation Helper
echo ===============================================
echo.
echo This script will help you set up TinyMCE locally.
echo.
echo Option 1: Download from GitHub (Recommended)
echo Option 2: Use npm (requires Node.js)
echo Option 3: Manual Download from official website
echo.
echo ===============================================
echo.
echo MANUAL INSTALLATION STEPS:
echo.
echo 1. Download TinyMCE from one of these sources:
echo    - GitHub: https://github.com/tinymce/tinymce-dist/releases
echo    - Official: https://www.tiny.cloud/tinymce/
echo.
echo 2. Extract the downloaded file
echo.
echo 3. Create folder: LuxfordPTAWeb\wwwroot\lib\tinymce\
echo.
echo 4. Copy the entire contents of the extracted TinyMCE folder
echo    into: LuxfordPTAWeb\wwwroot\lib\tinymce\
echo.
echo 5. Your structure should look like:
echo    wwwroot\
echo      lib\
echo        tinymce\
echo          tinymce.min.js
echo          js\
echo          themes\
echo          plugins\
echo          icons\
echo          skins\
echo.
echo 6. Build the project:
echo    dotnet build
echo.
echo 7. Run the application:
echo    dotnet run
echo.
echo 8. Test at: https://localhost:5001/admin/programs
echo.
echo ===============================================
echo.
echo For npm installation (if you have Node.js):
echo.
echo npm install tinymce
echo xcopy node_modules\tinymce wwwroot\lib\tinymce\ /E /I
echo.
echo ===============================================
echo.
pause
