using System.Collections;
using UnityEngine;

namespace Framework.Samples
{
    public class GlowControl : MonoBehaviour
    {
        private static readonly int BoostId = Shader.PropertyToID("_Boost");
        private static readonly int FlashId = Shader.PropertyToID("_Flash");

        [SerializeField]
        private Renderer _renderer;
        [SerializeField]
        private string _colourProperty = "_GlowColor";
        [SerializeField, ColorUsage(true, true)]
        private Color _baseColour = Color.cyan;
        [SerializeField, ColorUsage(true, true)]
        private Color _secondaryColour = Color.green;
        [SerializeField]
        private float _idleBoost = 1f;
        [SerializeField]
        private float _litBoost = 2.2f;
        [SerializeField]
        private float _flashPeak = 3f;
        [SerializeField]
        private float _flashSeconds = 0.9f;

        private MaterialPropertyBlock _block;
        private int _colourId;
        private float _boost;
        private float _flash;
        private bool _usesSecondary;
        private Coroutine _flashRoutine;

        private void Awake()
        {
            _block = new MaterialPropertyBlock();
            _colourId = Shader.PropertyToID(_colourProperty);
            _boost = _idleBoost;
            Apply();
        }

        public void GlowOn()
        {
            _boost = _litBoost;
            Apply();
        }

        public void GlowOff()
        {
            _boost = _idleBoost;
            Apply();
        }

        public void UseSecondaryColour()
        {
            _usesSecondary = true;
            Apply();
        }

        public void UseBaseColour()
        {
            _usesSecondary = false;
            Apply();
        }

        public void Burst()
        {
            if (_flashRoutine != null)
            {
                StopCoroutine(_flashRoutine);
            }
            _flashRoutine = StartCoroutine(GuardedCoroutine.Run(FlashDown(), _ => ClearFlash(), this));
        }

        public void ResetGlow()
        {
            if (_flashRoutine != null)
            {
                StopCoroutine(_flashRoutine);
                _flashRoutine = null;
            }
            _flash = 0f;
            _boost = _idleBoost;
            _usesSecondary = false;
            Apply();
        }

        private IEnumerator FlashDown()
        {
            var elapsed = 0f;
            while (elapsed < _flashSeconds)
            {
                _flash = _flashPeak * (1f - elapsed / _flashSeconds);
                Apply();
                yield return null;
                elapsed += Time.deltaTime;
            }
            ClearFlash();
        }

        private void ClearFlash()
        {
            _flash = 0f;
            _flashRoutine = null;
            Apply();
        }

        private void Apply()
        {
            if (_renderer == null)
            {
                return;
            }

            _renderer.GetPropertyBlock(_block);
            _block.SetColor(_colourId, _usesSecondary ? _secondaryColour : _baseColour);
            _block.SetFloat(BoostId, _boost);
            _block.SetFloat(FlashId, _flash);
            _renderer.SetPropertyBlock(_block);
        }
    }
}
