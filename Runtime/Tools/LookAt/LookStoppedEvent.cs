using System;
using UnityEngine.Events;

namespace Framework
{
    [Serializable]
    public class LookStoppedEvent : UnityEvent<LookStopReason>
    {
    }
}
