using UnityEngine;
using UnityEditor;
using GDS.Core;
using UnityEngine.UIElements;

namespace GDS.Demos.Basic {

    public class BasicRootWindow : EditorWindow {
        [MenuItem("Tools/GDS/Basic/Main Window")]
        public static void Open() {
            var wnd = GetWindow<BasicRootWindow>();
            wnd.titleContent = new GUIContent("Basic::Main Window");
        }

        public void CreateGUI() {
            var root = rootVisualElement;
            root.Add(new RootLayer());
            root.styleSheets.Add(Resources.Load<StyleSheet>("Basic/BasicTheme"));
        }
    }
}