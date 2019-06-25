all:bin/Release/Devlog.exe Release/Devlog.exe Release/Devlog.app/Contents/MacOS/Devlog.exe

bin/Release/Devlog.exe:*cs* *.sln ../TrickyUnits/*.cs ../TrickyUnits/gtk/*
	@echo Compiling Devlog
	@msbuild /p:Configuration=Release Devlog.sln > CompileLog.txt

Release/Devlog.exe:bin/Release/Devlog.exe
	@mkdir -p Release
	@echo Copying release
	@cp bin/Release/Devlog.exe Release

Release/Devlog.app/Contents/MacOS/Devlog.exe:Release/Devlog.exe
	@echo Creating Mac application bundle
	@mkdir -p Release/Devlog.app
	@rm    -R Release/Devlog.app
	@mkdir Release/Devlog.app
	@mkdir Release/Devlog.app/Contents
	@mkdir Release/Devlog.app/Contents/MacOS
	@mkdir Release/Devlog.app/Contents/Resources
	@cp macneeds/Devlog.icns Release/Devlog.app/Contents/Resources
	@cp macneeds/Info.plist Release/Devlog.app/Contents/
	@cp macneeds/RunShell.sh Release/Devlog.app/Contents/MacOS/Devlog
	@cp Release/Devlog.exe Release/Devlog.app/Contents/MacOS/Devlog.exe
	@chmod +x macneeds/RunShell.sh Release/Devlog.app/Contents/MacOS/Devlog


