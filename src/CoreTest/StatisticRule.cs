namespace CoreTest
{
    public class StatisticRule
    {
        public int RuleId { get; set; }
        public string ChatType { get; set; }
        public string StatisticWord { get; set; }
        public long CharId { get; set; }

        public StatisticRule(int id, string chatType, string statisticWord, long chatId)
        {
            RuleId = id;
            ChatType = chatType;
            StatisticWord = statisticWord;
            CharId = chatId;
        }
    }
}
