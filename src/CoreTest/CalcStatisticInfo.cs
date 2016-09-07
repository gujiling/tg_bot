using System;
using System.Collections.Generic;
using System.Linq;
using iBoxDB.LocalServer;

namespace CoreTest
{
    public class CalcStatisticInfo
    {
        #region const

        private const string RuleFile = "StatisticRules.json";
        private const string InfoFile = "StatisticInfos.json";
        private const string IboxDbFile = "StatisticInfos.json";

        #endregion

        #region Singleton

        private static readonly object _locker = new object();

        private static volatile CalcStatisticInfo _instance;
        public static CalcStatisticInfo Instance
        {
            get
            {
                if (_instance != null) return _instance;
                lock (_locker)
                {
                    if (_instance == null)
                        _instance = new CalcStatisticInfo();
                }
                return _instance;
            }
        }

        #endregion

        #region construct

        private CalcStatisticInfo()
        {
            RuleList = JsonOperator.DeserializeObjectFromFile<List<StatisticRule>>(RuleFile) ?? new List<StatisticRule>();
            InfoList = JsonOperator.DeserializeObjectFromFile<List<StatisticInfo>>(InfoFile) ?? new List<StatisticInfo>();
        }

        #endregion

        #region fields & properties

        private readonly object _lockerRule = new object();

        private List<StatisticRule> RuleList { get; set; }
        private List<StatisticInfo> InfoList { get; set; }

        #endregion

        #region rule functions

        private int GetRuleIndex()
        {
            lock (_lockerRule)
            {
                if (RuleList == null || RuleList.Count == 0) return 0;
                return RuleList.Max(o => o.RuleId) + 1;
            }
        }

        public void AddRule(string chatType, string statisticWord, long chatId)
        {
            RuleList.Add(new StatisticRule(GetRuleIndex(), chatType, statisticWord, chatId));
            JsonOperator.SerializeObjectToFile(RuleList, RuleFile);
            using (var box = new DB())
            {
            }
        }

        public long? GetRuleId(long chatId, string word)
        {
            var rule = RuleList.SingleOrDefault(o => o.CharId == chatId && o.StatisticWord == word);
            return rule?.RuleId;
        }

        public StatisticRule IsNeedAddInfo(long chatId, string text)
        {
            return RuleList.FirstOrDefault(rule => rule.CharId == chatId && text.Contains(rule.StatisticWord));
        }

        #endregion

        #region statistic functions

        public void AddInfo(int ruleId, string user)
        {
            var info = InfoList.SingleOrDefault(o => o.RuleId == ruleId && o.UserId == user);
            if (info == null)
            {
                info = new StatisticInfo(ruleId, user);
                InfoList.Add(info);
            }
            info.Count++;
            JsonOperator.SerializeObjectToFile(InfoList, InfoFile);
        }

        private List<StatisticInfo> GetStatictisByRuleId(long ruleId)
        {
            return InfoList.FindAll(o => o.RuleId == ruleId);
        }

        public string GetTopString(long? ruleId, string word, int topNum = 1)
        {
            var ret = string.Empty;
            if (ruleId == null) ret = $"Can't find rule: \"{word}\".";
            var infoList = GetStatictisByRuleId(Convert.ToInt64(ruleId));
            if (infoList == null || infoList.Count == 0) ret = $"No one has said word: \"{word}\".";
            var fnd = infoList.OrderByDescending(o => o.Count).Take(topNum);
            var i = 1;
            foreach (var info in fnd)
            {
                ret += $"Top{i}: {info.UserId}, count: {info.Count}" + Environment.NewLine;
                i++;
            }
            return ret;
        }

        #endregion
    }
}
