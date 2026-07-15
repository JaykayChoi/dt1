# FabSim → USD 익스포터 (관통선)

FabSim의 C# DES 도메인 모델(레일 그래프·비히클)을 **USD(.usda)로 내보내는** 헤드리스
.NET 콘솔이다. 커리큘럼의 관통선을 코드로 잇는다:

```
FabSim (C# DES)  →  fab.usda (OpenUSD)  →  Omniverse (Phase 12)
```

Phase 11이 "usd-core로 USD를 손으로 조립"이었다면, 여기서는 **이미 있는 도메인 모델을 USD로
직렬화**한다. 실물 파이프라인에서 트윈이 하는 일 — "내 시스템의 상태를 표준 씬 포맷으로 뽑아
남에게 넘긴다" — 그 자체다.

## 핵심 아이디어 — "USD 쓰기는 그냥 텍스트"

이 익스포터는 **USD SDK를 쓰지 않는다.** `.usda`는 사람이 읽는 ASCII 텍스트라, 도메인 모델을
순회하며 문자열로 직렬화하면 유효한 USD가 나온다(`UsdaWriter.cs`). 읽기·합성(reference·
variant·payload 해석)에는 엔진이 필요하지만, **쓰기는 텍스트면 충분**하다. 그래서 어떤 언어·
런타임에서도 USD를 내보낼 수 있다 — 이것이 USD가 산업 교환 포맷이 된 실용적 이유 하나다.

검증은 Phase 11의 `usd-core`(읽는 쪽)로 한다 — 내보낸 텍스트를 엔진이 열 수 있으면 유효한 USD다.

## 구조 (twin-bridge와 같은 패턴)

```
fabsim-usd-export/
├── FabUsdExport.csproj   DesCore 참조 + FabSim-P8-Complete/Assets/Scripts/Sim/*.cs 링크(복사 아님)
├── UsdaWriter.cs         최소 .usda 텍스트 작성기(Header/BeginPrim/Translate/Points/DisplayColor)
├── Program.cs            RailGraph·FabModel.Vehicles 순회 → fab.usda 직렬화
└── README.md
```

`Sim/*.cs`를 **링크**해, 익스포터가 읽는 레이아웃·비히클이 Unity의 FabSim이 굴리는 것과
**동일한 코드**에서 나온다(단일 진리 원천). `Sim/*.cs`는 UnityEngine 무의존이라 헤드리스로 돈다.

## 내보내는 것

- `/World/Fab/Rail` — 일방통행 루프를 **BasisCurves**로(레일 그래프의 노드를 이어 폐곡선)
- `/World/Fab/Ports/Port_00..` — 픽업/드롭 포트를 **Cube** 마커로
- `/World/Fab/OHT_00..` — 비히클을 **이름 붙은 Xform + 드로어블 Cube "Body"**로. 초기 위치는
  `FabModel` 생성자가 루프에 고르게 흩뿌린 노드 좌표. 이름이 `OHT_xx`라 **Phase 12 Kit 확장이
  그대로 스캔·채색**한다.

## 실행

```
# 빌드 후 실행 — fab.usda 생성 (비히클 수·시드 조절 가능)
dotnet run --project examples/fabsim-usd-export -c Release -- --out fab.usda --vehicles 8 --seed 7
```

## 검증 (읽는 쪽 = usd-core)

```
# Phase 11 랩의 usd-core로 열어 구조를 확인
python -c "from pxr import Usd; s=Usd.Stage.Open('fab.usda'); print([p.GetPath() for p in s.Traverse()])"
```

`Usd.Stage.Open`이 성공하면 내보낸 텍스트가 유효한 USD라는 증거다. 눈으로 보려면 Phase 11의
뷰어(NVIDIA usdview 등)나 Omniverse에서 `fab.usda`를 연다.

## Omniverse로 잇기 (Phase 12)

내보낸 `fab.usda`(와 참조 파일이 있으면 함께)를 `examples/omniverse-kit-ext/assets/`로 복사해,
Phase 12의 `dt1.fabsim.viz` 확장으로 **스캔 → 상태색**을 칠하면 관통선의 끝에 닿는다:
FabSim에서 굴린 팹이 표준 USD를 거쳐 Omniverse의 실시간 트윈 뷰로 열린다.
