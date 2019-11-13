$path = $Args[0]
$pass = $Args[1]
certutil -f -p $pass -importPFX $path
$guid = [guid]::NewGuid().ToString("B")
$cert = (Get-ChildItem cert:\LocalMachine\My | where-object { $_.Subject -like "*jcf-ai.com" } | Select-Object -First 1).Thumbprint
netsh http delete sslcert ipport=0.0.0.0:16384
netsh http add sslcert ipport=0.0.0.0:16384 certhash=$cert appid="$guid"