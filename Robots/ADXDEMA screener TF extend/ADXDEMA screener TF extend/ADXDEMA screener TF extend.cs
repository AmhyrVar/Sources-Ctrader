using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class ADXDEMAscreenerTFextend : Robot
    {

        //keep        
        [Parameter("Bot's timeframe")]
        public TimeFrame BotTimeFrame { get; set; }

        [Parameter("Fast EMA Periods", DefaultValue = 50)]
        public int EMA1P { get; set; }
        [Parameter("Slow EMA Periods", DefaultValue = 100)]
        public int EMA2P { get; set; }
        [Parameter("ADX Periods", DefaultValue = 14)]
        public int ADXPeriods { get; set; }

        [Parameter("Volume", DefaultValue = 1000)]
        public int volume { get; set; }

        [Parameter("Stop loss in pips", DefaultValue = 100)]
        public int SL_Pips { get; set; }

        [Parameter("SL in pips trigger", DefaultValue = true)]
        public bool SL_Pips_Trigger { get; set; }

        [Parameter("SL CrossOver trigger", DefaultValue = true)]
        public bool SL_CrossOver_Trigger { get; set; }
        [Parameter("TP in pips", DefaultValue = 100)]
        public int TP_Pips { get; set; }

        [Parameter("TP in pips trigger", DefaultValue = true)]
        public bool TP_Pips_Trigger { get; set; }

        [Parameter("TP CrossOver trigger", DefaultValue = true)]
        public bool TP_CrossOver_Trigger { get; set; }

        [Parameter("Enter Position on signal", DefaultValue = true)]
        public bool EnterPosition { get; set; }

        [Parameter("Telegram Notification", DefaultValue = true)]
        public bool TelegramNotification { get; set; }



        [Parameter()]
        public DataSeries Source { get; set; }


        private ExponentialMovingAverage FastEMA;
        private ExponentialMovingAverage SlowEMA;
        private DirectionalMovementSystem ADX;


        protected override void OnStart()
        {
            //keep
            Bars bars = MarketData.GetBars(BotTimeFrame);



            FastEMA = Indicators.ExponentialMovingAverage(bars.ClosePrices, EMA1P);
            SlowEMA = Indicators.ExponentialMovingAverage(bars.ClosePrices, EMA2P);
            ADX = Indicators.DirectionalMovementSystem(bars, ADXPeriods);




        }



        protected void LongScenario()
        {
            if (TelegramNotification)
            {
                //send notificationLong
            }
            if (EnterPosition)
            {

                ExecuteMarketOrder(TradeType.Buy, SymbolName, volume, "ADXEMA");

            }
        }

        protected void ShortScenario()
        {
            if (TelegramNotification)
            {
                //send notificationShort
            }
            if (EnterPosition)
            {
                ExecuteMarketOrder(TradeType.Sell, SymbolName, volume, "ADXEMA");
            }

        }


        protected override void OnTick()
        {
            if (TP_Pips_Trigger)
            {
                foreach (var position in Positions)
                {
                    if (position.Label == "ADXEMA" && position.Pips >= TP_Pips)
                    {
                        ClosePosition(position);
                    }
                }
            }
            if (SL_Pips_Trigger)
            {
                foreach (var position in Positions)
                {
                    if (position.Label == "ADXEMA" && -position.Pips >= SL_Pips)
                    {
                        ClosePosition(position);
                    }
                }
            }

            if (SL_CrossOver_Trigger)
            {
                foreach (var position in Positions)
                {
                    if (position.Label == "ADXEMA" && position.Pips < 0)
                    {
                        if (FastEMA.Result.HasCrossedAbove(SlowEMA.Result, 1) || FastEMA.Result.HasCrossedBelow(SlowEMA.Result, 1))
                        {
                            ClosePosition(position);
                        }
                    }
                }
            }

            if (TP_CrossOver_Trigger)
            {
                foreach (var position in Positions)
                {
                    if (position.Label == "ADXEMA" && position.Pips > 0)
                    {
                        if (FastEMA.Result.HasCrossedAbove(SlowEMA.Result, 1) || FastEMA.Result.HasCrossedBelow(SlowEMA.Result, 1))
                        {
                            ClosePosition(position);
                        }
                    }
                }
            }
            /* BUY If the Fast Exponential Moving Average crosses from below and close above the Slow Exponential Moving Average, 
            open LONG position if the cross is confirmed at the candle closure and if ADX line in the Direction Movement indicator is above 20.*/
            if (FastEMA.Result.HasCrossedAbove(SlowEMA.Result, 1) && ADX.ADX.LastValue > 20)
            {
                LongScenario();
            }
            if (FastEMA.Result.HasCrossedBelow(SlowEMA.Result, 1) && ADX.ADX.LastValue > 20)
            {
                ShortScenario();
            }



        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }
    }
}
