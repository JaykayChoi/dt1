using UnityEngine;

namespace FabViz
{
    /// <summary>
    /// 바닥 타일 한 칸의 혼잡도 값을 FabViz/Heatmap 셰이더의 _Value로 넘긴다.
    /// 값은 직렬화되어 씬을 열면 색이 복원되고(ExecuteAlways), 런타임에 바꾸면
    /// 히트맵이 실시간으로 변한다(docs/phase3/06 의 히트맵).
    /// </summary>
    [ExecuteAlways]
    public class HeatmapCell : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)]
        private float value;

        private static readonly int ValueId = Shader.PropertyToID("_Value");

        public float Value
        {
            get => value;
            set
            {
                this.value = Mathf.Clamp01(value);
                Apply();
            }
        }

        private void OnEnable()
        {
            Apply();
        }

        private void OnValidate()
        {
            Apply();
        }

        private void Apply()
        {
            var renderer = GetComponent<Renderer>();
            if (renderer == null)
            {
                return;
            }

            var block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
            block.SetFloat(ValueId, value);
            renderer.SetPropertyBlock(block);
        }
    }
}
