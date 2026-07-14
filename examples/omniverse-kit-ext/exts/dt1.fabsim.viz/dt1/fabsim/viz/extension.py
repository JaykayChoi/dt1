"""DT1 FabSim Viz — Omniverse Kit 확장.

현재 열려 있는 USD 스테이지를 순회해 이름에 "OHT" / "AGV" 가 포함된 반송 장비
프림을 찾고, 각 프림의 상태에 따라 displayColor 를 칠해 팹 상태를 한눈에 본다.

Kit은 이 파일에서 omni.ext.IExt 를 상속한 클래스를 찾아
on_startup(ext_id) / on_shutdown() 을 호출한다.
"""

import omni.ext
import omni.ui as ui
import omni.usd

# pxr(USD 파이썬 바인딩)은 Kit 런타임이 항상 제공한다.
from pxr import Usd, UsdGeom, Gf


# 데모 상태 → displayColor(RGB, 0~1) 매핑.
# 실제 트윈이라면 이벤트 스트림/시뮬레이션에서 상태가 오지만, 여기서는
# 프림 인덱스로 상태를 순환시켜 색이 실제로 바뀌는 것을 보여준다.
_STATE_COLORS = {
    "idle": Gf.Vec3f(0.55, 0.55, 0.58),     # 유휴 — 회색
    "moving": Gf.Vec3f(0.15, 0.70, 0.95),   # 이동 — 파랑
    "carrying": Gf.Vec3f(0.20, 0.80, 0.35), # 반송 중 — 초록
    "fault": Gf.Vec3f(0.95, 0.25, 0.20),    # 고장 — 빨강
}

_STATE_ORDER = ["idle", "moving", "carrying", "fault"]

# 반송 장비로 취급할 이름 토큰.
_VEHICLE_TOKENS = ("OHT", "AGV")


def _is_vehicle_prim(prim: Usd.Prim) -> bool:
    """프림 이름(또는 경로)에 OHT/AGV 토큰이 들어 있으면 반송 장비로 본다."""
    name = prim.GetName().upper()
    return any(token in name for token in _VEHICLE_TOKENS)


def _set_display_color(prim: Usd.Prim, color: Gf.Vec3f) -> bool:
    """프림에 displayColor 를 설정한다. Gprim(메시/큐브 등 그릴 수 있는 프림)만 지원.

    반환값: 색을 실제로 설정했으면 True.
    """
    gprim = UsdGeom.Gprim(prim)
    if not gprim:
        return False
    color_attr = gprim.GetDisplayColorAttr()
    if not color_attr:
        color_attr = gprim.CreateDisplayColorAttr()
    # displayColor 는 primvar(색 배열)라 리스트로 넣는다. 단색이면 원소 1개.
    color_attr.Set([color])
    return True


class Dt1FabsimVizExtension(omni.ext.IExt):
    """확장 진입점. Kit이 이 클래스의 인스턴스를 만들어 수명주기를 호출한다."""

    def on_startup(self, ext_id: str):
        self._ext_id = ext_id
        # 마지막 스캔에서 찾은 반송 장비 프림 경로 목록.
        self._vehicle_paths = []

        self._window = ui.Window("DT1 FabSim Viz", width=380, height=260)
        with self._window.frame:
            with ui.VStack(spacing=8, height=0):
                ui.Label(
                    "USD 팹 스테이지의 OHT/AGV 프림을 스캔하고 상태색을 칠합니다.",
                    word_wrap=True,
                )
                ui.Button("팹 프림 스캔", clicked_fn=self._on_scan, height=32)
                ui.Button("상태색 적용", clicked_fn=self._on_apply_colors, height=32)
                ui.Separator(height=4)
                self._status_label = ui.Label("대기 중 — 스테이지를 연 뒤 '팹 프림 스캔'을 누르세요.")

    def on_shutdown(self):
        self._vehicle_paths = []
        self._status_label = None
        # 창을 파괴해 확장 비활성화 시 UI가 남지 않게 한다.
        if self._window is not None:
            self._window.destroy()
            self._window = None

    # ------------------------------------------------------------------
    # 버튼 콜백
    # ------------------------------------------------------------------
    def _get_stage(self):
        """현재 열린 USD 스테이지. 없으면 None."""
        return omni.usd.get_context().get_stage()

    def _on_scan(self):
        stage = self._get_stage()
        if stage is None:
            self._set_status("열린 스테이지가 없습니다. USD 씬을 먼저 여세요.")
            return

        self._vehicle_paths = []
        total = 0
        # Traverse() 는 스테이지의 모든 프림을 깊이 우선으로 순회한다.
        for prim in stage.Traverse():
            total += 1
            if _is_vehicle_prim(prim):
                self._vehicle_paths.append(prim.GetPath())

        self._set_status(
            f"스캔 완료 — 전체 프림 {total}개 중 반송 장비 {len(self._vehicle_paths)}개 발견.\n"
            "'상태색 적용'으로 displayColor를 칠하세요."
        )

    def _on_apply_colors(self):
        stage = self._get_stage()
        if stage is None:
            self._set_status("열린 스테이지가 없습니다.")
            return
        if not self._vehicle_paths:
            self._set_status("먼저 '팹 프림 스캔'을 눌러 대상 프림을 찾으세요.")
            return

        painted = 0
        skipped = 0
        for i, path in enumerate(self._vehicle_paths):
            prim = stage.GetPrimAtPath(path)
            if not prim or not prim.IsValid():
                skipped += 1
                continue
            # 데모: 프림 순서로 상태를 순환시켜 색을 다양하게 보여준다.
            state = _STATE_ORDER[i % len(_STATE_ORDER)]
            if _set_display_color(prim, _STATE_COLORS[state]):
                painted += 1
            else:
                skipped += 1

        self._set_status(
            f"상태색 적용 완료 — {painted}개 채색, {skipped}개 건너뜀"
            "(Gprim이 아닌 프림)."
        )

    # ------------------------------------------------------------------
    def _set_status(self, text: str):
        if self._status_label is not None:
            self._status_label.text = text
