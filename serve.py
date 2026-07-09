"""DT1 학습 사이트 로컬 서버.

docs/ 정적 파일을 서빙하고, /open/<project> 요청이 오면 해당 유니티
프로젝트를 Unity 에디터로 연다 (HTML의 "유니티 프로젝트 열기" 버튼용).

실행: python serve.py  (또는 open-docs.bat 더블클릭)
"""

import json
import subprocess
from functools import partial
from http.server import HTTPServer, SimpleHTTPRequestHandler
from pathlib import Path

ROOT = Path(__file__).resolve().parent
DOCS_DIR = ROOT / "docs"
UNITY_EXE = Path(r"C:\Program Files\Unity\Hub\Editor\6000.3.8f1\Editor\Unity.exe")
PORT = 8377

PROJECTS = {
    "fabsim": ROOT / "FabSim",                    # 실습용 (Phase 3~5 공용)
    "p3-complete": ROOT / "FabSim-P3-Complete",   # Phase 3 완성본 (참고용)
    "p4-complete": ROOT / "FabSim-P4-Complete",   # Phase 4 완성본 (최적화 실험장)
    "p5-complete": ROOT / "FabSim-P5-Complete",   # Phase 5 완성본 (미니 팹 시뮬레이터)
    "p3viz-complete": ROOT / "FabSim-P3Viz-Complete",  # Phase 3 DT 시각화 완성본
}


class DocsHandler(SimpleHTTPRequestHandler):
    """docs/ 정적 서빙 + 유니티 실행 엔드포인트."""

    def do_GET(self):
        if self.path.startswith("/open/"):
            self.handle_open(self.path.removeprefix("/open/"))
            return
        super().do_GET()

    def handle_open(self, key):
        project_dir = PROJECTS.get(key)
        if project_dir is None:
            body = {"ok": False, "message": f"알 수 없는 프로젝트: {key}"}
        elif not UNITY_EXE.exists():
            body = {"ok": False, "message": f"Unity 에디터를 찾을 수 없음: {UNITY_EXE}"}
        elif not project_dir.exists():
            body = {"ok": False, "message": f"프로젝트 없음: {project_dir}"}
        else:
            try:
                subprocess.Popen([str(UNITY_EXE), "-projectPath", str(project_dir)])
                body = {"ok": True, "message": f"Unity 에디터 실행 중: {project_dir.name}"}
            except OSError as exc:
                body = {"ok": False, "message": str(exc)}

        data = json.dumps(body, ensure_ascii=False).encode("utf-8")
        self.send_response(200)
        self.send_header("Content-Type", "application/json; charset=utf-8")
        self.send_header("Content-Length", str(len(data)))
        self.end_headers()
        self.wfile.write(data)

    def log_message(self, fmt, *args):
        # 유니티 실행 요청만 콘솔에 남긴다 (정적 파일 로그는 소음).
        if "/open/" in (args[0] if args else ""):
            super().log_message(fmt, *args)


def main():
    handler = partial(DocsHandler, directory=str(DOCS_DIR))
    server = HTTPServer(("127.0.0.1", PORT), handler)
    print(f"DT1 학습 사이트: http://localhost:{PORT}/index.html")
    print("이 창을 닫으면 서버가 종료됩니다.")
    server.serve_forever()


if __name__ == "__main__":
    main()
