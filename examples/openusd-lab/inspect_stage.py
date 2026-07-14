"""inspect_stage.py — 조립된 팹 스테이지를 열어 순회·질의한다.

`build_fab_stage.py`가 만든 fab_layout.usda를 열고:
  - 프림 트리를 타입·Kind와 함께 출력
  - 각 프림의 컴포지션 아크(reference/payload/variantSet)를 보고
  - PointInstancer 인스턴스 수를 센다
  - status 배리언트를 idle→busy→error로 바꿔 displayColor가 달라지는지 확인

USD가 "파일 포맷"이자 "컴포지션 엔진"임을 코드로 체감하는 것이 목적이다.
"""

from __future__ import annotations

import os
import sys

# 한국어 Windows 콘솔(cp949)에서도 유니코드 출력이 깨지지 않도록 UTF-8로 고정.
try:
    sys.stdout.reconfigure(encoding="utf-8")
except (AttributeError, ValueError):
    pass

from pxr import Usd, UsdGeom  # noqa: E402

HERE = os.path.dirname(os.path.abspath(__file__))
LAYOUT = os.path.join(HERE, "fab_layout.usda")


def dump_tree(stage: Usd.Stage) -> None:
    print("== 프림 트리 ==")
    for prim in stage.Traverse():
        depth = prim.GetPath().pathElementCount
        indent = "  " * depth
        kind = Usd.ModelAPI(prim).GetKind() or "-"
        arcs = []
        if prim.HasAuthoredReferences():
            arcs.append("ref")
        if prim.HasPayload():
            arcs.append("payload")
        if prim.HasVariantSets():
            arcs.append("variants=" + ",".join(prim.GetVariantSets().GetNames()))
        tag = ("  [" + " ".join(arcs) + "]") if arcs else ""
        print(f"{indent}{prim.GetName()}  <{prim.GetTypeName() or '-'}> ({kind}){tag}")


def count_instances(stage: Usd.Stage) -> None:
    print("\n== PointInstancer ==")
    for prim in stage.Traverse():
        if prim.IsA(UsdGeom.PointInstancer):
            pi = UsdGeom.PointInstancer(prim)
            ids = pi.GetProtoIndicesAttr().Get()
            protos = pi.GetPrototypesRel().GetTargets()
            n = len(ids) if ids else 0
            print(f"{prim.GetPath()} — 인스턴스 {n}개 / 프로토타입 {len(protos)}종")


def cycle_variants(stage: Usd.Stage, prim_path: str = "/World/Fab/OHT_01") -> None:
    print("\n== status 배리언트 순회 ==")
    prim = stage.GetPrimAtPath(prim_path)
    if not prim or not prim.HasVariantSets():
        print("  (배리언트 없음)")
        return
    vset = prim.GetVariantSets().GetVariantSet("status")
    for name in vset.GetVariantNames():
        vset.SetVariantSelection(name)
        body = UsdGeom.Gprim.Get(stage, prim_path + "/Body")
        color = body.GetDisplayColorAttr().Get() if body else None
        rgb = tuple(round(c, 2) for c in color[0]) if color else None
        print(f"  status={name:5s} → displayColor={rgb}")


def main() -> None:
    if not os.path.exists(LAYOUT):
        raise SystemExit("fab_layout.usda 가 없습니다. 먼저 `python build_fab_stage.py` 를 실행하세요.")
    # payload 를 포함해 완전히 로드한 상태로 연다.
    stage = Usd.Stage.Open(LAYOUT, load=Usd.Stage.LoadAll)
    dump_tree(stage)
    count_instances(stage)
    cycle_variants(stage)
    print("\nMetersPerUnit:", UsdGeom.GetStageMetersPerUnit(stage),
          "| UpAxis:", UsdGeom.GetStageUpAxis(stage))


if __name__ == "__main__":
    main()
