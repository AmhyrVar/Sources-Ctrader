using cAlgo.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace cAlgo {
    
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
    public class LPBSRSI  : LiPiBotCoreBase {

        private const string BOT_NAME = "LiPiBot_RSI";
        private const string BOT_DATE = "2022-05-02";
        private const string BOT_WEB = "";
        private const int BOT_SIGNAL_VERSION = 1;

        #region BOT PARAMETERS
        /*
         * Bot
         * */
        [Parameter("Bot Action", DefaultValue = Bot_Action.Trade, Group = "LiPiBot")]
        public override Bot_Action BotAction { get; set; }

        [Parameter("Position Action", DefaultValue = Bot_Position_Action.Open_And_Close_Positions, Group = "LiPiBot")]
        public override Bot_Position_Action BotPositionAction { get; set; }

        [Parameter("Trade Type", DefaultValue = Bot_Trade_Type.Buy_And_Sell, Group = "LiPiBot")]
        public override Bot_Trade_Type BotTradeType { get; set; }

        [Parameter("Check Position Profit/Loss From", DefaultValue = Bot_Check_Profit_From.Pips, Group = "LiPiBot")]
        public override Bot_Check_Profit_From BotCheckProfitFrom { get; set; }

        [Parameter("Label Suffix (Max. 5 Letters)", DefaultValue = "", Group = "LiPiBot")]
        public override string BotPositionLabelSuffix { get; set; }

        [Parameter("Show Dialog on Position Event", DefaultValue = Log_On_Position_Event.No, Group = "LiPiBot")]
        public override Log_On_Position_Event BotTradingTypeSignalMessageBox { get; set; }

        [Parameter("Write to Log on Position Event", DefaultValue = Log_On_Position_Event.All, Group = "LiPiBot")]
        public override Log_On_Position_Event BotTradingTypeSignalConsole { get; set; }

        [Parameter("Show Dialog on Start and End Trading Hour", DefaultValue = Yes_No.Yes, Group = "LiPiBot")]
        public override Yes_No BotTradingHourStartEndMessageBoxShow { get; set; }

        /*
         * Bot - Backtesting
         * */
        [Parameter("Close All Positions on Stop Backtesting", DefaultValue = Yes_No.Yes, Group = "LiPiBot - Backtesting")]
        public override Yes_No BotBacktestingCloseAllPositionsOnStop { get; set; }

        [Parameter("Stop Backtesting on Maximum Balance Drop", DefaultValue = 100, MinValue = 0, Step = 10, Group = "LiPiBot - Backtesting")]
        public override int BotBacktestingStopOnMaximumBalanceDrop { get; set; }

        [Parameter("Stop Backtesting on Maximum Equinity Drop", DefaultValue = 100, MinValue = 0, Step = 10, Group = "LiPiBot - Backtesting")]
        public override int BotBacktestingStopOnMaximumEquinityDrop { get; set; }

        [Parameter("Stop Backtesting on Maximum Losing Trades", DefaultValue = 0, MinValue = 0, Step = 5, Group = "LiPiBot - Backtesting")]
        public override int BotBacktestingStopOnMaximumLosingTrades { get; set; }
        
        [Parameter("Stop Backtesting on Start Validation Error", DefaultValue = Yes_No.Yes, Group = "LiPiBot - Backtesting")]
        public override Yes_No BotBacktestingStopOnStartValidationError { get; set; }


        /*
         * Position
         * */
        [Parameter("Volume (Lots)", DefaultValue = 0.01, Group = "Position", MinValue = 0, Step = 0.01)]
        public override double PositionVolumeInLots { get; set; }

        [Parameter("Stop Loss (Pips)", DefaultValue = 100, Group = "Position", MinValue = 0, MaxValue = 500, Step = 5)]
        public override double PositionStopLossInPips { get; set; }

        [Parameter("Take Profit (Pips)", DefaultValue = 100, Group = "Position", MinValue = 0, MaxValue = 500, Step = 5)]
        public override double PositionTakeProfitInPips { get; set; }

        [Parameter("Active Trailing Stop Loss", DefaultValue = Yes_No.No, Group = "Position")]
        public override Yes_No PositionActiveTrailingStopLoss { get; set; }


        /*
         * Trailing Stop Loss
         * */
        [Parameter("Move SL to Profit at Pips", DefaultValue = 40, Group = "Move Stop Loss to Profit", MinValue = 0, MaxValue = 500, Step = 5)]
        public override double MoveStopLossToProfitAtPips { get; set; }

        [Parameter("Set SL to Profit Value [% of Current Pips]", DefaultValue = 50, Group = "Move Stop Loss to Profit", MinValue = 0, MaxValue = 95, Step = 5)]
        public override double MoveStopLossToProfitSetValue { get; set; }

        [Parameter("Move SL to Profit Type", DefaultValue = Move_Stop_Loss_Type.Trailing_Stop_Loss, Group = "Move Stop Loss to Profit")]
        public override Move_Stop_Loss_Type MoveStopLossToProfitType { get; set; }

        [Parameter("Remove Take Profit on SL to Profit Moved", DefaultValue = Yes_No.Yes, Group = "Move Stop Loss to Profit")]
        public override Yes_No RemoveTakeProfitOnMoveStopLossToProfit { get; set; }


        /*
         * Position - Close
         * */
        [Parameter("Close Positions If Opposite Signal Exists", DefaultValue = Close_Positions_If_Opposite_Signal_Exists.None, Group = "Position - Close")]
        public override Close_Positions_If_Opposite_Signal_Exists ClosePositionsIfOppositeSignalExists { get; set; }

        [Parameter("Closing Aggregated Positions", DefaultValue = Closing_Aggregated_Positions.All, Group = "Position - Close")]
        public override Closing_Aggregated_Positions ClosingAggregatedAdditionalPositions { get; set; }

        [Parameter("Close Positions on Start Trading Hour", DefaultValue = Close_Positions_On_Start_Trading_Hour.None, Group = "Position - Close")]
        public override Close_Positions_On_Start_Trading_Hour ClosePositionsOnStartTradingHour { get; set; }

        [Parameter("Close Positions on End Trading Hour", DefaultValue = Close_Positions_On_End_Trading_Hour.None, Group = "Position - Close")]
        public override Close_Positions_On_End_Trading_Hour ClosePositionsOnEndTradingHour { get; set; }

        [Parameter("Close Positions by Age", DefaultValue = Close_Positions_By_Age.None, Group = "Position - Close")]
        public override Close_Positions_By_Age ClosePositionsByAge { get; set; }

        [Parameter("Close Positions by Age Value [minutes]", DefaultValue = 2 * 24 * 60, Group = "Position - Close", MinValue = 1, MaxValue = 30 * 24 * 60, Step = 5)]
        public override int ClosePositionByAgeValue { get; set; }

        [Parameter("Close Positions by Age Checking Interval [minutes]", DefaultValue = 2 * 60, Group = "Position - Close", MinValue = 1, MaxValue = 2 * 24 * 60, Step = 1)]
        public override int ClosePositionByAgeOnTimeInterval { get; set; }

        [Parameter("Close Positions Last Trading Day of Week", DefaultValue = Close_Positions_Last_Trading_Day_Of_Week.None, Group = "Position - Close")]
        public override Close_Positions_Last_Trading_Day_Of_Week ClosePositionsLastTradingDayOfWeek { get; set; }
        
        [Parameter("Close Positions Last Trading Day of Week Before End Trading [minutes]", DefaultValue = 5, Group = "Position - Close", MinValue = 0, MaxValue = 60, Step = 1)]
        public override int ClosePositionsLastTradingDayOfWeekBeforeEndTradingValue { get; set; }

        /*
         * Position - Open
         * */
        [Parameter("Open Opposite Prime Position If Opposite Signal Exists", DefaultValue = Yes_No.Yes, Group = "Position - Open")]
        public override Yes_No OpenOppositePrimePositionIfOppositeSignalExists { get; set; }


        /*
         * Positions Prime
         * */
        [Parameter("Maximum Prime Positions", DefaultValue = 2, Group = "Prime Positions", MinValue = 1, MaxValue = 30, Step = 1)]
        public override int PrimePositionsMax { get; set; }

        [Parameter("Minimum Loss to Open New Prime Position (Pips)", DefaultValue = 0, Group = "Prime Positions", MinValue = 0, MaxValue = 500, Step = 5)]
        public override double PrimePositionMinLossPips { get; set; }
        [Parameter("Minimum Loss Pips Multiplicator", DefaultValue = 0, Group = "Prime Positions", MinValue = -0.75, MaxValue = 10, Step = 0.25)]
        public override double PrimePositionMinLossPipsMultiplicator { get; set; }

        [Parameter("Minimum Profit to Open New Prime Position (Pips)", DefaultValue = 0, Group = "Prime Positions", MinValue = 0, MaxValue = 500, Step = 5)]
        public override double PrimePositionMinProfitPips { get; set; }
        [Parameter("Minimum Profit Pips Multiplicator", DefaultValue = 0, Group = "Prime Positions", MinValue = -0.75, MaxValue = 10, Step = 0.25)]
        public override double PrimePositionMinProfitPipsMultiplicator { get; set; }

        [Parameter("Minimum Age of Last P.Pos. to Open New Prime Position [minutes]", DefaultValue = 12 * 60, Group = "Prime Positions", MinValue = 0, MaxValue = 60 * 24 * 2, Step = 60)]
        public override int PrimePositionMinAge { get; set; }

        [Parameter("Minimum Age Multiplicator", DefaultValue = 0, Group = "Prime Positions", MinValue = -0.75, MaxValue = 10, Step = 0.25)]
        public override double PrimePositionMinAgeMultiplicator { get; set; }


        /*
         * Positions Additional
         * */
        [Parameter("Maximum Additional Positions", DefaultValue = 2, Group = "Additional Positions", MinValue = 0, MaxValue = 30, Step = 1)]
        public override int AdditionalPositionsMax { get; set; }

        [Parameter("Additional Positions Type", DefaultValue = Additional_Positions_Type.Separated, Group = "Additional Positions")]
        public override Additional_Positions_Type AdditionalPositionsType { get; set; }

        [Parameter("Minimum Loss to Open New Additional Positions (Pips)", DefaultValue = 0, Group = "Additional Positions", MinValue = 0, MaxValue = 500, Step = 5)]
        public override double AdditionalPositionsMinLossPips { get; set; }

        [Parameter("Minimum Loss Pips Multiplicator", DefaultValue = 0, Group = "Additional Positions", MinValue = -0.75, MaxValue = 10, Step = 0.25)]
        public override double AdditionalPositionsMinLossPipsMultiplicator { get; set; }

        [Parameter("Minimum Profit to Open New Additional Positions (Pips)", DefaultValue = 0, Group = "Additional Positions", MinValue = 0, MaxValue = 500, Step = 5)]
        public override double AdditionalPositionsMinProfitPips { get; set; }

        [Parameter("Minimum Profit Pips Multiplicator", DefaultValue = 0, Group = "Additional Positions", MinValue = -0.75, MaxValue = 10, Step = 0.25)]
        public override double AdditionalPositionsMinProfitPipsMultiplicator { get; set; }

        [Parameter("Signal Reqired to Open New Additional Positions", DefaultValue = Yes_No.Yes, Group = "Additional Positions")]
        public override Yes_No AdditionalPositionsSignalRequired { get; set; }

        [Parameter("Volume Multiply", DefaultValue = 2, Group = "Additional Positions", MinValue = 0, MaxValue = 5, Step = 1)]
        public override int AdditionalPositionsVolumeMultiply { get; set; }

        [Parameter("Volume Multiply Type", DefaultValue = Multiply_Volume_Type.Linear, Group = "Additional Positions")]
        public override Multiply_Volume_Type AdditionalPositionsVolumeMultiplyType { get; set; }


        /*
         * Trading Hours
         * */
        [Parameter("Active Trading Hours", DefaultValue = Yes_No.No, Group = "Trading Hours")]
        public override Yes_No TradingHoursActive { get; set; }

        [Parameter("Start Trading Hour [UTC]", DefaultValue = 0, Group = "Trading Hours", MinValue = 0, MaxValue = 23, Step = 1)]
        public override int TradingHourStart { get; set; }

        [Parameter("End Trading Hour [UTC]", DefaultValue = 0, Group = "Trading Hours", MinValue = 0, MaxValue = 23, Step = 1)]
        public override int TradingHourEnd { get; set; }
        #endregion


        /*
         * RSI
         * */
        [Parameter("Source", Group = "RSI")]
        public DataSeries RSI_Source { get; set; }

        [Parameter("Periods", Group = "RSI", DefaultValue = 14, MinValue = 2, MaxValue = 50, Step = 1)]
        public int RSI_Periods { get; set; }

        [Parameter("Level", DefaultValue = 30, Group = "RSI", MinValue = 0, MaxValue = 50, Step = 5)]
        public int RSI_LevelMin { get; set; }


        public override string GetBotVersion() { return BOT_SIGNAL_VERSION + "." + GetBotBaseVersion(); }
        public override string GetBotName() { return BOT_NAME; }
        public override string GetBotDate() { return BOT_DATE; }
        public override string GetBotWeb() { return BOT_WEB; }


        protected override ISignal CreateSignalWorker() {
            return new SignalRSI(this);
        }

        protected override bool IsInvalidParameterExists() {
            if (base.IsInvalidParameterExists()) return true;
            return false;
        }
    }
}
