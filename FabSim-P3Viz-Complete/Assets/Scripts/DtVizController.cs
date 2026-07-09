using UnityEngine;

namespace FabViz
{
    /// <summary>
    /// 데모 진입점 — 조작법을 콘솔에 안내한다. 상태·레이아웃은 에디터 빌더가
    /// 미리 구성해 두므로 여기서는 힌트만 출력한다.
    /// </summary>
    public class DtVizController : MonoBehaviour
    {
        private void Start()
        {
            Debug.Log(
                "[DT Viz] 장비 클릭 = 선택 하이라이트 · O 키 = 직교 탑다운/원근 전환. " +
                "상태 발광(정상/경고/정지)·히트맵·레일 트레일·바닥 존·라벨이 한 씬에 모여 있다.");
        }
    }
}
