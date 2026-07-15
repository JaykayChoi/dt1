"""test_fab_stage.py — 조립된 팹 스테이지의 계약을 assert로 책임진다.

Phase 6의 "숫자를 책임진다" 정신을 이 랩에도 적용한다. `inspect_stage.py`가
사람이 눈으로 읽는 print라면, 이 테스트는 CI가 회귀를 자동으로 잡는 그물이다.
usd-core는 RTX 없이 헤드리스로 돌므로 그대로 CI에 걸 수 있다.

실행:  pip install pytest  &&  pytest -q   (examples/openusd-lab 에서)
"""

from __future__ import annotations

import os

import pytest
from pxr import Usd, UsdGeom

import build_fab_stage as bfs

HERE = os.path.dirname(os.path.abspath(__file__))

# Phase 12 Kit 확장이 스캔하는 것과 동일한 규칙(이름에 OHT/AGV 포함).
VEHICLE_TOKENS = ("OHT", "AGV")
EXPECTED_FLEET = {"OHT_01", "OHT_02", "AGV_01", "AGV_02"}
EXPECTED_INSTANCES = 24


@pytest.fixture(scope="module")
def stage():
    """자산·레이아웃을 새로 조립하고 payload까지 로드한 스테이지를 연다."""
    oht = bfs.build_oht_asset()
    stocker = bfs.build_stocker_asset()
    layout = bfs.build_fab_layout(oht, stocker)
    return Usd.Stage.Open(layout, load=Usd.Stage.LoadAll)


def _is_vehicle(prim):
    name = prim.GetName().upper()
    return any(tok in name for tok in VEHICLE_TOKENS)


def _gprim_targets(prim):
    """프림 자신이 Gprim이면 [자신], 아니면 Gprim 자손 목록 — 확장의 채색 대상과 동일."""
    if UsdGeom.Gprim(prim):
        return [prim]
    return [p for p in Usd.PrimRange(prim) if p != prim and UsdGeom.Gprim(p)]


def test_default_prim_is_world(stage):
    assert stage.GetDefaultPrim().GetPath().pathString == "/World"


def test_point_instancer_has_24(stage):
    pi = UsdGeom.PointInstancer(stage.GetPrimAtPath("/World/Fab/Vehicles"))
    assert pi, "Vehicles PointInstancer가 없다"
    assert len(pi.GetProtoIndicesAttr().Get()) == EXPECTED_INSTANCES


def test_oht01_has_reference_and_variantset(stage):
    oht = stage.GetPrimAtPath("/World/Fab/OHT_01")
    assert oht and oht.HasAuthoredReferences(), "OHT_01에 reference가 없다"
    assert oht.HasVariantSets()
    vset = oht.GetVariantSets().GetVariantSet("status")
    assert set(vset.GetVariantNames()) == {"idle", "busy", "error"}


def test_variant_switch_changes_display_color(stage):
    oht = stage.GetPrimAtPath("/World/Fab/OHT_01")
    vset = oht.GetVariantSets().GetVariantSet("status")
    body = UsdGeom.Gprim.Get(stage, "/World/Fab/OHT_01/Body")
    seen = {}
    for name in ("idle", "busy", "error"):
        vset.SetVariantSelection(name)
        seen[name] = tuple(body.GetDisplayColorAttr().Get()[0])
    assert len(set(seen.values())) == 3, "배리언트마다 색이 달라야 한다"


def test_named_fleet_is_paintable(stage):
    """Phase 12 캡스톤 계약: 이름 붙은 차량이 모두 스캔·채색 가능해야 한다."""
    found = {p.GetName() for p in stage.Traverse() if _is_vehicle(p)}
    assert EXPECTED_FLEET <= found, f"함대 프림 누락: {EXPECTED_FLEET - found}"
    # 각 차량은 색칠 가능한 Gprim 대상을 하나 이상 가져야 한다(0개면 캡스톤 데모가 죽는다).
    for name in EXPECTED_FLEET:
        prim = stage.GetPrimAtPath("/World/Fab/" + name)
        assert _gprim_targets(prim), f"{name}에 채색 가능한 Gprim이 없다"
