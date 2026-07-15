# Omniverse Kit 확장 — 실습 스켈레톤 (Phase 12)

정답 `examples/omniverse-kit-ext/` 의 TODO 스켈레톤이다. 창·버튼·수명주기·상태색 표는 채워져
있고, **핵심 두 곳**만 `TODO(M4)`로 비어 있다:

1. `_on_scan` — 스테이지를 순회(`stage.Traverse()`)해 이름에 `OHT`/`AGV`가 든 프림을 모은다.
2. `_set_display_color` — 프림(또는 그 **Gprim 자손**)에 `displayColor`를 칠한다.

파일은 그대로 두 개만 채우면 정답과 같아진다: `exts/dt1.fabsim.viz/dt1/fabsim/viz/extension.py`.

## 실행 환경 — 학습자 본인 RTX PC

이 확장은 **RTX GPU가 있는 Omniverse Kit / USD Composer**에서 실행한다. 이 저장소에서 자동
실행·검증되지 않는다(RTX 게이트). 그래서 흐름은:

1. `extension.py`의 `TODO(M4)` 두 곳을 채운다(정답을 보기 전에 먼저).
2. Phase 11 랩 또는 FabSim 익스포터가 만든 USD를 `assets/`에 복사한다
   (`examples/openusd-lab/fab_layout.usda` 또는 `examples/fabsim-usd-export/fab.usda` — 둘 다
   이름이 `OHT_xx`인 프림을 갖는다).
3. Omniverse 앱에서 **Window ▸ Extensions ▸ ⚙ ▸ Extension Search Paths**에 이 폴더의 `exts`를 더하고
   `dt1.fabsim.viz`를 켠다.
4. USD를 열고 **팹 프림 스캔 → 상태색 적용**을 눌러, 내가 채운 로직이 프림을 실제로 칠하는지 본다.

## 완료 기준

- **스캔** 후 상태줄에 반송 장비 개수가 0이 아니게 뜬다(예: `fab.usda`면 OHT 8개).
- **상태색 적용** 후 "N개 채색, 0개 건너뜀"이 뜨고, 뷰포트에서 차량 색이 상태별로 바뀐다.
  - Xform형 차량(자식 Body가 실제 메시)까지 칠하려면 `_set_display_color`가 **Gprim 자손**을
    내려가 칠해야 한다 — 이 한 가지를 빠뜨리면 "스캔은 되는데 채색 0"이 된다.

막히면 정답 `examples/omniverse-kit-ext/`의 같은 파일을 열어 대조한다.
