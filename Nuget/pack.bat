@Echo Removing old files
del /Q Package\lib\net45\*.*
del /Q Package\tools\*.*
del /Q Package\content\*.*

@Echo Copying new files
xcopy ..\InsightFormFieldMapper\bin\Release\InsightFormFieldMapper.dll Package\lib\net45
xcopy ..\InsightFormFieldMapper\web.config.transform Package\content

@Echo Packing files
"..\.nuget\nuget.exe" pack package\InsightFormFieldMapper.nuspec

@Echo Moving package
move /Y *.nupkg c:\project\nuget.local\