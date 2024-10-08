﻿using cAlgo.API;
using cAlgo.API.Indicators;

namespace cAlgo.Robots
{
    [Robot(AccessRights = AccessRights.None)]
    public class MacdBot : Robot
    {
        private MacdHistogram _macd;
        private SimpleMovingAverage _SM1;
        private SimpleMovingAverage _SM2;

        private Position _position;

        [Parameter(DefaultValue = 10000, MinValue = 0)]
        public int Volume { get; set; }


        [Parameter("Period", DefaultValue = 9)]
        public int Period { get; set; }

        [Parameter("Long Cycle", DefaultValue = 26)]
        public int LongCycle { get; set; }

        [Parameter("Short Cycle", DefaultValue = 12)]
        public int ShortCycle { get; set; }

        protected override void OnStart()
        {
            _macd = Indicators.MacdHistogram(LongCycle, ShortCycle, Period);
            _SM1 = Indicators.SimpleMovingAverage(ClosePosition, 100);
            _SM2 = Indicators.SimpleMovingAverage(ClosePosition, 200);
        }

        protected override void OnBar()
        {
            if (Trade.IsExecuting)
                return;

                        /*bool isLongPositionOpen = _position != null && _position.TradeType == TradeType.Buy;*/
bool isShortPositionOpen = _position != null && _position.TradeType == TradeType.Sell;

                        /* if (_macd.Histogram.LastValue > 0.0 && _macd.Signal.IsRising() && !isLongPositionOpen)
            {
                ClosePosition();
                Buy();
            }*/

if (_macd.Histogram.LastValue < 0.0 && _macd.Signal.IsFalling() && !isShortPositionOpen && _SM2 > _SM1)
            {
                ClosePosition();
                Sell();
            }
        }
        private void ClosePosition()
        {
            if (_position != null)
            {
                Trade.Close(_position);
                _position = null;
            }
        }

        private void Buy()
        {
            Trade.CreateBuyMarketOrder(Symbol, Volume);
        }

        private void Sell()
        {
            Trade.CreateSellMarketOrder(Symbol, Volume);
        }

        protected override void OnPositionOpened(Position openedPosition)
        {
            _position = openedPosition;
        }

    }
}
