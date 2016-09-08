using System;
using System.Linq;

namespace CoreTest
{
    public class CalcStatisticInfo
    {
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

        #region fields & properties

        private readonly object _lockerRule = new object();

        #endregion

        #region rule functions

        private int GetRuleIndex()
        {
            lock (_lockerRule)
            {
                using (var db = new SqliteContext())
                {
                    return db.Rules.Any() ? db.Rules.Max(o => o.RuleId) : 0;
                }
            }
        }

        public void AddRule(string chatType, string statisticWord, long chatId)
        {
            var rule = new StatisticRule(GetRuleIndex(), chatType, statisticWord, chatId);
            using (var db = new SqliteContext())
            {
                db.Rules.Add(rule);
                db.SaveChanges();
            }
        }

        public long? GetRuleId(long chatId, string word)
        {
            using (var db = new SqliteContext())
            {
                var rule = db.Rules.SingleOrDefault(o => o.CharId == chatId && o.StatisticWord == word);
                return rule?.RuleId;
            }
        }

        public StatisticRule IsNeedAddInfo(long chatId, string text)
        {
            using (var db = new SqliteContext())
            {
                return db.Rules.SingleOrDefault(rule => rule.CharId == chatId && text.Contains(rule.StatisticWord));
            }
        }

        public string GetStatisticRule(long chatId)
        {
            using (var db = new SqliteContext())
            {
                var fnd = db.Rules.Where(o => o.CharId == chatId);
                var ret = @"All statistic rules：" + Environment.NewLine;
                foreach (var rule in fnd)
                {
                    ret += $"Word: {rule.StatisticWord}" + Environment.NewLine;
                }
                return ret;
            }
        }

        #endregion

        #region statistic functions

        public void AddInfo(int ruleId, string user)
        {
            using (var db = new SqliteContext())
            {
                var info = db.Infos.SingleOrDefault(o => o.RuleId == ruleId && o.UserId == user);
                if (info == null)
                {
                    info = new StatisticInfo(ruleId, user);
                    db.Infos.Add(info);
                    db.SaveChanges();
                }
                info.Count++;
                db.Infos.Update(info);
                db.SaveChanges();
            }

        }

        public string GetTopString(long? ruleId, string word, int topNum = 1)
        {
            var ret = string.Empty;
            if (ruleId == null) ret = $"Can't find rule: \"{word}\".";
            else
            {
                using (var db = new SqliteContext())
                {
                    var infoList = db.Infos.Where(o => o.RuleId == ruleId);
                    var i = 1;
                    foreach (var info in infoList.OrderByDescending(o => o.Count).Take(topNum))
                    {
                        ret += $"Top{i}: {info.UserId}, count: {info.Count}" + Environment.NewLine;
                        i++;
                    }
                }
            }
            return ret;
        }

        #endregion
    }
}
