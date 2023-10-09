using System;
using System.Linq;
using System.Net;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;
using System.Security.Cryptography.X509Certificates;
namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
    public class RSIalertsBot : Robot
    {

        private RelativeStrengthIndex _rsiFast;
        private RelativeStrengthIndex _rsiSlow;


        private RelativeStrengthIndex _rsi5;
        private RelativeStrengthIndex _rsi15;
        private RelativeStrengthIndex _rsi30;
        private RelativeStrengthIndex _rsiH1;
        private RelativeStrengthIndex _rsiH4;
        private RelativeStrengthIndex _rsiD1;







        [Parameter()]
        public DataSeries Source { get; set; }






        [Parameter(DefaultValue = 14)]
        public int RSI { get; set; }


        [Parameter(DefaultValue = 14)]
        public int RSI_Fast_Period { get; set; }

        [Parameter(DefaultValue = 20)]
        public int RSI_Slow_Period { get; set; }


        private MarketSeries series5;
        private MarketSeries series15;


        private MarketSeries series30;
        private MarketSeries seriesH1;
        private MarketSeries seriesH4;
        private MarketSeries seriesD1;




        // if you know which channel you want to broadcast to, add it here
        // otherwise the bot looks at the people and channels its' interacted with and
        // sends messages to everyone
        [Parameter("Telegram Bot Key", DefaultValue = "1282406359:AAGzi9N3s2xSdswfHdn4gH1qnzv6niPsB8k")]
        public string BOT_API_KEY { get; set; }

        [Parameter("Bot TimeFrame", DefaultValue = "")]
        public string BotTimeFrame { get; set; }

        [Parameter("ChannelId", DefaultValue = "897988131")]
        public string ChannelId { get; set; }


        List<string> _telegramChannels = new List<string>();









        protected override void OnStart()
        {
            series5 = MarketData.GetSeries(TimeFrame.Minute5);
            series15 = MarketData.GetSeries(TimeFrame.Minute15);
            series30 = MarketData.GetSeries(TimeFrame.Minute30);
            seriesH1 = MarketData.GetSeries(TimeFrame.Hour);
            seriesH4 = MarketData.GetSeries(TimeFrame.Hour4);
            seriesD1 = MarketData.GetSeries(TimeFrame.Daily);


            _rsiFast = Indicators.RelativeStrengthIndex(Source, RSI_Fast_Period);
            _rsiSlow = Indicators.RelativeStrengthIndex(Source, RSI_Slow_Period);



            _rsi5 = Indicators.RelativeStrengthIndex(series5.Close, RSI);
            _rsi15 = Indicators.RelativeStrengthIndex(series15.Close, RSI);
            _rsi30 = Indicators.RelativeStrengthIndex(series30.Close, RSI);
            _rsiH1 = Indicators.RelativeStrengthIndex(seriesH1.Close, RSI);
            _rsiH4 = Indicators.RelativeStrengthIndex(seriesH4.Close, RSI);
            _rsiD1 = Indicators.RelativeStrengthIndex(seriesD1.Close, RSI);



            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            // If you didn't set a channel, then find channels by looking at the updates
            if (string.IsNullOrEmpty(ChannelId))
            {
                _telegramChannels = ParseChannels(GetBotUpdates());
            }
            else
            {
                _telegramChannels.Add(ChannelId);
            }
            SendMessageToAllChannels("The bot is running on the " + BotTimeFrame + " timeframe");

        }

        private List<string> ParseChannels(string jsonData)
        {
            var matches = new Regex("\"id\"\\:(\\d+)").Matches(jsonData);
            List<string> channels = new List<string>();
            if (matches.Count > 0)
            {
                foreach (Match m in matches)
                {
                    if (!channels.Contains(m.Groups[1].Value))
                    {
                        channels.Add(m.Groups[1].Value);
                    }
                }
            }
            foreach (var v in channels)
            {
                Print("DEBUG: Found Channel {0} ", v);
            }
            return channels;
        }

        protected int updateOffset = -1;
        private string GetBotUpdates()
        {
            Dictionary<string, string> values = new Dictionary<string, string>();
            if (updateOffset > -1)
            {
                values.Add("offset", (updateOffset++).ToString());
            }

            var jsonData = MakeTelegramRequest(BOT_API_KEY, "getUpdates", values);

            var matches = new Regex("\"message_id\"\\:(\\d+)").Matches(jsonData);
            if (matches.Count > 0)
            {
                foreach (Match m in matches)
                {
                    int msg_id = -1;
                    int.TryParse(m.Groups[1].Value, out msg_id);
                    if (msg_id > updateOffset)
                    {
                        updateOffset = msg_id;
                    }
                }
            }
            return jsonData;
        }

        private void SendMessageToAllChannels(string message)
        {
            foreach (var c in _telegramChannels)
            {
                SendMessageToChannel(c, message);
            }
        }

        private string SendMessageToChannel(string chat_id, string message)
        {
            var values = new Dictionary<string, string> 
            {
                {
                    "chat_id",
                    chat_id
                },
                {
                    "text",
                    message
                }
            };

            return MakeTelegramRequest(BOT_API_KEY, "sendMessage", values);
        }

        private string MakeTelegramRequest(string api_key, string method, Dictionary<string, string> values)
        {
            string TELEGRAM_CALL_URI = string.Format("https://api.telegram.org/bot{0}/{1}", api_key, method);

            var request = WebRequest.Create(TELEGRAM_CALL_URI);
            request.ContentType = "application/x-www-form-urlencoded";
            request.Method = "POST";

            StringBuilder data = new StringBuilder();
            foreach (var d in values)
            {
                data.Append(string.Format("{0}={1}&", d.Key, d.Value));
            }
            byte[] byteArray = Encoding.UTF8.GetBytes(data.ToString());
            request.ContentLength = byteArray.Length;

            Stream dataStream = request.GetRequestStream();

            dataStream.Write(byteArray, 0, byteArray.Length);

            dataStream.Close();

            WebResponse response = request.GetResponse();

            Print("DEBUG {0}", ((HttpWebResponse)response).StatusDescription);

            dataStream = response.GetResponseStream();

            StreamReader reader = new StreamReader(dataStream);

            string outStr = reader.ReadToEnd();

            Print("DEBUG {0}", outStr);

            reader.Close();

            return outStr;
        }

        protected override void OnTick()
        {
            // SendMessageToAllChannels("WMA" + FastPeriods + " has just changed color to match WMA" + SlowPeriods + " in " + SymbolName + ", in " + BotTimeFrame + " timeframe");
            //WMA10 has just changed color to match WMA 100 in EURUSD, m30 timeframe

            //1: when the slow/fast RSI is above 75 or below 25 lets say             
            //2: when fast crosses slow

            //et ce pour toutes les TF

            if ((_rsi5.Result.Last(2) < 75 && _rsi5.Result.Last(1) > 75))
            {
                SendMessageToAllChannels("RSI 5 is overbaught" + " in " + SymbolName + " in 5M timeframe");
            }

            if ((_rsi5.Result.Last(2) > 25 && _rsi5.Result.Last(1) < 25))
            {
                SendMessageToAllChannels("RSI 5 is oversold" + " in " + SymbolName + " in 5M timeframe");
            }





        }


    }
}
