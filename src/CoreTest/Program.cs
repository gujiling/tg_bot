using System;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace CoreTest
{
    public class Program
    {
        private static string _tokenkey = @"token";

        private static readonly TelegramBotClient Bot = new TelegramBotClient(_tokenkey);

        public static void Main(string[] args)
        {
            Bot.OnCallbackQuery += BotOnCallbackQueryReceived;
            Bot.OnMessage += BotOnMessageReceived;
            Bot.OnMessageEdited += BotOnMessageReceived;
            Bot.OnReceiveError += BotOnReceiveError;
            Bot.OnUpdate += BotOnOnUpdate;

            var me = Bot.GetMeAsync().Result;

            Console.Title = me.Username;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            Bot.StartReceiving();
            Console.ReadLine();
            Bot.StopReceiving();
        }

        #region event

        private static void BotOnReceiveError(object sender, ReceiveErrorEventArgs e)
        {
            Console.WriteLine($"Bot rec error: {e.ApiRequestException.Message}");
        }

        private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;

            if (message == null || message.Type != MessageType.TextMessage) return;

#if Debug
            Console.WriteLine($"Bot on msg rec text: {message.Text}");
            Console.WriteLine($"Bot on msg rec from user id: {messageEventArgs.Message.From.Id}");
#endif

            if (message.Text.StartsWith("/request")) // request location or contact
            {
                var keyboard = new ReplyKeyboardMarkup(new[]
                {
                    new KeyboardButton("Location")
                    {
                        RequestLocation = true
                    },
                    new KeyboardButton("Contact")
                    {
                        RequestContact = true
                    },
                });
                await Bot.SendTextMessageAsync(message.Chat.Id, "Who or Where are you?", replyMarkup: keyboard);
            }
            else if (message.Text.StartsWith("/SetStatsWord"))
            {
                var str = HandleSetStatsWord(message);
                await Bot.SendTextMessageAsync(message.Chat.Id, str, replyMarkup: new ReplyKeyboardHide());
            }
            else if (message.Text.StartsWith("/GetWordStats"))
            {
                var str = HandleGetWordStats(message);
                await Bot.SendTextMessageAsync(message.Chat.Id, str, replyMarkup: new ReplyKeyboardHide());
            }
            else
            {
                var usage = @"Usage:
/request - request location or contact
/SetStatsWord - set the word you want to statistic, format: /SetStatsWord [word]
/GetWordStats - get word statistic detail, format: /GetWordStats [word] [top number]";
                await Bot.SendTextMessageAsync(message.Chat.Id, usage, replyMarkup: new ReplyKeyboardHide());
            }
        }

        private static async void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            await Bot.AnswerCallbackQueryAsync(callbackQueryEventArgs.CallbackQuery.Id, $"Received {callbackQueryEventArgs.CallbackQuery.Data}");
        }


        private static void BotOnOnUpdate(object sender, UpdateEventArgs e)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Console.WriteLine($"Bot on update type: {e.Update.Message.Chat.Type}");
            if (e.Update.Message.Type == MessageType.TextMessage && !e.Update.Message.Text.StartsWith(@"/"))
            {
#if Debug
                if (e.Update.Message.Text.Contains("测试"))
                    Console.WriteLine("get");
#endif

                var rule = CalcStatisticInfo.Instance.IsNeedAddInfo(e.Update.Message.Chat.Id, e.Update.Message.Text);
                if (rule != null)
                    CalcStatisticInfo.Instance.AddInfo(rule.RuleId, e.Update.Message.From.Username);
                Console.WriteLine($"Bot on update: {e.Update.Message.Text}");
            }
        }

        #endregion

        #region handle methods

        private static string HandleSetStatsWord(Message message)
        {
#if Release
            //only group can add statictis rule
            if (message.Chat.Type!=ChatType.Group || message.Chat.Type != ChatType.Supergroup)
            {
                return $"Please add bot in a group or supergroup";
            }
#endif
            var cmds = message.Text.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            if (cmds.Length != 2)
            {
                return $"Command: \"{message.Text}\" syntax error! Please use: /SetStatsWord [word] or see /help.";
            }
            CalcStatisticInfo.Instance.AddRule(message.Chat.Type.ToString(), cmds[1], message.Chat.Id);
            return $"Add statstic rule: {cmds[1]} OK!";
        }

        private static string HandleGetWordStats(Message message)
        {
#if Release
            //only group can add statictis rule
            if (message.Chat.Type!=ChatType.Group || message.Chat.Type != ChatType.Supergroup)
            {
                return $"Please add bot in a group or supergroup";
            }
#endif
            var cmds = message.Text.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            if (cmds.Length == 2)
            {
                var ruleId = CalcStatisticInfo.Instance.GetRuleId(message.Chat.Id, cmds[1]);
                return CalcStatisticInfo.Instance.GetTopString(ruleId, cmds[1]);
            }
            if (cmds.Length == 3)
            {
                var ruleId = CalcStatisticInfo.Instance.GetRuleId(message.Chat.Id, cmds[1]);
                return CalcStatisticInfo.Instance.GetTopString(ruleId, cmds[1], Convert.ToInt32(cmds[2]));
            }
            return $"Command: \"{message.Text}\" syntax error! Please use: /GetWordStats [word] [top number] or see /help.";
        }

        #endregion
    }
}
