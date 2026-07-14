# OpenUSD 랩 — 실습 스켈레톤

Phase 11 실습용 TODO 스켈레톤. 도우미와 자산 생성은 채워져 있고, **컴포지션의 핵심 단계**
(reference · PointInstancer · variantSet)만 `build_fab_stage.py`의 `TODO(M2~M4)`로 비어 있다.

## 순서

```
pip install -r requirements.txt

# 1) TODO를 채우기 전 — reference/instancer/variant 없이 뼈대만 생성된다
python build_fab_stage.py

# 2) build_fab_stage.py 의 TODO(M2~M4)를 직접 채운다

# 3) 다시 생성하고 검증한다
python build_fab_stage.py
python inspect_stage.py
```

## 완료 기준

`inspect_stage.py` 출력에서:

- `OHT_01` 프림에 `[ref variants=status]` 태그가 붙는다
- `PointInstancer` 인스턴스가 24개로 잡힌다
- `status` 배리언트를 idle/busy/error로 바꾸면 `displayColor`가 각각 달라진다

정답은 `../openusd-lab/`에 있다. 막히면 해당 함수만 열어 본다.
