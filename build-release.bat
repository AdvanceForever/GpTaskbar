@echo off
echo 正在发布Release版本...
dotnet publish -c Release -r win-x64 -o ./publish /p:PublishSingleFile=true /p:SelfContained=true
echo 发布完成！输出目录：%cd%\publish
pause