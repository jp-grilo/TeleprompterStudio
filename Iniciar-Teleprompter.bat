@echo off
title Teleprompter Studio
setlocal enabledelayedexpansion

cd /d "%~dp0"

:: 1. Verificar se o executável portátil existe em dist-bin
if exist "teleprompter-ui\dist-bin\Teleprompt 1.0.0.exe" (
    start "" "teleprompter-ui\dist-bin\Teleprompt 1.0.0.exe"
    exit /b 0
)

:: 2. Verificar se existe versão descompactada em win-unpacked
if exist "teleprompter-ui\dist-bin\win-unpacked\Teleprompt.exe" (
    start "" "teleprompter-ui\dist-bin\win-unpacked\Teleprompt.exe"
    exit /b 0
)

:: 3. Verificar qualquer executavel na pasta dist-bin
for %%F in ("teleprompter-ui\dist-bin\*.exe") do (
    start "" "%%~fF"
    exit /b 0
)

:: 4. Se não houver executável compilado, tentar executar via Node.js em modo desenvolvimento
where node >nul 2>&1
if %ERRORLEVEL% equ 0 (
    echo Iniciando o Teleprompter Studio via ambiente de desenvolvimento...
    cd /d "%~dp0teleprompter-ui"
    if not exist "node_modules\" (
        echo Instalando dependencias necessarias...
        call npm install
    )
    call npm run dev
    exit /b 0
)

:: 5. Se não houver executável nem Node.js
echo ============================================================
echo               TELEPROMPTER STUDIO
echo ============================================================
echo.
echo Nenhum executavel local foi encontrado e o Node.js nao esta instalado.
echo.
echo Voce pode baixar a versao executavel (.exe) pronta para uso diretamente
echo na pagina de Releases do projeto no GitHub.
echo.
set /p ABRIR="Deseja abrir a pagina de download agora? (S/N): "
if /i "%ABRIR%"=="S" (
    start https://github.com/jp-grilo/TeleprompterStudio/releases/latest
)

exit /b 0
