using UnityEditor;

using GDS.Core.Events;

namespace GDS.Core.Editor {

    /// <summary>
    /// Listens for play mode changes in the Unity Editor and triggers a `Reset` event when exiting play mode.
    /// </summary>
    [InitializeOnLoadAttribute]
    public static class PlayModeStateChanged {

        static PlayModeStateChanged() {
            EditorApplication.playModeStateChanged += onPlayModeChange;
        }

        private static void onPlayModeChange(PlayModeStateChange state) {
            if (state == PlayModeStateChange.ExitingPlayMode) {
                EventBus.Global.Publish(new Reset());
            }

        }
    }
}