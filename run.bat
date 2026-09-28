@echo off
title Executar Teleprompter Studio
echo ==========================================
echo    Limpando e Iniciando Teleprompter
echo ==========================================
echo.

echo [1/3] Encerrando processos travados em segundo plano...
taskkill /F /IM Teleprompter.Wpf.exe /T >nul 2>&1
timeout /t 1 /nobreak >nul

echo.
echo [2/3] Limpando o ambiente (removendo builds antigas)...
dotnet clean Teleprompter.Wpf

echo.
echo [3/3] Compilando e abrindo o aplicativo...
dotnet run --project Teleprompter.Wpf

echo.
pause
