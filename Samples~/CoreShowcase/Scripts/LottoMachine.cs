using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Samples
{
    public class LottoMachine : MonoBehaviour
    {
        private const float MaxDepenetrationSpeed = 1.5f;
        private const float MinimumBallSpacing = 0.2f;
        private const int SpotAttempts = 60;

        [SerializeField]
        private List<Snappable> _balls = new List<Snappable>();
        [SerializeField]
        private List<LottoTray> _trays = new List<LottoTray>();
        [SerializeField]
        private Transform _drumFloor;
        [SerializeField]
        private float _drumRadius = 0.4f;
        [SerializeField]
        private float _drumHeight = 0.9f;
        [SerializeField]
        private float _stirIntervalSeconds = 0.12f;
        [SerializeField]
        private float _stirImpulse = 0.07f;
        [SerializeField]
        private float _drawStaggerSeconds = 0.18f;
        [SerializeField]
        private float _firstDrawAfterSeconds = 5f;
        [SerializeField]
        private int _firstDrawTrayIndex;
        [SerializeField]
        private TextMesh _status;

        private Coroutine _drawing;
        private bool _isDrawing;

        public IReadOnlyList<Snappable> Balls => _balls;
        public IReadOnlyList<LottoTray> Trays => _trays;
        public bool IsDrawing => _isDrawing;

        private void Start()
        {
            ScatterBalls();
            StartCoroutine(GuardedCoroutine.Run(StirForever(), _ => { }, this));
            if (_firstDrawAfterSeconds >= 0f && _firstDrawTrayIndex < _trays.Count)
            {
                Invoke(nameof(DrawFirstTray), _firstDrawAfterSeconds);
            }
        }

        public void Draw(LottoTray tray)
        {
            if (_isDrawing)
            {
                return;
            }

            _isDrawing = true;
            var routine = StartCoroutine(GuardedCoroutine.Run(DrawRoutine(tray), _ => FinishDrawing(), this));
            _drawing = _isDrawing ? routine : null;
        }

        public void Shuffle()
        {
            if (_drawing != null)
            {
                StopCoroutine(_drawing);
            }
            FinishDrawing();

            foreach (var tray in _trays)
            {
                tray.ReleaseAll();
            }
            ScatterBalls();
            Say("Shuffled. Press 1 2 3 4 to draw a colour.");
        }

        private void DrawFirstTray()
        {
            Draw(_trays[_firstDrawTrayIndex]);
        }

        private IEnumerator DrawRoutine(LottoTray tray)
        {
            var snapped = 0;
            var refused = 0;
            foreach (var ball in _balls.ToArray())
            {
                if (ball.IsSnapped)
                {
                    continue;
                }

                if (tray.TryTake(ball))
                {
                    snapped++;
                    Say($"{tray.ColourName}: {snapped} snapped, {refused} refused");
                    yield return new WaitForSeconds(_drawStaggerSeconds);
                }
                else
                {
                    refused++;
                }
            }

            Say(snapped == 0
                ? $"No {tray.ColourName} left to snap. Press Space to shuffle."
                : $"{tray.ColourName}: {snapped} snapped, {refused} refused");
            FinishDrawing();
        }

        private void FinishDrawing()
        {
            _isDrawing = false;
            _drawing = null;
        }

        private IEnumerator StirForever()
        {
            var wait = new WaitForSeconds(_stirIntervalSeconds);
            while (true)
            {
                yield return wait;
                foreach (var ball in _balls)
                {
                    if (!ball.IsSnapped && Random.value < 0.35f)
                    {
                        Stir(ball.GetComponent<Rigidbody>());
                    }
                }
            }
        }

        private void Stir(Rigidbody body)
        {
            var sideways = Random.insideUnitCircle * 0.5f;
            var push = new Vector3(sideways.x, 1f, sideways.y) * _stirImpulse;
            body.AddForce(push, ForceMode.Impulse);
        }

        private void ScatterBalls()
        {
            var placed = new List<Vector3>();
            foreach (var ball in _balls)
            {
                var body = ball.GetComponent<Rigidbody>();
                var spot = FindFreeSpot(placed);
                placed.Add(spot);
                body.isKinematic = false;
                body.maxDepenetrationVelocity = MaxDepenetrationSpeed;
                Halt(body);
                body.position = spot;
                body.transform.position = spot;
            }
        }

        private Vector3 FindFreeSpot(List<Vector3> placed)
        {
            var spot = RandomSpot();
            for (var attempt = 0; attempt < SpotAttempts && !IsFree(spot, placed); attempt++)
            {
                spot = RandomSpot();
            }
            return spot;
        }

        private Vector3 RandomSpot()
        {
            var inDisc = Random.insideUnitCircle * _drumRadius;
            return _drumFloor.position + new Vector3(inDisc.x, Random.Range(0.12f, _drumHeight), inDisc.y);
        }

        private static bool IsFree(Vector3 spot, List<Vector3> placed)
        {
            foreach (var other in placed)
            {
                if (Vector3.Distance(spot, other) < MinimumBallSpacing)
                {
                    return false;
                }
            }
            return true;
        }

        private void Say(string message)
        {
            if (_status != null)
            {
                _status.text = message;
            }
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
