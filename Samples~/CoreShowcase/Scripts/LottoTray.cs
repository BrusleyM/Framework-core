using System.Collections.Generic;
using UnityEngine;

namespace Framework.Samples
{
    public class LottoTray : MonoBehaviour
    {
        [SerializeField]
        private string _colourName = "RED";
        [SerializeField]
        private LottoMachine _machine;
        [SerializeField]
        private List<SnapPoint> _sockets = new List<SnapPoint>();

        public string ColourName => _colourName;
        public IReadOnlyList<SnapPoint> Sockets => _sockets;

        public int OccupiedCount
        {
            get
            {
                var occupied = 0;
                foreach (var socket in _sockets)
                {
                    occupied += socket.IsOccupied ? 1 : 0;
                }
                return occupied;
            }
        }

        public bool IsFull => OccupiedCount == _sockets.Count;

        public void Draw()
        {
            _machine.Draw(this);
        }

        public bool TryTake(Snappable ball)
        {
            foreach (var socket in _sockets)
            {
                if (socket.TryOffer(ball))
                {
                    return true;
                }
            }
            return false;
        }

        public void ReleaseAll()
        {
            foreach (var socket in _sockets)
            {
                socket.Release();
            }
        }
    }
}
