using System;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(AccessRights = AccessRights.None)]
    public class VICBOTWITHTIMESANDEQUITYSTOP6 : Robot
    {
        int upClose;
        int upCloseBefore;
        int insideBar;
        int downClose;
        int downCloseBefore;
        int counter = 0;
        Position position;

        [Parameter(DefaultValue = 10000)]
        public int Volume { get; set; }

        [Parameter("Stop Loss (pips)", DefaultValue = 10)]
        public int StopLoss { get; set; }

        [Parameter("Take Profit (pips)", DefaultValue = 10)]
        public int TakeProfit { get; set; }

        [Parameter("From", DefaultValue = 7)]
        public int From { get; set; }

        [Parameter("To", DefaultValue = 10)]
        public int To { get; set; }
        [Parameter("Use Equity Stop Out", DefaultValue = true, Group = "Risk Management")]
        public bool UseMaxEquityStopOut { get; set; }
        [Parameter("Equity Stop Out (profit)", DefaultValue = 500, Group = "Risk Management")]
        public double MaxEquityProfit { get; set; }
        [Parameter("Equity Stop Out (loss)", DefaultValue = 500, Group = "Risk Management")]
        public double MaxEquityLoss { get; set; }
        [Parameter("Instance", DefaultValue = "Instance")]
        public string Instance { get; set; }

        private double _maxEquity;
        private double _maxBalance;
        private double _startingEquity;

        protected override void OnStart()
        {
            // Put your initialization logic here
            _maxEquity = Account.Equity;
            _maxBalance = Account.Balance;
            _startingEquity = Account.Equity;

        }

        protected override void OnBar()
        {
            if (Server.Time.Hour >= From && Server.Time.Hour < To)
            {
                if (Trade.IsExecuting)
                {
                    return;
                }
                if (MarketSeries.Close[MarketSeries.Close.Count - 1] > MarketSeries.Close[MarketSeries.Close.Count - 2])
                {
                    upClose = 1;
                }
                else
                {
                    upClose = 0;
                }

                if (MarketSeries.Close[MarketSeries.Close.Count - 3] > MarketSeries.Close[MarketSeries.Close.Count - 4])
                {
                    upCloseBefore = 1;
                }
                else
                {
                    upCloseBefore = 0;
                }

                if ((MarketSeries.High[MarketSeries.High.Count - 2] < MarketSeries.High[MarketSeries.High.Count - 3]) && (MarketSeries.Low[MarketSeries.Low.Count - 2] > MarketSeries.Low[MarketSeries.Low.Count - 3]))
                {
                    insideBar = 1;
                }
                else
                {
                    insideBar = 0;
                }

                if (MarketSeries.Close[MarketSeries.Close.Count - 1] < MarketSeries.Close[MarketSeries.Close.Count - 2])
                {
                    downClose = 1;
                }
                else
                {
                    downClose = 0;
                }

                if (MarketSeries.Close[MarketSeries.Close.Count - 3] < MarketSeries.Close[MarketSeries.Close.Count - 4])
                {
                    downCloseBefore = 1;
                }
                else
                {
                    downCloseBefore = 0;
                }

                if (counter == 0)
                {
                    if (upClose == 1 && insideBar == 1 && upCloseBefore == 1)
                    {
                        Trade.CreateMarketOrder(TradeType.Buy, Symbol, Volume);
                    }
                    if (downClose == 1 && insideBar == 1 && downCloseBefore == 1)
                    {
                        Trade.CreateMarketOrder(TradeType.Sell, Symbol, Volume);
                    }
                }
            }
        }
        protected override void OnTick()
        {


            if (UseMaxEquityStopOut)
            {
                if (Account.Equity > _startingEquity + MaxEquityProfit)
                {

                    {
                        position.Close();
                    }
                    Print("cBot stopped due to  Equity Stop Out (profit)");
                    Stop();
                }

                if (Account.Equity < _startingEquity - MaxEquityLoss)
                {

                    {
                        position.Close();
                    }
                    Print("cBot stopped due to  Equity Stop Out (loss)");
                    Stop();
                }
            }
        }

        protected override void OnPositionOpened(Position openedPosition)
        {

            {
                position = openedPosition;
                counter = 1;
                ModifyPosition(openedPosition, GetAbsoluteStopLoss(openedPosition, StopLoss), GetAbsoluteTakeProfit(openedPosition, TakeProfit));
            }
        }

        protected override void OnPositionClosed(Position position)
        {
            counter = 0;
        }

        private double GetAbsoluteStopLoss(Position position, int stopLossInPips)
        {
            return position.TradeType == TradeType.Buy ? position.EntryPrice - Symbol.PipSize * stopLossInPips : position.EntryPrice + Symbol.PipSize * stopLossInPips;
        }

        private double GetAbsoluteTakeProfit(Position position, int takeProfitInPips)
        {
            return position.TradeType == TradeType.Buy ? position.EntryPrice + Symbol.PipSize * takeProfitInPips : position.EntryPrice - Symbol.PipSize * takeProfitInPips;
        }
    }
}
