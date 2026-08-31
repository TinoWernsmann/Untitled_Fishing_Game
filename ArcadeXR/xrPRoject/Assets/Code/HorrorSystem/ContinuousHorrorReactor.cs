using UnityEngine;
using Horror.Manager;

namespace Horror.Changer
{
    public abstract class ContinuousHorrorReactor : MonoBehaviour
    {
        [SerializeField] private AnimationCurve _remapCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        private void Update()
        {
            float t = HorrorManager.Instance.CurrentValue / 100f;
            float remapped = _remapCurve.Evaluate(t);
            Apply(remapped);
        }

        protected abstract void Apply(float t);
    }
}
