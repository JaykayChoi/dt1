# Omniverse Kit 확장 — DT1 FabSim Viz (Phase 12 실습)

NVIDIA Omniverse Kit 위에서 도는 확장(extension) 예제다. Phase 11에서 만든 USD 팹 스테이지를
Omniverse에 열고, 이 확장으로 **OHT/AGV 프림을 스캔해 상태색을 칠한다**. Unity가 아닌
Omniverse(USD 네이티브) 쪽에서 같은 팹을 시각화해 보는 것이 Phase 12의 목적이다.

> 이 확장은 **RTX GPU가 있는 학습자 본인 PC**의 Omniverse Kit / USD Composer에서 실행한다.
> 저장소 안에서 자동 실행·검증되지 않는다 — 소스와 순서는 정확히 맞춰 두었으니 실행은 사용자가 한다.

## 필요 환경

- RTX 계열 GPU (Omniverse RTX 렌더러 요구사항)
- Omniverse Kit 기반 앱 중 하나: **USD Composer**(구 Create), 또는 Kit SDK로 만든 커스텀 앱
- Phase 11 산출 USD 씬 — `examples/openusd-lab` 에서 생성한 `.usda` 파일

## 구조

```
omniverse-kit-ext/
├── README.md                       ← 이 파일
├── assets/                         Phase 11 USD를 복사해 둘 자리 (아래 "USD 씬 준비" 참고)
└── exts/
    └── dt1.fabsim.viz/             확장 하나 = 폴더 하나
        ├── config/
        │   └── extension.toml      확장 메타데이터([package]/[dependencies]/[[python.module]])
        ├── docs/
        │   └── README.md           확장 자체 설명
        └── dt1/fabsim/viz/         python.module 이름 = dt1.fabsim.viz 와 폴더 경로 일치
            ├── __init__.py         from .extension import *
            └── extension.py        omni.ext.IExt — 창 + 스캔/채색 로직
```

핵심 규칙: `extension.toml` 의 `[[python.module]] name = "dt1.fabsim.viz"` 는 그 아래
`dt1/fabsim/viz/` 폴더 경로와 **정확히 일치**해야 Kit이 모듈을 import 한다.

## USD 씬 준비

1. Phase 11 랩(`examples/openusd-lab`)에서 팹 씬 `.usda` 를 생성한다.
2. 그 `.usda`(와 참조 파일)를 이 폴더의 `assets/` 로 복사한다. 예: `assets/fab_layout.usda`.
   - 프림 이름에 `OHT` / `AGV` 가 들어 있어야 확장이 반송 장비로 인식한다
     (예: `/World/Fleet/OHT_01`, `/World/Fleet/AGV_03`).

## 설치 — 확장 검색 경로 등록

Omniverse 앱은 `exts/` 같은 폴더를 **확장 검색 경로**에 추가하면 그 안의 확장을 목록에 띄운다.

1. USD Composer(또는 Kit 앱)를 연다.
2. 상단 메뉴 **Window → Extensions** 로 Extensions 매니저를 연다.
3. 매니저 좌측 상단 **⚙(설정/기어) 아이콘**을 눌러 **Extension Search Paths** 를 연다.
4. `+` 로 새 경로를 추가한다. 이 저장소의 `exts` 폴더 절대경로를 넣는다:

   ```
   C:/work/dt1/examples/omniverse-kit-ext/exts
   ```

   (경로는 `exts` 폴더 자체를 가리킨다. 그 하위의 `dt1.fabsim.viz` 가 확장으로 잡힌다.)

## 활성화

1. Extensions 매니저 검색창에 `dt1.fabsim.viz` 또는 `FabSim` 을 입력한다.
2. **DT1 FabSim Viz** 항목의 토글을 켠다(활성화).
   - 계속 켜 두려면 **AUTOLOAD**(자동 로드)도 체크한다.
3. 활성화되면 **DT1 FabSim Viz** 창이 뜬다.

## 실행 순서

1. **File → Open** 으로 `assets/` 에 복사한 팹 `.usda` 를 연다(스테이지 로드).
2. **DT1 FabSim Viz** 창에서 **팹 프림 스캔** 클릭 → 스테이지의 OHT/AGV 프림 개수가 상태줄에 뜬다.
3. **상태색 적용** 클릭 → 반송 장비 프림들의 `displayColor` 가 상태별 색(회색/파랑/초록/빨강)으로 바뀐다.
4. 뷰포트에서 프림 색이 상태에 따라 달라지는 것을 확인한다.

## 문제 해결

- **확장이 목록에 안 보임** — 검색 경로가 `exts` 폴더(그 하위 확장 폴더가 아니라)를 가리키는지 확인.
- **활성화 시 import 에러** — `extension.toml` 의 `python.module` 이름과 `dt1/fabsim/viz` 폴더 경로가
  일치하는지 확인.
- **스캔 결과 0개** — 프림 이름에 `OHT`/`AGV` 가 실제로 들어 있는지(대소문자 무관) 확인.
- **채색은 됐는데 색 안 보임** — 대상이 `UsdGeom.Gprim`(메시/큐브 등 그릴 수 있는 프림)인지 확인.
  그룹(Xform)만 있는 프림에는 `displayColor` 가 적용되지 않는다.
