@echo off
cd /d "C:\Users\krammy\My project"

"C:\Users\krammy\AppData\Local\Microsoft\dotnet\dotnet.exe" ^
  "C:\Users\krammy\My project\Packages\com.ivanmurzak.unity.mcp\Server\bin~\Release\net9.0\win-x64\com.IvanMurzak.Unity.MCP.Server.dll" ^
  --port=60606 ^
  --plugin-timeout=30000

