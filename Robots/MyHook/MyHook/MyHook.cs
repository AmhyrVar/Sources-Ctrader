using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
    public class MyHook : Robot
    {
        [Parameter("Webhook", Group = "Params", DefaultValue = "https://discord.com/api/webhooks...")]
        public string Webhook { get; set; }

        [Parameter("Name", Group = "Params", DefaultValue = "Username")]
        public string User { get; set; }

        protected override void OnStart()
        {
            Positions.Opened += OnPositionOpened;
            Positions.Closed += OnPositionClosed;


        }

        public void OnPositionOpened(PositionOpenedEventArgs args)
        {

            var Message = "#{0} opened {1} position at {2} for {3} lots";
            string messageformat = string.Format(Message, args.Position.SymbolName, args.Position.TradeType, args.Position.EntryPrice, args.Position.Quantity);

            DiscordSendMessage(Webhook, User, messageformat);

        }
        public void OnPositionClosed(PositionClosedEventArgs args)
        {


            var Message = "#{0} closed {1} position at {2} for {3} lots";
            string messageformat = string.Format(Message, args.Position.SymbolName, args.Position.TradeType, args.Position.EntryPrice, args.Position.Quantity);

            DiscordSendMessage(Webhook, User, messageformat);

        }


        public static void DiscordSendMessage(string url, string username, string content)
        {
            WebClient wc = new WebClient();

            try
            {
                wc.UploadValues(url, new NameValueCollection 
                {
                    {
                        "content",
                        content
                    },


                    {
                        "username",
                        username
                    }
                });

            } catch (WebException ex)
            {

            }
        }



    }
}
