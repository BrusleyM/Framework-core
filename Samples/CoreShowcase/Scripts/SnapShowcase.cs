using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Samples
{
    [Serializable]
    public class SnapShowcaseBlock
    {
        public Rigidbody Body;
        public Vector3 DropOffset;
        public float DelaySeconds;
    }

    public class SnapShowcase : MonoBehaviour
    {
        [SerializeField]
        private SnapPoint _pad;
        [SerializeField]
        private Transform _dropCentre;
        [SerializeField]
        private List<SnapShowcaseBlock> _blocks = new List<SnapShowcaseBlock>();
        [SerializeField]
        private float _launchOnStartSeconds = 1.5f;

        private Coroutine _launching;

        private void Start()
        {
            HoldBlocksAtTheirDropPoints();
            if (_launchOnStartSeconds >= 0f)
            {
                Invoke(nameof(Launch), _launchOnStartSeconds);
            }
        }

        public void Launch()
        {
            if (_launching != null)
            {
                StopCoroutine(_launching);
            }
            ReleaseEverything();
            HoldBlocksAtTheirDropPoints();
            _launching = StartCoroutine(GuardedCoroutine.Run(DropBlocksOneByOne(), _ => _launching = null, this));
        }

        private void ReleaseEverything()
        {
            if (_pad != null)
            {
                _pad.Release();
            }
        }

        private void HoldBlocksAtTheirDropPoints()
        {
            foreach (var block in _blocks)
            {
                if (block.Body == null)
                {
                    continue;
                }

                if (!block.Body.isKinematic)
                {
                    Halt(block.Body);
                }
                block.Body.isKinematic = true;
                block.Body.transform.SetPositionAndRotation(_dropCentre.position + block.DropOffset, Quaternion.Euler(0f, 25f, 0f));
            }
        }

        private IEnumerator DropBlocksOneByOne()
        {
            var elapsed = 0f;
            var next = 0;
            var ordered = new List<SnapShowcaseBlock>(_blocks);
            ordered.Sort((left, right) => left.DelaySeconds.CompareTo(right.DelaySeconds));
            while (next < ordered.Count)
            {
                if (elapsed >= ordered[next].DelaySeconds)
                {
                    if (ordered[next].Body != null)
                    {
                        ordered[next].Body.isKinematic = false;
                    }
                    next++;
                    continue;
                }
                yield return null;
                elapsed += Time.deltaTime;
            }
            _launching = null;
        }

        private static void Halt(Rigidbody body)
        {
#if UNITY_6000_0_OR_NEWER
            body.linearVelocity = Vector3.zero;
#else
            body.velocity = Vector3.zero;
#endif
            body.angularVelocity = Vector3.zero;
        }
    }
}
