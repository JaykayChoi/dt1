@echo off
rem DT1 cleanup - removes system-level changes made during the study project.
rem All logic and Korean UI live in tools\cleanup.ps1 (cmd cannot parse UTF-8 Korean reliably).
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0tools\cleanup.ps1"
pause
