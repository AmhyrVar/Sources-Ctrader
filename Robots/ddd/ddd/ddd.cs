using System;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
    public class DayTradingSignalBot : Robot
    {
        [Parameter(DefaultValue = 20, MinValue = 1)]
        public int Length { get; set; }

        [Parameter(DefaultValue = 2.0)]
        public double MultFactor { get; set; }

        [Parameter(DefaultValue = 20, MinValue = 1)]
        public int KCLength { get; set; }

        [Parameter(DefaultValue = 1.5)]
        public double KCMultFactor { get; set; }

        [Parameter(DefaultValue = true)]
        public bool UseTrueRange { get; set; }

        private SqueezeMomentum _squeezeMomentumIndicator;
        private SimpleMovingAverage _sma;

        protected override void OnStart()
        {
            _squeezeMomentumIndicator = Indicators.GetIndicator<SqueezeMomentum>(Length, MultFactor, KCLength, KCMultFactor, UseTrueRange);
            _sma = Indicators.SimpleMovingAverage(MarketData.GetSeries(TimeFrame).Close, KCLength);
        }

        protected override void OnTick()
        {
            double val = _squeezeMomentumIndicator.LastValue;
            double avg = _sma.Result.LastValue;

            if (val > 0)
            {
                if (val > _squeezeMomentumIndicator.Result[1])
                {
                    if (Close[0] > avg)
                        Buy();
                }
                else
                {
                    if (Close[0] < avg)
                        Sell();
                }
            }
            else
            {
                if (val < _squeezeMomentumIndicator.Result[1])
                {
                    if (Close[0] < avg)
                        Sell();
                }
                else
                {
                    if (Close[0] > avg)
                        Buy();
                }
            }
        }
    }
}
