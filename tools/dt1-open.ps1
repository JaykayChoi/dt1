# dt1open:// URL 프로토콜 핸들러 — 학습 사이트의 "유니티 프로젝트 열기" 버튼용.
# 보안: URI에서 받은 키를 화이트리스트에 대조만 하고, 경로는 아래 표의 고정값만 사용한다.
param([string]$Uri)

$logPath = Join-Path $env:TEMP 'dt1-open.log'
"$(Get-Date -Format s) uri=$Uri" | Add-Content $logPath

$projects = @{
    'fabsim'      = 'C:\work\dt1\FabSim'
    'p3-complete' = 'C:\work\dt1\FabSim-P3-Complete'
}
$unityExe = 'C:\Program Files\Unity\Hub\Editor\6000.3.8f1\Editor\Unity.exe'

$key = (($Uri -replace '^dt1open:/*', '') -replace '/+$', '').ToLower()

if (-not $projects.ContainsKey($key)) {
    "unknown key: $key" | Add-Content $logPath
    exit 1
}

if (-not (Test-Path $unityExe)) {
    "unity not found: $unityExe" | Add-Content $logPath
    exit 1
}

Start-Process -FilePath $unityExe -ArgumentList '-projectPath', $projects[$key]
"launched: $key -> $($projects[$key])" | Add-Content $logPath
