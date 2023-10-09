using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cAlgo.API;
using cAlgo.API.Collections;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
    public class TFTBouncerv2 : Robot
    {

        [Parameter("Active", Group = "Automatic Trading", DefaultValue = false)]
        public bool AutoActive { get; set; }

        [Parameter("Identifier", Group = "Automatic Trading", DefaultValue = "Default")]
        public string Identifier { get; set; }

        [Parameter("Trade Direction", Group = "Automatic Trading", DefaultValue = DirectionSelector.AllTrades)]
        public DirectionSelector TheDirection { get; set; }

        [Parameter("Distance away from Buy (in pips)", Group = "Automatic Trading", MinValue = 0)]
        public double BuyDistance { get; set; }

        [Parameter("Distance away from Sell (in pips)", Group = "Automatic Trading", MinValue = 0)]
        public double SellDistance { get; set; }

        [Parameter("Active", Group = "Passive Collection", MinValue = true)]
        public bool PassiveCollection { get; set; }

        [Parameter("Active", Group = "Aggressive Collection", MinValue = true)]
        public bool ActiveRecovery { get; set; }

        [Parameter("Recovery Trade Distance (in pips)", Group = "Aggressive Collection", MinValue = 0)]
        public double RecoveryDistance { get; set; }

        [Parameter("Volume", Group = "Order Parameters", DefaultValue = 0.01, MinValue = 0.01)]
        public double Volume { get; set; }

        [Parameter("Take Profit (in pips)", Group = "Order Parameters", MinValue = 0)]
        public double TakeProfit { get; set; }

        [Parameter("Stop Loss (in pips)", Group = "Order Parameters", MinValue = 0)]
        public double StopLoss { get; set; }

        [Parameter("Fibonacci Take Profit (level)", Group = "Order Parameters", MinValue = 0)]
        public double FiboTakeProfit { get; set; }

        [Parameter("Fibonacci Stop Loss (level)", Group = "Order Parameters", MinValue = 0)]
        public double FiboStopLoss { get; set; }

        [Parameter("Multiplication Factor", Group = "Greek Settings", DefaultValue = 0.1, MinValue = 0)]
        public double MultFactor { get; set; }

        [Parameter("Add to Average", Group = "Greek Settings", DefaultValue = 0.0042, MinValue = 0)]
        public double AddFactor { get; set; }

        [Parameter("Chart Factor", Group = "Greek Settings", DefaultValue = 10, MinValue = 0)]
        public double ChartFactor { get; set; }

        [Parameter("Add to Total", Group = "Greek Settings", DefaultValue = 0.0014, MinValue = 0)]
        public double AddValue { get; set; }

        [Parameter("(+) Range", Group = "Greek Settings", DefaultValue = 0.0089, MinValue = 0)]
        public double PositiveRange { get; set; }

        [Parameter("(-) Range", Group = "Greek Settings", DefaultValue = 0.0089, MinValue = 0)]
        public double NegativeRange { get; set; }

        [Parameter("Show Fibonacci", Group = "Fibonacci Levels", DefaultValue = false)]
        public bool ShowFibo { get; set; }

        [Parameter("Show positive levels (Greek Level separate with , / Empty = on current price level)", Group = "Fibonacci Levels", DefaultValue = "")]
        public string PositiveLevelsToShow { get; set; }

        [Parameter("Show Negative levels (Greek Level separate with , / Empty = on current price level)", Group = "Fibonacci Levels", DefaultValue = "")]
        public string NegativeLevelsToShow { get; set; }

        [Parameter("Level (%)", Group = "Fibonacci Levels", MinValue = 0, DefaultValue = 0)]
        public double Level1 { get; set; }

        [Parameter("Level (%)", Group = "Fibonacci Levels", MinValue = 0, DefaultValue = 23.6)]
        public double Level2 { get; set; }

        [Parameter("Level (%)", Group = "Fibonacci Levels", MinValue = 0, DefaultValue = 38.2)]
        public double Level3 { get; set; }

        [Parameter("Level (%)", Group = "Fibonacci Levels", MinValue = 0, DefaultValue = 50)]
        public double Level4 { get; set; }

        [Parameter("Level (%)", Group = "Fibonacci Levels", MinValue = 0, DefaultValue = 61.8)]
        public double Level5 { get; set; }

        [Parameter("Level (%)", Group = "Fibonacci Levels", MinValue = 0, DefaultValue = 77)]
        public double Level6 { get; set; }

        [Parameter("Level (%)", Group = "Fibonacci Levels", MinValue = 0, DefaultValue = 100)]
        public double Level7 { get; set; }

        [Parameter("Level (%)", Group = "Fibonacci Levels", MinValue = 0, DefaultValue = 123.6)]
        public double Level8 { get; set; }

        [Parameter("Level (%)", Group = "Fibonacci Levels", MinValue = 0, DefaultValue = 150)]
        public double Level9 { get; set; }

        [Parameter("Level (%)", Group = "Fibonacci Levels", MinValue = 0, DefaultValue = 200)]
        public double Level10 { get; set; }

        [Parameter("Color", Group = "Fibonacci Visuals", DefaultValue = ColorSelector.Red)]
        public ColorSelector FiboColor { get; set; }

        [Parameter("Line Style", Group = "Fibonacci Visuals", DefaultValue = LineSelector.Solid)]
        public LineSelector FiboLineStyle { get; set; }

        [Parameter("Thickness", Group = "Fibonacci Visuals", MinValue = 1, DefaultValue = 1)]
        public int FiboThickness { get; set; }

        [Parameter("Drawing offset (in bars +/-)", Group = "Fibonacci Visuals", DefaultValue = 20)]
        public int FiboOffset { get; set; }

        [Parameter("Positive Color", Group = "Greek Visuals", DefaultValue = ColorSelector.Green)]
        public ColorSelector PositiveGreekColor { get; set; }

        [Parameter("Negative Color", Group = "Greek Visuals", DefaultValue = ColorSelector.Red)]
        public ColorSelector NegativeGreekColor { get; set; }

        [Parameter("Line Style", Group = "Greek Visuals", DefaultValue = LineSelector.Solid)]
        public LineSelector GreekLineStyle { get; set; }

        [Parameter("Thickness", Group = "Greek Visuals", MinValue = 1, DefaultValue = 1)]
        public int GreekThickness { get; set; }

        [Parameter("Drawing offset (in bars +/-)", Group = "Greek Visuals", DefaultValue = -20)]
        public int GreekOffset { get; set; }

        [Parameter("Hue", Group = "Global Visuals", DefaultValue = HueSelector.Main)]
        public HueSelector Hue { get; set; }

        public List<GreekLine> greeks;
        public List<string> greekNames;
        public Bars month;
        public double preOpen, preClose, preHigh, preLow;
        public double _passiveTop, _passiveBottom;
        public string _greekTopName, _greekBottomName;

        public enum DirectionSelector
        {
            LongOnly,
            ShortOnly,
            AllTrades
        }

        public enum HueSelector
        {
            Main,
            Light,
            Dark
        }

        public enum LineSelector
        {
            Solid,
            Lines,
            LinesDots,
            Dots,
            DotsRare,
            DotsVeryRare
        }

        public enum ColorSelector
        {
            Default,
            White,
            Black,
            Gray,
            Blue,
            Cyan,
            Red,
            Pink,
            Green,
            Orange,
            Yellow,
            Purple
        }

        protected override void OnStart()
        {
            Initialize();

            Positions.Opened += Positions_Opened;
            Positions.Closed += Positions_Closed;
        }

        private void Positions_Opened(PositionOpenedEventArgs obj)
        {
            if (obj.Position.TradeType == TradeType.Buy)
            {
                Print("Long Position Opened | " + obj.Position.Label);
                var line = greeks.Where(x => obj.Position.Label.Contains(x.Name)).FirstOrDefault();

                if (line != null)
                {
                    line.LongPositionOpen = true;
                    line.LongOrderActive = false;

                    Print("Long PositionOpen = " + line.LongPositionOpen + "| Long OrderActive = " + line.LongOrderActive);
                }

                if (ActiveRecovery)
                {
                    PlaceRecoveryOrder(obj.Position, obj.Position.Label);
                }
            }
            else
            {
                Print("Short Position Opened | " + obj.Position.Label);
                var line = greeks.Where(x => obj.Position.Label.Contains(x.Name)).FirstOrDefault();

                if (line != null)
                {
                    line.ShortPositionOpen = true;
                    line.ShortOrderActive = false;

                    Print("Short PositionOpen = " + line.ShortPositionOpen + "| Short OrderActive = " + line.ShortOrderActive);

                    if (ActiveRecovery)
                    {
                        PlaceRecoveryOrder(obj.Position, obj.Position.Label);
                    }
                }
            }
        }

        private void Positions_Closed(PositionClosedEventArgs obj)
        {
            if (obj.Position.TradeType == TradeType.Buy)
            {
                Print("Long Position Closed | " + obj.Position.Label);
                var line = greeks.Where(x => obj.Position.Label.Contains(x.Name)).FirstOrDefault();

                if (line != null)
                {
                    line.LongPositionOpen = false;
                    line.LongOrderActive = false;
                    Print("Long PositionOpen = " + line.LongPositionOpen + "| Long OrderActive = " + line.LongOrderActive);

                    PlaceTrades("Long");
                }

                if (ActiveRecovery)
                {
                    if (obj.Position.NetProfit > 0)
                    {
                        foreach (var ord in PendingOrders)
                        {
                            if (ord.Label.Contains(obj.Position.Label))
                                CancelPendingOrder(ord);
                        }
                    }
                }
            }
            else
            {
                Print("Short Position Closed | " + obj.Position.Label);
                var line = greeks.Where(x => obj.Position.Label.Contains(x.Name)).FirstOrDefault();

                if (line != null)
                {
                    line.ShortPositionOpen = false;
                    line.ShortOrderActive = false;
                    Print("Short PositionOpen = " + line.ShortPositionOpen + "| Short OrderActive = " + line.ShortOrderActive);

                    PlaceTrades("Short");
                }

                if (ActiveRecovery)
                {
                    if (obj.Position.NetProfit > 0)
                    {
                        foreach (var ord in PendingOrders)
                        {
                            if (ord.Label.Contains(obj.Position.Label))
                                CancelPendingOrder(ord);
                        }
                    }
                }
            }
        }

        protected void Initialize()
        {
            month = MarketData.GetBars(TimeFrame.Monthly);

            double high = month.HighPrices.Last(1) * MultFactor;
            double low = month.LowPrices.Last(1) * MultFactor;
            double close = month.ClosePrices.Last(1) * MultFactor;
            double open = month.OpenPrices.Last(1) * MultFactor;

            preHigh = high;
            preLow = low;
            preClose = close;
            preOpen = open;

            greeks = new List<GreekLine>();
            greekNames = new List<string>();
            greeks = GetGreekLines(open, close, high, low);

            PaintGreekLines(greeks, Bars.Count - 1);
            SetFibonaccis();

            if (ShowFibo)
                DrawFibonacciLevels();

            if (AutoActive)
            {
                if (TheDirection == DirectionSelector.AllTrades || TheDirection == DirectionSelector.LongOnly)
                {
                    PlaceTrades("Long");
                }

                if (TheDirection == DirectionSelector.AllTrades || TheDirection == DirectionSelector.ShortOnly)
                {
                    PlaceTrades("Short");
                }
            }
        }

        public void OnBar(int index)
        {
            double high = month.HighPrices.Last(1) * MultFactor;
            double low = month.LowPrices.Last(1) * MultFactor;
            double close = month.ClosePrices.Last(1) * MultFactor;
            double open = month.OpenPrices.Last(1) * MultFactor;

            if (open != preOpen || low != preLow)
            {

                greeks.Clear();
                greeks = GetGreekLines(open, close, high, low);
                greeks.Sort();

                PaintGreekLines(greeks, index);

                preHigh = high;
                preLow = low;
                preClose = close;
                preOpen = open;
            }

            if (PassiveCollection)
            {
                if (Bars.ClosePrices[index - 1] > _passiveTop)
                {
                    PassiveTrigger("Long", _greekTopName);
                }

                if (Bars.ClosePrices[index - 1] < _passiveBottom)
                {
                    PassiveTrigger("Bottom", _greekBottomName);
                }
            }
            
            //if(AutoActive)
            //PlaceTrades();
        }

        public void PassiveTrigger(string direction, string label)
        {

            double lots = Symbol.QuantityToVolumeInUnits(Volume);

            if (direction == "Long")
            {
                var pos = ExecuteMarketOrder(TradeType.Buy, SymbolName, lots, label, StopLoss, TakeProfit);
                if (pos.IsSuccessful)
                {
                    Print("Long Trade successfully placed");
                }
            }
            else
            {
                var pos = ExecuteMarketOrder(TradeType.Sell, SymbolName, lots, label, StopLoss, TakeProfit);
                if (pos.IsSuccessful)
                {
                    Print("Sell Trade successfully placed");
                }
            }
        }

        public void PaintGreekLines(List<GreekLine> greekLines, int index)
        {
            foreach (GreekLine line in greekLines)
            {
                Color clr = line.Prefix > 0 ? GetColor(PositiveGreekColor, Hue) : line.Prefix < 0 ? GetColor(NegativeGreekColor, Hue) : GetColor(PositiveGreekColor, Hue);
                string label = line.Prefix > 0 ? "+ " + line.Name : line.Prefix < 0 ? "- " + line.Name : "CS";
                string txtName = label + "TXT";
                double level = line.Level;

                Chart.DrawText(txtName, label, index + GreekOffset, level, clr);
                Chart.DrawHorizontalLine(label, level, clr, GreekThickness, GetLineStyle(GreekLineStyle));
            }
        }

        public List<GreekLine> GetGreekLines(double open, double close, double high, double low)
        {
            List<GreekLine> greeklevels = new List<GreekLine>();

            double average = ((open - high - low - close) / 2) * -1;
            average += AddFactor;
            double currentSwirl = average * ChartFactor;
            currentSwirl += AddValue;

            for (int i = 24; i > 0; i--)
            {
                double levelPlus = currentSwirl + PositiveRange * i;
                double levelMinus = currentSwirl - NegativeRange * i;
                string name = GetGreekName(i);

                GreekLine plusline = new GreekLine(name, levelPlus, 1);
                GreekLine minusline = new GreekLine(name, levelMinus, -1);

                greeklevels.Add(plusline);
                greeklevels.Add(minusline);
                greekNames.Add(plusline.Name);
            }

            GreekLine currentSwirlLine = new GreekLine("CS", currentSwirl, 1);
            greeklevels.Add(currentSwirlLine);

            List<GreekLine> sortedGreeks = greeklevels.OrderBy(o => o.Level).ToList();
            return sortedGreeks;
        }

        public string GetGreekName(int level)
        {
            switch (level)
            {
                case 1:
                    return "Alpha";
                case 2:
                    return "Beta";
                case 3:
                    return "Gamma";
                case 4:
                    return "Delta";
                case 5:
                    return "Epsilon";
                case 6:
                    return "Zita";
                case 7:
                    return "Eta";
                case 8:
                    return "Theta";
                case 9:
                    return "Iota";
                case 10:
                    return "Kappa";
                case 11:
                    return "Lamda";
                case 12:
                    return "Mi";
                case 13:
                    return "Ni";
                case 14:
                    return "Xi";
                case 15:
                    return "Omikron";
                case 16:
                    return "Pi";
                case 17:
                    return "Rho";
                case 18:
                    return "Sigma";
                case 19:
                    return "Taf";
                case 20:
                    return "Ipsilon";
                case 21:
                    return "Phi";
                case 22:
                    return "Chi";
                case 23:
                    return "Psi";
                case 24:
                    return "Omega";
                default:
                    return "";
            }
        }

        public void SetFibonaccis()
        {

            foreach (var greek in greeks)
            {
                var line = greeks.Where(x => x.Level > greek.Level).FirstOrDefault();

                if (line != null)
                {
                    double bottomLine = greek.Level;
                    double topLine = line.Level;

                    Fibonacci fib = new Fibonacci(this, bottomLine, topLine);

                    greek.Fib = fib;
                }
            }

        }

        public void DrawFibonacciLevels()
        {
            if (PositiveLevelsToShow == "" && NegativeLevelsToShow == "")
            {
                var greek = greeks.Where(x => x.Level <= Bars.ClosePrices.LastValue).LastOrDefault();

                if (greek != null)
                    DrawFibonacci(greek.Fib, greek, greek.Prefix);
            }
            else
            {
                string[] poslvls = PositiveLevelsToShow.Split(',');
                string[] neglvls = PositiveLevelsToShow.Split(',');

                if (poslvls != null)
                {
                    foreach (string lev in poslvls)
                    {
                        var greek = greeks.Where(x => x.Name == lev.Trim() && x.Prefix == 1).FirstOrDefault();

                        if (greek != null)
                            DrawFibonacci(greek.Fib, greek, 1);
                    }
                }

                if (neglvls != null)
                {
                    foreach (string lev in poslvls)
                    {
                        var greek = greeks.Where(x => x.Name == lev.Trim() && x.Prefix == -1).FirstOrDefault();

                        if (greek != null)
                            DrawFibonacci(greek.Fib, greek, -1);
                    }
                }

            }

        }

        public void DrawFibonacci(Fibonacci fibo, GreekLine greek, int prefix)
        {
            double[,] pairs = new double[,]
            {
               { Level1, fibo.FiboLevel1},
               { Level2, fibo.FiboLevel2},
               { Level3, fibo.FiboLevel3},
               { Level4, fibo.FiboLevel4},
               { Level5, fibo.FiboLevel5},
               { Level6, fibo.FiboLevel6},
               { Level7, fibo.FiboLevel7},
               { Level8, fibo.FiboLevel8},
               { Level9, fibo.FiboLevel9},
               { Level10, fibo.FiboLevel10}
            };

            for (int i = 0; i < 10; i++)
            {
                string name = greek.Name + " -- " + pairs[i, 0] + prefix;
                string pr = prefix == 1 ? "+" : "-";
                string text = "(" + pr + ")" + greek.Name + " -- " + pairs[i, 0];
                double lvl = pairs[i, 1];

                Chart.DrawHorizontalLine(name, lvl, GetColor(FiboColor, Hue), FiboThickness, GetLineStyle(FiboLineStyle));
                Chart.DrawText(name + " text", text + "%", Bars.OpenTimes.Count + FiboOffset, lvl + 0.5 * Symbol.PipSize, GetColor(FiboColor, Hue));
            }
        }

        public void PlaceTrades(string direction)
        {
            Print("Place trades function triggered");

            var greekBot = greeks.Where(x => x.Level <= Bars.ClosePrices.LastValue).LastOrDefault();
            var greekTop = greeks.Where(x => x.Level >= Bars.ClosePrices.LastValue).FirstOrDefault();
            double lots = Symbol.QuantityToVolumeInUnits(Volume);

            if (direction == "Long")
            {
                if (!greekTop.LongOrderActive && !greekTop.LongPositionOpen)
                {
                    double entry = Math.Round(greekTop.Level + BuyDistance * Symbol.PipSize, Symbol.Digits);
                    string label = greekTop.Prefix.ToString() + greekTop.Name;
                    _passiveTop = entry;
                    _greekTopName = label;

                    if (!PassiveCollection)
                    {
                        var ord = PlaceStopOrder(TradeType.Buy, SymbolName, lots, entry, label, StopLoss, TakeProfit);
                        if (ord.IsSuccessful)
                        {
                            greekTop.LongOrderActive = true;
                            Print("Long Order successfully placed");
                            Print("Long OrderActive = " + greekTop.LongOrderActive);
                        }
                    }
                }

                if (!greekBot.LongOrderActive && !greekBot.LongPositionOpen)
                {
                    double entry = Math.Round(greekBot.Level + BuyDistance * Symbol.PipSize, Symbol.Digits);
                    string label = greekBot.Prefix.ToString() + greekBot.Name;
                    _passiveBottom = entry;
                    _greekBottomName = label;

                    if (!PassiveCollection)
                    {
                        var ord = PlaceLimitOrder(TradeType.Buy, SymbolName, lots, entry, label, StopLoss, TakeProfit);
                        if (ord.IsSuccessful)
                        {
                            greekBot.LongOrderActive = true;
                            Print("Long Order successfully placed");
                            Print("Long OrderActive = " + greekBot.LongOrderActive);
                        }
                    }
                }
            }
            else
            {
                if (!greekTop.ShortOrderActive && !greekTop.ShortPositionOpen)
                {
                    double entry = Math.Round(greekTop.Level - SellDistance * Symbol.PipSize, Symbol.Digits);
                    string label = greekTop.Prefix.ToString() + greekTop.Name;
                    _passiveTop = entry;
                    _greekTopName = label;

                    if (!PassiveCollection)
                    {
                        var ord = PlaceLimitOrder(TradeType.Sell, SymbolName, lots, entry, label, StopLoss, TakeProfit);

                        if (ord.IsSuccessful)
                        {
                            greekTop.ShortOrderActive = true;
                            Print("Short Order successfully placed");
                            Print("Short OrderActive = " + greekTop.ShortOrderActive);
                        }
                    }
                }

                if (!greekBot.ShortOrderActive && !greekBot.ShortPositionOpen)
                {
                    double entry = Math.Round(greekBot.Level - SellDistance * Symbol.PipSize, Symbol.Digits);
                    string label = greekBot.Prefix.ToString() + greekBot.Name;
                    _passiveBottom = entry;
                    _greekBottomName = label;

                    if (!PassiveCollection)
                    {
                        var ord = PlaceStopOrder(TradeType.Sell, SymbolName, lots, entry, label, StopLoss, TakeProfit);
                        if (ord.IsSuccessful)
                        {
                            greekBot.ShortOrderActive = true;
                            Print("Short Order successfully placed");
                            Print("Short OrderActive = " + greekBot.ShortOrderActive);
                        }
                    }
                }
            }
        }

        public class GreekLine : TFTBouncerv2
        {
            public string Name { get; set; }
            public double Level { get; set; }
            public int Prefix { get; set; }
            public Fibonacci Fib { get; set; }

            public bool LongOrderActive { get; set; }
            public bool LongPositionOpen { get; set; }
            public bool ShortOrderActive { get; set; }
            public bool ShortPositionOpen { get; set; }

            public GreekLine(string name, double level, int prefix)
            {
                Name = name;
                Level = level;
                Prefix = prefix;
                LongOrderActive = false;
                ShortPositionOpen = false;
                ShortOrderActive = false;
                LongPositionOpen = false;
            }
        }

        public PendingOrder PlaceRecoveryOrder(Position pos, string label)
        {
            double lots = Symbol.QuantityToVolumeInUnits(Volume);
            int iteration = Int32.Parse(pos.Comment) + 1;
            string getIndex = new String(pos.Label.TakeWhile(Char.IsDigit).ToArray());
            string _label = label + " | " + "Recovery";
            TradeType direction = pos.TradeType == TradeType.Buy ? TradeType.Sell : TradeType.Buy;
            double distance = Math.Round(RecoveryDistance * Symbol.PipSize, Symbol.Digits);
            double entry = pos.TradeType == TradeType.Buy ? pos.EntryPrice - distance : pos.EntryPrice + distance;
            double takeProfit = StopLoss + (iteration - 1) * StopLoss;

            var ord = PlaceStopOrder(direction, SymbolName, lots, entry, _label, StopLoss, takeProfit, null, iteration.ToString());

            if (ord.IsSuccessful)
                Print("Recovery Order Placed");

            return ord.PendingOrder;
        }

        private Color GetColor(ColorSelector color, HueSelector hue)
        {

            switch (color)
            {

                case ColorSelector.Black:
                    {
                        return Color.Black;
                    }
                case ColorSelector.White:
                    {
                        return Color.White;
                    }
                case ColorSelector.Gray:
                    {
                        if (hue.Equals(HueSelector.Dark))
                        {
                            return Color.DarkGray;
                        }
                        else if (hue.Equals(HueSelector.Light))
                        {
                            return Color.LightGray;
                        }
                        else
                        {
                            return Color.Gray;
                        }
                    }
                case ColorSelector.Blue:
                    {
                        if (hue.Equals(HueSelector.Dark))
                        {
                            return Color.DarkBlue;
                        }
                        else if (hue.Equals(HueSelector.Light))
                        {
                            return Color.LightBlue;
                        }
                        else
                        {
                            return Color.Blue;
                        }
                    }
                case ColorSelector.Cyan:
                    {
                        if (hue.Equals(HueSelector.Dark))
                        {
                            return Color.DarkCyan;
                        }
                        else if (hue.Equals(HueSelector.Light))
                        {
                            return Color.LightCyan;
                        }
                        else
                        {
                            return Color.Cyan;
                        }
                    }
                case ColorSelector.Red:
                    {
                        if (hue.Equals(HueSelector.Dark))
                        {
                            return Color.DarkRed;
                        }
                        else if (hue.Equals(HueSelector.Light))
                        {
                            return Color.OrangeRed;
                        }
                        else
                        {
                            return Color.Red;
                        }
                    }
                case ColorSelector.Pink:
                    {
                        if (hue.Equals(HueSelector.Dark))
                        {
                            return Color.DarkMagenta;
                        }
                        else if (hue.Equals(HueSelector.Light))
                        {
                            return Color.LightPink;
                        }
                        else
                        {
                            return Color.Pink;
                        }
                    }
                case ColorSelector.Green:
                    {
                        if (hue.Equals(HueSelector.Dark))
                        {
                            return Color.DarkGreen;
                        }
                        else if (hue.Equals(HueSelector.Light))
                        {
                            return Color.LightGreen;
                        }
                        else
                        {
                            return Color.Green;
                        }
                    }
                case ColorSelector.Yellow:
                    {
                        if (hue.Equals(HueSelector.Dark))
                        {
                            return Color.Gold;
                        }
                        else if (hue.Equals(HueSelector.Light))
                        {
                            return Color.LightYellow;
                        }
                        else
                        {
                            return Color.Yellow;
                        }
                    }
                case ColorSelector.Purple:
                    {
                        if (hue.Equals(HueSelector.Dark))
                        {
                            return Color.DarkViolet;
                        }
                        else if (hue.Equals(HueSelector.Light))
                        {
                            return Color.Violet;
                        }
                        else
                        {
                            return Color.Purple;
                        }
                    }
                case ColorSelector.Default:
                    {
                        if (Application.ColorTheme.Equals(ColorTheme.Dark))
                            return Color.White;
                        else
                            return Color.Black;
                    }
                default:
                    {
                        if (Application.ColorTheme.Equals(ColorTheme.Dark))
                            return Color.White;
                        else
                            return Color.Black;
                    }
            }

        }

        private LineStyle GetLineStyle(LineSelector line)
        {

            switch (line)
            {
                case LineSelector.Solid:
                    return LineStyle.Solid;
                case LineSelector.Lines:
                    return LineStyle.Lines;
                case LineSelector.LinesDots:
                    return LineStyle.LinesDots;
                case LineSelector.Dots:
                    return LineStyle.Dots;
                case LineSelector.DotsRare:
                    return LineStyle.DotsRare;
                case LineSelector.DotsVeryRare:
                    return LineStyle.DotsVeryRare;
                default:
                    return LineStyle.Solid;
            }

        }

        public class Fibonacci : TFTBouncerv2
        {

            private readonly TFTBouncerv2 _robot;

            public double Base { get; set; }
            public double Top { get; set; }

            public Fibonacci(TFTBouncerv2 robot, double levelBase, double levelTop)
            {
                _robot = robot;
                Base = levelBase;
                Top = levelTop;

                FiboLevel1 = this.FiboLevel1;
                FiboLevel2 = this.FiboLevel2;
                FiboLevel3 = this.FiboLevel3;
                FiboLevel4 = this.FiboLevel4;
                FiboLevel5 = this.FiboLevel5;
                FiboLevel6 = this.FiboLevel6;
                FiboLevel7 = this.FiboLevel7;
                FiboLevel8 = this.FiboLevel8;
                FiboLevel9 = this.FiboLevel9;
                FiboLevel10 = this.FiboLevel10;
            }

            private double _FiboLevel1;
            public double FiboLevel1
            {
                get { return _FiboLevel1; }
                set { _FiboLevel1 = SetFiboLevel(_robot, 1); }
            }

            private double _FiboLevel2;
            public double FiboLevel2
            {
                get { return _FiboLevel2; }
                set { _FiboLevel2 = SetFiboLevel(_robot, 2); }
            }

            private double _FiboLevel3;
            public double FiboLevel3
            {
                get { return _FiboLevel3; }
                set { _FiboLevel3 = SetFiboLevel(_robot, 3); }
            }

            private double _FiboLevel4;
            public double FiboLevel4
            {
                get { return _FiboLevel4; }
                set { _FiboLevel4 = SetFiboLevel(_robot, 4); }
            }


            private double _FiboLevel5;
            public double FiboLevel5
            {
                get { return _FiboLevel5; }
                set { _FiboLevel5 = SetFiboLevel(_robot, 5); }
            }

            private double _FiboLevel6;
            public double FiboLevel6
            {
                get { return _FiboLevel6; }
                set { _FiboLevel6 = SetFiboLevel(_robot, 6); }
            }

            private double _FiboLevel7;
            public double FiboLevel7
            {
                get { return _FiboLevel7; }
                set { _FiboLevel7 = SetFiboLevel(_robot, 7); }
            }

            private double _FiboLevel8;
            public double FiboLevel8
            {
                get { return _FiboLevel8; }
                set { _FiboLevel8 = SetFiboLevel(_robot, 8); }
            }

            private double _FiboLevel9;
            public double FiboLevel9
            {
                get { return _FiboLevel9; }
                set { _FiboLevel9 = SetFiboLevel(_robot, 9); }
            }

            private double _FiboLevel10;
            public double FiboLevel10
            {
                get { return _FiboLevel10; }
                set { _FiboLevel10 = SetFiboLevel(_robot, 10); }
            }

            private double SetFiboLevel(TFTBouncerv2 bot, int level)
            {
                double fiboLevel;
                var distance = this.Top - this.Base;

                if (level == 1)
                    fiboLevel = bot.Level1;
                else if (level == 2)
                    fiboLevel = bot.Level2;
                else if (level == 3)
                    fiboLevel = bot.Level3;
                else if (level == 4)
                    fiboLevel = bot.Level4;
                else if (level == 5)
                    fiboLevel = bot.Level5;
                else if (level == 6)
                    fiboLevel = bot.Level6;
                else if (level == 7)
                    fiboLevel = bot.Level7;
                else if (level == 8)
                    fiboLevel = bot.Level8;
                else if (level == 9)
                    fiboLevel = bot.Level9;
                else
                    fiboLevel = bot.Level10;

                var lev = fiboLevel / 100 * distance;

                return Base + lev;
            }

        }

    }

}