namespace Framework
{
    public static class LookStopRules
    {
        public static bool ShouldStop(LookStopRule rule, int seenCount, int targetCount)
        {
            switch (rule)
            {
                case LookStopRule.WhenAnyTargetIsSeen:
                    return seenCount > 0;
                case LookStopRule.WhenAllTargetsAreSeen:
                    return targetCount > 0 && seenCount >= targetCount;
                default:
                    return false;
            }
        }

        public static LookStopReason ReasonFor(LookStopRule rule)
        {
            return rule == LookStopRule.WhenAllTargetsAreSeen ? LookStopReason.AllTargetsSeen : LookStopReason.AnyTargetSeen;
        }
    }
}
