using UnityEngine;

namespace FabViz
{
    /// <summary>
    /// 화면 클릭 → 레이캐스트 → 장비 선택. 선택된 장비의 "SelectionHalo"(프레넬 림)
    /// 자식을 켜서 강조한다(docs/phase3/07 의 상호작용 훅 → 06 의 아웃라인).
    /// </summary>
    public class SelectionController : MonoBehaviour
    {
        private GameObject current;

        private void Update()
        {
            if (!Application.isPlaying || !Input.GetMouseButtonDown(0))
            {
                return;
            }

            Camera cam = Camera.main;
            if (cam == null)
            {
                return;
            }

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                MachineStatus machine = hit.collider.GetComponentInParent<MachineStatus>();
                Select(machine != null ? machine.gameObject : null);
            }
            else
            {
                Select(null);
            }
        }

        private void Select(GameObject target)
        {
            if (current == target)
            {
                return;
            }

            SetHalo(current, false);
            current = target;
            SetHalo(current, true);
        }

        private void SetHalo(GameObject machine, bool on)
        {
            if (machine == null)
            {
                return;
            }

            Transform halo = machine.transform.Find("SelectionHalo");
            if (halo != null)
            {
                halo.gameObject.SetActive(on);
            }
        }
    }
}
