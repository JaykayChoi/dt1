@echo off
rem DT1 학습 사이트를 로컬 서버로 연다.
rem - YouTube 인라인 재생과 "FabSim 유니티 프로젝트 열기" 버튼이 동작한다.
rem - 이 창을 닫으면 서버도 종료된다.
start "" http://localhost:8377/index.html
python "%~dp0serve.py"
