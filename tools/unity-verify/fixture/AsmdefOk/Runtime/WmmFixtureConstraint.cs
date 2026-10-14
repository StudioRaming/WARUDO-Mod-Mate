using UnityEngine;

namespace WmmFixture
{
    // Uses a type that only WARUDO provides, so the assembly needs its asmdef reference to compile.
    public class WmmFixtureConstraint : MonoBehaviour, Warudo.Plugins.Core.Utils.ITransformConstraint
    {
        public float gain = 1f;

        public void Initialize()
        {
        }

        public void OnUpdate()
        {
            transform.localScale = Vector3.one * gain;
        }
    }
}
