using UnityEngine;

namespace FabViz
{
    /// <summary>
    /// O 키로 원근 워크스루 ↔ 직교 탑다운(물류 관제 뷰)을 전환한다
    /// (docs/phase3/07 의 카메라 구도). 플레이 모드에서 동작.
    /// </summary>
    public class CameraRig : MonoBehaviour
    {
        [SerializeField]
        private Camera cam;

        [SerializeField]
        private Vector3 perspPosition = new Vector3(0f, 3.4f, -9.8f);

        [SerializeField]
        private Vector3 perspEuler = new Vector3(14f, 0f, 0f);

        [SerializeField]
        private Vector3 topPosition = new Vector3(0f, 16f, -0.5f);

        [SerializeField]
        private Vector3 topEuler = new Vector3(90f, 0f, 0f);

        [SerializeField]
        private float topSize = 8f;

        private bool topDown;

        private void Update()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.O))
            {
                topDown = !topDown;
                ApplyView();
            }
        }

        private void ApplyView()
        {
            if (cam == null)
            {
                cam = Camera.main;
            }

            if (cam == null)
            {
                return;
            }

            cam.orthographic = topDown;
            if (topDown)
            {
                cam.orthographicSize = topSize;
                cam.transform.SetPositionAndRotation(topPosition, Quaternion.Euler(topEuler));
            }
            else
            {
                cam.transform.SetPositionAndRotation(perspPosition, Quaternion.Euler(perspEuler));
            }
        }
    }
}
