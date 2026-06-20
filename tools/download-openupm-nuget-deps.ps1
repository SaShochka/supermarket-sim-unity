$ErrorActionPreference = 'Stop'

$destRoot = 'C:\Users\krammy\My project\Packages'
$base = 'https://package.openupm.com'

$deps = @(
  @{ Name = 'org.nuget.microsoft.aspnetcore.signalr.client'; Ver = '9.0.4' }
  @{ Name = 'org.nuget.microsoft.aspnetcore.http.connections.client'; Ver = '9.0.4' }
  @{ Name = 'org.nuget.microsoft.aspnetcore.http.connections.common'; Ver = '9.0.4' }
  @{ Name = 'org.nuget.microsoft.aspnetcore.connections.abstractions'; Ver = '9.0.4' }
  @{ Name = 'org.nuget.microsoft.aspnetcore.signalr.client.core'; Ver = '9.0.4' }
  @{ Name = 'org.nuget.microsoft.aspnetcore.signalr.protocols.json'; Ver = '9.0.4' }
  @{ Name = 'org.nuget.microsoft.aspnetcore.signalr.common'; Ver = '9.0.4' }
  @{ Name = 'org.nuget.microsoft.bcl.memory'; Ver = '9.0.4' }
  @{ Name = 'org.nuget.system.memory'; Ver = '4.5.5' }
  @{ Name = 'org.nuget.system.runtime.compilerservices.unsafe'; Ver = '6.0.0' }
  @{ Name = 'org.nuget.microsoft.codeanalysis.csharp'; Ver = '4.13.0' }
  @{ Name = 'org.nuget.microsoft.codeanalysis.analyzers'; Ver = '3.11.0' }
  @{ Name = 'org.nuget.microsoft.codeanalysis.common'; Ver = '4.13.0' }
  @{ Name = 'org.nuget.system.buffers'; Ver = '4.5.1' }
  @{ Name = 'org.nuget.system.collections.immutable'; Ver = '8.0.0' }
  @{ Name = 'org.nuget.system.numerics.vectors'; Ver = '4.5.0' }
  @{ Name = 'org.nuget.system.reflection.metadata'; Ver = '8.0.0' }
  @{ Name = 'org.nuget.system.text.encoding.codepages'; Ver = '7.0.0' }
  @{ Name = 'org.nuget.system.threading.tasks.extensions'; Ver = '4.5.4' }
  @{ Name = 'org.nuget.microsoft.extensions.caching.abstractions'; Ver = '9.0.4' }
  @{ Name = 'org.nuget.microsoft.extensions.primitives'; Ver = '9.0.4' }
  @{ Name = 'org.nuget.microsoft.extensions.dependencyinjection.abstractions'; Ver = '9.0.4' }
  @{ Name = 'org.nuget.microsoft.bcl.asyncinterfaces'; Ver = '9.0.4' }
  @{ Name = 'org.nuget.microsoft.extensions.hosting'; Ver = '9.0.4' }
  @{ Name = 'org.nuget.microsoft.extensions.configuration'; Ver = '9.0.4' }
  @{ Name = 'org.nuget.microsoft.extensions.configuration.abstractions'; Ver = '9.0.4' }
  @{ Name = 'org.nuget.microsoft.extensions.configuration.binder'; Ver = '9.0.4' }
  @{ Name = 'org.nuget.microsoft.extensions.configuration.commandline'; Ver = '9.0.4' }
  @{ Name = 'org.nuget.microsoft.extensions.configuration.environmentvariables'; Ver = '9.0.4' }
  @{ Name = 'org.nuget.microsoft.extensions.configuration.fileextensions'; Ver = '9.0.4' }
  @{ Name = 'org.nuget.microsoft.extensions.configuration.json'; Ver = '9.0.4' }
  @{ Name = 'org.nuget.microsoft.extensions.configuration.usersecrets'; Ver = '9.0.4' }
  @{ Name = 'org.nuget.microsoft.extensions.dependencyinjection'; Ver = '9.0.4' }
  @{ Name = 'org.nuget.microsoft.extensions.diagnostics'; Ver = '9.0.4' }
  @{ Name = 'org.nuget.microsoft.extensions.diagnostics.abstractions'; Ver = '9.0.4' }
  @{ Name = 'org.nuget.microsoft.extensions.fileproviders.abstractions'; Ver = '9.0.4' }
  @{ Name = 'org.nuget.microsoft.extensions.fileproviders.physical'; Ver = '9.0.4' }
  @{ Name = 'org.nuget.microsoft.extensions.logging'; Ver = '9.0.4' }
  @{ Name = 'org.nuget.microsoft.extensions.logging.configuration'; Ver = '9.0.4' }
  @{ Name = 'org.nuget.microsoft.extensions.logging.console'; Ver = '9.0.4' }
  @{ Name = 'org.nuget.microsoft.extensions.logging.debug'; Ver = '9.0.4' }
  @{ Name = 'org.nuget.microsoft.extensions.logging.eventlog'; Ver = '9.0.4' }
  @{ Name = 'org.nuget.microsoft.extensions.logging.eventsource'; Ver = '9.0.4' }
  @{ Name = 'org.nuget.microsoft.extensions.options'; Ver = '9.0.4' }
  @{ Name = 'org.nuget.microsoft.extensions.options.configurationextensions'; Ver = '9.0.4' }
  @{ Name = 'org.nuget.microsoft.extensions.hosting.abstractions'; Ver = '9.0.4' }
  @{ Name = 'org.nuget.microsoft.extensions.logging.abstractions'; Ver = '9.0.4' }
  @{ Name = 'org.nuget.r3'; Ver = '1.3.0' }
  @{ Name = 'org.nuget.microsoft.bcl.timeprovider'; Ver = '8.0.0' }
  @{ Name = 'org.nuget.system.componentmodel.annotations'; Ver = '5.0.0' }
  @{ Name = 'org.nuget.system.threading.channels'; Ver = '8.0.0' }
  @{ Name = 'org.nuget.system.text.json'; Ver = '9.0.4' }
  @{ Name = 'org.nuget.system.diagnostics.diagnosticsource'; Ver = '9.0.4' }
  @{ Name = 'org.nuget.system.diagnostics.eventlog'; Ver = '9.0.4' }
  @{ Name = 'org.nuget.system.security.principal.windows'; Ver = '5.0.0' }
  @{ Name = 'org.nuget.system.io.pipelines'; Ver = '9.0.4' }
  @{ Name = 'org.nuget.system.text.encodings.web'; Ver = '9.0.4' }
  @{ Name = 'org.nuget.system.net.serversentevents'; Ver = '9.0.4' }
  @{ Name = 'org.nuget.microsoft.extensions.filesystemglobbing'; Ver = '9.0.4' }
  @{ Name = 'org.nuget.microsoft.extensions.features'; Ver = '9.0.4' }
)

foreach ($d in $deps) {
  $tgz = "$($d.Name)-$($d.Ver).tgz"
  $url = "$base/$($d.Name)/-/$tgz"
  $out = Join-Path $destRoot $tgz

  Write-Host "Downloading $tgz"
  Invoke-WebRequest -Uri $url -OutFile $out -UseBasicParsing -TimeoutSec 120
}

Write-Host "Done."

