using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class TakeProfit : Robot
    {


        [Parameter("Instance Name", DefaultValue = "D001")]
        public string PositionId { get; set; }

        //  [Parameter("Quantity (Lots)", Group = "Volume", DefaultValue = 1, MinValue = 0.01, Step = 0.01)]
        // public double Quantity { get; set; }

        public double Quantity = 1;

        [Parameter("Enabled", Group = "Tralling Stop", DefaultValue = false)]
        public bool TrallingStopEnabled { get; set; }

        [Parameter("Tralling Stop (pips)", Group = "Tralling Stop", DefaultValue = 20.0)]
        public int StopLoss { get; set; }

        [Parameter("OnTick", Group = "Tralling Stop", DefaultValue = false)]
        public bool OnTickTrallingStopEnabled { get; set; }


        // [Parameter("Only Show", Group = "Take Profit", DefaultValue = false)]
        public bool onlyShowEnabled = false;



        [Parameter("Enabled", Group = "Take Profit 1", DefaultValue = false)]
        public bool TakeProfit1Enabled { get; set; }

        [Parameter("Pips", Group = "Take Profit 1", DefaultValue = 10)]
        public double TakeProfit1Pips { get; set; }

        [Parameter("Volume", Group = "Take Profit 1", DefaultValue = 1000)]
        public int TakeProfit1Volume { get; set; }

        [Parameter("Enabled", Group = "Take Profit 2", DefaultValue = false)]
        public bool TakeProfit2Enabled { get; set; }

        [Parameter("Pips", Group = "Take Profit 2", DefaultValue = 20)]
        public double TakeProfit2Pips { get; set; }

        [Parameter("Volume", Group = "Take Profit 2", DefaultValue = 1000)]
        public int TakeProfit2Volume { get; set; }

        [Parameter("Enabled", Group = "Take Profit 3", DefaultValue = false)]
        public bool TakeProfit3Enabled { get; set; }

        [Parameter("Pips", Group = "Take Profit 3", DefaultValue = 10)]
        public double TakeProfit3Pips { get; set; }

        [Parameter("Volume", Group = "Take Profit 3", DefaultValue = 1000)]
        public int TakeProfit3Volume { get; set; }


        [Parameter("Enabled", Group = "Take Profit 4", DefaultValue = false)]
        public bool TakeProfit4Enabled { get; set; }

        [Parameter("Pips", Group = "Take Profit 4", DefaultValue = 10)]
        public double TakeProfit4Pips { get; set; }

        [Parameter("Volume", Group = "Take Profit 4", DefaultValue = 1000)]
        public int TakeProfit4Volume { get; set; }


        [Parameter("Enabled", Group = "Break Even", DefaultValue = false)]
        public bool BreakEvenEnabled { get; set; }

        [Parameter("Add Pips", Group = "Break Even", DefaultValue = 0.0, MinValue = 0.0)]
        public double AddPips { get; set; }

        [Parameter("Trigger Pips", Group = "Break Even", DefaultValue = 10, MinValue = 1)]
        public double TriggerPips { get; set; }

        [Parameter("OnTick", Group = "Break Even", DefaultValue = false)]
        public bool OnTickBreakEvenEnabled { get; set; }


        [Parameter("Enabled", Group = "Copyright", DefaultValue = true)]
        public bool Copyright { get; set; }

        private TakeProfitLevel[] _levels;
        private bool entrar = true;
        private SymbolInfo _symbolInfo;


        protected override void OnStart()
        {
            if (Copyright)
            {
                Chart.DrawStaticText("Copyright_001", "Copyright Unmeego", VerticalAlignment.Bottom, HorizontalAlignment.Left, Color.Gold);
            }
        }

        protected override void OnTick()
        {
            // Put your core logic here

            var position = Positions.Find(PositionId);
            if (position != null && entrar)
            {

                _symbolInfo = Symbols.GetSymbolInfo(position.SymbolName);
                _levels = GetTakeProfitLevels();

                ValidateLevels(position);
                entrar = false;
            }

            if (position == null)
            {
                entrar = true;
            }

            profit123();


            if (Symbol.Ask == 1.22569 && Time.Hour > 10)
            {
                //    vender();
            }


            if (TrallingStopEnabled && OnTickTrallingStopEnabled)
            {
                TrallingStop();
            }

            if (BreakEvenEnabled && OnTickBreakEvenEnabled)
            {
                BreakEvenIfNeeded();
            }
        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }

        protected override void OnBar()
        {
            if (BreakEvenEnabled && OnTickBreakEvenEnabled == false)
            {
                BreakEvenIfNeeded();
            }

            if (TrallingStopEnabled && OnTickTrallingStopEnabled == false)
            {
                TrallingStop();
            }

        }


        protected void profit123()
        {
            var position = Positions.Find(PositionId);
            if (position != null)
            {

                var reachedLevels = _levels.Where(level => level.IsEnabled && !level.IsTriggered && level.Pips <= position.Pips);

                foreach (var reachedLevel in reachedLevels)
                {
                    reachedLevel.MarkAsTriggered();

                    Print("Level \"" + reachedLevel.Name + "\" is reached. Level.Pips: " + reachedLevel.Pips + ", Position.Pips: " + position.Pips + ", Position.Id: " + position.Id);
                    var volumeToClose = Math.Min(reachedLevel.Volume, position.VolumeInUnits);

                    if (position.TradeType == TradeType.Buy)
                    {
                        Chart.DrawIcon("poMarker1" + Bars.OpenTimes.Last(0).ToString() + "" + reachedLevel.Pips, ChartIconType.Square, Bars.OpenTimes.LastValue, Bars.LowPrices.Last(0) - (reachedLevel.Pips * Symbol.PipSize), reachedLevel.mColor);
                    }
                    else
                    {
                        Chart.DrawIcon("poMarker1" + Bars.OpenTimes.Last(0).ToString() + "" + reachedLevel.Pips, ChartIconType.Square, Bars.OpenTimes.LastValue, Bars.HighPrices.Last(0) + (reachedLevel.Pips * Symbol.PipSize), reachedLevel.mColor);

                    }

                    if (onlyShowEnabled == false)
                    {
                        ClosePosition(position, volumeToClose);
                    }

                }
            }
        }

        protected void TrallingStop()
        {
            var position = Positions.Find(PositionId);
            if (position != null)
            {


                if (position.TradeType == TradeType.Sell)
                {
                    double profit1 = position.GrossProfit + position.Commissions - StopLoss * 10 * position.Volume / 100000.0;

                    if (profit1 > 0.0 && position.TradeType == TradeType.Sell)
                    {
                        double? stopLossPrice = Symbol.Bid + StopLoss * Symbol.PipSize;
                        if (StopLoss != 0 && stopLossPrice < position.StopLoss && stopLossPrice < position.EntryPrice)
                        {
                            if (stopLossPrice - Symbol.Bid > 0)
                            {
                                Trade.ModifyPosition(position, stopLossPrice, position.TakeProfit);
                            }
                        }
                    }
                }
                else
                {
                    double profit2 = position.GrossProfit + position.Commissions - StopLoss * 10 * position.Volume / 100000.0;

                    if (profit2 > 0.0 && position.TradeType == TradeType.Buy)
                    {
                        double stopLossPrice = Symbol.Ask - StopLoss * Symbol.PipSize;

                        if (StopLoss != 0 && stopLossPrice > position.StopLoss && stopLossPrice > position.EntryPrice)
                            if (stopLossPrice - Symbol.Ask < 0)
                                Trade.ModifyPosition(position, stopLossPrice, position.TakeProfit);
                    }
                }
            }
        }



        private void ValidateLevels(Position position)
        {
            MakeSureAnyLevelEnabled();
            ValidateTotalVolume(position);
            ValidateReachedLevels(position);
            ValidateVolumes();
        }

        private void ValidateVolumes()
        {
            var enabledLevels = _levels.Where(level => level.IsEnabled);
            foreach (var level in enabledLevels)
            {
                if (level.Volume < _symbolInfo.VolumeInUnitsMin)
                    PrintErrorAndStop("Volume for " + _symbolInfo.Name + " cannot be less than " + _symbolInfo.VolumeInUnitsMin);
                if (level.Volume > _symbolInfo.VolumeInUnitsMax)
                    PrintErrorAndStop("Volume for " + _symbolInfo.Name + " cannot be greater than " + _symbolInfo.VolumeInUnitsMax);
                if (level.Volume % _symbolInfo.VolumeInUnitsMin != 0)
                    PrintErrorAndStop("Volume " + level.Volume + " is invalid");
            }
        }

        private void ValidateReachedLevels(Position position)
        {
            var reachedLevel = _levels.FirstOrDefault(l => l.Pips <= position.Pips);
            if (reachedLevel != null)
                PrintErrorAndStop("Level " + reachedLevel.Name + " is already reached. The amount of Pips must be more than the amount of Pips that the Position is already gaining");
        }

        private void MakeSureAnyLevelEnabled()
        {
            if (_levels.All(level => !level.IsEnabled))
                PrintErrorAndStop("You have to enable at least one \"Take Profit\" in cBot Parameters");
        }

        private void ValidateTotalVolume(Position position)
        {
            var totalVolume = _levels.Where(level => level.IsEnabled).Sum(level => level.Volume);

            if (totalVolume > position.VolumeInUnits)
                PrintErrorAndStop("The sum of all Take Profit respective volumes cannot be larger than the Position's volume");
        }

        private TakeProfitLevel[] GetTakeProfitLevels()
        {
            return new[] 
            {
                new TakeProfitLevel("Take Profit 1", TakeProfit1Enabled, TakeProfit1Pips, TakeProfit1Volume, Color.LightBlue),
                new TakeProfitLevel("Take Profit 2", TakeProfit2Enabled, TakeProfit2Pips, TakeProfit2Volume, Color.SkyBlue),
                new TakeProfitLevel("Take Profit 3", TakeProfit3Enabled, TakeProfit3Pips, TakeProfit3Volume, Color.MidnightBlue),
                new TakeProfitLevel("Take Profit 4", TakeProfit4Enabled, TakeProfit4Pips, TakeProfit4Volume, Color.Blue)
            };
        }



        private void PrintErrorAndStop(string errorMessage)
        {
            Print(errorMessage);
            //  Stop();

            // throw new Exception(errorMessage);
        }



        private void BreakEvenIfNeeded()
        {
            var position = Positions.Find(PositionId);
            if (position == null)
                return;

            if (position.Pips < TriggerPips)
                return;

            var desiredNetProfitInDepositAsset = AddPips * _symbolInfo.PipValue * position.VolumeInUnits;
            var desiredGrossProfitInDepositAsset = desiredNetProfitInDepositAsset - position.Commissions * 2 - position.Swap;
            var quoteToDepositRate = _symbolInfo.PipValue / _symbolInfo.PipSize;
            var priceDifference = desiredGrossProfitInDepositAsset / (position.VolumeInUnits * quoteToDepositRate);

            var priceAdjustment = GetPriceAdjustmentByTradeType(position.TradeType, priceDifference);
            var breakEvenLevel = position.EntryPrice + priceAdjustment;
            var roundedBreakEvenLevel = RoundPrice(breakEvenLevel, position.TradeType);


            if (position.TradeType == TradeType.Sell && position.StopLoss < position.EntryPrice)
                return;

            if (position.TradeType == TradeType.Buy && position.StopLoss > position.EntryPrice)
                return;

            Print(roundedBreakEvenLevel + " " + position.StopLoss);
            ModifyPosition(position, roundedBreakEvenLevel, position.TakeProfit);

            Print("Stop loss for position PID" + position.Id + " has been moved to break even.");

        }

        private double RoundPrice(double price, TradeType tradeType)
        {
            var multiplier = Math.Pow(10, _symbolInfo.Digits);

            if (tradeType == TradeType.Buy)
                return Math.Ceiling(price * multiplier) / multiplier;

            return Math.Floor(price * multiplier) / multiplier;
        }

        private static double GetPriceAdjustmentByTradeType(TradeType tradeType, double priceDifference)
        {
            if (tradeType == TradeType.Buy)
                return priceDifference;

            return -priceDifference;
        }

        protected void vender()
        {
            var position = Positions.Find(PositionId);
            if (position == null)
            {

                var operation = ExecuteMarketOrder(TradeType.Sell, SymbolName, VolumeInUnits, PositionId, 50, 0);
                Print(operation.ToString());
                Chart.DrawIcon("poMarker1" + Bars.OpenTimes.Last(0).ToString(), ChartIconType.UpArrow, Bars.OpenTimes.LastValue, Bars.LowPrices.Last(0) - (3 * Symbol.PipSize), Color.Gold);
            }
        }


        private double VolumeInUnits
        {
            get { return Symbol.QuantityToVolumeInUnits(Quantity); }
        }
    }


    internal class TakeProfitLevel
    {
        public string Name { get; private set; }

        public bool IsEnabled { get; private set; }

        public double Pips { get; private set; }

        public int Volume { get; private set; }

        public bool IsTriggered { get; private set; }

        public Color mColor;

        public TakeProfitLevel(string name, bool isEnabled, double pips, int volume, Color c)
        {
            Name = name;
            IsEnabled = isEnabled;
            Pips = pips;
            Volume = volume;
            mColor = c;
        }

        public void MarkAsTriggered()
        {
            IsTriggered = true;
        }
    }
}
