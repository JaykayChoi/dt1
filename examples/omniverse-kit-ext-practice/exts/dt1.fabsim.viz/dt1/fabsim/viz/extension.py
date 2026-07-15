"""DT1 FabSim Viz — Omniverse Kit 확장 (실습 스켈레톤).

현재 열린 USD 스테이지를 순회해 이름에 "OHT" / "AGV" 가 포함된 반송 장비 프림을
찾고, 상태에 따라 displayColor 를 칠해 팹 상태를 한눈에 본다.

창·버튼·수명주기는 채워져 있고, 두 곳이 TODO다:
  - TODO(M4): _on_scan — 스테이지를 순회해 이름에 OHT/AGV가 든 프림을 모은다.
  - TODO(M4): _set_display_color — 프림(또는 그 Gprim 자손)에 displayColor를 칠한다.

정답은 `examples/omniverse-kit-ext/`. RTX GPU가 있는 Omniverse Kit/USD Composer에서
직접 실행·검증한다(이 저장소에서 자동 실행되지 않는다).
"""

import omni.ext
import omni.ui as ui
import omni.usd

# pxr(USD 파이썬 바인딩)은 Kit 런타임이 항상 제공한다.
from pxr import Usd, UsdGeom, Gf


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
    """프림 이름에 OHT/AGV 토큰이 들어 있으면 반송 장비로 본다."""
    name = prim.GetName().upper()
    return any(token in name for token in _VEHICLE_TOKENS)


def _paint_gprim(prim: Usd.Prim, color: Gf.Vec3f) -> None:
    """한 Gprim에 displayColor 를 설정한다."""
    gprim = UsdGeom.Gprim(prim)
    color_attr = gprim.GetDisplayColorAttr()
    if not color_attr:
        color_attr = gprim.CreateDisplayColorAttr()
    color_attr.Set([color])


def _set_display_color(prim: Usd.Prim, color: Gf.Vec3f) -> bool:
    """프림에 상태색을 칠한다. 반환값: 하나라도 칠했으면 True.

    반송 장비 프림은 대개 Xform(그룹)이고 실제 메시는 그 자식이다(예: reference로
    붙인 OHT_01 의 자식 Body). 그래서 프림 자신이 Gprim이면 그것을, 아니면 Gprim
    자손 전부를 칠해야 한다.
    """
    # TODO(M4): 아래를 채운다.
    #   1) UsdGeom.Gprim(prim) 이 참이면 _paint_gprim(prim, color) 후 True.
    #   2) 아니면 Usd.PrimRange(prim) 로 자손을 돌며, prim 자신을 제외한
    #      Gprim 자손마다 _paint_gprim(child, color). 하나라도 칠했으면 True.
    #   막히면 정답 examples/omniverse-kit-ext/.../extension.py 참고.
    return False


class Dt1FabsimVizExtension(omni.ext.IExt):
    """확장 진입점. Kit이 이 클래스의 인스턴스를 만들어 수명주기를 호출한다."""

    def on_startup(self, ext_id: str):
        self._ext_id = ext_id
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
        if self._window is not None:
            self._window.destroy()
            self._window = None

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
        # TODO(M4): stage.Traverse() 로 모든 프림을 순회하며 total을 세고,
        #   _is_vehicle_prim(prim) 이 참인 프림의 GetPath()를 self._vehicle_paths에 담는다.

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
            state = _STATE_ORDER[i % len(_STATE_ORDER)]
            if _set_display_color(prim, _STATE_COLORS[state]):
                painted += 1
            else:
                skipped += 1

        self._set_status(
            f"상태색 적용 완료 — {painted}개 채색, {skipped}개 건너뜀"
            "(Gprim이 아닌 프림)."
        )

    def _set_status(self, text: str):
        if self._status_label is not None:
            self._status_label.text = text
