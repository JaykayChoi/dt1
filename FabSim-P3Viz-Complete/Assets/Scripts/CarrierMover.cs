using UnityEngine;

namespace FabViz
{
    /// <summary>
    /// 웨이포인트 루프를 따라 반송차를 이동시킨다. TrailRenderer가 붙어 있으면
    /// 최근 경로가 잔상으로 남는다(docs/phase3/07 의 LineRenderer/TrailRenderer).
    /// 이동은 플레이 모드에서만 일어난다.
    /// </summary>
    public class CarrierMover : MonoBehaviour
    {
        [SerializeField]
        private Vector3[] waypoints;

        [SerializeField]
        private float speed = 3f;

        private int target;

        public void SetPath(Vector3[] points)
        {
            waypoints = points;
        }

        private void Update()
        {
            if (!Application.isPlaying || waypoints == null || waypoints.Length < 2)
            {
                return;
            }

            Vector3 goal = waypoints[target];
            transform.position = Vector3.MoveTowards(
                transform.position, goal, speed * Time.deltaTime);

            if ((transform.position - goal).sqrMagnitude < 0.0001f)
            {
                target = (target + 1) % waypoints.Length;
            }
        }
    }
}
