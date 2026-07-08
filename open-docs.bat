@echo off
rem DT1 학습 사이트를 로컬 서버로 연다 (YouTube 인라인 재생을 위해 필요).
rem 창을 닫으면 서버도 종료된다.
start "" http://localhost:8377/index.html
python -m http.server 8377 --directory "%~dp0docs"
