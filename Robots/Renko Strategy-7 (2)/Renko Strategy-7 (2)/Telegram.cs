using System;
using System.Threading.Tasks;
using Telegram.Bot;

namespace cAlgo
{
    public class TelegramService
    {
        public TelegramService()
        {

        }

        public string SendTelegram(string chatId, string token, string telegramMessage)
        {
            string message = SendTelegramAsync(chatId, token, telegramMessage).Result;
            return message;
        }

        private async Task<string> SendTelegramAsync(string chatId, string token, string telegramMessage)
        {
            string reply = string.Empty;

            try
            {
                var bot = new TelegramBotClient(token);
                await bot.SendTextMessageAsync(chatId, telegramMessage);
                reply = "SUCCESS";
            }
            catch (Exception ex)
            {
                reply = "ERROR: " + ex.Message;
            }

            return reply;
        }
    }
}