using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using cAlgo.API;
using cAlgo.API.Collections;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using System.Threading.Tasks;
using System.Diagnostics;

namespace cAlgo.Robots
{
    [Robot(AccessRights = AccessRights.None)]
    public class MartingaleEAstop : Robot
    {
        public enum SymbolNameType
        {
            NONE,
            AUDCAD,
            AUDCHF,
            AUDJPY,
            AUDNZD,
            AUDUSD,
            CADCHF,
            CADJPY,
            CHFJPY,
            EURAUD,
            EURCAD,
            EURCHF,
            EURCZK,
            EURDKK,
            EURGBP,
            EURHKD,
            EURHUF,
            EURJPY,
            EURMXN,
            EURNOK,
            EURNZD,
            EURPLN,
            EURSEK,
            EURTRY,
            EURUSD,
            EURZAR,
            GBPAUD,
            GBPCAD,
            GBPCHF,
            GBPJPY,
            GBPNOK,
            GBPNZD,
            GBPUSD,
            GBPZAR,
            NZDCAD,
            NZDCHF,
            NZDSGD,
            NZDUSD,
            SGDJPY,
            USDCAD,
            USDCHF,
            USDCNH,
            USDDKK,
            USDHUF,
            USDJPY,
            USDMXN,
            USDNOK,
            USDPLN,
            USDSEK,
            USDTRY,
            USDZAR,
            XAUUSD,

        }

        #region SymbolInput

        [Parameter("Symbol 1", Group = "Pairs", DefaultValue = SymbolNameType.NONE)]
        public SymbolNameType S1Symbol { get; set; }

        [Parameter("Symbol 2", Group = "Pairs", DefaultValue = SymbolNameType.NONE)]
        public SymbolNameType S2Symbol { get; set; }

        [Parameter("Symbol 3", Group = "Pairs", DefaultValue = SymbolNameType.NONE)]
        public SymbolNameType S3Symbol { get; set; }

        [Parameter("Symbol 4", Group = "Pairs", DefaultValue = SymbolNameType.NONE)]
        public SymbolNameType S4Symbol { get; set; }

        [Parameter("Symbol 5", Group = "Pairs", DefaultValue = SymbolNameType.NONE)]
        public SymbolNameType S5Symbol { get; set; }

        [Parameter("Symbol 6", Group = "Pairs", DefaultValue = SymbolNameType.NONE)]
        public SymbolNameType S6Symbol { get; set; }

        [Parameter("Symbol 7", Group = "Pairs", DefaultValue = SymbolNameType.NONE)]
        public SymbolNameType S7Symbol { get; set; }

        [Parameter("Symbol 8", Group = "Pairs", DefaultValue = SymbolNameType.NONE)]
        public SymbolNameType S8Symbol { get; set; }

        [Parameter("Symbol 9", Group = "Pairs", DefaultValue = SymbolNameType.NONE)]
        public SymbolNameType S9Symbol { get; set; }

        [Parameter("Symbol 10", Group = "Pairs", DefaultValue = SymbolNameType.NONE)]
        public SymbolNameType S10Symbol { get; set; }



        //[Parameter("Maximum Broker Lotsize", Group = "General Settings", DefaultValue = 50, MinValue = 1)]
        //public int MaximumLotSize { get; set; }

        [Parameter("Target profit in percent", Group = "General Settings", DefaultValue = 5, MinValue = 0)]
        public double TargetProfitInput { get; set; }

        [Parameter("Accepted loss in percent", Group = "General Settings", DefaultValue = 5, MinValue = 0)]
        public double AcceptedLossInput { get; set; }


        [Parameter("Opening direction", Group = "Symbol Settings", DefaultValue = TradeType.Buy)]
        public TradeType OpeningDirection { get; set; }

        [Parameter("Opening lot size", Group = "Symbol Settings", DefaultValue = 0.1, MinValue = 0)]
        public double OpeningLotSize { get; set; }

        [Parameter("Multiple Instance Trading", Group = "Symbol Settings", DefaultValue = 1, MinValue = 1)]
        public int MultipleInstance { get; set; }

        [Parameter("Hedge level in pip", Group = "Symbol Settings", DefaultValue = 5, MinValue = 0)]
        public double HedgeLevelPip { get; set; }

        [Parameter("Opening TP in pips", Group = "Symbol Settings", DefaultValue = 20, MinValue = 0)]
        public double OpeningTPpip { get; set; }

        [Parameter("Opening SL in pips", Group = "Symbol Settings", DefaultValue = 20, MinValue = 0)]
        public double OpeningSLPip { get; set; }

        [Parameter("First Multiplier", Group = "Symbol Settings", DefaultValue = 1.5, MinValue = 0)]
        public double FirstMultiplier { get; set; }

        [Parameter("Second Multiplier", Group = "Symbol Settings", DefaultValue = 1.5, MinValue = 0)]
        public double SecondMultiplier { get; set; }



        [Parameter("Reduced TP at x level order", Group = "Safety Settings", DefaultValue = 10, MinValue = 0)]
        public int AmountOfOrderBeforeReducedTP { get; set; }

        [Parameter("Reduced TP in pips", Group = "Safety Settings", DefaultValue = 15, MinValue = 0)]
        public double ReducedTP { get; set; }

        [Parameter("Breakeven at x level order", Group = "Safety Settings", DefaultValue = 10, MinValue = 0)]
        public int AmountOfOrderBeforeBreakeven { get; set; }

        [Parameter("AllForOne OneForAll  at x level order", Group = "Safety Settings", DefaultValue = 10, MinValue = 0)]
        public int AllForOne { get; set; }

        [Parameter("CloseAll at x level order", Group = "Safety Settings", DefaultValue = 10, MinValue = 0)]
        public int CloseALl { get; set; }


        #endregion SymbolInput


        private List<Symbol> allSymbol;

        private string _levelNameStart;
        private double _accountBalance;
        private string _label;
        private int _numberOfOrderPlaced;
        private bool _isCheckedLock;
        private int _iterations;
        private bool _isCloseAllPositionLock;
        private bool _activateOneForAll;
        private Dictionary<string,double> _level;
        private Dictionary<string, int> _levelLocked;
        private List<string> _reduced;
        private Dictionary<string, int> _instance;
        private List<string> _breakEven;

        protected override void OnStart()
        {


            PendingOrders.Filled += OnPendingOrderFilled;
            Positions.Closed += OnPostionsClosed;
            //var result = System.Diagnostics.Debugger.Launch();
            _iterations = 0;
            Restart();
            
            
            

        }

   
        protected override void OnTick()
        {
            if (allSymbol.Count < 1)
            {
                return;
            }

            if (!_isCheckedLock)
            {
                CheckTargetReached();
            }

            if (_activateOneForAll)
            {
                ExecuteAllForOneOneForAll();
            }
            if(_breakEven.Count > 0)
            {
                var breakEvenList = new List<string>(_breakEven);
                foreach (var label in breakEvenList)
                {
                    var allPositions = Positions.Where(x => x.Label.Contains(label));
                    var allOrders = PendingOrders.Where(x => x.Label.Contains(label));
;                    if(allPositions.Sum(x=>x.NetProfit) >= 0)
                    {
                        var symbolName = allPositions.First().SymbolName;
                        var symbol = Symbols.GetSymbol(symbolName);
                        var volume = symbol.QuantityToVolumeInUnits(OpeningLotSize);
                        var normalizedVolume = symbol.NormalizeVolumeInUnits(volume, RoundingMode.Up);
                        foreach (var pos in allPositions)
                        {
                            ClosePositionAsync(pos);
                        }
                        foreach(var order in allOrders)
                        {
                            CancelPendingOrderAsync(order);
                        }

                        var keyTodelete = _level.Keys.Where(k => k.Contains(label)).ToList();

                        foreach (var item in keyTodelete)
                        {
                            if (item != label + "0")
                            {
                                _level.Remove(item);
                            }
                        }

                        var level = _level[label+"0"];

                        if (level > symbol.Bid)
                        {
                            for (var i = 0; i < MultipleInstance; i++)
                            {
                                PlaceStopOrderAsync(TradeType.Buy, symbolName, normalizedVolume, level, label + 0, null, null, null, null, false, StopTriggerMethod.Trade, StopTriggerMethod.Opposite);
                            }

                        }
                        else if (level < symbol.Bid)
                        {
                            for (var i = 0; i < MultipleInstance; i++)
                            {
                                PlaceStopOrderAsync(TradeType.Sell, symbolName, normalizedVolume, level - HedgeLevelPip * symbol.PipSize, label + 0, null, null, null, null, false, StopTriggerMethod.Opposite, StopTriggerMethod.Trade);
                            }
                        }
                        _breakEven.Remove(label);
                    }
                }
            }

        }

        protected override void OnStop()
        {
            foreach (var pos in Positions)
            {
                if (pos.Label.Contains("BOT"))
                {
                    ClosePositionAsync(pos);

                }
            }
            foreach (var order in PendingOrders)
            {
                if (order.Label.Contains("BOT"))
                {
                    CancelPendingOrderAsync(order);
                }
            }
        }


        private void ExecuteInitialOrder(Symbol symbol)
        {
            var label = FormatLabel(symbol.Name, _levelNameStart, 0);
            var volume = symbol.QuantityToVolumeInUnits(OpeningLotSize);
            var normalizedVolume = symbol.NormalizeVolumeInUnits(volume, RoundingMode.Up);
            var bidPrice = symbol.Bid;

            for (var i = 0; i < MultipleInstance; i++)
            {
                if (OpeningDirection == TradeType.Buy)
                {
                    PlaceStopOrderAsync(OpeningDirection, symbol.Name, normalizedVolume, bidPrice, label, null, null, null, null, false, StopTriggerMethod.Trade, StopTriggerMethod.Opposite);

                }
                else
                {
                    PlaceStopOrderAsync(OpeningDirection, symbol.Name, normalizedVolume, bidPrice, label, null, null, null, null, false, StopTriggerMethod.Opposite, StopTriggerMethod.Trade);

                }
            }
            if (OpeningDirection == TradeType.Sell)
            {
                _level[label] = Math.Round(bidPrice + HedgeLevelPip * symbol.PipSize, symbol.Digits);


            }
            else
            {
                _level[label] = bidPrice;


            }


        }
        private void OnPostionsClosed(PositionClosedEventArgs args)
        {
            if (!args.Position.Label.Contains("BOT"))
            {
                return;
            }
            var label = GetLevelWithoutPositionLabel(args.Position.Label);
            if (_levelLocked.ContainsKey(label+0)) 
            {
                return;
            }

            if (args.Reason==PositionCloseReason.TakeProfit || args.Reason == PositionCloseReason.StopLoss)
            {

                //CloseLevelPositions(label);
                CancelLevelOrders(label);

                var level = _level[label + 0];
                var keyTodelete = _level.Keys.Where(k => k.Contains(label)).ToList();


                foreach(var item in keyTodelete)
                {
                    if(item != label + "0")
                    {
                        _level.Remove(item);
                    }
                }
                var symbol = Symbols.GetSymbol(args.Position.SymbolName);
                var volume = symbol.QuantityToVolumeInUnits(OpeningLotSize);
                var normalizedVolume = symbol.NormalizeVolumeInUnits(volume, RoundingMode.Up);

                if (args.Reason == PositionCloseReason.TakeProfit)
                {
                    if(level>= args.Position.TakeProfit)
                    {
                        for (var i = 0; i < MultipleInstance; i++)
                        {
                            PlaceStopOrderAsync(TradeType.Buy, args.Position.SymbolName, normalizedVolume, level, label + 0,null,null,null,null,false, StopTriggerMethod.Trade, StopTriggerMethod.Opposite);
                        }
                        
                    }
                    else if(level< args.Position.TakeProfit)
                    {
                        for (var i = 0; i < MultipleInstance; i++)
                        {
                            PlaceStopOrderAsync(TradeType.Sell, args.Position.SymbolName, normalizedVolume, level - HedgeLevelPip * symbol.PipSize, label + 0, null, null, null, null, false, StopTriggerMethod.Opposite, StopTriggerMethod.Trade);
                        }
                    }
                }
                else if(args.Reason == PositionCloseReason.StopLoss)
                {
                    if (level >= args.Position.StopLoss)
                    {
                        for (var i = 0; i < MultipleInstance; i++)
                        {
                            PlaceStopOrderAsync(TradeType.Buy, args.Position.SymbolName, normalizedVolume, level, label + 0, null, null, null, null, false, StopTriggerMethod.Trade, StopTriggerMethod.Opposite);
                        }
                    }
                    else if (level < args.Position.StopLoss)
                    {
                        for (var i = 0; i < MultipleInstance; i++)
                        {
                            PlaceStopOrderAsync(TradeType.Sell, args.Position.SymbolName, normalizedVolume, level - HedgeLevelPip * symbol.PipSize, label + 0, null, null, null, null, false, StopTriggerMethod.Opposite, StopTriggerMethod.Trade);
                        }
                    }
                }

                _levelLocked.Add(label+0,MultipleInstance);

                

            }
        }

        private void OnPendingOrderFilled(PendingOrderFilledEventArgs args)
        {
            if (!args.Position.Label.Contains("BOT"))
            {
                return;
            }
            if (!_instance.ContainsKey(args.Position.Label))
            {
                _instance[args.Position.Label] = 0;
            }
            _instance[args.Position.Label]++;

            var symbol = Symbols.GetSymbol(args.Position.SymbolName);

            bool isBuy = args.Position.TradeType == TradeType.Buy;
            var orderLabel = args.Position.Label.Split("_").ToList();
            var levelId = orderLabel[2];
            _ = int.TryParse(orderLabel.Last(), out int positionCount);
            _ = int.TryParse(levelId, out int levelIdInt);
            var levelLabel = FormatLabel(args.Position.SymbolName, levelId, 0);
            var allPositionsLevelLabel = FormatLabel(args.Position.SymbolName, levelId);

            var reverseDirection = isBuy ? TradeType.Sell : TradeType.Buy;
            var volume = symbol.NormalizeVolumeInUnits(args.Position.VolumeInUnits * SecondMultiplier, RoundingMode.Up);
            var bidPrice = symbol.Bid;
            var levelPrice = bidPrice;


            double allCostPips = GetAllCostPips(symbol, levelLabel, allPositionsLevelLabel);

            if (_level.ContainsKey(levelLabel))
            {
                levelPrice = _level[levelLabel];
            }


            var hedgePrice = isBuy? levelPrice - HedgeLevelPip * symbol.PipSize: levelPrice;


            hedgePrice = Math.Round(hedgePrice, symbol.Digits);


            // Check safety feautures
            if (positionCount >= AmountOfOrderBeforeReducedTP && positionCount < AmountOfOrderBeforeBreakeven)
            {

                if (_instance[args.Position.Label] >= MultipleInstance)
                {
                    ExecuteReduceTargetProfit(symbol, levelIdInt);

                }
            }

            if (positionCount >= AmountOfOrderBeforeBreakeven)
            {
                if (_instance[args.Position.Label] >= MultipleInstance)
                {
     
                    if (!_breakEven.Contains(allPositionsLevelLabel))
                    {
                        _breakEven.Add(allPositionsLevelLabel);

                    }
                }
            }


            if (positionCount == AllForOne)
            {
                ExecuteAllForOneOneForAll();
            }

            if (positionCount == CloseALl)
            {
                CloseAllPositions();
            }
            var newPositionCount = positionCount + 1;


            if (positionCount == 0)
            {
                if (_levelLocked.ContainsKey(levelLabel))
                {
                    _levelLocked[levelLabel]--;
                    if (_levelLocked[levelLabel] <= 0)
                    {
                        _levelLocked.Remove(levelLabel);
                        _reduced.Remove(levelLabel);
                    }
                }

                else
                {
                    var volumeInUnit = symbol.QuantityToVolumeInUnits(OpeningLotSize);
                    var normalizedVolume = symbol.NormalizeVolumeInUnits(volumeInUnit, RoundingMode.Up);
                    var targetPrice = isBuy ? levelPrice + (OpeningTPpip + allCostPips) * symbol.PipSize : levelPrice - (OpeningTPpip + allCostPips) * symbol.PipSize;
                    var stopPrice = isBuy ? levelPrice - (OpeningSLPip + HedgeLevelPip + allCostPips) * symbol.PipSize : levelPrice + (OpeningSLPip + HedgeLevelPip + allCostPips) * symbol.PipSize;


                    if (OpeningDirection == TradeType.Sell && levelIdInt == 0)
                    {
                        targetPrice = levelPrice - (OpeningTPpip + HedgeLevelPip + allCostPips) * symbol.PipSize;
                        stopPrice = levelPrice + (OpeningSLPip + allCostPips) * symbol.PipSize;
                    }

                    targetPrice = Math.Round(targetPrice, symbol.Digits);
                    stopPrice = Math.Round(stopPrice, symbol.Digits);


                    int assignedLevelId = isBuy ? levelIdInt + 1 : levelIdInt - 1;

                    if (levelIdInt == 0)
                    {
                        assignedLevelId = reverseDirection == TradeType.Buy ? levelIdInt + 1 : levelIdInt - 1;
                        levelLabel = FormatLabel(symbol.Name, assignedLevelId.ToString(), 0);
                        if (_level.ContainsKey(levelLabel))
                        {
                            stopPrice = _level[levelLabel];
                        }
                        if (reverseDirection == TradeType.Buy)
                        {
                            PlaceStopOrderAsync(reverseDirection, symbol.Name, normalizedVolume, stopPrice, levelLabel, null, null, null, null, false, StopTriggerMethod.Trade, StopTriggerMethod.Opposite);
                        }
                        else
                        {
                            PlaceStopOrderAsync(reverseDirection, symbol.Name, normalizedVolume, stopPrice, levelLabel, null, null, null, null, false, StopTriggerMethod.Opposite, StopTriggerMethod.Trade);
     
                        }
                        AddLevel(levelLabel, stopPrice);

                    }

                    assignedLevelId = isBuy ? levelIdInt + 1 : levelIdInt - 1;
                    levelLabel = FormatLabel(symbol.Name, assignedLevelId.ToString(), 0);
                    if (_level.ContainsKey(levelLabel))
                    {
                        targetPrice = _level[levelLabel];
                    }
                    if (args.Position.TradeType == TradeType.Buy)
                    {
                        PlaceStopOrderAsync(args.Position.TradeType, symbol.Name, normalizedVolume, targetPrice, levelLabel, null, null, null, null, false, StopTriggerMethod.Trade, StopTriggerMethod.Opposite);
                    
                    }
                    else
                    {
                        PlaceStopOrderAsync(args.Position.TradeType, symbol.Name, normalizedVolume, targetPrice, levelLabel, null, null, null, null, false, StopTriggerMethod.Opposite, StopTriggerMethod.Trade);
 
                    }

                    AddLevel(levelLabel, targetPrice);


                    volume = symbol.NormalizeVolumeInUnits(args.Position.VolumeInUnits * FirstMultiplier, RoundingMode.Up);
                }


            }


            string label = FormatLabel(args.Position.SymbolName, levelId, newPositionCount);
            if (_level.ContainsKey(label))
            {
                hedgePrice = _level[label];
            }
            if (reverseDirection == TradeType.Buy)
            {
                PlaceStopOrderAsync(reverseDirection, args.Position.SymbolName, volume, hedgePrice, label, null, null, null, null, false, StopTriggerMethod.Trade, StopTriggerMethod.Opposite);

            }
            else
            {
                PlaceStopOrderAsync(reverseDirection, args.Position.SymbolName, volume, hedgePrice, label, null, null, null, null, false, StopTriggerMethod.Opposite, StopTriggerMethod.Trade);
            }

            AddLevel(label, hedgePrice);

            var orders = PendingOrders.Where(x => x.Label.Equals(FormatLabel(args.Position.SymbolName, levelIdInt + 1, 0)) || x.Label.Equals(FormatLabel(args.Position.SymbolName, levelIdInt - 1, 0))).ToList();

            if (positionCount >= AmountOfOrderBeforeReducedTP || positionCount >= AmountOfOrderBeforeBreakeven)
            {
                if (_instance[args.Position.Label] >= MultipleInstance)
                {
                    _instance.Remove(args.Position.Label);
                }
                return;
            }

            if (positionCount != 0 && _instance[args.Position.Label] >= MultipleInstance)
            {

                AddCommissions(symbol, levelLabel, allCostPips, orders);
                _instance.Remove(args.Position.Label);
            }


        }

        private double GetAllCostPips(Symbol symbol, string levelLabel, string allPositionsLevelLabel)
        {
            var rate = 1 / CheckRate(symbol);
            var allPositionsInLevel = Positions.Where(x => x.Label.Contains(allPositionsLevelLabel));
            var sumCommissions = allPositionsInLevel.Sum(x => Math.Abs(x.Commissions * 2)) * rate;
            var sumSpread = allPositionsInLevel.Where(x => x.TradeType == TradeType.Buy).Sum(x => (x.EntryPrice - _level[levelLabel]) / symbol.PipSize * x.VolumeInUnits * symbol.PipValue);
            var sumVolumeValue = allPositionsInLevel.Sum(x => x.TradeType == TradeType.Buy ? x.VolumeInUnits * symbol.PipValue : -x.VolumeInUnits * symbol.PipValue);
            var allCostPips = Math.Abs((sumCommissions + sumSpread) / sumVolumeValue);
            return allCostPips;
        }

        private void AddCommissions(Symbol symbol, string currentLabel, double allCostPips, List<PendingOrder> orders)
        {
            foreach (var order in orders)
            {
                
                var levelPrice = _level[currentLabel];
                if (order.TradeType == TradeType.Buy)
                {
                    var price = Math.Round(levelPrice + (OpeningTPpip + allCostPips)*symbol.PipSize, symbol.Digits);
                    order.ModifyTargetPrice(price);
                    _level[order.Label]= price;
                }
                else if (order.TradeType == TradeType.Sell)
                {

                    var price = Math.Round(levelPrice - (allCostPips+ OpeningSLPip+HedgeLevelPip) * symbol.PipSize, symbol.Digits);
                    var modifiedLevelPrice = Math.Round(levelPrice - (allCostPips + OpeningSLPip) * symbol.PipSize, symbol.Digits);
                    order.ModifyTargetPrice(price);
                    _level[order.Label] = modifiedLevelPrice;
                }
            }
        }

        private void AddLevel(string label, double bidPrice)
        {
            if (_level.ContainsKey(label))
            {
                return;
            }
            _level[label] = bidPrice;

        }

        private bool CheckTargetReached()
        {
            if (_isCheckedLock)
            {
                return true;
            }

            if (IsAccountBalanceTargetReached() || IsPairBalanceTargetReached())
            {
                _isCheckedLock = true;
                CloseAllPositions();
                return true;

            }
            return false;
        }

        private void CancelLevelOrders(string label)
        {
            foreach(var order in PendingOrders)
            {
                if (order.Label.Contains(label))
                {
                    CancelPendingOrderAsync(order);
                }
            }

        }

        private void CloseAllPositions()
        {
            if (_isCloseAllPositionLock)
            {
                return;
            }
            _numberOfOrderPlaced = Positions.Count;
            foreach (var pos in Positions)
            {
                if (pos.Label.Contains("BOT"))
                {
                    ClosePositionAsync(pos, OnClosedCallBack);
                }
                

            }

            _isCloseAllPositionLock = true;
        }

        private void ExecuteReduceTargetProfit(Symbol symbol, int levelId)
        {
            var topLevel = levelId + 1;
            var botLevel = levelId - 1;
            var topLabel = FormatLabel(symbol.Name, topLevel, 0);
            var botLabel = FormatLabel(symbol.Name, botLevel, 0);
            var topLevelPrice = _level[topLabel];
            var botLevelPrice = _level[botLabel];

            var targetTopPrice = Math.Round(topLevelPrice - ReducedTP * symbol.PipSize - symbol.Spread,symbol.Digits);
            var targetBotPrice = Math.Round(botLevelPrice + ReducedTP * symbol.PipSize + symbol.Spread,symbol.Digits);

            var allPositions = Positions.Where(x=>x.Label.Contains(FormatLabel(symbol.Name, levelId))).ToList();

            foreach (var pos in allPositions)
            {
                if(pos.StopLoss.HasValue || pos.TakeProfit.HasValue)
                {
                    continue;
                }
                else if (pos.TradeType == TradeType.Buy)
                {
                    pos.ModifyTakeProfitPrice(targetTopPrice);
                    pos.ModifyStopLossPrice(targetBotPrice);

                }
                else if(pos.TradeType == TradeType.Sell)
                {
                    pos.ModifyTakeProfitPrice(targetBotPrice);
                    pos.ModifyStopLossPrice(targetTopPrice);

                }

            }

        }

        //private void ExecuteBreakEvenTargetProfit(Symbol symbol, int levelId)
        //{
        //    var levelLabel = FormatLabel(symbol.Name, levelId, 0);
        //    var allPositionsLevelLabel = FormatLabel(symbol.Name, levelId);

        //    double allCostPips = GetAllCostPips(symbol, levelLabel, allPositionsLevelLabel);
            
        //    var openingDirection = Positions.Where(x => x.Label.Equals(FormatLabel(symbol.Name, levelId, 0))).First().TradeType;
        //    var allPositions = Positions.Where(x => x.Label.Contains(allPositionsLevelLabel));

        //    var currentLevelPrice = _level[levelLabel];

        //    var targetPrice = Math.Round(currentLevelPrice + (allCostPips + HedgeLevelPip) * symbol.PipSize, symbol.Digits);
        //    var stoplossPrice = Math.Round(currentLevelPrice - (allCostPips + HedgeLevelPip) * symbol.PipSize, symbol.Digits);

        //    foreach (var pos in allPositions)
        //    {
        //        if(openingDirection == TradeType.Buy)
        //        {
        //            switch (pos.TradeType)
        //            {
        //                case TradeType.Buy:
        //                    pos.ModifyTakeProfitPrice(targetPrice);
        //                    pos.ModifyStopLossPrice(stoplossPrice);

        //                    break;
        //                case TradeType.Sell:
        //                    pos.ModifyStopLossPrice(targetPrice);
        //                    pos.ModifyTakeProfitPrice(stoplossPrice);
        //                    break;
        //                default:
        //                    break;
        //            }
        //        }
        //        else
        //        {
        //            switch (pos.TradeType)
        //            {
        //                case TradeType.Buy:
        //                    pos.ModifyStopLossPrice(stoplossPrice);
        //                    pos.ModifyTakeProfitPrice(targetPrice);

        //                    break;
        //                case TradeType.Sell:
        //                    pos.ModifyTakeProfitPrice(stoplossPrice);
        //                    pos.ModifyStopLossPrice(targetPrice);

        //                    break;
        //                default:
        //                    break;
        //            }
        //        }

        //    }

        //}

        private bool ExecuteAllForOneOneForAll()
        {
            var sum= 0.0;
            foreach(var pos in Positions)
            {
                if (pos.Label.Contains("BOT"))
                {
                    sum += pos.NetProfit;
                }
            }
            if(sum >= 0)
            {

                CloseAllPositions();
                return true;
            }
            _activateOneForAll = true;
            return false;
        }

        double CheckRate(Symbol symbol)
        {
            var accountCurrency = Account.Asset.Name;
            double homeRate;
            var homeSymbol = symbol.Name[..3];
            var baseSymbol = symbol.Name[3..];


            //if (homeSymbol == accountCurrency)
            //{
            //    homeRate = 1;
            //}
            //else
            //{
            Symbol accountHome = null, homeAccount = null, accountBase = null, baseAccount= null;

            if (Symbols.Exists(accountCurrency + homeSymbol))
            {
                accountHome = Symbols.GetSymbol(accountCurrency + homeSymbol);

            }
         

            if (Symbols.Exists(accountCurrency + baseSymbol))
            {
                baseAccount = Symbols.GetSymbol(accountCurrency + baseSymbol);

            }

            if (Symbols.Exists(homeSymbol + accountCurrency))
            {
                homeAccount = Symbols.GetSymbol(homeSymbol + accountCurrency);

            }
            if (Symbols.Exists(baseSymbol + accountCurrency))
            {
                accountBase = Symbols.GetSymbol(baseSymbol + accountCurrency);

            }

            if (accountHome != null)
            {
                homeRate = 1 / accountHome.Bid;
            }
            else if (homeAccount != null)
            {
                homeRate = homeAccount.Bid;
            }
            else if (baseAccount != null)
            {
                homeRate = 1 / baseAccount.Bid;
            }
            else if (accountBase != null)
            {
                homeRate = accountBase.Bid;

            }
            else
            {
                if (Symbols.Exists("USD" + homeSymbol))
                {
                    accountHome = Symbols.GetSymbol("USD" + homeSymbol);

                }

                if (Symbols.Exists("USD" + baseSymbol))
                {
                    baseAccount = Symbols.GetSymbol(accountCurrency + baseSymbol);

                }

                if (Symbols.Exists(homeSymbol + "USD"))
                {
                    homeAccount = Symbols.GetSymbol(homeSymbol + accountCurrency);

                }
                if (Symbols.Exists(baseSymbol + "USD"))
                {
                    accountBase = Symbols.GetSymbol(baseSymbol + accountCurrency);

                }

                if (accountHome != null)
                {
                    homeRate = 1 / accountHome.Bid;
                }
                else if (homeAccount != null)
                {
                    homeRate = homeAccount.Bid;
                }
                else if (baseAccount != null)
                {
                    homeRate = 1 / baseAccount.Bid;
                }
                else if (accountBase != null)
                {
                    homeRate = accountBase.Bid;

                }
                else
                {
                    homeRate = 0;
                    Print("######### ERROR ########");
                    Print("Account currency not supported!");
                    Print("######### ERROR ########");
                    Stop();

                }



              
            }
            //}

            return homeRate;
        }

        private void OnClosedCallBack(TradeResult tradeResult)
        {
            _numberOfOrderPlaced--;
            if (_numberOfOrderPlaced == 0)
            {
                //PrintStats();
                Stop();
                //Restart();
            }
        }

        private void Restart()
        {
            _iterations++;
            Chart.RemoveAllObjects();
            SetInitialValues();
            AddSymbols();
            foreach(var pair in allSymbol)
            {
                ExecuteInitialOrder(pair);

            }
        }

        private void AddSymbols()
        {
            
            if (S1Symbol != SymbolNameType.NONE)
            {
                allSymbol.Add(Symbols.GetSymbol(S1Symbol.ToString()));
            }
            if (S2Symbol != SymbolNameType.NONE)
            {
                allSymbol.Add(Symbols.GetSymbol(S2Symbol.ToString()));
            }
            if (S3Symbol != SymbolNameType.NONE)
            {
                allSymbol.Add(Symbols.GetSymbol(S3Symbol.ToString()));
            }
            if (S4Symbol != SymbolNameType.NONE)
            {
                allSymbol.Add(Symbols.GetSymbol(S4Symbol.ToString()));
            }
            if (S5Symbol != SymbolNameType.NONE)
            {
                allSymbol.Add(Symbols.GetSymbol(S5Symbol.ToString()));
            }
            if (S6Symbol != SymbolNameType.NONE)
            {
                allSymbol.Add(Symbols.GetSymbol(S6Symbol.ToString()));
            }
            if (S7Symbol != SymbolNameType.NONE)
            {
                allSymbol.Add(Symbols.GetSymbol(S7Symbol.ToString()));
            }
            if (S8Symbol != SymbolNameType.NONE)
            {
                allSymbol.Add(Symbols.GetSymbol(S8Symbol.ToString()));
            }
            if (S9Symbol != SymbolNameType.NONE)
            {
                allSymbol.Add(Symbols.GetSymbol(S9Symbol.ToString()));
            }
            if (S10Symbol != SymbolNameType.NONE)
            {
                allSymbol.Add(Symbols.GetSymbol(S10Symbol.ToString()));
            }

        }

        private void SetInitialValues()
        {
            //_currentTime = Server.Time;
            _level = new Dictionary<string, double>();
            _levelLocked = new Dictionary<string, int>();
            _reduced = new List<string>();
            _instance = new Dictionary<string, int>();
            _breakEven = new List<string>();
            _label = "BOT";
            _levelNameStart = "0";
            _accountBalance = Account.Balance;
            allSymbol = new List<Symbol>();
            _isCheckedLock = false;
            _isCloseAllPositionLock = false;
            _activateOneForAll = false;
        }


        private bool IsAccountBalanceTargetReached()
        {
            var currentBalance = Account.Balance + Account.UnrealizedNetProfit;

            if (currentBalance >= _accountBalance * (1 + (TargetProfitInput / 100)))
            {
                return true;
            }
            else if (currentBalance <= _accountBalance * (1 - (AcceptedLossInput / 100)))
            {
                return true;
            }
            return false;
        }

        private bool IsPairBalanceTargetReached()
        {
            double maxSum = 0;
            double minSum = 0;

            foreach (var pair in allSymbol)
            {

                var total = Positions.Where(x => x.Label.StartsWith(FormatLabel(pair.Name))).Sum(Position => Position.NetProfit);
                if (maxSum < total)
                {
                    maxSum = total;
                }
                if (minSum > total)
                {
                    minSum = total;
                }


            }


            var currentHighestBalance = Account.Balance + maxSum;
            var currentLowestBalance = Account.Balance + minSum;

            if (currentHighestBalance >= _accountBalance * (1 + (TargetProfitInput / 100)))
            {
                return true;
            }
            else if (currentLowestBalance <= _accountBalance * (1 - (AcceptedLossInput / 100)))
            {
                return true;
            }

            return false;
        }



        private string FormatLabel(string symbolName, string levelsId, int positionCount)
        {
            return String.Format("{0}-{1}_{2}_{3}_{4}", _label, _iterations, symbolName, levelsId, positionCount);
        }

        private string FormatLabel(string symbolName, int levelsId, int positionCount)
        {
            return String.Format("{0}-{1}_{2}_{3}_{4}", _label, _iterations, symbolName, levelsId, positionCount);
        }

        private string FormatLabel(string symbolName, string levelsId)
        {
            return String.Format("{0}-{1}_{2}_{3}_", _label, _iterations, symbolName, levelsId);
        }

        private string FormatLabel(string symbolName, int levelsId)
        {
            return String.Format("{0}-{1}_{2}_{3}_", _label, _iterations, symbolName, levelsId);
        }
        private string FormatLabel(string symbolName)
        {
            return String.Format("{0}-{1}_{2}_", _label, _iterations, symbolName);
        }


        //private string FormatLabel()
        //{
        //    return String.Format("{0}-{1}_", _label, _iterations);
        //}

        private static string GetLevelWithoutPositionLabel(string label)
        {
            var split = label.Split("_").ToList();

            split.RemoveAt(split.Count-1);

            return string.Join("_", split)+"_";
        }

    }
}