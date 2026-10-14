using UnityEngine;

namespace WmmFixtureBad
{
    // Calls APIs that a mod's code security check is expected to refuse. Never run in WARUDO.
    public class WmmFixtureBad : MonoBehaviour
    {
        private void Awake()
        {
            System.Diagnostics.Process.Start("wmm-fixture-never-runs");
            System.IO.File.WriteAllText("wmm-fixture-never-written.txt", "x");
        }
    }
}
