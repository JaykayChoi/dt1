"""render_scene.py — USD 팹 씬을 실제로 읽어 이미지(PNG)로 그린다.

usd-core에는 GUI 뷰어가 없지만, 스테이지를 순회해 좌표를 뽑으면 matplotlib로 씬을
그릴 수 있다. 뷰어 설치 전에도 "내가 조립한 씬이 이렇게 생겼다"를 눈으로 보는 용도.

    python render_scene.py [input.usda] [output.png]

기본: fab_layout.usda → scene.png. 레일(BasisCurves), 이름 붙은 비히클(Xform),
포트(Cube), PointInstancer 인스턴스를 위에서 본 지도 + 3D 뷰로 그린다.
"""

from __future__ import annotations

import os
import sys

import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt  # noqa: E402
from pxr import Usd, UsdGeom, Gf  # noqa: E402

HERE = os.path.dirname(os.path.abspath(__file__))


def _world_translation(prim):
    m = UsdGeom.Xformable(prim).ComputeLocalToWorldTransform(Usd.TimeCode.Default())
    t = m.ExtractTranslation()
    return (t[0], t[1], t[2])


def collect(stage):
    rail = []
    vehicles = []   # (name, x, y, z)
    ports = []      # (x, y, z)
    instances = []  # (x, y, z)
    up = UsdGeom.GetStageUpAxis(stage)
    for prim in stage.Traverse():
        name = prim.GetName()
        if prim.IsA(UsdGeom.BasisCurves):
            pts = UsdGeom.BasisCurves(prim).GetPointsAttr().Get() or []
            rail = [(p[0], p[1], p[2]) for p in pts]
        elif prim.IsA(UsdGeom.PointInstancer):
            pi = UsdGeom.PointInstancer(prim)
            wx = _world_translation(prim)
            for p in (pi.GetPositionsAttr().Get() or []):
                instances.append((wx[0] + p[0], wx[1] + p[1], wx[2] + p[2]))
        elif name.upper().startswith(("OHT", "AGV")):
            x, y, z = _world_translation(prim)
            vehicles.append((name, x, y, z))
        elif name.startswith("Port_"):
            ports.append(_world_translation(prim))
    return rail, vehicles, ports, instances, up


def _hz(coord, up):
    """위에서 본 지도를 위한 수평 두 축(x, 깊이)을 up축에 맞춰 고른다."""
    x, y, z = coord
    return (x, z) if up == "Y" else (x, y)


def render(in_path, out_path):
    stage = Usd.Stage.Open(in_path, load=Usd.Stage.LoadAll)
    rail, vehicles, ports, instances, up = collect(stage)

    fig = plt.figure(figsize=(11, 5))
    fig.suptitle(os.path.basename(in_path), fontsize=11)

    # (1) 위에서 본 지도
    ax = fig.add_subplot(1, 2, 1)
    ax.set_title("top-down", fontsize=10)
    if rail:
        xs = [_hz(p, up)[0] for p in rail]
        ys = [_hz(p, up)[1] for p in rail]
        ax.plot(xs, ys, color="#3a4048", lw=2, zorder=1)
    if instances:
        ax.scatter([_hz(p, up)[0] for p in instances], [_hz(p, up)[1] for p in instances],
                   s=14, c="#9aa4b0", marker="s", label=f"instances({len(instances)})", zorder=2)
    if ports:
        ax.scatter([_hz(p, up)[0] for p in ports], [_hz(p, up)[1] for p in ports],
                   s=70, c="#e6b422", marker="D", label=f"ports({len(ports)})", zorder=3)
    for name, x, y, z in vehicles:
        hx, hy = _hz((x, y, z), up)
        ax.scatter([hx], [hy], s=60, c="#2f9e6a", zorder=4)
        ax.annotate(name, (hx, hy), fontsize=7, xytext=(3, 3), textcoords="offset points")
    ax.set_aspect("equal")
    ax.legend(loc="upper right", fontsize=7)
    ax.grid(True, ls=":", alpha=0.4)

    # (2) 3D 뷰
    ax3 = fig.add_subplot(1, 2, 2, projection="3d")
    ax3.set_title("3D", fontsize=10)
    if rail:
        ax3.plot([p[0] for p in rail], [p[2] for p in rail], [p[1] for p in rail],
                 color="#3a4048", lw=2)
    if instances:
        ax3.scatter([p[0] for p in instances], [p[2] for p in instances], [p[1] for p in instances],
                    s=8, c="#9aa4b0", marker="s")
    if ports:
        ax3.scatter([p[0] for p in ports], [p[2] for p in ports], [p[1] for p in ports],
                    s=40, c="#e6b422", marker="D")
    for name, x, y, z in vehicles:
        ax3.scatter([x], [z], [y], s=45, c="#2f9e6a")
    ax3.set_xlabel("X"); ax3.set_ylabel("Z"); ax3.set_zlabel("Y(up)")

    fig.tight_layout()
    fig.savefig(out_path, dpi=110)
    print(f"rendered {out_path}  (rail={len(rail)} pts, vehicles={len(vehicles)}, "
          f"ports={len(ports)}, instances={len(instances)})")


def main():
    in_path = sys.argv[1] if len(sys.argv) > 1 else os.path.join(HERE, "fab_layout.usda")
    out_path = sys.argv[2] if len(sys.argv) > 2 else os.path.join(HERE, "scene.png")
    render(in_path, out_path)


if __name__ == "__main__":
    main()
