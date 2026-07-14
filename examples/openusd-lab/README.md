# OpenUSD 랩 — usd-core로 팹 씬 조립하기

Phase 11(OpenUSD)의 실습 정답 프로젝트. Unity가 아니라 **순수 Python(`usd-core`)**으로
USD의 데이터 모델과 컴포지션을 손으로 만져 본다. 렌더링은 하지 않는다 — USD가 "3D 파일
포맷"이자 "씬을 합성하는 컴포지션 엔진"이라는 두 얼굴을 코드로 체감하는 것이 목표다.

## 무엇을 만드나

미니 팹 레이아웃을 USD 스테이지로 조립한다:

- **재사용 자산**을 별도 레이어로 분리 — OHT 캐리어(`oht_asset.usda`), 스토커(`stocker_asset.usda`)
- **컴포지션 아크**로 합성 — OHT는 `reference`, 무거운 스토커는 `payload`(지연 로드)
- **PointInstancer**로 동일 캐리어 24대를 인스턴스로 배치(개별 프림이 아니라 한 프로토타입 참조)
- **variantSet**(`status`: idle/busy/error)으로 장비 상태를 스위칭 — 배리언트마다 `displayColor`가 다르다
- **UsdShade**(`UsdPreviewSurface`) PBR 머티리얼 바인딩

## 설치

```
pip install -r requirements.txt
```

`usd-core`는 Pixar USD의 파이썬 바인딩(`pxr`)을 담은 경량 배포판이다. C++ 빌드나 Omniverse
설치가 필요 없다.

## 실행

```
# 1) USD 파일 세 개를 생성 (oht_asset / stocker_asset / fab_layout)
python build_fab_stage.py

# 2) 조립된 스테이지를 열어 트리·컴포지션·인스턴싱·배리언트를 확인
python inspect_stage.py
```

`inspect_stage.py`는 프림 트리를 타입·Kind·컴포지션 아크(ref/payload/variants)와 함께 찍고,
PointInstancer 인스턴스 수를 세고, `status` 배리언트를 idle→busy→error로 바꿔 가며
`displayColor`가 달라지는지 보여 준다.

### usdview로 눈으로 보기 (선택)

`usd-core`는 파이썬 바인딩(`pxr`)만 담아 **GUI 뷰어 `usdview`를 포함하지 않는다**. 씬을 3D로
보려면 뷰어를 따로 마련한다 — 아래 중 하나:

1. **NVIDIA 배포 usdview** — NVIDIA "Learn OpenUSD"의 설치 안내를 따라 prebuilt usdview(PySide6·
   PyOpenGL 포함)를 받는다: <https://docs.nvidia.com/learn-openusd/latest/usdview-install-instructions.html>
2. **OpenUSD 소스 빌드** — [PixarAnimationStudios/OpenUSD](https://github.com/PixarAnimationStudios/OpenUSD)를
   `build_usd.py`로 빌드하면 `usdview`가 함께 나온다(USD 툴셋: <https://openusd.org/release/toolset.html>).
3. **Omniverse USD Composer**(Phase 12, RTX GPU) 또는 NVIDIA USD View 앱으로 `.usda`를 연다.

```
usdview fab_layout.usda        # 뷰포트에서 레일·24대 인스턴스·status 배리언트를 눈으로 확인
```

Phase 12에서는 바로 이 `fab_layout.usda`를 **Omniverse**에서 열어 실시간 RTX로 렌더한다.

## 파일

| 파일 | 역할 |
|------|------|
| `build_fab_stage.py` | 팹 씬을 USD로 조립(자산 → 레이아웃 합성) |
| `inspect_stage.py` | 조립된 스테이지 순회·질의(컴포지션·인스턴싱·배리언트 확인) |
| `requirements.txt` | `usd-core` 의존성 |
| `oht_asset.usda` 등 | 실행 시 생성되는 USD 산출물 |

실습 스켈레톤(직접 채워 보기)은 `../openusd-lab-practice/`에 있다.
