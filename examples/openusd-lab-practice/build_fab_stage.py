"""build_fab_stage.py — 실습 스켈레톤 (본문 TODO).

Phase 11 실습용. 도우미(_make_box, _make_pbr_material)와 파일 경로는 채워져 있고,
컴포지션의 핵심 단계는 TODO로 비어 있다. 아래 TODO(M2~M4)를 채우면
정답(`../openusd-lab/build_fab_stage.py`)과 같은 fab_layout.usda가 나와야 한다.

막히면 정답 스크립트의 해당 함수를 참고한다.
"""

from __future__ import annotations

import os
import sys

try:
    sys.stdout.reconfigure(encoding="utf-8")
except (AttributeError, ValueError):
    pass

from pxr import Usd, UsdGeom, UsdShade, Sdf, Gf, Vt, Kind  # noqa: E402

HERE = os.path.dirname(os.path.abspath(__file__))


def _p(name: str) -> str:
    return os.path.join(HERE, name)


def build_oht_asset() -> str:
    """재사용 OHT 캐리어 자산. (참고용으로 완성 제공 — 여기서 references 대상이 만들어진다.)"""
    path = _p("oht_asset.usda")
    stage = Usd.Stage.CreateNew(path) if not os.path.exists(path) else Usd.Stage.Open(path)
    stage.GetRootLayer().Clear()
    UsdGeom.SetStageUpAxis(stage, UsdGeom.Tokens.z)
    oht = UsdGeom.Xform.Define(stage, "/OHT")
    stage.SetDefaultPrim(oht.GetPrim())
    Usd.ModelAPI(oht).SetKind(Kind.Tokens.component)
    body = UsdGeom.Mesh.Define(stage, "/OHT/Body")
    _make_box(body, size=(1.2, 0.8, 0.6))
    mat = _make_pbr_material(stage, "/OHT/Looks/Carrier", base=(0.55, 0.57, 0.6), rough=0.5, metal=0.2)
    UsdShade.MaterialBindingAPI(body).Bind(mat)
    stage.GetRootLayer().Save()
    return path


def build_fab_layout(oht_asset: str) -> str:
    path = _p("fab_layout.usda")
    stage = Usd.Stage.CreateNew(path) if not os.path.exists(path) else Usd.Stage.Open(path)
    stage.GetRootLayer().Clear()
    UsdGeom.SetStageUpAxis(stage, UsdGeom.Tokens.z)

    world = UsdGeom.Xform.Define(stage, "/World")
    stage.SetDefaultPrim(world.GetPrim())
    UsdGeom.Xform.Define(stage, "/World/Fab")

    # TODO(M2 · reference): /World/Fab/OHT_01 Xform을 정의하고,
    #   oht_asset(oht_asset.usda)을 'reference'로 붙여라(GetReferences().AddReference(...)).
    #   힌트: 같은 폴더의 파일은 os.path.basename(oht_asset)로 상대 참조.

    # TODO(M3 · PointInstancer): /World/Fab/Vehicles 에 PointInstancer를 만들고,
    #   프로토타입 메시 1종(_make_box)으로 24대의 위치(Positions)와 ProtoIndices를 채워라.

    # TODO(M4 · variantSet): /World/Fab/OHT_01 에 "status" variantSet을 추가하고
    #   idle/busy/error 배리언트마다 /World/Fab/OHT_01/Body 의 displayColor를 다르게 옵션하라.
    #   힌트: vset.GetVariantEditContext() 안에서 CreateDisplayColorAttr(...).

    stage.GetRootLayer().Save()
    return path


# ---- 도우미 (완성 제공) -----------------------------------------------------
def _make_box(mesh: UsdGeom.Mesh, size=(1.0, 1.0, 1.0)) -> None:
    sx, sy, sz = (s * 0.5 for s in size)
    pts = [
        Gf.Vec3f(-sx, -sy, -sz), Gf.Vec3f(sx, -sy, -sz),
        Gf.Vec3f(sx, sy, -sz), Gf.Vec3f(-sx, sy, -sz),
        Gf.Vec3f(-sx, -sy, sz), Gf.Vec3f(sx, -sy, sz),
        Gf.Vec3f(sx, sy, sz), Gf.Vec3f(-sx, sy, sz),
    ]
    counts = [4, 4, 4, 4, 4, 4]
    idx = [0, 1, 2, 3, 4, 5, 6, 7, 0, 4, 7, 3, 1, 5, 6, 2, 0, 1, 5, 4, 3, 2, 6, 7]
    mesh.CreatePointsAttr(Vt.Vec3fArray(pts))
    mesh.CreateFaceVertexCountsAttr(Vt.IntArray(counts))
    mesh.CreateFaceVertexIndicesAttr(Vt.IntArray(idx))


def _make_pbr_material(stage, path, base, rough, metal):
    mat = UsdShade.Material.Define(stage, path)
    shader = UsdShade.Shader.Define(stage, path + "/PreviewSurface")
    shader.CreateIdAttr("UsdPreviewSurface")
    shader.CreateInput("diffuseColor", Sdf.ValueTypeNames.Color3f).Set(Gf.Vec3f(*base))
    shader.CreateInput("roughness", Sdf.ValueTypeNames.Float).Set(rough)
    shader.CreateInput("metallic", Sdf.ValueTypeNames.Float).Set(metal)
    mat.CreateSurfaceOutput().ConnectToSource(shader.ConnectableAPI(), "surface")
    return mat


def main() -> None:
    oht = build_oht_asset()
    layout = build_fab_layout(oht)
    print("생성:", os.path.relpath(oht, HERE), "/", os.path.relpath(layout, HERE))
    print("TODO(M2~M4)를 채운 뒤 `python inspect_stage.py`로 확인하세요.")


if __name__ == "__main__":
    main()
