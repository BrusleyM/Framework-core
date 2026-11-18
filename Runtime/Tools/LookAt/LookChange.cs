using System;

namespace Framework
{
    [Flags]
    public enum LookChange
    {
        None = 0,
        Started = 1,
        Ended = 2,
        Seen = 4
    }
}
