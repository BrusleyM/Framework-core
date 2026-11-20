using System.Collections.Generic;
using UnityEngine;

namespace Framework.Samples
{
    public class GazeRing : MonoBehaviour
    {
        private const float DefaultDiameter = 0.7f;
        private const float DiameterOverTargetSize = 1.5f;

        private static readonly int FillId = Shader.PropertyToID("_Fill");
        private static readonly int CompleteId = Shader.PropertyToID("_Complete");

        [SerializeField]
        private Material _ringMaterial;

        private readonly Dictionary<LookTarget, Ring> _rings = new Dictionary<LookTarget, Ring>();
        private readonly List<LookTarget> _lookedAt = new List<LookTarget>();

        private void Start()
        {
            foreach (var target in FindObjectsByType<LookTarget>(FindObjectsSortMode.None))
            {
                _rings[target] = CreateRingFor(target);
                target.LookStarted += () => Show(target);
                target.LookEnded += () => Hide(target);
            }
        }

        private void Update()
        {
            var eyes = Camera.main;
            if (eyes == null)
            {
                return;
            }

            foreach (var target in _lookedAt)
            {
                var ring = _rings[target];
                ring.Quad.rotation = eyes.transform.rotation;
                ring.Quad.position = target.AimPoint.position;
                var done = target.IsSeen;
                ring.Material.SetFloat(FillId, done ? 1f : target.LookProgress);
                ring.Material.SetFloat(CompleteId, done ? 1f : 0f);
            }
        }

        private void Show(LookTarget target)
        {
            if (!_lookedAt.Contains(target))
            {
                _lookedAt.Add(target);
            }
            _rings[target].Quad.gameObject.SetActive(true);
        }

        private void Hide(LookTarget target)
        {
            _lookedAt.Remove(target);
            _rings[target].Quad.gameObject.SetActive(false);
        }

        private Ring CreateRingFor(LookTarget target)
        {
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = target.name + " gaze ring";
            Destroy(quad.GetComponent<Collider>());
            quad.transform.SetParent(transform, false);
            quad.transform.localScale = Vector3.one * DiameterFor(target);
            var renderer = quad.GetComponent<Renderer>();
            var ringMaterial = new Material(_ringMaterial);
            renderer.sharedMaterial = ringMaterial;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            quad.SetActive(false);
            return new Ring(quad.transform, ringMaterial);
        }

        private static float DiameterFor(LookTarget target)
        {
            var renderers = target.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                return DefaultDiameter;
            }

            var bounds = renderers[0].bounds;
            foreach (var renderer in renderers)
            {
                bounds.Encapsulate(renderer.bounds);
            }
            return Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z) * DiameterOverTargetSize;
        }

        private readonly struct Ring
        {
            public readonly Transform Quad;
            public readonly Material Material;

            public Ring(Transform quad, Material material)
            {
                Quad = quad;
                Material = material;
            }
        }
    }
}
