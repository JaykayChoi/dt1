# DT1 학습 환경 정리 — cleanup.bat이 호출한다.
# 학습 과정에서 시스템에 등록한 것들을 제거한다. 저장소·유니티 프로젝트는 건드리지 않는다.

$ErrorActionPreference = 'SilentlyContinue'

Write-Host "============================================================"
Write-Host " DT1 학습 환경 정리 (cleanup)"
Write-Host "============================================================"
Write-Host ""
Write-Host "이 스크립트는 학습 과정에서 시스템에 등록한 것들을 제거합니다:"
Write-Host ""
Write-Host "  1. dt1open:// URL 프로토콜"
Write-Host "     (레지스트리 HKCU\Software\Classes\dt1open — 유니티 열기 버튼용)"
Write-Host "  2. 프로토콜 핸들러 로그 (`$env:TEMP\dt1-open.log)"
Write-Host "  3. (선택) Python 패키지 simpy — Phase 2 예제용"
Write-Host ""
Write-Host "저장소 폴더와 유니티 프로젝트(FabSim 등)는 삭제하지 않습니다."
Write-Host "필요하면 폴더째 직접 지우면 됩니다."
Write-Host ""

$confirm = Read-Host "계속할까요? (Y/N)"
if ($confirm -notmatch '^[Yy]$') {
    Write-Host ""
    Write-Host "취소됨 — 아무것도 변경하지 않았습니다."
    exit 0
}

Write-Host ""

if (Test-Path 'HKCU:\Software\Classes\dt1open') {
    Remove-Item 'HKCU:\Software\Classes\dt1open' -Recurse -Force
    Write-Host "[OK] dt1open:// 프로토콜 제거됨 — HTML의 유니티 열기 버튼은 더 이상 동작하지 않습니다"
}
else {
    Write-Host "[--] dt1open:// 프로토콜이 이미 없습니다"
}

if (Test-Path "$env:TEMP\dt1-open.log") {
    Remove-Item "$env:TEMP\dt1-open.log" -Force
    Write-Host "[OK] 핸들러 로그 정리됨"
}
else {
    Write-Host "[--] 핸들러 로그가 이미 없습니다"
}

Write-Host ""
$simpy = Read-Host "simpy 파이썬 패키지도 제거할까요? (Y/N)"
if ($simpy -match '^[Yy]$') {
    pip uninstall -y simpy
    Write-Host "[OK] simpy 제거 완료"
}
else {
    Write-Host "[--] simpy 유지"
}

Write-Host ""
Write-Host "정리 완료. 저장소와 유니티 프로젝트는 그대로 남아 있습니다."
