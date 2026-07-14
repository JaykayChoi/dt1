"""build_fab_stage.py — usd-core로 미니 팹 씬을 USD로 조립한다.

Phase 11 실습의 정답 스크립트. 실행하면 이 폴더에 세 개의 .usda가 생긴다:

    oht_asset.usda    재사용 가능한 OHT 캐리어 자산(레퍼런스 대상)
    stocker_asset.usda 무거운 스토커 자산(페이로드 대상 — 지연 로드 시연)
    fab_layout.usda   위 둘을 합성한 팹 레이아웃 스테이지(컴포지션·인스턴싱·배리언트)

렌더링은 하지 않는다 — USD의 데이터 모델과 컴포지션만 다룬다. 결과는
`python inspect_stage.py` 또는 `usdview fab_layout.usda`(설치돼 있으면)로 확인한다.
"""

from __future__ import annotations

import os
import sys

# 한국어 Windows 콘솔(cp949)에서도 유니코드 출력이 깨지지 않도록 UTF-8로 고정.
try:
    sys.stdout.reconfigure(encoding="utf-8")
except (AttributeError, ValueError):
    pass

from pxr import Usd, UsdGeom, UsdShade, Sdf, Gf, Vt, Kind  # noqa: E402

HERE = os.path.dirname(os.path.abspath(__file__))


def _p(name: str) -> str:
    return os.path.join(HERE, name)


# ---------------------------------------------------------------------------
# 1) 재사용 자산: OHT 캐리어 (레퍼런스로 팹에 여러 번 인스턴스화된다)
# ---------------------------------------------------------------------------
def build_oht_asset() -> str:
    path = _p("oht_asset.usda")
    stage = Usd.Stage.CreateNew(path) if not os.path.exists(path) else Usd.Stage.Open(path)
    stage.GetRootLayer().Clear()

    UsdGeom.SetStageUpAxis(stage, UsdGeom.Tokens.z)
    UsdGeom.SetStageMetersPerUnit(stage, 1.0)

    # 자산의 기본 프림 — 레퍼런스하면 이 프림이 붙는다.
    oht = UsdGeom.Xform.Define(stage, "/OHT")
    stage.SetDefaultPrim(oht.GetPrim())
    Usd.ModelAPI(oht).SetKind(Kind.Tokens.component)

    # 몸체: 단순 박스 메시(실제 반송차의 자리표시자).
    body = UsdGeom.Mesh.Define(stage, "/OHT/Body")
    _make_box(body, size=(1.2, 0.8, 0.6))

    # 자산 로컬 머티리얼 — 기본 회색.
    mat = _make_pbr_material(stage, "/OHT/Looks/Carrier", base=(0.55, 0.57, 0.6), rough=0.5, metal=0.2)
    UsdShade.MaterialBindingAPI(body).Bind(mat)

    stage.GetRootLayer().Save()
    return path


# ---------------------------------------------------------------------------
# 2) 무거운 자산: 스토커 (페이로드로 지연 로드 — 대규모 씬 성능 시연용)
# ---------------------------------------------------------------------------
def build_stocker_asset() -> str:
    path = _p("stocker_asset.usda")
    stage = Usd.Stage.CreateNew(path) if not os.path.exists(path) else Usd.Stage.Open(path)
    stage.GetRootLayer().Clear()

    UsdGeom.SetStageUpAxis(stage, UsdGeom.Tokens.z)
    stocker = UsdGeom.Xform.Define(stage, "/Stocker")
    stage.SetDefaultPrim(stocker.GetPrim())
    Usd.ModelAPI(stocker).SetKind(Kind.Tokens.component)

    # "무거운" 것을 흉내내려 선반 격자를 여러 개 만든다.
    for i in range(6):
        shelf = UsdGeom.Mesh.Define(stage, f"/Stocker/Shelf_{i:02d}")
        _make_box(shelf, size=(4.0, 0.4, 0.3))
        UsdGeom.Xformable(shelf).AddTranslateOp().Set(Gf.Vec3d(0, 0, 0.5 * i))

    stage.GetRootLayer().Save()
    return path


# ---------------------------------------------------------------------------
# 3) 팹 레이아웃: 위 자산을 합성 (references · payload · PointInstancer · variantSet)
# ---------------------------------------------------------------------------
def build_fab_layout(oht_asset: str, stocker_asset: str) -> str:
    path = _p("fab_layout.usda")
    stage = Usd.Stage.CreateNew(path) if not os.path.exists(path) else Usd.Stage.Open(path)
    stage.GetRootLayer().Clear()

    UsdGeom.SetStageUpAxis(stage, UsdGeom.Tokens.z)
    UsdGeom.SetStageMetersPerUnit(stage, 1.0)

    world = UsdGeom.Xform.Define(stage, "/World")
    stage.SetDefaultPrim(world.GetPrim())

    fab = UsdGeom.Xform.Define(stage, "/World/Fab")
    Usd.ModelAPI(fab).SetKind(Kind.Tokens.group)

    # --- 레일: BasisCurves 로 천장 레일 노선 ---
    rail = UsdGeom.BasisCurves.Define(stage, "/World/Fab/Rail")
    pts = [Gf.Vec3f(x, 0.0, 3.0) for x in range(-10, 11, 2)]
    rail.CreatePointsAttr(pts)
    rail.CreateCurveVertexCountsAttr([len(pts)])
    rail.CreateTypeAttr(UsdGeom.Tokens.linear)
    rail.CreateWidthsAttr([0.15] * len(pts))
    rail_mat = _make_pbr_material(stage, "/World/Fab/Looks/Rail", base=(0.2, 0.22, 0.25), rough=0.3, metal=0.9)
    UsdShade.MaterialBindingAPI(rail).Bind(rail_mat)

    # --- OHT_01: 자산을 '레퍼런스'로 합성 (강도 R) ---
    oht01 = UsdGeom.Xform.Define(stage, "/World/Fab/OHT_01")
    oht01.GetPrim().GetReferences().AddReference(os.path.basename(oht_asset))
    oht01.AddTranslateOp().Set(Gf.Vec3d(-6.0, 0.0, 2.4))

    # --- Stocker_A: '페이로드'로 합성 (무거운 지오는 필요할 때만 로드) ---
    stocker = stage.DefinePrim("/World/Fab/Stocker_A", "Xform")
    stocker.GetPayloads().AddPayload(os.path.basename(stocker_asset))
    UsdGeom.Xformable(stocker).AddTranslateOp().Set(Gf.Vec3d(8.0, -3.0, 0.0))

    # --- Vehicles: PointInstancer 로 동일 OHT 다수를 인스턴싱 ---
    _build_vehicle_instancer(stage, "/World/Fab/Vehicles", count=24)

    # --- 장비 상태 배리언트: variantSet "status" (idle/busy/error) ---
    _add_status_variants(stage, "/World/Fab/OHT_01")

    stage.GetRootLayer().Save()
    return path


def _build_vehicle_instancer(stage: Usd.Stage, path: str, count: int) -> None:
    """PointInstancer — 프로토타입 하나로 count대의 차량을 인스턴스로 배치."""
    pi = UsdGeom.PointInstancer.Define(stage, path)

    # 프로토타입: 인스턴서 아래에 감춰 두는 원형 메시.
    proto = UsdGeom.Mesh.Define(stage, path + "/Prototypes/Carrier")
    _make_box(proto, size=(1.0, 0.7, 0.5))
    pi.CreatePrototypesRel().SetTargets([proto.GetPath()])

    positions = []
    proto_indices = []
    for i in range(count):
        row = i % 12
        col = i // 12
        positions.append(Gf.Vec3f(-11.0 + row * 2.0, -2.0 - col * 2.0, 0.3))
        proto_indices.append(0)
    pi.CreatePositionsAttr(Vt.Vec3fArray(positions))
    pi.CreateProtoIndicesAttr(Vt.IntArray(proto_indices))


def _add_status_variants(stage: Usd.Stage, prim_path: str) -> None:
    """장비 상태를 variantSet 으로 스위칭 — 각 배리언트가 displayColor 를 다르게 옵션한다."""
    prim = stage.GetPrimAtPath(prim_path)
    vset = prim.GetVariantSets().AddVariantSet("status")
    colors = {
        "idle": (0.30, 0.65, 0.35),
        "busy": (0.90, 0.70, 0.20),
        "error": (0.85, 0.25, 0.25),
    }
    for name, rgb in colors.items():
        vset.AddVariant(name)
        vset.SetVariantSelection(name)
        with vset.GetVariantEditContext():
            body = UsdGeom.Gprim.Get(stage, prim_path + "/Body")
            if body:
                body.CreateDisplayColorAttr(Vt.Vec3fArray([Gf.Vec3f(*rgb)]))
    vset.SetVariantSelection("idle")


# ---------------------------------------------------------------------------
# 도우미
# ---------------------------------------------------------------------------
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
    mesh.CreateExtentAttr(UsdGeom.PointBased(mesh).ComputeExtent(mesh.GetPointsAttr().Get()))


def _make_pbr_material(stage, path, base, rough, metal):
    """UsdPreviewSurface 기반 PBR 머티리얼 — MaterialX 대신 이식성 높은 표준 셰이더."""
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
    stocker = build_stocker_asset()
    layout = build_fab_layout(oht, stocker)
    print("생성 완료:")
    for p in (oht, stocker, layout):
        print("  -", os.path.relpath(p, HERE))
    print("\n확인: python inspect_stage.py   (또는 usdview fab_layout.usda)")


if __name__ == "__main__":
    main()
