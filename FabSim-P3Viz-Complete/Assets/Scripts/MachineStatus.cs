using UnityEngine;

namespace FabViz
{
    /// <summary>
    /// 장비의 상태를 발광(Emission)으로 그린다. 경고는 느린 맥동, 정지는 빠른 점멸.
    /// 색은 MaterialPropertyBlock으로 렌더러별로 덮어써 GC 없이 실시간 갱신한다.
    /// 에디트 모드에서도 동작해(ExecuteAlways) 씬을 열면 바로 상태색이 보인다.
    /// </summary>
    [ExecuteAlways]
    public class MachineStatus : MonoBehaviour
    {
        [SerializeField]
        private MachineState state = MachineState.Running;

        [SerializeField]
        private Renderer indicator;

        [SerializeField]
        private float baseIntensity = 1.6f;

        private MaterialPropertyBlock block;

        // 상태등은 URP Unlit 머티리얼의 _BaseColor를 HDR로 밀어 스스로 빛나게 한다
        // (값 > 1 이면 블룸이 잡는다). Lit 이미시브보다 배치 빌드에서 안정적이다.
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        public MachineState State
        {
            get => state;
            set
            {
                state = value;
                Apply(1f);
            }
        }

        private void OnEnable()
        {
            Apply(1f);
        }

        private void Update()
        {
            if (!Application.isPlaying)
            {
                Apply(1f);
                return;
            }

            float pulse = 1f;
            if (state == MachineState.Warning)
            {
                pulse = 0.55f + 0.45f * Mathf.Abs(Mathf.Sin(Time.time * 3f));
            }
            else if (state == MachineState.Down)
            {
                pulse = Mathf.Sin(Time.time * 8f) > 0f ? 1f : 0.15f;
            }

            Apply(pulse);
        }

        private void Apply(float pulse)
        {
            if (indicator == null)
            {
                return;
            }

            if (block == null)
            {
                block = new MaterialPropertyBlock();
            }

            indicator.GetPropertyBlock(block);
            Color color = StatusPalette.Color(state) * (baseIntensity * pulse);
            color.a = 1f;
            block.SetColor(BaseColorId, color);
            indicator.SetPropertyBlock(block);
        }
    }
}
