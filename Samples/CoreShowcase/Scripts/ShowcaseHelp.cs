using UnityEngine;

namespace Framework.Samples
{
    public class ShowcaseHelp : MonoBehaviour
    {
        [SerializeField]
        private ShowcaseKeys _keys;
        [SerializeField]
        private string _title = "Framework Core showcase";
        [SerializeField]
        private bool _visible = true;

        private GUIStyle _titleStyle;
        private GUIStyle _lineStyle;

        private void OnGUI()
        {
            EnsureStyles();
            if (!_visible)
            {
                GUI.Label(new Rect(14f, 10f, 260f, 24f), "H: help", _lineStyle);
            }
            else
            {
                var lines = _keys != null ? _keys.Bindings.Count : 0;
                GUI.Box(new Rect(10f, 10f, 330f, 112f + lines * 22f), GUIContent.none);
                GUI.Label(new Rect(22f, 16f, 300f, 28f), _title, _titleStyle);
                GUI.Label(new Rect(22f, 46f, 300f, 22f), "WASD walk   Shift run   Mouse look   Esc free mouse", _lineStyle);
                var y = 72f;
                if (_keys != null)
                {
                    foreach (var binding in _keys.Bindings)
                    {
                        GUI.Label(new Rect(22f, y, 310f, 22f), $"[{binding.Key}]  {binding.Label}", _lineStyle);
                        y += 22f;
                    }
                }
                GUI.Label(new Rect(22f, y, 300f, 22f), "[H]  hide this help", _lineStyle);
            }

#if ENABLE_LEGACY_INPUT_MANAGER
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.H)
            {
                _visible = !_visible;
            }
#endif
        }

        private void EnsureStyles()
        {
            if (_titleStyle != null)
            {
                return;
            }

            _titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold };
            _lineStyle = new GUIStyle(GUI.skin.label) { fontSize = 13 };
        }
    }
}
