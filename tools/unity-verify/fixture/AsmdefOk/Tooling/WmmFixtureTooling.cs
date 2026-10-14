using UnityEditor;

namespace WmmFixture.Tooling
{
    // Editor-only assembly outside an Editor folder: Mod Mate must leave it out and say so in preflight.
    public static class WmmFixtureTooling
    {
        public static bool IsEditorOnly()
        {
            return EditorApplication.isPlaying || true;
        }
    }
}
