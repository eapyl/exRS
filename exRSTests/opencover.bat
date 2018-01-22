..\..\..\packages\OpenCover.4.6.519\tools\OpenCover.Console.exe -skipautoprops -target:xunitrun.bat -register:user -filter:"+[exRS]* -[exRS]exRS.Proxy.* -[exRS]exRS.SSRSService.* -[exRS]exRS.Properties.*"
..\..\..\packages\ReportGenerator.3.1.1\tools\reportgenerator.exe -reports:results.xml -targetdir:coverage
