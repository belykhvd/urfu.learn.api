copy appsettings\core.json build\core\netcoreapp3.1\appsettings.json
rem copy appsettings\core.json build\jsjob\netcoreapp3.1\appsettings.json
copy appsettings\mailjob.json build\mailjob\netcoreapp3.1\appsettings.json

start /d "build/core/netcoreapp3.1" Core.exe
rem start /d "build/jsjob/netcoreapp3.1" JsJob.exe
start /d "build/mailjob/netcoreapp3.1" EmailService.exe