using System;

namespace CoreTest
{
    public class StatisticInfo : IComparable<StatisticInfo>
    {
        public int RuleId { get; set; }
        public string UserId { get; set; }
        public int Count { get; set; }

        public StatisticInfo(int ruleId, string user)
        {
            RuleId = ruleId;
            UserId = user;
            Count = 0;
        }

        public int CompareTo(StatisticInfo other)
        {
            return Count - other.Count;
        }
    }
}
