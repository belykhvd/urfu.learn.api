copy appsettings.json build\core\netcoreapp3.1\appsettings.json
start /d "build/core/netcoreapp3.1" Core.exe
start /d "build/jsjob/netcoreapp3.1" JsJob.exe