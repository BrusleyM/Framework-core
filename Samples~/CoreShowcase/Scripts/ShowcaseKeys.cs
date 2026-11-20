using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Framework.Samples
{
    [Serializable]
    public class ShowcaseKeyBinding
    {
        public KeyCode Key;
        public string Label;
        public UnityEvent OnPressed = new UnityEvent();
    }

    public class ShowcaseKeys : MonoBehaviour
    {
        [SerializeField]
        private List<ShowcaseKeyBinding> _bindings = new List<ShowcaseKeyBinding>();

        public IReadOnlyList<ShowcaseKeyBinding> Bindings => _bindings;

        public ShowcaseKeyBinding AddBinding(KeyCode key, string label)
        {
            var binding = new ShowcaseKeyBinding { Key = key, Label = label };
            _bindings.Add(binding);
            return binding;
        }

        public bool Press(KeyCode key)
        {
            foreach (var binding in _bindings)
            {
                if (binding.Key == key)
                {
                    SafeEvents.Invoke(binding.OnPressed, this);
                    return true;
                }
            }
            return false;
        }

#if ENABLE_LEGACY_INPUT_MANAGER
        private void Update()
        {
            foreach (var binding in _bindings)
            {
                if (Input.GetKeyDown(binding.Key))
                {
                    SafeEvents.Invoke(binding.OnPressed, this);
                }
            }
        }
#endif
    }
}
