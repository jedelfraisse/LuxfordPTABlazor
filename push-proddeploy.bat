@echo off
setlocal

cd /d "%~dp0"

echo [1/6] Fetching latest refs from origin...
git fetch origin
if errorlevel 1 goto :fail

echo [2/6] Updating local main from origin/main...
git checkout main
if errorlevel 1 goto :fail
git pull origin main
if errorlevel 1 goto :fail

echo [3/6] Switching to ProdDeploy branch...
git checkout ProdDeploy 2>nul
if errorlevel 1 (
    echo ProdDeploy does not exist locally. Creating it...
    git checkout -b ProdDeploy origin/ProdDeploy 2>nul
    if errorlevel 1 git checkout -b ProdDeploy
    if errorlevel 1 goto :fail
)

echo [4/6] Fast-forwarding ProdDeploy to main...
git merge --ff-only main
if errorlevel 1 (
    echo Could not fast-forward ProdDeploy to main.
    echo If ProdDeploy has unique commits, resolve manually or reset it.
    goto :fail
)

echo [5/6] Pushing ProdDeploy to origin...
git push origin ProdDeploy
if errorlevel 1 goto :fail

echo [6/6] Done. ProdDeploy now matches main and is pushed.
goto :end

:fail
echo.
echo Script failed. Review the git output above.
exit /b 1

:end
exit /b 0
pause