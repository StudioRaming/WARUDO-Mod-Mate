using UnityEngine;

namespace WmmFixture
{
    // A second component with a serialized reference to the first, to see the field link survive the build.
    public class WmmFixtureData : MonoBehaviour
    {
        public WmmFixtureConstraint peer;
        public Transform target;
        public int count = 3;
    }
}
