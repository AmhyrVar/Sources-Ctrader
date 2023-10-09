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

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
    public class MaxdistMK2 : Robot
    {
        [Parameter("Quantity (Lots)", Group = "Volume", DefaultValue = 1, MinValue = 0.01, Step = 0.01)]
        public double Quantity { get; set; }

        //[Parameter("MA Type", Group = "Moving Average")]
        //public MovingAverageType MAType { get; set; }

        [Parameter("Source", Group = "Moving Average")]
        public DataSeries SourceSeries { get; set; }

        [Parameter("MA2", Group = "Moving Average", DefaultValue = 10)]
        public int SlowPeriods { get; set; }

        [Parameter("MA1", Group = "Moving Average", DefaultValue = 5)]
        public int FastPeriods { get; set; }

        [Parameter("Num Bars", DefaultValue = 20)]
        public int Num_Bars { get; set; }

        [Parameter("Bot method : 1 for %, 2 for Pips", DefaultValue = 1)]
        public int BotMethod { get; set; }

        [Parameter("% = ", DefaultValue = 50)]
        public double Percentage { get; set; }
        [Parameter("Pips Method", DefaultValue = 35)]
        public int MethodPips { get; set; }

        public string distratio;


        //public int fastMaState;
        //0 for falling 1 for rising
        public int slowMaState;
        List<int> fastMaState = new List<int>();

        [Parameter("Telegram Bot Key", DefaultValue = "1282406359:AAGzi9N3s2xSdswfHdn4gH1qnzv6niPsB8k")]
        public string BOT_API_KEY { get; set; }

        [Parameter("Bot TimeFrame", DefaultValue = "")]
        public string BotTimeFrame { get; set; }

        [Parameter("ChannelId", DefaultValue = "897988131")]
        public string ChannelId { get; set; }


        List<string> _telegramChannels = new List<string>();




        private MovingAverage slowMa;
        private MovingAverage fastMa;

        public int g = 0;
        public int r = 0;



        List<double> maxtab = new List<double>();



        protected override void OnStart()
        {
            fastMa = Indicators.MovingAverage(SourceSeries, FastPeriods, MovingAverageType.Weighted);
            slowMa = Indicators.MovingAverage(SourceSeries, SlowPeriods, MovingAverageType.Weighted);
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


        protected override void OnBar()
        {
            maxtab.Clear();

            var currentSlowMa = slowMa.Result.Last(0);
            var currentFastMa = fastMa.Result.Last(0);


            var x = 0;




            //if it wan't falling
            if (fastMa.Result.IsFalling())
            {
                //if it ain't falling previously list gonna have 0 value
                fastMaState.Add(0);
            }
            if (fastMa.Result.IsRising())
            {
                fastMaState.Add(1);
            }
            if (fastMa.Result.IsRising() != true && fastMa.Result.IsFalling() != true)
            {
                fastMaState.Add(2);
            }

            // 0 for falling 1 for rising 2 for horizontal


            if (x <= Num_Bars)
            {
                x++;
                maxtab.Add(Math.Abs((slowMa.Result.Last(x) - fastMa.Result.Last(x)) / Symbol.PipSize));

            }

            var maxDist = maxtab.Max<double>();
            var actualDist = Math.Abs((slowMa.Result.LastValue - fastMa.Result.LastValue) / Symbol.PipSize);
            var triggerDist1 = maxDist * (Percentage / 100);



            if (BotMethod == 1 && fastMaState.Count > 3)
            {
                if (actualDist > triggerDist1)
                {
                    distratio = "lesser";


                    //if it wasn't falling and now it is
                    if (fastMaState[fastMaState.Count - 1] == 0 && fastMaState[fastMaState.Count - 2] != 0 && slowMa.Result.IsFalling() && fastMa.Result.IsFalling() && fastMa.Result.LastValue < slowMa.Result.LastValue)
                    {



                        Chart.DrawVerticalLine("Red line" + r, Bars.OpenTimes.Last(1), "Red", 5);
                        SendMessageToAllChannels("WMA" + FastPeriods + " has just changed color to match WMA" + SlowPeriods + " in " + SymbolName + ", in " + BotTimeFrame + " timeframe");
                        //WMA10 has just changed color to match WMA 100 in EURUSD, m30 timeframe
                    }

                    //same for going up
                    if (fastMaState[fastMaState.Count - 1] == 1 && fastMaState[fastMaState.Count - 2] != 1 && slowMa.Result.IsRising() && fastMa.Result.IsRising() && fastMa.Result.LastValue > slowMa.Result.LastValue)
                    {

                        g++;
                        var greenname = "greenline " + g;
                        Print("Green line name = ", greenname);
                        Chart.DrawVerticalLine("Green line " + g, Bars.OpenTimes.Last(1), "Green", 5);
                        SendMessageToAllChannels("WMA" + FastPeriods + " has just changed color to match WMA" + SlowPeriods + " in " + SymbolName + ", in " + BotTimeFrame + " timeframe");
                        //WMA10 has just changed color to match WMA 100 in EURUSD, m30 timeframe

                    }




                }



            }

            else if (BotMethod == 2)
            {

                if (actualDist > MethodPips)
                {
                    distratio = "lesser";


                    //if it wasn't falling and now it is
                    if (fastMaState[fastMaState.Count - 1] == 0 && fastMaState[fastMaState.Count - 2] != 0 && slowMa.Result.IsFalling() && fastMa.Result.IsFalling() && fastMa.Result.LastValue < slowMa.Result.LastValue)
                    {



                        Chart.DrawVerticalLine("Red line" + r, Bars.OpenTimes.Last(1), "Red", 5);
                        SendMessageToAllChannels("WMA" + FastPeriods + " has just changed color to match WMA" + SlowPeriods + " in " + SymbolName + ", in " + BotTimeFrame + " timeframe");
                        //WMA10 has just changed color to match WMA 100 in EURUSD, m30 timeframe

                        r++;
                    }

                    //same for going up
                    if (fastMaState[fastMaState.Count - 1] == 1 && fastMaState[fastMaState.Count - 2] != 1 && slowMa.Result.IsRising() && fastMa.Result.IsRising() && fastMa.Result.LastValue > slowMa.Result.LastValue)
                    {

                        g++;
                        var greenname = "greenline " + g;
                        Print("Green line name = ", greenname);
                        SendMessageToAllChannels("WMA" + FastPeriods + " has just changed color to match WMA" + SlowPeriods + " in " + SymbolName + ", in " + BotTimeFrame + " timeframe");
                        //WMA10 has just changed color to match WMA 100 in EURUSD, m30 timeframe

                        Chart.DrawVerticalLine("Green line " + g, Bars.OpenTimes.Last(1), "Green", 5);

                    }




                }
            }



        }


    }
}
