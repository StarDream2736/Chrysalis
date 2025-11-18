@echo off
chcp 65001 >nul
title Chrysalis - 开发工具

:menu
cls
echo ╔════════════════════════════════════════╗
echo ║   Chrysalis - 丝之歌Mod管理器开发工具  ║
echo ╚════════════════════════════════════════╝
echo.
echo [1] 编译项目
echo [2] 运行项目 (Debug模式)
echo [3] 清理编译文件
echo [4] 发布 Windows x64 版本
echo [5] 发布所有平台
echo [6] 运行测试
echo [7] 打开项目文件夹
echo [8] 打开Visual Studio
echo [9] 查看项目状态文档
echo [0] 退出
echo.
set /p choice="请选择操作 [0-9]: "

if "%choice%"=="1" goto build
if "%choice%"=="2" goto run
if "%choice%"=="3" goto clean
if "%choice%"=="4" goto publish_win
if "%choice%"=="5" goto publish_all
if "%choice%"=="6" goto test
if "%choice%"=="7" goto open_folder
if "%choice%"=="8" goto open_vs
if "%choice%"=="9" goto open_docs
if "%choice%"=="0" goto end
goto menu

:build
echo.
echo 正在编译项目...
dotnet restore
dotnet build -c Debug
echo.
echo 编译完成！
pause
goto menu

:run
echo.
echo 正在运行项目...
cd Chrysalis-Main
dotnet run
cd ..
pause
goto menu

:clean
echo.
echo 正在清理编译文件...
dotnet clean
rd /s /q Chrysalis-Main\bin 2>nul
rd /s /q Chrysalis-Main\obj 2>nul
rd /s /q Chrysalis.Tests\bin 2>nul
rd /s /q Chrysalis.Tests\obj 2>nul
echo 清理完成！
pause
goto menu

:publish_win
echo.
echo 正在发布 Windows x64 版本...
dotnet publish Chrysalis-Main\Chrysalis.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
echo.
echo 发布完成！文件位于: Chrysalis-Main\bin\Release\net8.0\win-x64\publish\
pause
goto menu

:publish_all
echo.
echo 正在发布所有平台版本...
echo.
echo [1/3] Windows x64...
dotnet publish Chrysalis-Main\Chrysalis.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
echo.
echo [2/3] Linux x64...
dotnet publish Chrysalis-Main\Chrysalis.csproj -c Release -r linux-x64 --self-contained -p:PublishSingleFile=true
echo.
echo [3/3] macOS ARM64...
dotnet publish Chrysalis-Main\Chrysalis.csproj -c Release -r osx-arm64 --self-contained -p:PublishSingleFile=true
echo.
echo 所有平台发布完成！
pause
goto menu

:test
echo.
echo 正在运行测试...
dotnet test
pause
goto menu

:open_folder
echo.
echo 正在打开项目文件夹...
explorer .
goto menu

:open_vs
echo.
echo 正在打开Visual Studio...
start Chrysalis.sln
goto menu

:open_docs
echo.
echo 正在打开项目状态文档...
start 项目状态和下一步.md
goto menu

:end
echo.
echo 感谢使用 Chrysalis 开发工具！
echo.
timeout /t 2 >nul
exit
