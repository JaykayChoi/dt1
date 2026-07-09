using UnityEngine;

namespace FabViz
{
    /// <summary>
    /// 항상 카메라를 향하도록 회전하는 라벨용 빌보드. 어느 각도에서도 텍스트가
    /// 읽히게 한다(docs/phase3/07 의 월드 스페이스 라벨/빌보드).
    /// </summary>
    [ExecuteAlways]
    public class Billboard : MonoBehaviour
    {
        private void LateUpdate()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                return;
            }

            transform.rotation = Quaternion.LookRotation(
                transform.position - cam.transform.position);
        }
    }
}
