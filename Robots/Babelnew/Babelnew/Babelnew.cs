using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cAlgo.API;
using cAlgo.API.Collections;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using Microsoft.Win32;

namespace cAlgo.Robots
{
    [Robot(AccessRights = AccessRights.FullAccess)]
    public class BableNew : Robot
    {

        //---------------------------------------------------------------------- Enums ----------------------------------------------------------------------\\
        #region Enums

        public enum entType
        {
            disabled = 0,
            // Disabled
            enabled = 1,
            // Enabled
            reverse = 2
            // Reverse
        }

        public enum crossType
        {
            ic_disable = 0,             // Disabled
            ic_enabled = 1,             // Cross
            ic_cloud = 2              // Cross Cloud Direction 
        }

        public enum mktCond
        {
            uptrend = 0,
            // Long only
            downtrend = 1,
            // Short only
            range = 2,
            // Long & short
            automatic = 3
            // Automatic
        }

        public enum enumPosType
        {
            None = 0,
            All = 1,
            Basket = 2,
            Hedge = 3,
            Ticket = 4,
            Pending = 5
            //#define A 1                     //All (Basket + Hedge)
            //#define B 2                     //Basket
            //#define H 3                     //Hedge
            //#define T 4                     //Ticket
            //#define P 5                     //Pending
        }
        public enum enumPortChgs
        {
            No_change = 0,
            // No changes
            Increase = 1,
            // Increase only
            Any = -1
            // Increase / decrease
        }

        public enum macdPrice
        {
            Close = 0,
            Open = 1,
            High = 2,
            Low = 3,
            HL2 = 4,
            HLC3 = 5,
            HLCC4 = 6
        }

        public enum appliedPrice
        {
            Close = 0,
            Open = 1,
            High = 2,
            Low = 3,
            Median = 4,
            Typical = 5,
            Weighted = 6
        }

        #endregion Enums

        //---------------------------------------------------------------------- Parameters ----------------------------------------------------------------------\\
        #region Parameters

        [Parameter("Daily MA Entry", Group = "Moving Average")]
        public entType DailyMAEntry { get; set; }

        [Parameter("MA Setup", Group = "Moving Average")]
        public entType MAEntry_ { get; set; }

        [Parameter("MA Period", Group = "Moving Average", DefaultValue = 100, MinValue = 1)]
        public int MAPeriod { get; set; }

        [Parameter("Distance (pips) from MA to be treated as Ranging Market", Group = "Moving Average", DefaultValue = 10, MinValue = 1)]
        public double MADistance { get; set; }


        [Parameter("CCI Setup", Group = "CCI")]
        public entType CCIEntry_ { get; set; }

        [Parameter("CCI Period", Group = "CCI", DefaultValue = 14, MinValue = 1)]
        public int CCIPeriod { get; set; }


        [Parameter(DefaultValue = "", Group = "ICHIMOKU CLOUD")]
        public string LabelIchi { get; set; }
        [Parameter(DefaultValue = 0, Group = "ICHIMOKU CLOUD")]
        public entType IchimokuEntry { get; set; }
        [Parameter(DefaultValue = 0, Group = "ICHIMOKU CLOUD")]
        public TimeFrame ICHI_TF { get; set; }
        [Parameter(DefaultValue = 9, Group = "ICHIMOKU CLOUD")]
        public int Tenkan_Sen { get; set; }
        [Parameter(DefaultValue = 26, Group = "ICHIMOKU CLOUD")]
        public int Kijun_Sen { get; set; }
        [Parameter(DefaultValue = 52, Group = "ICHIMOKU CLOUD")]
        public int Senkou_Span { get; set; }
        [Parameter(DefaultValue = true, Group = "ICHIMOKU CLOUD")]
        public bool useCloudBreakOut { get; set; }
        [Parameter(DefaultValue = 2, Group = "ICHIMOKU CLOUD")]
        public crossType useTenken_Kijun_cross { get; set; }
        [Parameter(DefaultValue = false, Group = "ICHIMOKU CLOUD")]
        public bool usePriceCrossTenken { get; set; }
        [Parameter(DefaultValue = false, Group = "ICHIMOKU CLOUD")]
        public bool usePriceCrossKijun { get; set; }
        [Parameter(DefaultValue = false, Group = "ICHIMOKU CLOUD")]
        public bool useChikuspan { get; set; }
        [Parameter(DefaultValue = false, Group = "ICHIMOKU CLOUD")]
        public bool useCloudSL { get; set; }



        [Parameter("Bollinger Band Setup", Group = "Bollinger Bands")]
        public entType BollingerEntry_ { get; set; }

        [Parameter("Period", Group = "Bollinger Bands", DefaultValue = 10, MinValue = 1)]
        public int BollPeriod { get; set; }

        [Parameter("Standard deviation multiplier for channel", Group = "Bollinger Bands", DefaultValue = 2)]
        public double BollDeviation { get; set; }

        [Parameter("Up/Down spread", Group = "Bollinger Bands", DefaultValue = 10)]
        public double BollDistance { get; set; }



        [Parameter("Stochastic Setup", Group = "Stochastic")]
        public entType StochEntry_ { get; set; }

        [Parameter("Determines Overbought and Oversold Zones", Group = "Stochastic", DefaultValue = 20, MinValue = 0)]
        public int BuySellStochZone { get; set; }

        [Parameter("KPeriod", Group = "Stochastic", DefaultValue = 10, MinValue = 0)]
        public int KPeriod { get; set; }

        [Parameter("DPeriod", Group = "Stochastic", DefaultValue = 2, MinValue = 0)]
        public int DPeriod { get; set; }

        [Parameter("Slowing", Group = "Stochastic", DefaultValue = 2, MinValue = 0)]
        public int Slowing { get; set; }



        [Parameter("MACD Setup", Group = "MACD")]
        public entType MACDEntry_ { get; set; }

        [Parameter("Timeframe", Group = "MACD")]
        public TimeFrame MACD_TF { get; set; }

        [Parameter("EMA Fast Period", Group = "MACD", DefaultValue = 12, MinValue = 0)]
        public int FastPeriod { get; set; }

        [Parameter("EMA Slow Period", Group = "MACD", DefaultValue = 26, MinValue = 0)]
        public int SlowPeriod { get; set; }

        [Parameter("EMA Signal Period", Group = "MACD", DefaultValue = 9, MinValue = 0)]
        public int SignalPeriod { get; set; }

        [Parameter("Applied Price", Group = "MACD")]
        public macdPrice MACDPrice { get; set; }



        [Parameter("PAC and TLI Setup", Group = "PAC and TLI")]
        public entType MPACEntry_ { get; set; }

        [Parameter("Timeframe", Group = "PAC and TLI")]
        public TimeFrame MPAC_TimeFrame { get; set; }

        [Parameter("PAC A - Period", Group = "PAC and TLI", DefaultValue = 6)]
        public int PACA_Period { get; set; }

        [Parameter("PAC B - Period", Group = "PAC and TLI", DefaultValue = 30)]
        public int PACB_Period { get; set; }

        [Parameter("TLI - Period", Group = "PAC and TLI", DefaultValue = 14)]
        public int TLI_Period { get; set; }

        [Parameter("TLI Snake - Bull Entry below TLI zone %", Group = "PAC and TLI", DefaultValue = 25)]
        public int TLI_Snake_BullEntry_Zone { get; set; }

        [Parameter("TLI Snake - Bear Entry above TLI zone %", Group = "PAC and TLI", DefaultValue = 75)]
        public int TLI_Snake_BearEntry_Zone { get; set; }



        [Parameter("Auto calc of TakeProfit and Grid size", Group = "Grid Settings", DefaultValue = false)]
        public bool AutoCal { get; set; }

        [Parameter("TimeFrame for ATR calc", Group = "Grid Settings")]
        public TimeFrame ATRTF { get; set; }

        [Parameter("Number of periods for the ATR calc", Group = "Grid Settings", DefaultValue = 21, MinValue = 1)]
        public int ATRPeriods { get; set; }

        [Parameter("Widens/Squishes Grid in increments/decrements of .1", Group = "Grid Settings", DefaultValue = 1.0, MinValue = 0.1)]
        public double GAF { get; set; }

        [Parameter("Time Grid in seconds, avoid opening lots of levels in fast market", Group = "Grid Settings", DefaultValue = 2400, MinValue = 1000)]
        public int EntryDelay { get; set; }

        [Parameter("In pips, used in conjunction with logic to offset first trade entry", Group = "Grid Settings", DefaultValue = 5, MinValue = 1)]
        public double EntryOffset { get; set; }

        [Parameter("True = use RSI/MA calculation for next grid order", Group = "Grid Settings", DefaultValue = true)]
        public bool UseSmartGrid { get; set; }


        [Parameter("Open trades in each block (separated by a comma)", Group = "Grid Size", DefaultValue = "4,4")]
        public string SetCountArray { get; set; }

        [Parameter("Number of pips away to issue limit order (separated by a comma)", Group = "Grid Size", DefaultValue = "25,50,100")]
        public string GridSetArray_ { get; set; }

        [Parameter("Take profit for each block (separated by a comma)", Group = "Grid Size", DefaultValue = "50,100,200")]
        public string TP_SetArray_ { get; set; }



        [Parameter("TimeFrame for RSI calc (should be lower than chart TF)", Group = "Smart Grid")]
        public TimeFrame RSI_TF { get; set; }

        [Parameter("Period for RSI calc", Group = "Smart Grid", DefaultValue = 14, MinValue = 1)]
        public int RSI_Period { get; set; }

        [Parameter("RSI Applied Price", Group = "Smart Grid", DefaultValue = appliedPrice.Close)]
        public appliedPrice RSI_Price { get; set; }

        [Parameter("Period for MA of RSI calc", Group = "Smart Grid", DefaultValue = 10, MinValue = 1)]
        public int RSI_MA_Period { get; set; }

        [Parameter("RSI MA Method", Group = "Smart Grid", DefaultValue = MovingAverageType.Simple)]
        public MovingAverageType RSI_MA_Method { get; set; }



        [Parameter("Turns DD hedge on/off", Group = "Hedge Settings", DefaultValue = false)]
        public bool UseHedge { get; set; }

        [Parameter("Drawdown Percent at which Hedge starts", Group = "Hedge Settings", DefaultValue = 20)]
        public double HedgeStart { get; set; }

        [Parameter("Hedge Lots with trend = Open Lots * hLotMult", Group = "Hedge Settings", DefaultValue = 0.25)]
        public double hLotMult { get; set; }

        [Parameter("Hedge Lots against trend = Open Lots * hLotMult", Group = "Hedge Settings", DefaultValue = 1.1)]
        public double hLotMult_AgainstTrend { get; set; }

        [Parameter("Hedge Take Profit as % of Balance", Group = "Hedge Settings", DefaultValue = 30)]
        public double hTakeProfit { get; set; }



        [Parameter("Percentage of balance lost before trading stops", Group = "Account Settings", DefaultValue = 10.0)]
        public double StopTradePercent_ { get; set; }

        [Parameter("Small Lot Account (0.01)", Group = "Account Settings", DefaultValue = false)]
        public bool NanoAccount { get; set; }

        [Parameter("Percentage of account you want to trade on this pair", Group = "Account Settings", DefaultValue = 100)]
        public double PortionPC { get; set; }

        [Parameter("Permitted Portion change with open basket", Group = "Account Settings", DefaultValue = enumPortChgs.Increase)]
        public enumPortChgs PortionChange { get; set; }

        [Parameter("Percent of portion for max drawdown level", Group = "Account Settings", DefaultValue = 50)]






        public double MaxDDPercent { get; set; }

        [Parameter("Allowed Order Types", Group = "Trade Settings")]
        public mktCond ForceMarketCond_ { get; set; }

        [Parameter("B3 Traditional", Group = "Trade Settings")]
        public bool B3Traditional { get; set; }
        // Stop/Limits for entry if true, Buys/Sells if false
        [Parameter("Use Any Entry", Group = "Trade Settings", DefaultValue = false)]
        public bool UseAnyEntry { get; set; }
        // true = ANY entry can be used to open orders, false = ALL entries used to open orders

        [Parameter("Max allowed slippage (pips)", Group = "Trade Settings", DefaultValue = 0.5, MinValue = 0.1)]
        public double MaxSlippagePips { get; set; }




        [Parameter("Maximum number of trades to place (stops placing orders when reaches MaxTrades)", Group = "Trading", DefaultValue = 15)]
        public int MaxTrades { get; set; }

        [Parameter("Close All level, when reaches this level, doesn't wait for TP to be hit", Group = "Trading", DefaultValue = 12)]
        public int BreakEvenTrade { get; set; }

        [Parameter("Pips added to Break Even Point before BE closure", Group = "Trading", DefaultValue = 2)]
        public double BEPlusPips { get; set; }

        [Parameter("Close the oldest open trade after CloseTradesLevel is reached", Group = "Trading", DefaultValue = true)]
        public bool UseCloseOldest { get; set; }

        [Parameter("Will start closing oldest open trade at this level", Group = "Trading", DefaultValue = 5)]
        public int CloseTradesLevel { get; set; }

        [Parameter("Will close the oldest trade whether it has potential profit or not", Group = "Trading", DefaultValue = true)]
        public bool ForceCloseOldest { get; set; }

        [Parameter("Maximum number of oldest trades to close", Group = "Trading", DefaultValue = 4)]
        public int MaxCloseTrades_ { get; set; }

        [Parameter("After Oldest Trades have closed, Forces Take Profit to BE +/- xx Pips", Group = "Trading", DefaultValue = 10)]
        public double CloseTPPips { get; set; }

        [Parameter("Force Take Profit to BE +/- xx Pips", Group = "Trading", DefaultValue = 0)]
        public double ForceTPPips { get; set; }

        [Parameter("Ensure Take Profit is at least BE +/- xx Pips", Group = "Trading", DefaultValue = 0)]
        public double MinTPPips { get; set; }



        [Parameter("Recoup Hedge/CloseOldest losses", Group = "Other", DefaultValue = true)]
        public bool RecoupClosedLoss { get; set; }

        [Parameter("Largest Assumed Basket size.  Lower number = higher start lots", Group = "Other", DefaultValue = 7)]
        public int Level_ { get; set; }




        [Parameter("Use Money Management", Group = "Lot Size", DefaultValue = true)]
        public bool UseMM { get; set; }

        [Parameter("Adjusts MM base lot for large accounts", Group = "Lot Size", DefaultValue = 0.5)]
        public double LAF { get; set; }

        [Parameter("Starting lots if Money Management is off", Group = "Lot Size", DefaultValue = 0.01)]
        public double Lot_ { get; set; }

        [Parameter("Multiplier on each level", Group = "Lot Size", DefaultValue = 1.4, MinValue = 1)]
        public double Multiplier_ { get; set; }




        [Parameter("Turns on TP move and Profit Trailing Stop Feature", Group = "Exits", DefaultValue = false)]
        public bool MaximizeProfit { get; set; }


        [Parameter("Profit trailing stop: Lock in profit at set percent of Total Profit Potential", Group = "Exits", DefaultValue = 70)]
        public double ProfitSet { get; set; }

        [Parameter("Transmits a SL in case of internet loss", Group = "Exits", DefaultValue = false)]
        public bool UsePowerOutSL { get; set; }

        [Parameter("Power Out Stop Loss in pips", Group = "Exits", DefaultValue = 600)]
        public double POSLPips { get; set; }

        [Parameter("Moves TP this amount in pips ", Group = "Exits", DefaultValue = 30)]
        public double MoveTP { get; set; }

        [Parameter("Number of times you want TP to move before stopping movement", Group = "Exits", DefaultValue = 2)]
        public int TotalMoves { get; set; }

        [Parameter("Use Stop Loss and/or Trailing Stop Loss", Group = "Exits", DefaultValue = false)]
        public bool UseStopLoss { get; set; }


        [Parameter("Print Info to Log", Group = "Log Messages", DefaultValue = true)]
        public bool PrintMessagesInLog { get; set; }

        #endregion Parameters

        //---------------------------------------------------------------------- Global Variables ----------------------------------------------------------------------\\
        #region Global Variables

        bool UseHedge_;
        double HedgeStart_;
        double SLh;
        double hDDStart;
        double BEa;
        double TPa;
        double TPaF;

        bool AllowTrading;
        bool PendLot;
        int AccountType_;

        double MoveTP_;
        double EntryOffset_;
        double BollDistance_;
        double POSLPips_;
        double hTakeProfit_;
        double CloseTPPips_;
        double ForceTPPips_;
        double MinTPPips_;
        double BEPlusPips_;
        double SLPips_;
        double TSLPips_;
        double TSLPipsMin_;
        double ProfitSet_;

        double DrawDownPC;
        double MaxDDPer;
        double MaxDD;

        int CountOpenBuy = 0;
        int CountOpenSell = 0;
        int CountTotalOpen, CountTotalPending, CountTotalClosed;
        int CountTotalHedge = 0;
        int CountBuyStop = 0;
        int CountSellLimit = 0;
        int CountBuyLimit = 0;
        int CountSellStop = 0;
        int ThO;
        int Set1Level, Set2Level, Set3Level, Set4Level;
        double OPbL, OPpBL, OPbN;
        double OPpSL;
        double g2;
        double tp2;
        double Pb;
        double BCh;
        double BEh;
        double BCb;
        double BEb;
        int ChB;
        double LhB;
        double LbB;
        int ChS;
        double LhS, LbS;
        double LbT;
        double LhT;
        double BCa;
        double InitialAB, StopTradeBalance, PortionBalance;
        double MinLotSize, MinLot;
        int LotDecimal;
        double PaC, ProfitTotalClosed, PhC;
        bool hActive;
        double Ph;
        double bTS;
        double SLbL;
        double PbMax, PbMin;
        double PortionPC_;
        double OrderLotsFirst;
        double TakeProfitb;
        int PositionIdFirst;
        int PositionIdO;
        DateTime OrderOpenTimeLast;
        DateTime OrderOpenTimeO;
        DateTime OrderOpenTimeFirst;
        DateTime OrderOpenTimeHO;
        double OrderOpenPriceO;
        enumPosType CloseOrdersOfType;
        bool TradesOpen;

        double pacA_TopAngle = 0;
        double pacA_BottomAngle = 0;
        double pacB_TopAngle = 0;
        double pacB_BottomAngle = 0;
        double tli_TopAngle = 0;
        double tli_BottomAngle = 0;
        bool pacA_green_up = false;
        bool pacA_green_down = false;
        bool pacB_green_up = false;
        bool pacB_green_down = false;
        bool tli_green_up = false;
        bool tli_green_down = false;
        bool tli_openchannel = false;
        bool tli_closedchannel = false;
        bool tli_parallelchannel = false;
        bool pacA_parallel = false;
        bool pacB_parallel = false;
        bool tli_bear_chasingchart = false;
        bool tli_bull_chasingchart = false;
        double tliLastTopValueB = 0;
        double tliLastBottomValueB = 0;
        double currPriceZone = 0;
        bool fivelinerule_bullentry = false;
        bool fivelinerule_bearentry = false;
        double CurrIndexHigh_OnSameAngleAsTopLine = 0;
        double CurrIndexLow_OnSameAngleAsBottomLine = 0;
        bool tli_snake_bullentry = false;
        bool tli_snake_bearentry = false;

        string str_BuyLimit = "";
        string str_SellLimit = "";
        string str_BuyStop = "";
        string str_SellStop = "";
        string str_IndicatorDirection = "";
        string str_HedgeBuy = "";
        string str_HedgeSell = "";

        int ticksinceLastRun;

        string str_Default_TradeComment = "";
        private const string str_BotRegistryKey = "BABLE";

        double GridTP;
        double TargetPips;
        double TotalProfitTarget = 0;
        double ProfitPotential = 0;
        private int Moves = 0;
        string SetCountArray_;
        int CaL = 0;
        double BEbL, BCaL;

        double nLots = 0;
        double MADistance_;
        double LotMult, MinMult;
        double Pip;
        double PipVal2;
        int Trend = 0;
        string ATrend;
        string IndicatorUsed;
        int IndEntry;
        bool BuyMe, SellMe, BuyMe_Temp, SellMe_Temp;
        string UAE;
        bool FirstRun;
        List<double> Lots = new List<double>();
        List<double> GridArray0 = new List<double>();
        List<double> GridArray1 = new List<double>();

        private MovingAverage ima_;
        private MovingAverage ima_bb;
        private StandardDeviation std_bb;
        private CommodityChannelIndex cci_1;
        private CommodityChannelIndex cci_2;
        private CommodityChannelIndex cci_3;
        private CommodityChannelIndex cci_4;
        private StochasticOscillator iStochastic_;
        private MacdCrossOver macd_;
        private RelativeStrengthIndex rsi_;
        private AverageTrueRange atr_;
        private IchimokuKinkoHyo ichimoku_;

        double[] rsi = new double[100];
        double CbT;
        double stop_trade_amount;

        #endregion Global Variables

        //---------------------------------------------------------------------- Bot System ----------------------------------------------------------------------\\
        #region Bot System

        protected override void OnStart()
        {
            Print("Bot Started!");

            FirstRun = true;
            AllowTrading = true;

            str_BuyLimit = "Buy Limit " + Symbol.Name;
            str_SellLimit = "Sell Limit " + Symbol.Name;
            str_BuyStop = "Buy Stop " + Symbol.Name;
            str_SellStop = "Sell Stop " + Symbol.Name;
            str_Default_TradeComment = "BABLE 2.2 " + Symbol.Name;
            str_HedgeBuy = "Hedge Buy " + Symbol.Name;
            str_HedgeSell = "Hedge Sell " + Symbol.Name;


            // ----> Initializations
            InitIndcies();
            InitVars();
            InitTradeSettings();

            WriteRegistryKeyValue("LotMult", LotMult);

            InitLotArray();
            InitGridTP();
            InitSmartGrid();

            TradingLogic();

            ticksinceLastRun = 0;
        }

        protected override void OnTick()
        {
            ticksinceLastRun++;

            if (ticksinceLastRun > 3)
            {
                MonitorTrades();
                ticksinceLastRun = 0;
            }
        }

        protected override void OnBar()
        {
            TradingLogic();
        }

        protected override void OnStop()
        {
            Print("Bot Stopped!");
        }

        private void TradingLogic()
        {
            BuyMe = false;
            SellMe = false;
            BuyMe_Temp = false;
            SellMe_Temp = false;
            IndEntry = 0;

            // ----> Order Logic
            MonitorTrades();
            Trading();
            MonitorTrades();
            HedgeSetup();
            FirstRun = false;
        }

        private void MonitorTrades()
        {
            GetOrders();
            GetPendingOrders();

            CalculateProfit();
            CalculateDrawDown();
            CalculateStopTrade();
            ProfitManagement();
            CloseOldTrades();
            MoneyManagement();
            
            TrailingStop();
            CalculateTP();
            DeleteHangOrders();
        }

        private void Trading()
        {
            CalculateGridATR();

            // ----> Read Indicies

            IndicatorUsed = "";
            str_IndicatorDirection = "";

            ReadMA();
            ReadCCI();
            ReadBollinger();
            ReadStochastic();
            ReadMACD();
            ReadAllEntry();

            // ----> Trading Start
            DeleteWrongDir();
            TradingSelection();
        }

        #endregion Bot System

        //---------------------------------------------------------------------- Read Indicies ----------------------------------------------------------------------\\
        #region Read Indicies

        private void ReadAllEntry()
        {
            if (!UseAnyEntry && IndEntry > 1 && BuyMe && SellMe)
            {
                BuyMe = false;
                SellMe = false;
            }

            if (!UseAnyEntry && IndEntry > 1 && BuyMe_Temp && SellMe_Temp)
            {
                BuyMe_Temp = false;
                SellMe_Temp = false;
            }

            if ((ForceMarketCond_ == mktCond.uptrend || ForceMarketCond_ == mktCond.downtrend) && IndEntry == 0 && !FirstRun)
            {
                if (ForceMarketCond_ == mktCond.uptrend)
                    BuyMe_Temp = true;
                else if (ForceMarketCond_ == mktCond.downtrend)
                    SellMe_Temp = true;
            }

            if ((ForceMarketCond_ == mktCond.uptrend || ForceMarketCond_ == mktCond.downtrend) && IndEntry == 0 && CountTotalOpen == 0 && !FirstRun)
            {
                if (ForceMarketCond_ == mktCond.uptrend)
                    BuyMe = true;
                else if (ForceMarketCond_ == mktCond.downtrend)
                    SellMe = true;

                IndicatorUsed = " FMC ";
            }
        }

        private void ReadMACD()
        {
            if (MACDEntry_ != entType.disabled)
            {
                if (macd_.MACD.LastValue > macd_.Signal.LastValue)
                {
                    str_IndicatorDirection += ", MACD Bull";
                }
                else if (macd_.MACD.LastValue < macd_.Signal.LastValue)
                {
                    str_IndicatorDirection += ", MACD Bear";
                }
                else
                {
                    str_IndicatorDirection += ", MACD Range";
                }

                if (macd_.MACD.LastValue > macd_.Signal.LastValue)
                {
                    if (MACDEntry_ == entType.enabled)
                    {
                        if (ForceMarketCond_ != mktCond.downtrend && (UseAnyEntry || IndEntry == 0 || (!UseAnyEntry && IndEntry > 0 && BuyMe_Temp)))
                            BuyMe_Temp = true;
                        else
                            BuyMe_Temp = false;

                        if (!UseAnyEntry && IndEntry > 0 && SellMe_Temp && (!B3Traditional || (B3Traditional && Trend != 2)))
                            SellMe_Temp = false;
                    }
                    else if (MACDEntry_ == entType.reverse)
                    {
                        if (ForceMarketCond_ != mktCond.uptrend && (UseAnyEntry || IndEntry == 0 || (!UseAnyEntry && IndEntry > 0 && SellMe_Temp)))
                            SellMe_Temp = true;
                        else
                            SellMe_Temp = false;

                        if (!UseAnyEntry && IndEntry > 0 && BuyMe_Temp && (!B3Traditional || (B3Traditional && Trend != 2)))
                            BuyMe_Temp = false;
                    }
                }
                else if (macd_.MACD.LastValue < macd_.Signal.LastValue)
                {
                    if (MACDEntry_ == entType.enabled)
                    {
                        if (ForceMarketCond_ != mktCond.uptrend && (UseAnyEntry || IndEntry == 0 || (!UseAnyEntry && IndEntry > 0 && SellMe_Temp)))
                            SellMe_Temp = true;
                        else
                            SellMe_Temp = false;

                        if (!UseAnyEntry && IndEntry > 0 && BuyMe_Temp && (!B3Traditional || (B3Traditional && Trend != 2)))
                            BuyMe_Temp = false;
                    }
                    else if (MACDEntry_ == entType.reverse)
                    {
                        if (ForceMarketCond_ != mktCond.downtrend && (UseAnyEntry || IndEntry == 0 || (!UseAnyEntry && IndEntry > 0 && BuyMe_Temp)))
                            BuyMe_Temp = true;
                        else
                            BuyMe_Temp = false;

                        if (!UseAnyEntry && IndEntry > 0 && SellMe_Temp && (!B3Traditional || (B3Traditional && Trend != 2)))
                            SellMe_Temp = false;
                    }
                }
                else if (!UseAnyEntry && IndEntry > 0)
                {
                    BuyMe_Temp = false;
                    SellMe_Temp = false;
                }
            }

            if (MACDEntry_ != entType.disabled && CountTotalOpen == 0 && CountTotalPending < 2)
            {
                //double MACDm = iMACD(NULL, TF[MACD_TF], FastPeriod, SlowPeriod, SignalPeriod, MACDPrice, 0, 0);
                //double MACDs = iMACD(NULL, TF[MACD_TF], FastPeriod, SlowPeriod, SignalPeriod, MACDPrice, 1, 0);

                if (macd_.MACD.LastValue > macd_.Signal.LastValue)
                {
                    if (MACDEntry_ == entType.enabled)
                    {
                        if (ForceMarketCond_ != mktCond.downtrend && (UseAnyEntry || IndEntry == 0 || (!UseAnyEntry && IndEntry > 0 && BuyMe)))
                            BuyMe = true;
                        else
                            BuyMe = false;

                        if (!UseAnyEntry && IndEntry > 0 && SellMe && (!B3Traditional || (B3Traditional && Trend != 2)))
                            SellMe = false;
                    }
                    else if (MACDEntry_ == entType.reverse)
                    {
                        if (ForceMarketCond_ != mktCond.uptrend && (UseAnyEntry || IndEntry == 0 || (!UseAnyEntry && IndEntry > 0 && SellMe)))
                            SellMe = true;
                        else
                            SellMe = false;

                        if (!UseAnyEntry && IndEntry > 0 && BuyMe && (!B3Traditional || (B3Traditional && Trend != 2)))
                            BuyMe = false;
                    }
                }
                else if (macd_.MACD.LastValue < macd_.Signal.LastValue)
                {
                    if (MACDEntry_ == entType.enabled)
                    {
                        if (ForceMarketCond_ != mktCond.uptrend && (UseAnyEntry || IndEntry == 0 || (!UseAnyEntry && IndEntry > 0 && SellMe)))
                            SellMe = true;
                        else
                            SellMe = false;

                        if (!UseAnyEntry && IndEntry > 0 && BuyMe && (!B3Traditional || (B3Traditional && Trend != 2)))
                            BuyMe = false;
                    }
                    else if (MACDEntry_ == entType.reverse)
                    {
                        if (ForceMarketCond_ != mktCond.downtrend && (UseAnyEntry || IndEntry == 0 || (!UseAnyEntry && IndEntry > 0 && BuyMe)))
                            BuyMe = true;
                        else
                            BuyMe = false;

                        if (!UseAnyEntry && IndEntry > 0 && SellMe && (!B3Traditional || (B3Traditional && Trend != 2)))
                            SellMe = false;
                    }
                }
                else if (!UseAnyEntry && IndEntry > 0)
                {
                    BuyMe = false;
                    SellMe = false;
                }

                if (IndEntry > 0)
                    IndicatorUsed += UAE;

                IndEntry++;
                IndicatorUsed += " MACD ";
            }
        }

        private void ReadStochastic()
        {
            int zoneBUY = BuySellStochZone;
            int zoneSELL = 100 - BuySellStochZone;

            if (StochEntry_ != entType.disabled)
            {
                if (iStochastic_.PercentK.LastValue < zoneBUY && iStochastic_.PercentD.LastValue < zoneBUY)
                {
                    str_IndicatorDirection += ", ST Bull";
                }
                else if (iStochastic_.PercentK.LastValue > zoneSELL && iStochastic_.PercentD.LastValue > zoneSELL)
                {
                    str_IndicatorDirection += ", ST Bear";
                }
                else
                {
                    str_IndicatorDirection += ", ST Range";
                }

                if (iStochastic_.PercentK.LastValue < zoneBUY && iStochastic_.PercentD.LastValue < zoneBUY)
                {
                    if (StochEntry_ == entType.enabled)
                    {
                        if (ForceMarketCond_ != mktCond.downtrend && (UseAnyEntry || IndEntry == 0 || (!UseAnyEntry && IndEntry > 0 && BuyMe_Temp)))
                            BuyMe_Temp = true;
                        else
                            BuyMe_Temp = false;

                        if (!UseAnyEntry && IndEntry > 0 && SellMe_Temp && (!B3Traditional || (B3Traditional && Trend != 2)))
                            SellMe_Temp = false;
                    }
                    else if (StochEntry_ == entType.reverse)
                    {
                        if (ForceMarketCond_ != mktCond.uptrend && (UseAnyEntry || IndEntry == 0 || (!UseAnyEntry && IndEntry > 0 && SellMe_Temp)))
                            SellMe_Temp = true;
                        else
                            SellMe_Temp = false;

                        if (!UseAnyEntry && IndEntry > 0 && BuyMe_Temp && (!B3Traditional || (B3Traditional && Trend != 2)))
                            BuyMe_Temp = false;
                    }
                }
                else if (iStochastic_.PercentK.LastValue > zoneSELL && iStochastic_.PercentD.LastValue > zoneSELL)
                {
                    if (StochEntry_ == entType.enabled)
                    {
                        if (ForceMarketCond_ != mktCond.uptrend && (UseAnyEntry || IndEntry == 0 || (!UseAnyEntry && IndEntry > 0 && SellMe_Temp)))
                            SellMe_Temp = true;
                        else
                            SellMe_Temp = false;

                        if (!UseAnyEntry && IndEntry > 0 && BuyMe_Temp && (!B3Traditional || (B3Traditional && Trend != 2)))
                            BuyMe_Temp = false;
                    }
                    else if (StochEntry_ == entType.reverse)
                    {
                        if (ForceMarketCond_ != mktCond.downtrend && (UseAnyEntry || IndEntry == 0 || (!UseAnyEntry && IndEntry > 0 && BuyMe_Temp)))
                            BuyMe_Temp = true;
                        else
                            BuyMe_Temp = false;

                        if (!UseAnyEntry && IndEntry > 0 && SellMe_Temp && (!B3Traditional || (B3Traditional && Trend != 2)))
                            SellMe_Temp = false;
                    }
                }
                else if (!UseAnyEntry && IndEntry > 0)
                {
                    BuyMe_Temp = false;
                    SellMe_Temp = false;
                }
            }

            if (StochEntry_ != entType.disabled && CountTotalOpen == 0 && CountTotalPending < 2)
            {
                if (iStochastic_.PercentK.LastValue < zoneBUY && iStochastic_.PercentD.LastValue < zoneBUY)
                {
                    if (StochEntry_ == entType.enabled)
                    {
                        if (ForceMarketCond_ != mktCond.downtrend && (UseAnyEntry || IndEntry == 0 || (!UseAnyEntry && IndEntry > 0 && BuyMe)))
                            BuyMe = true;
                        else
                            BuyMe = false;

                        if (!UseAnyEntry && IndEntry > 0 && SellMe && (!B3Traditional || (B3Traditional && Trend != 2)))
                            SellMe = false;
                    }
                    else if (StochEntry_ == entType.reverse)
                    {
                        if (ForceMarketCond_ != mktCond.uptrend && (UseAnyEntry || IndEntry == 0 || (!UseAnyEntry && IndEntry > 0 && SellMe)))
                            SellMe = true;
                        else
                            SellMe = false;

                        if (!UseAnyEntry && IndEntry > 0 && BuyMe && (!B3Traditional || (B3Traditional && Trend != 2)))
                            BuyMe = false;
                    }
                }
                else if (iStochastic_.PercentK.LastValue > zoneSELL && iStochastic_.PercentD.LastValue > zoneSELL)
                {
                    if (StochEntry_ == entType.enabled)
                    {
                        if (ForceMarketCond_ != mktCond.uptrend && (UseAnyEntry || IndEntry == 0 || (!UseAnyEntry && IndEntry > 0 && SellMe)))
                            SellMe = true;
                        else
                            SellMe = false;

                        if (!UseAnyEntry && IndEntry > 0 && BuyMe && (!B3Traditional || (B3Traditional && Trend != 2)))
                            BuyMe = false;
                    }
                    else if (StochEntry_ == entType.reverse)
                    {
                        if (ForceMarketCond_ != mktCond.downtrend && (UseAnyEntry || IndEntry == 0 || (!UseAnyEntry && IndEntry > 0 && BuyMe)))
                            BuyMe = true;
                        else
                            BuyMe = false;

                        if (!UseAnyEntry && IndEntry > 0 && SellMe && (!B3Traditional || (B3Traditional && Trend != 2)))
                            SellMe = false;
                    }
                }
                else if (!UseAnyEntry && IndEntry > 0)
                {
                    BuyMe = false;
                    SellMe = false;
                }

                if (IndEntry > 0)
                    IndicatorUsed += UAE;

                IndEntry++;
                IndicatorUsed += " Stoch ";
            }
        }

        private void ReadBollinger()
        {
            double bup = ima_bb.Result.LastValue + (BollDeviation * std_bb.Result.LastValue);
            double bdn = ima_bb.Result.LastValue - (BollDeviation * std_bb.Result.LastValue);
            double bux = bup + BollDistance_;
            double bdx = bdn - BollDistance_;

            if (BollingerEntry_ != entType.disabled)
            {
                if (Symbol.Ask < bdx)
                {
                    str_IndicatorDirection += ", BB Bull";
                }
                else if (Symbol.Bid > bux)
                {
                    str_IndicatorDirection += ", BB Bear";
                }
                else
                {
                    str_IndicatorDirection += ", BB Range";
                }

                if (Symbol.Ask < bdx)
                {
                    if (BollingerEntry_ == entType.enabled)
                    {
                        if (ForceMarketCond_ != mktCond.downtrend && (UseAnyEntry || IndEntry == 0 || (!UseAnyEntry && IndEntry > 0 && BuyMe_Temp)))
                            BuyMe_Temp = true;
                        else
                            BuyMe_Temp = false;

                        if (!UseAnyEntry && IndEntry > 0 && SellMe_Temp && (!B3Traditional || (B3Traditional && Trend != 2)))
                            SellMe_Temp = false;
                    }
                    else if (BollingerEntry_ == entType.reverse)
                    {
                        if (ForceMarketCond_ != mktCond.uptrend && (UseAnyEntry || IndEntry == 0 || (!UseAnyEntry && IndEntry > 0 && SellMe_Temp)))
                            SellMe_Temp = true;
                        else
                            SellMe_Temp = false;

                        if (!UseAnyEntry && IndEntry > 0 && BuyMe_Temp && (!B3Traditional || (B3Traditional && Trend != 2)))
                            BuyMe_Temp = false;
                    }
                }
                else if (Symbol.Bid > bux)
                {
                    if (BollingerEntry_ == entType.enabled)
                    {
                        if (ForceMarketCond_ != mktCond.uptrend && (UseAnyEntry || IndEntry == 0 || (!UseAnyEntry && IndEntry > 0 && SellMe_Temp)))
                            SellMe_Temp = true;
                        else
                            SellMe_Temp = false;

                        if (!UseAnyEntry && IndEntry > 0 && BuyMe_Temp && (!B3Traditional || (B3Traditional && Trend != 2)))
                            BuyMe_Temp = false;
                    }
                    else if (BollingerEntry_ == entType.reverse)
                    {
                        if (ForceMarketCond_ != mktCond.downtrend && (UseAnyEntry || IndEntry == 0 || (!UseAnyEntry && IndEntry > 0 && BuyMe_Temp)))
                            BuyMe_Temp = true;
                        else
                            BuyMe_Temp = false;

                        if (!UseAnyEntry && IndEntry > 0 && SellMe_Temp && (!B3Traditional || (B3Traditional && Trend != 2)))
                            SellMe_Temp = false;
                    }
                }
                else if (!UseAnyEntry && IndEntry > 0)
                {
                    BuyMe_Temp = false;
                    SellMe_Temp = false;
                }
            }

            if (BollingerEntry_ != entType.disabled && CountTotalOpen == 0 && CountTotalPending < 2)
            {
                if (Symbol.Ask < bdx)
                {
                    if (BollingerEntry_ == entType.enabled)
                    {
                        if (ForceMarketCond_ != mktCond.downtrend && (UseAnyEntry || IndEntry == 0 || (!UseAnyEntry && IndEntry > 0 && BuyMe)))
                            BuyMe = true;
                        else
                            BuyMe = false;

                        if (!UseAnyEntry && IndEntry > 0 && SellMe && (!B3Traditional || (B3Traditional && Trend != 2)))
                            SellMe = false;
                    }
                    else if (BollingerEntry_ == entType.reverse)
                    {
                        if (ForceMarketCond_ != mktCond.uptrend && (UseAnyEntry || IndEntry == 0 || (!UseAnyEntry && IndEntry > 0 && SellMe)))
                            SellMe = true;
                        else
                            SellMe = false;

                        if (!UseAnyEntry && IndEntry > 0 && BuyMe && (!B3Traditional || (B3Traditional && Trend != 2)))
                            BuyMe = false;
                    }
                }
                else if (Symbol.Bid > bux)
                {
                    if (BollingerEntry_ == entType.enabled)
                    {
                        if (ForceMarketCond_ != mktCond.uptrend && (UseAnyEntry || IndEntry == 0 || (!UseAnyEntry && IndEntry > 0 && SellMe)))
                            SellMe = true;
                        else
                            SellMe = false;

                        if (!UseAnyEntry && IndEntry > 0 && BuyMe && (!B3Traditional || (B3Traditional && Trend != 2)))
                            BuyMe = false;
                    }
                    else if (BollingerEntry_ == entType.reverse)
                    {
                        if (ForceMarketCond_ != mktCond.downtrend && (UseAnyEntry || IndEntry == 0 || (!UseAnyEntry && IndEntry > 0 && BuyMe)))
                            BuyMe = true;
                        else
                            BuyMe = false;

                        if (!UseAnyEntry && IndEntry > 0 && SellMe && (!B3Traditional || (B3Traditional && Trend != 2)))
                            SellMe = false;
                    }
                }
                else if (!UseAnyEntry && IndEntry > 0)
                {
                    BuyMe = false;
                    SellMe = false;
                }

                if (IndEntry > 0)
                    IndicatorUsed += UAE;

                IndEntry++;
                IndicatorUsed += " BBands ";
            }
        }

        private void ReadCCI()
        {
            if (CCIEntry_ != entType.disabled)
            {
                if (cci_1.Result.Last(2) > 0 && cci_2.Result.Last(2) > 0 && cci_3.Result.Last(2) > 0 && cci_4.Result.Last(2) > 0 && cci_1.Result.LastValue > 0 && cci_2.Result.LastValue > 0 && cci_3.Result.LastValue > 0 && cci_4.Result.LastValue > 0)
                {
                    str_IndicatorDirection += ", CCI Bull";
                }
                else if (cci_1.Result.Last(2) < 0 && cci_2.Result.Last(2) < 0 && cci_3.Result.Last(2) < 0 && cci_4.Result.Last(2) < 0 && cci_1.Result.LastValue < 0 && cci_2.Result.LastValue < 0 && cci_3.Result.LastValue < 0 && cci_4.Result.LastValue < 0)
                {
                    str_IndicatorDirection += ", CCI Bear";
                }
                else
                {
                    str_IndicatorDirection += ", CCI Range";
                }


                if (cci_1.Result.Last(2) > 0 && cci_2.Result.Last(2) > 0 && cci_3.Result.Last(2) > 0 && cci_4.Result.Last(2) > 0 && cci_1.Result.LastValue > 0 && cci_2.Result.LastValue > 0 && cci_3.Result.LastValue > 0 && cci_4.Result.LastValue > 0)
                {
                    if (ForceMarketCond_ == mktCond.automatic)
                        Trend = 0;

                    if (CCIEntry_ == entType.enabled)
                    {
                        if (ForceMarketCond_ != mktCond.downtrend && (UseAnyEntry || IndEntry == 0 || (!UseAnyEntry && IndEntry > 0 && BuyMe_Temp)))
                            BuyMe_Temp = true;
                        else
                            BuyMe_Temp = false;

                        if (!UseAnyEntry && IndEntry > 0 && SellMe_Temp && (!B3Traditional || (B3Traditional && Trend != 2)))
                            SellMe_Temp = false;
                    }
                    else if (CCIEntry_ == entType.reverse)
                    {
                        if (ForceMarketCond_ != 0 && (UseAnyEntry || IndEntry == 0 || (!UseAnyEntry && IndEntry > 0 && SellMe_Temp)))
                            SellMe_Temp = true;
                        else
                            SellMe_Temp = false;

                        if (!UseAnyEntry && IndEntry > 0 && BuyMe_Temp && (!B3Traditional || (B3Traditional && Trend != 2)))
                            BuyMe_Temp = false;
                    }
                }
                else if (cci_1.Result.Last(2) < 0 && cci_2.Result.Last(2) < 0 && cci_3.Result.Last(2) < 0 && cci_4.Result.Last(2) < 0 && cci_1.Result.LastValue < 0 && cci_2.Result.LastValue < 0 && cci_3.Result.LastValue < 0 && cci_4.Result.LastValue < 0)
                {
                    if (ForceMarketCond_ == mktCond.automatic)
                        Trend = 1;

                    if (CCIEntry_ == entType.enabled)
                    {
                        if (ForceMarketCond_ != mktCond.uptrend && (UseAnyEntry || IndEntry == 0 || (!UseAnyEntry && IndEntry > 0 && SellMe_Temp)))
                            SellMe_Temp = true;
                        else
                            SellMe_Temp = false;

                        if (!UseAnyEntry && IndEntry > 0 && BuyMe_Temp && (!B3Traditional || (B3Traditional && Trend != 2)))
                            BuyMe_Temp = false;
                    }
                    else if (CCIEntry_ == entType.reverse)
                    {
                        if (ForceMarketCond_ != mktCond.downtrend && (UseAnyEntry || IndEntry == 0 || (!UseAnyEntry && IndEntry > 0 && BuyMe_Temp)))
                            BuyMe_Temp = true;
                        else
                            BuyMe_Temp = false;

                        if (!UseAnyEntry && IndEntry > 0 && SellMe_Temp && (!B3Traditional || (B3Traditional && Trend != 2)))
                            SellMe_Temp = false;
                    }
                }
                else if (!UseAnyEntry && IndEntry > 0)
                {
                    BuyMe_Temp = false;
                    SellMe_Temp = false;
                }
            }

            if (CCIEntry_ != entType.disabled && CountTotalOpen == 0 && CountTotalPending < 2)
            {

                if (cci_1.Result.Last(2) > 0 && cci_2.Result.Last(2) > 0 && cci_3.Result.Last(2) > 0 && cci_4.Result.Last(2) > 0 && cci_1.Result.LastValue > 0 && cci_2.Result.LastValue > 0 && cci_3.Result.LastValue > 0 && cci_4.Result.LastValue > 0)
                {
                    if (ForceMarketCond_ == mktCond.automatic)
                        Trend = 0;

                    if (CCIEntry_ == entType.enabled)
                    {
                        if (ForceMarketCond_ != mktCond.downtrend && (UseAnyEntry || IndEntry == 0 || (!UseAnyEntry && IndEntry > 0 && BuyMe)))
                            BuyMe = true;
                        else
                            BuyMe = false;

                        if (!UseAnyEntry && IndEntry > 0 && SellMe && (!B3Traditional || (B3Traditional && Trend != 2)))
                            SellMe = false;
                    }
                    else if (CCIEntry_ == entType.reverse)
                    {
                        if (ForceMarketCond_ != 0 && (UseAnyEntry || IndEntry == 0 || (!UseAnyEntry && IndEntry > 0 && SellMe)))
                            SellMe = true;
                        else
                            SellMe = false;

                        if (!UseAnyEntry && IndEntry > 0 && BuyMe && (!B3Traditional || (B3Traditional && Trend != 2)))
                            BuyMe = false;
                    }
                }
                else if (cci_1.Result.Last(2) < 0 && cci_2.Result.Last(2) < 0 && cci_3.Result.Last(2) < 0 && cci_4.Result.Last(2) < 0 && cci_1.Result.LastValue < 0 && cci_2.Result.LastValue < 0 && cci_3.Result.LastValue < 0 && cci_4.Result.LastValue < 0)
                {
                    if (ForceMarketCond_ == mktCond.automatic)
                        Trend = 1;

                    if (CCIEntry_ == entType.enabled)
                    {
                        if (ForceMarketCond_ != mktCond.uptrend && (UseAnyEntry || IndEntry == 0 || (!UseAnyEntry && IndEntry > 0 && SellMe)))
                            SellMe = true;
                        else
                            SellMe = false;

                        if (!UseAnyEntry && IndEntry > 0 && BuyMe && (!B3Traditional || (B3Traditional && Trend != 2)))
                            BuyMe = false;
                    }
                    else if (CCIEntry_ == entType.reverse)
                    {
                        if (ForceMarketCond_ != mktCond.downtrend && (UseAnyEntry || IndEntry == 0 || (!UseAnyEntry && IndEntry > 0 && BuyMe)))
                            BuyMe = true;
                        else
                            BuyMe = false;

                        if (!UseAnyEntry && IndEntry > 0 && SellMe && (!B3Traditional || (B3Traditional && Trend != 2)))
                            SellMe = false;
                    }
                }
                else if (!UseAnyEntry && IndEntry > 0)
                {
                    BuyMe = false;
                    SellMe = false;
                }

                if (IndEntry > 0)
                    IndicatorUsed += UAE;

                IndEntry++;
                IndicatorUsed += " CCI ";
            }
        }

        private void ReadMA()
        {
            if (ForceMarketCond_ == mktCond.automatic)
            {
                if (Symbol.Bid > ima_.Result.LastValue + MADistance_)
                {
                    Trend = 0;
                    if (MAEntry_ != entType.disabled) { str_IndicatorDirection += "MA Bull"; }
                }
                else if (Symbol.Ask < ima_.Result.LastValue - MADistance_)
                {
                    Trend = 1;
                    if (MAEntry_ != entType.disabled) { str_IndicatorDirection += "MA Bear"; }
                }
                else
                {
                    Trend = 2;
                    if (MAEntry_ != entType.disabled) { str_IndicatorDirection += "MA Range"; }
                }
            }
            else
            {
                Trend = (int)ForceMarketCond_;

                if (Trend != 0 && Symbol.Bid > ima_.Result.LastValue + MADistance_)
                {
                    ATrend = "U";
                    if (MAEntry_ != entType.disabled) { str_IndicatorDirection += "MA Bull"; }
                }

                if (Trend != 1 && Symbol.Ask < ima_.Result.LastValue - MADistance_)
                {
                    ATrend = "D";
                    if (MAEntry_ != entType.disabled) { str_IndicatorDirection += "MA Bear"; }
                }
                if (Trend != 2 && (Symbol.Bid < ima_.Result.LastValue + MADistance_ && Symbol.Ask > ima_.Result.LastValue - MADistance_))
                {
                    ATrend = "R";
                    if (MAEntry_ != entType.disabled) { str_IndicatorDirection += "MA Range"; }
                }
            }

            if (MAEntry_ != entType.disabled)
            {
                if (Symbol.Bid > ima_.Result.LastValue + MADistance_ && (!B3Traditional || (B3Traditional && Trend != 2)))
                {
                    if (MAEntry_ == entType.enabled)
                    {
                        if (ForceMarketCond_ != mktCond.downtrend && (UseAnyEntry || IndEntry == 0 || (!UseAnyEntry && IndEntry > 0 && BuyMe_Temp)))
                            BuyMe_Temp = true;
                        else
                            BuyMe_Temp = false;

                        if (!UseAnyEntry && IndEntry > 0 && SellMe_Temp && (!B3Traditional || (B3Traditional && Trend != 2)))
                            SellMe_Temp = false;
                    }
                    else if (MAEntry_ == entType.reverse)
                    {
                        if (ForceMarketCond_ != mktCond.uptrend && (UseAnyEntry || IndEntry == 0 || (!UseAnyEntry && IndEntry > 0 && SellMe_Temp)))
                            SellMe_Temp = true;
                        else
                            SellMe_Temp = false;

                        if (!UseAnyEntry && IndEntry > 0 && BuyMe_Temp && (!B3Traditional || (B3Traditional && Trend != 2)))
                            BuyMe_Temp = false;
                    }
                }
                else if (Symbol.Ask < ima_.Result.LastValue - MADistance_ && (!B3Traditional || (B3Traditional && Trend != 2)))
                {
                    if (MAEntry_ == entType.enabled)
                    {
                        if (ForceMarketCond_ != mktCond.uptrend && (UseAnyEntry || IndEntry == 0 || (!UseAnyEntry && IndEntry > 0 && SellMe_Temp)))
                            SellMe_Temp = true;
                        else
                            SellMe_Temp = false;

                        if (!UseAnyEntry && IndEntry > 0 && BuyMe_Temp && (!B3Traditional || (B3Traditional && Trend != 2)))
                            BuyMe_Temp = false;
                    }
                    else if (MAEntry_ == entType.reverse)
                    {
                        if (ForceMarketCond_ != mktCond.downtrend && (UseAnyEntry || IndEntry == 0 || (!UseAnyEntry && IndEntry > 0 && BuyMe_Temp)))
                            BuyMe_Temp = true;
                        else
                            BuyMe_Temp = false;

                        if (!UseAnyEntry && IndEntry > 0 && SellMe_Temp && (!B3Traditional || (B3Traditional && Trend != 2)))
                            SellMe_Temp = false;
                    }
                }
                else if (B3Traditional && Trend == 2)
                {
                    if (ForceMarketCond_ != mktCond.downtrend && (UseAnyEntry || IndEntry == 0 || (!UseAnyEntry && IndEntry > 0 && BuyMe_Temp)))
                        BuyMe_Temp = true;

                    if (ForceMarketCond_ != mktCond.uptrend && (UseAnyEntry || IndEntry == 0 || (!UseAnyEntry && IndEntry > 0 && SellMe_Temp)))
                        SellMe_Temp = true;
                }
                else
                {
                    BuyMe_Temp = false;
                    SellMe_Temp = false;
                }


                if (MAEntry_ != entType.disabled && CountTotalOpen == 0 && CountTotalPending < 2)
                {
                    if (Symbol.Bid > ima_.Result.LastValue + MADistance_ && (!B3Traditional || (B3Traditional && Trend != 2)))
                    {
                        if (MAEntry_ == entType.enabled)
                        {
                            if (ForceMarketCond_ != mktCond.downtrend && (UseAnyEntry || IndEntry == 0 || (!UseAnyEntry && IndEntry > 0 && BuyMe)))
                                BuyMe = true;
                            else
                                BuyMe = false;

                            if (!UseAnyEntry && IndEntry > 0 && SellMe && (!B3Traditional || (B3Traditional && Trend != 2)))
                                SellMe = false;
                        }
                        else if (MAEntry_ == entType.reverse)
                        {
                            if (ForceMarketCond_ != mktCond.uptrend && (UseAnyEntry || IndEntry == 0 || (!UseAnyEntry && IndEntry > 0 && SellMe)))
                                SellMe = true;
                            else
                                SellMe = false;

                            if (!UseAnyEntry && IndEntry > 0 && BuyMe && (!B3Traditional || (B3Traditional && Trend != 2)))
                                BuyMe = false;
                        }
                    }
                    else if (Symbol.Ask < ima_.Result.LastValue - MADistance_ && (!B3Traditional || (B3Traditional && Trend != 2)))
                    {
                        if (MAEntry_ == entType.enabled)
                        {
                            if (ForceMarketCond_ != mktCond.uptrend && (UseAnyEntry || IndEntry == 0 || (!UseAnyEntry && IndEntry > 0 && SellMe)))
                                SellMe = true;
                            else
                                SellMe = false;

                            if (!UseAnyEntry && IndEntry > 0 && BuyMe && (!B3Traditional || (B3Traditional && Trend != 2)))
                                BuyMe = false;
                        }
                        else if (MAEntry_ == entType.reverse)
                        {
                            if (ForceMarketCond_ != mktCond.downtrend && (UseAnyEntry || IndEntry == 0 || (!UseAnyEntry && IndEntry > 0 && BuyMe)))
                                BuyMe = true;
                            else
                                BuyMe = false;

                            if (!UseAnyEntry && IndEntry > 0 && SellMe && (!B3Traditional || (B3Traditional && Trend != 2)))
                                SellMe = false;
                        }
                    }
                    else if (B3Traditional && Trend == 2)
                    {
                        if (ForceMarketCond_ != mktCond.downtrend && (UseAnyEntry || IndEntry == 0 || (!UseAnyEntry && IndEntry > 0 && BuyMe)))
                            BuyMe = true;

                        if (ForceMarketCond_ != mktCond.uptrend && (UseAnyEntry || IndEntry == 0 || (!UseAnyEntry && IndEntry > 0 && SellMe)))
                            SellMe = true;
                    }
                    else
                    {
                        BuyMe = false;
                        SellMe = false;
                    }

                    if (IndEntry > 0)
                        IndicatorUsed = IndicatorUsed + UAE;

                    IndEntry++;
                    IndicatorUsed = IndicatorUsed + " MA ";
                }
            }
        }

        #endregion Read Indicies

        //---------------------------------------------------------------------- Bot Functions ----------------------------------------------------------------------\\
        #region Bot Functions

        private void HedgeSetup()
        {
            if ((UseHedge_ && CountTotalOpen > 0) || CountTotalHedge > 0)
            {
                if (hDDStart == 0 && CountTotalHedge > 0)
                    hDDStart = Math.Max(HedgeStart_, DrawDownPC);

                if (hDDStart > HedgeStart_ && hDDStart > DrawDownPC)
                    hDDStart = DrawDownPC;

                if (hActive == false)
                {
                    double OrderLot = 0;

                    if (DrawDownPC > hDDStart)
                    {
                        OrderLot = LotSize(LbT * hLotMult);

                        if (CountOpenSell > 0)
                        {
                            int Ticket = SendMarketOrder(TradeType.Buy, OrderLot, str_HedgeBuy, Color.MidnightBlue);

                            if (Ticket > 0)
                            {
                                hActive = true;

                                if (PrintMessagesInLog)
                                    Print("Hedge Buy: Stoploss @ ", DTS(SLh, Symbol.Digits));
                            }
                        }

                        if (CountOpenBuy > 0)
                        {
                            int Ticket = SendMarketOrder(TradeType.Sell, OrderLot, str_HedgeSell, Color.Maroon);

                            if (Ticket > 0)
                            {
                                if (PrintMessagesInLog)
                                    Print("Hedge Sell: Stoploss @ ", DTS(SLh, Symbol.Digits));

                                hActive = true;
                            }
                        }
                    }
                }
                else if (hActive == true)
                {
                    if (GetProfitOnAllOpenPositions() > 0)
                    {
                        ExitTrades(enumPosType.All, "Hedge Breakeven");
                    }
                }

            }
        }

        private int TradingSelection()
        {
            double OrderLot = 0;
            double Entry;
            int Ticket = 0;
            double StopLevel = Symbol.PipSize;

            try
            {
                OrderLot = LotSize(Lots[Decimal.ToInt32(Decimal.Round(Math.Min(CountTotalOpen + CountTotalClosed, MaxTrades - 1), 0))] * LotMult);
            }
            catch (OverflowException e)
            {
                OrderLot = Lots[0];
                Print("Order Lot Size Error in TradeSelection Method. " + e.Message);
            }

            if (CountTotalOpen == 0 && CountTotalPending < 2)
            {
                if (B3Traditional)
                {
                    if (BuyMe)
                    {
                        if (CountBuyStop == 0 && CountSellLimit == 0 && ((Trend != 2 || MAEntry_ == entType.disabled) || (Trend == 2 && MAEntry_ == entType.enabled)))
                        {
                            Entry = g2 - (Symbol.Ask % g2) + EntryOffset_;

                            if (Entry > StopLevel)
                            {
                                Ticket = SendStopOrder(TradeType.Buy, OrderLot, Entry, Color.White);

                                if (Ticket > 0)
                                {
                                    if (PrintMessagesInLog)
                                        Print("Indicator Entry - (", IndicatorUsed, ") BuyStop MC = ", Trend);

                                    CountBuyStop++;
                                }
                            }
                        }

                        if (CountBuyLimit == 0 && CountSellStop == 0 && ((Trend != 2 || MAEntry_ == entType.disabled) || (Trend == 2 && MAEntry_ == entType.reverse)))
                        {
                            Entry = (Symbol.Ask % g2) + EntryOffset_;

                            if (Entry > StopLevel)
                            {
                                Ticket = SendLimitOrder(TradeType.Buy, OrderLot, -Entry, Color.White);

                                if (Ticket > 0)
                                {
                                    if (PrintMessagesInLog)
                                        Print("Indicator Entry - (", IndicatorUsed, ") BuyLimit MC = ", Trend);

                                    CountBuyLimit++;
                                }
                            }
                        }
                    }

                    if (SellMe)
                    {
                        if (CountSellLimit == 0 && CountBuyStop == 0 && ((Trend != 2 || MAEntry_ == entType.disabled) || (Trend == 2 && MAEntry_ == entType.reverse)))
                        {
                            Entry = g2 - (Symbol.Bid % g2) + EntryOffset_;

                            if (Entry > StopLevel)
                            {
                                Ticket = SendLimitOrder(TradeType.Sell, OrderLot, Entry, Color.White);

                                if (Ticket > 0 && PrintMessagesInLog)
                                    Print("Indicator Entry - (", IndicatorUsed, ") SellLimit MC = ", Trend);
                            }
                        }

                        if (CountSellStop == 0 && CountBuyLimit == 0 && ((Trend != 2 || MAEntry_ == entType.disabled) || (Trend == 2 && MAEntry_ == entType.enabled)))
                        {
                            Entry = (Symbol.Bid % g2) + EntryOffset_;

                            if (Entry > StopLevel)
                            {
                                Ticket = SendStopOrder(TradeType.Sell, OrderLot, -Entry, Color.White);

                                if (Ticket > 0 && PrintMessagesInLog)
                                    Print("Indicator Entry - (", IndicatorUsed, ") SellStop MC = ", Trend);
                            }
                        }
                    }
                }
                else
                {
                    if (BuyMe)
                    {
                        Ticket = SendMarketOrder(TradeType.Buy, OrderLot, str_Default_TradeComment, Color.DeepSkyBlue);

                        if (Ticket > 0 && PrintMessagesInLog)
                            Print("Indicator Entry - (", IndicatorUsed, ") Buy");
                    }
                    else if (SellMe)
                    {
                        Ticket = SendMarketOrder(TradeType.Sell, OrderLot, str_Default_TradeComment, Color.DarkOrange);

                        if (Ticket > 0 && PrintMessagesInLog)
                            Print("Indicator Entry - (", IndicatorUsed, ") Sell");
                    }

                }

                if (Ticket > 0)
                    return (0);
            }
            else if (Server.Time.AddSeconds(-EntryDelay) > OrderOpenTimeLast && CountTotalOpen + CountTotalClosed < MaxTrades)
            {
                if (CountOpenBuy > 0)
                {
                    if (OPbL > Symbol.Ask)
                        Entry = OPbL - (Math.Round((OPbL - Symbol.Ask) / g2) + 1) * g2;
                    else
                        Entry = OPbL - g2;

                    if (UseSmartGrid)
                    {
                        if (Symbol.Ask < OPbL - g2)
                        {
                            //if (rsi_.Result.LastValue > rsima_.RSI_Result_MA.LastValue)
                            //{
                                Ticket = SendMarketOrder(TradeType.Buy, OrderLot, str_Default_TradeComment, Color.DeepSkyBlue);

                            if (Ticket > 0 && PrintMessagesInLog)
                                Print("Sending Buy Order SmartGrid");
                                    //Print("SmartGrid Buy RSI: ", rsi_.Result.LastValue.ToString(), " > MA: ", rsima_.RSI_Result_MA.LastValue.ToString());
                            //}

                            OPbN = 0;
                        }
                        else
                            OPbN = OPbL - g2;
                    }
                    else if (CountBuyLimit == 0)
                    {
                        if (Symbol.Ask - Entry <= StopLevel)
                            Entry = OPbL - (int)Math.Round((((OPbL - Symbol.Ask + StopLevel) / g2) + 1) * g2, 0);

                        Ticket = SendLimitOrder(TradeType.Buy, OrderLot, Entry - Symbol.Ask, Color.SkyBlue);

                        if (Ticket > 0 && PrintMessagesInLog)
                            Print("BuyLimit grid");
                    }
                    else if (CountBuyLimit == 1 && Entry - OPpBL > g2 / 2 && Symbol.Ask - Entry > StopLevel)
                    {
                        foreach (var order in PendingOrders)
                        {
                            if (order.Label == str_BuyLimit && order.SymbolName == Symbol.Name)
                            {
                                double newPrice = Symbol.Ask + 5 * Symbol.PipSize;
                                var trResult = ModifyPendingOrder(order, Entry);

                                if (!trResult.IsSuccessful)
                                {
                                    if (PrintMessagesInLog)
                                        Print("Error modifying pending order. " + trResult.Error);
                                }
                            }
                        }
                    }
                }
                else if (CountOpenSell > 0)
                {
                    if (Symbol.Bid > OPbL)
                        Entry = OPbL + (Math.Round((-OPbL + Symbol.Bid) / g2) + 1) * g2;
                    else
                        Entry = OPbL + g2;

                    if (UseSmartGrid)
                    {
                        if (Symbol.Bid > OPbL + g2)
                        {
                            //if (rsi_.Result.LastValue < rsima_.RSI_Result_MA.LastValue)
                            //{
                                Ticket = SendMarketOrder(TradeType.Sell, OrderLot, str_Default_TradeComment, Color.OrangeRed);

                                if (Ticket > 0 && PrintMessagesInLog)
                                Print("Sending Sell Order SmartGrid");
                            //Print("SmartGrid Sell RSI: ", rsi_.Result.LastValue.ToString(), " < MA: ", rsima_.RSI_Result_MA.LastValue.ToString());
                            //}

                            OPbN = 0;
                        }
                        else
                            OPbN = OPbL + g2;
                    }
                    else if (CountSellLimit == 0)
                    {
                        if (Entry - Symbol.Bid <= StopLevel)
                            Entry = OPbL + (int)Math.Round(((-OPbL + Symbol.Bid + StopLevel) / g2) + 1) * g2;

                        Ticket = SendLimitOrder(TradeType.Sell, OrderLot, Entry - Symbol.Bid, Color.Coral);

                        if (Ticket > 0 && PrintMessagesInLog)
                            Print("SellLimit grid");
                    }
                    else if (CountSellLimit == 1 && OPpSL - Entry > g2 / 2 && Entry - Symbol.Bid > StopLevel)
                    {
                        foreach (var order in PendingOrders)
                        {
                            if (order.Label == str_SellLimit)
                            {
                                var trResult = ModifyPendingOrder(order, Entry);

                                if (!trResult.IsSuccessful)
                                {
                                    if (PrintMessagesInLog)
                                        Print("Error modifying pending order. " + trResult.Error);
                                }
                            }
                        }
                    }
                }

                if (Ticket > 0)
                    return (0);
            }

            return (0);
        }

        private void DeleteWrongDir()
        {
            if (CountTotalOpen == 0 && CountTotalPending > 0)
            {
                bool closeBull = false;
                bool closeBear = false;

                if (BuyMe_Temp && !SellMe_Temp) { closeBear = true; }
                if (!BuyMe_Temp && SellMe_Temp) { closeBull = true; }

                foreach (var order in PendingOrders)
                {
                    if (order.SymbolName == SymbolName)
                    {
                        if (order.TradeType == TradeType.Buy && closeBull)
                        {
                            TradeResult trResult = order.Cancel();
                            if (trResult.IsSuccessful)
                            {
                                if (PrintMessagesInLog)
                                    Print("Delete pending buy order - trend is down");
                            }
                        }

                        if (order.TradeType == TradeType.Sell && closeBear)
                        {
                            TradeResult trResult = order.Cancel();
                            if (trResult.IsSuccessful)
                            {
                                if (PrintMessagesInLog)
                                    Print("Delete pending sell order - trend is up");
                            }
                        }
                    }
                }
            }
        }

        private void CalculateGridATR()
        {
            if (AutoCal)
            {
                double GridATR = atr_.Result.LastValue / Pip;

                if ((CountTotalOpen + CountTotalClosed > Set4Level) && Set4Level > 0)
                {
                    g2 = GridATR * 12;
                    //GS*2*2*2*1.5
                    tp2 = GridATR * 18;
                    //GS*2*2*2*1.5*1.5
                }
                else if ((CountTotalOpen + CountTotalClosed > Set3Level) && Set3Level > 0)
                {
                    g2 = GridATR * 8;
                    //GS*2*2*2
                    tp2 = GridATR * 12;
                    //GS*2*2*2*1.5
                }
                else if ((CountTotalOpen + CountTotalClosed > Set2Level) && Set2Level > 0)
                {
                    g2 = GridATR * 4;
                    //GS*2*2
                    tp2 = GridATR * 8;
                    //GS*2*2*2
                }
                else if ((CountTotalOpen + CountTotalClosed > Set1Level) && Set1Level > 0)
                {
                    g2 = GridATR * 2;
                    //GS*2
                    tp2 = GridATR * 4;
                    //GS*2*2
                }
                else
                {
                    g2 = GridATR;
                    tp2 = GridATR * 2;
                }

                GridTP = GridATR * 2;
            }
            else
            {
                int Index = (int)Math.Max(Math.Min(CountTotalOpen + CountTotalClosed, MaxTrades) - 1, 0);
                g2 = GridArray0[Index];
                tp2 = GridArray1[Index];
                GridTP = GridArray1[0];
            }

            g2 = Math.Round(Math.Max(g2 * GAF * Pip, Pip), Symbol.Digits);
            tp2 = Math.Round(tp2 * GAF * Pip, Symbol.Digits);
            GridTP = Math.Round(GridTP * GAF * Pip, Symbol.Digits);
        }

        private void DeleteHangOrders()
        {
            if (CountTotalOpen == 0 && !PendLot)
            {
                PendLot = true;

                foreach (var order in PendingOrders)
                {
                    if (order.SymbolName == SymbolName)
                    {
                        if (Math.Round(Symbol.VolumeInUnitsToQuantity(order.VolumeInUnits), LotDecimal) > Math.Round(Lots[0] * LotMult, LotDecimal))
                        {
                            TradeResult trResult = order.Cancel();
                            if (trResult.IsSuccessful)
                            {
                                if (PrintMessagesInLog)
                                    Print("Delete pending > Lot");
                            }
                        }
                    }
                }

                return;
            }
            else if ((CountTotalOpen > 0 || (CountTotalOpen == 0 && CountTotalPending > 0 && !B3Traditional)) && PendLot)
            {
                PendLot = false;

                foreach (var order in PendingOrders)
                {
                    if (order.SymbolName == SymbolName)
                    {
                        if (Math.Round(Symbol.VolumeInUnitsToQuantity(order.VolumeInUnits), LotDecimal) == Math.Round(Lots[0] * LotMult, LotDecimal))
                        {
                            PendLot = true;

                            TradeResult trResult = order.Cancel();
                            if (trResult.IsSuccessful)
                            {
                                PendLot = false;

                                if (PrintMessagesInLog)
                                    Print("Delete pending = Lot");
                            }
                        }
                    }
                }

                return;
            }

            return;
        }

        private void TrailingStop_()
        {
            CalculateEarlyExit();
            AdjustBEb();

            BEa = BEb;
            nLots = LbB - LbS;
            TPa = TakeProfitb;

            double SLb;
            double TPbMP;

            if (MaximizeProfit && !FirstRun && CountTotalOpen > 0)
            {
                SLb = Math.Round(BEa + (TPa - BEa) * ProfitSet_, Symbol.Digits);

                if (Pb + Ph < 0 && SLb > 0)
                    SLb = 0;

                if (SLb > 0 && ((nLots > 0 && Symbol.Bid < SLb) || (nLots < 0 && Symbol.Ask > SLb)))
                {
                    ExitTrades(enumPosType.All, "Profit Trailing Stop Reached (" + DTS(ProfitSet_ * 100, 2) + "%)");
                    return;
                }

                if (TotalProfitTarget > 0)
                {
                    TPbMP = Math.Round(BEa + (TPa - BEa) * ProfitSet_, Symbol.Digits);

                    if ((nLots > 0 && Symbol.Bid > TPbMP) || (nLots < 0 && Symbol.Ask < TPbMP))
                        SLb = TPbMP;
                }

                if (SLb > 0 && SLb != SLbL && MoveTP_ > 0 && TotalMoves > Moves)
                {
                    TakeProfitb = 0;
                    Moves++;

                    if (PrintMessagesInLog)
                        Print("MoveTP");

                    SLbL = SLb;

                    return;
                }

            }

            if (!FirstRun && TPaF > 0)
            {
                if (hActive == false)
                {
                    if ((nLots > 0 && Symbol.Bid >= TPaF) || (nLots < 0 && Symbol.Ask <= TPaF))
                    {
                        ExitTrades(enumPosType.All, "Profit Target Reached @ " + DTS(TPaF, Symbol.Digits));

                        return;
                    }
                }
                //hedge is Active
                else if (hActive == true)
                {
                    if (GetProfitOnAllOpenPositions() > 0)
                    {
                        ExitTrades(enumPosType.All, "Break Even on Active Hedge.");

                        return;
                    }
                }
            }

            if (!FirstRun && UseStopLoss)
            {
                double bSL;
                BEa = BEb;

                if (SLPips_ > 0)
                {
                    if (nLots > 0)
                    {
                        bSL = BEa - SLPips_;

                        if (Symbol.Bid <= bSL)
                        {
                            ExitTrades(enumPosType.All, "Stop Loss Reached");

                            return;
                        }
                    }
                    else if (nLots < 0)
                    {
                        bSL = BEa + SLPips_;

                        if (Symbol.Ask >= bSL)
                        {
                            ExitTrades(enumPosType.All, "Stop Loss Reached");

                            return;
                        }
                    }
                }

                if (TSLPips_ != 0)
                {
                    if (nLots > 0)
                    {
                        bTS = 0;
                        if (TSLPips_ > 0 && Symbol.Bid > BEa + TSLPips_)
                        {
                            if (bTS > 0)
                                bTS = Math.Max(bTS, Symbol.Bid - TSLPips_);
                            else bTS = Symbol.Bid - TSLPips_;
                        }

                        if (TSLPips_ < 0 && Symbol.Bid > BEa - TSLPips_)
                            bTS = Math.Max(bTS, Symbol.Bid - Math.Max(TSLPipsMin_, -TSLPips_ * (1 - (Symbol.Bid - BEa + TSLPips_) / (-TSLPips_ * 2))));

                        if (bTS > 0 && Symbol.Bid <= bTS)
                        {
                            ExitTrades(enumPosType.All, "Trailing Stop Reached");

                            return;
                        }

                    }
                    else if (nLots < 0)
                    {
                        bTS = 0;

                        if (TSLPips_ > 0 && Symbol.Ask < BEa - TSLPips_)
                        {
                            if (bTS > 0)
                                bTS = Math.Min(bTS, Symbol.Ask + TSLPips_);
                            else
                                bTS = Symbol.Ask + TSLPips_;
                        }

                        if (TSLPips_ < 0 && Symbol.Ask < BEa + TSLPips_)
                            bTS = Math.Min(bTS, Symbol.Ask + Math.Max(TSLPipsMin_, -TSLPips_ * (1 - (BEa - Symbol.Ask + TSLPips_) / (-TSLPips_ * 2))));

                        if (bTS > 0 && Symbol.Ask >= bTS)
                        {
                            ExitTrades(enumPosType.All, "Trailing Stop Reached");

                            return;
                        }

                    }
                }
            }
        }

        private void TrailingStop()
        {
            CalculateEarlyExit();
            AdjustBEb();

            nLots = LbB - LbS;

            double SLb;
            double TPbMP;

            if (MaximizeProfit && !FirstRun && CountTotalOpen > 0)
            {
                //Print($"TPaF : {TPaF}");
                //Print($"Maximize Profit : {MaximizeProfit}");

                SLb = Symbol.Ask + ProfitSet_ * Symbol.PipSize; //Math.Round(BEa + (TPa - BEa) * ProfitSet_, Symbol.Digits); //
                Print($"SLb : {SLb}");

                if (Pb + Ph < 0 && SLb > 0)
                    SLb = 0;

                if (SLb > 0 && ((nLots > 0 && Symbol.Bid < SLb) || (nLots < 0 && Symbol.Ask < SLb)))
                {
                    ExitTrades(enumPosType.All, "Profit Trailing Stop Reached (" + ProfitSet_ + "%)");
                    return;
                }

                if (TotalProfitTarget > 0)
                {
                    TPbMP = Math.Round(BEa + (TPa - BEa) * ProfitSet_, Symbol.Digits);
                    
                    if ((nLots > 0 && Symbol.Bid > TPbMP) || (nLots < 0 && Symbol.Ask < TPbMP))
                        SLb = TPbMP;
                }

                if (SLb > 0 && SLb != SLbL && MoveTP_ > 0 && TotalMoves > Moves)
                {
                    
                    TakeProfitb = 0;
                    Moves += 1;

                    if (PrintMessagesInLog)
                        Print($"MoveTP {Moves}");

                    SLbL = SLb;

                    return;
                }

            }

            if (!FirstRun && TPaF > 0)
            {
                //Print($"TPaF : {TPaF}");
                //Print($"Maximize Profit : {MaximizeProfit}");

                //TPaF = Symbol.Ask + ProfitSet_ * Symbol.PipSize;
                TPaF -= ProfitSet_ * Symbol.PipSize;

                if (hActive == false)
                {
                    if ((nLots > 0 && Symbol.Bid >= TPaF) || (nLots < 0 && Symbol.Ask >= TPaF))
                    {
                        ExitTrades(enumPosType.All, "Profit Target Reached @ " + DTS(TPaF, Symbol.Digits));

                        return;
                    }
                }
                else if (hActive == true)//hedge is Active
                {
                    if (GetProfitOnAllOpenPositions() > 0)
                    {
                        ExitTrades(enumPosType.All, "Break Even on Active Hedge.");

                        return;
                    }
                }
            }

            if (!FirstRun && UseStopLoss)
            {
                double bSL;

                if (SLPips_ > 0)
                {
                    if (nLots > 0)
                    {
                        bSL = BEa - SLPips_;

                        if (Symbol.Bid <= bSL)
                        {
                            ExitTrades(enumPosType.All, "Stop Loss Reached");

                            return;
                        }
                    }
                    else if (nLots < 0)
                    {
                        bSL = BEa + SLPips_;

                        if (Symbol.Ask >= bSL)
                        {
                            ExitTrades(enumPosType.All, "Stop Loss Reached");

                            return;
                        }
                    }
                }

                if (TSLPips_ != 0)
                {
                    if (nLots > 0)
                    {
                        if (TSLPips_ > 0 && Symbol.Bid > BEa + TSLPips_)
                            bTS = Math.Max(bTS, Symbol.Bid - TSLPips_);

                        if (TSLPips_ < 0 && Symbol.Bid > BEa - TSLPips_)
                            bTS = Math.Max(bTS, Symbol.Bid - Math.Max(TSLPipsMin_, -TSLPips_ * (1 - (Symbol.Bid - BEa + TSLPips_) / (-TSLPips_ * 2))));

                        if (bTS > 0 && Symbol.Bid <= bTS)
                        {
                            ExitTrades(enumPosType.All, "Trailing Stop Reached");

                            return;
                        }
                    }
                    else if (nLots < 0)
                    {
                        if (TSLPips_ > 0 && Symbol.Ask < BEa - TSLPips_)
                        {
                            if (bTS > 0)
                                bTS = Math.Min(bTS, Symbol.Ask + TSLPips_);
                            else
                                bTS = Symbol.Ask + TSLPips_;
                        }

                        if (TSLPips_ < 0 && Symbol.Ask < BEa + TSLPips_)
                            bTS = Math.Min(bTS, Symbol.Ask + Math.Max(TSLPipsMin_, -TSLPips_ * (1 - (BEa - Symbol.Ask + TSLPips_) / (-TSLPips_ * 2))));

                        if (bTS > 0 && Symbol.Ask >= bTS)
                        {
                            ExitTrades(enumPosType.All, "Trailing Stop Reached");

                            return;
                        }
                    }
                }
            }
        }

        private void CalculateEarlyExit()
        {
            TPaF = TakeProfitb;
        }

        private void AdjustBEb()
        {
            if (UseHedge_)
            {
                nLots += LhB - LhS;

                double PhPips;

                if (hActive == true)
                {
                    if (nLots == 0)
                    {
                        BEa = 0;
                        TPa = 0;
                    }
                    else
                    {
                        if (nLots > 0)
                        {
                            if (CountOpenBuy > 0)
                                BEa = Math.Round((BEb * LbT - (BEh - Symbol.Spread) * LhT) / (LbT - LhT), Symbol.Digits);
                            else
                                BEa = Math.Round(((BEb - (Symbol.Ask - Symbol.Bid)) * LbT - BEh * LhT) / (LbT - LhT), Symbol.Digits);

                            TPa = Math.Round(BEa + TargetPips, Symbol.Digits);
                        }
                        else
                        {
                            if (CountOpenSell > 0)
                                BEa = Math.Round((BEb * LbT - (BEh + Symbol.Spread) * LhT) / (LbT - LhT), Symbol.Digits);
                            else
                                BEa = Math.Round(((BEb + Symbol.Ask - Symbol.Bid) * LbT - BEh * LhT) / (LbT - LhT), Symbol.Digits);

                            TPa = Math.Round(BEa - TargetPips, Symbol.Digits);
                        }
                    }

                    if (ChB > 0)
                        PhPips = Math.Round((Symbol.Bid - BEh) / Pip, 1);

                    if (ChS > 0)
                        PhPips = Math.Round((BEh - Symbol.Ask) / Pip, 1);
                }
                else
                {
                    BEa = BEb;
                    TPa = TakeProfitb;
                }
            }
        }

        private void CalculateTP()
        {
            nLots = LbB - LbS;

            if (CountTotalOpen > 0 && (TakeProfitb == 0 || CountTotalOpen + CountTotalHedge != CaL || BEbL != BEb || BCa != BCaL || FirstRun))
            {
                string sCalcTP = "Set New TP:  BE: " + DTS(BEb, Symbol.Digits);
                double NewTP = 0, BasePips;
                CaL = CountTotalOpen + CountTotalHedge;
                BCaL = BCa;
                BEbL = BEb;
                if (nLots == 0)
                {
                    nLots = 1;
                }
                
                BasePips = Math.Round(Lot_ * LotMult * GridTP * (CountTotalOpen + CountTotalClosed) / nLots, Symbol.Digits);

                if (CountOpenBuy > 0)
                {
                    Print($"COB : {CountOpenBuy}");

                    if (ForceTPPips_ > 0)
                    {
                        NewTP = BEb + ForceTPPips_;
                        sCalcTP = sCalcTP + " +Force TP (" + DTS(ForceTPPips_, Symbol.Digits) + ") ";
                    }
                    else if (CountTotalClosed > 0 && CloseTPPips_ > 0)
                    {
                        NewTP = BEb + CloseTPPips_;
                        sCalcTP = sCalcTP + " +Close TP (" + DTS(CloseTPPips_, Symbol.Digits) + ") ";
                    }
                    else if (BEb + BasePips > OPbL - tp2)
                    {
                        NewTP = BEb + BasePips;
                        sCalcTP = sCalcTP + " +Base TP: (" + DTS(BasePips, Symbol.Digits) + ") ";
                    }
                    else
                    {
                        NewTP = OPbL + tp2;
                        sCalcTP = sCalcTP + " +Grid TP: (" + DTS(tp2, Symbol.Digits) + ") ";
                    }

                    if (MinTPPips_ > 0)
                    {
                        NewTP = Math.Max(NewTP, BEb + MinTPPips_);
                        sCalcTP = sCalcTP + " >Minimum TP: ";
                    }

                    NewTP += MoveTP_ * ProfitSet_;

                    if (BreakEvenTrade > 0 && CountTotalOpen + CountTotalClosed >= BreakEvenTrade)
                    {
                        NewTP = BEb + BEPlusPips_;
                        sCalcTP = sCalcTP + " >BreakEven: (" + DTS(BEPlusPips_, Symbol.Digits) + ") ";
                    }

                    sCalcTP = (sCalcTP + "Buy: TakeProfit: ");

                    Print($"Test : {MoveTP_ * 2}");
                    Print($"Move TP NEW BUY : {NewTP}");
                    Print($"MoveTP NEW BUY : {MoveTP_}");
                    Print($"Moves NEW BUY : {Moves}");
                }
                    else if (CountOpenSell > 0)
                    {
                        Print($"COS : {CountOpenSell}");
                        if (ForceTPPips_ > 0)
                        {
                            NewTP = BEb - ForceTPPips_;
                            sCalcTP = sCalcTP + " -Force TP (" + DTS(ForceTPPips_, Symbol.Digits) + ") ";
                        }
                        else if (CountTotalClosed > 0 && CloseTPPips_ > 0)
                        {
                            NewTP = BEb - CloseTPPips_;
                            sCalcTP = sCalcTP + " -Close TP (" + DTS(CloseTPPips_, Symbol.Digits) + ") ";
                        }
                        else if (BEb - BasePips < OPbL + tp2)
                        {
                            NewTP = BEb - BasePips;
                            sCalcTP = sCalcTP + " -Base TP: (" + DTS(BasePips, Symbol.Digits) + ") ";
                        }
                        else
                        {
                            NewTP = OPbL - tp2;
                            sCalcTP = sCalcTP + " -Grid TP: (" + DTS(tp2, Symbol.Digits) + ") ";
                        }

                        if (MinTPPips_ > 0)
                        {
                            NewTP = Math.Max(NewTP, BEb + MinTPPips_);
                            sCalcTP = sCalcTP + " >Minimum TP: ";
                        }

                        NewTP -= MoveTP_ * ProfitSet_;

                        if (BreakEvenTrade > 0 && CountTotalOpen + CountTotalClosed >= BreakEvenTrade)
                        {
                            NewTP = BEb - BEPlusPips_;
                            sCalcTP = sCalcTP + " >BreakEven: (" + DTS(BEPlusPips_, Symbol.Digits) + ") ";
                        }

                        sCalcTP = (sCalcTP + "Sell: TakeProfit: ");

                        Print($"Move TP NEW SELL : {NewTP}");
                    }

                if (TakeProfitb != NewTP)
                {
                    Print($"TPB : {TakeProfitb}");
                    TakeProfitb = NewTP;

                    if (nLots > 0)
                        TargetPips = Math.Round(TakeProfitb - BEb, Symbol.Digits);
                    else
                        TargetPips = Math.Round(BEb - TakeProfitb, Symbol.Digits);

                    Print(sCalcTP + DTS(NewTP, Symbol.Digits));

                    return;
                }
            }

            TotalProfitTarget = TargetPips / Pip;
            ProfitPotential = Math.Round(TargetPips * PipVal2 * Math.Abs(nLots), 2);

            
        }

        private void MoneyManagement()
        {
            if (UseMM)
            {
                if (CountTotalOpen > 0)
                {
                    //double LotMultInReg = ReadRegistryKeyValue("LotMult");
                    //if (LotMultInReg != -1)
                    //    LotMult = LotMultInReg;

                    if (OrderLotsFirst != LotSize(Lots[0] * LotMult))
                    {
                        LotMult = (int)(OrderLotsFirst / Lots[0]);
                        WriteRegistryKeyValue("LotMult", LotMult);

                        //Print("LotMult reset to " + DTS(LotMult, 0));
                    }
                }
                else if (CountTotalOpen == 0)
                {
                    double Contracts, Factor, Lotsize;
                    Contracts = PortionBalance / 1000000;
                    // MarketInfo(Symbol(), MODE_LOTSIZE); ??
                    if (Multiplier_ <= 1)
                        Factor = Level_;
                    else
                        Factor = (Math.Pow(Multiplier_, Level_) - Multiplier_) / (Multiplier_ - 1);

                    Lotsize = LAF * AccountType_ * Contracts / (1 + Factor);
                    LotMult = (int)Math.Max(Math.Floor(Lotsize / Lot_), MinMult);
                    WriteRegistryKeyValue("LotMult", LotMult);
                }
            }
            else if (CountTotalOpen == 0)
                LotMult = MinMult;
        }

        private void CloseOldTrades()
        {
            if (UseCloseOldest && CountTotalOpen >= CloseTradesLevel && CountTotalClosed < MaxCloseTrades_)
            {
                if (TakeProfitb > 0 && (ForceCloseOldest || (CountOpenBuy > 0 && OrderOpenPriceO >= TakeProfitb) || (CountOpenSell > 0 && OrderOpenPriceO <= TakeProfitb)))
                {
                    ExitTrades(enumPosType.Ticket, "Close Oldest Trade", GetOldestOpenPosition());
                }
            }
        }

        private void ProfitManagement()
        {
            double Pa = Pb;
            PaC = ProfitTotalClosed + PhC;

            if (hActive == true && CountTotalHedge == 0)
            {
                PhC = FindClosedPL(enumPosType.Hedge);
                hActive = false;
            }
            else if (hActive == false && CountTotalHedge != 0)
                hActive = true;

            if (LbT > 0)
            {
                if (ProfitTotalClosed > 0 || (ProfitTotalClosed < 0 && RecoupClosedLoss))
                {
                    Pa += ProfitTotalClosed;
                    BEb -= Math.Round(ProfitTotalClosed / PipVal2 / (LbB - LbS), Symbol.Digits);
                }

                if (PhC > 0 || (PhC < 0 && RecoupClosedLoss))
                {
                    Pa += PhC;
                    BEb -= Math.Round(PhC / PipVal2 / (LbB - LbS), Symbol.Digits);
                }

                if (Ph > 0 || (Ph < 0 && RecoupClosedLoss))
                    Pa += Ph;
            }
        }

        private void CalculateStopTrade()
        {
            double StepAB = InitialAB * (1 + StopTradePercent_);
            double StepSTB;
            double temp_ab = Account.Balance;
            if (PortionPC > 100 && temp_ab > PortionPC)
            {
                StepSTB = PortionPC * (1 - StopTradePercent_);
            }
            else
            {
                StepSTB = temp_ab * (1 - StopTradePercent_);
            }
            double NextISTB = StepAB * (1 - StopTradePercent_);

            if (StepSTB > NextISTB)
            {
                InitialAB = StepAB;
                StopTradeBalance = StepSTB;
            }
            // Stop Trade Amount:
            double InitialAccountMultiPortion = StopTradeBalance * PortionPC_;
            stop_trade_amount = InitialAccountMultiPortion;

            if (PortionBalance < InitialAccountMultiPortion)
            {
                if (CbT == 0)
                {
                    AllowTrading = false;

                    Print("Portion Balance dropped below stop-trading percentage");

                    return;
                }
                else
                   if (!RecoupClosedLoss)
                {
                    

                    Print("Portion Balance dropped below stop-trading percentage");

                    return;
                }
            }
        }

        private void CalculateDrawDown()
        {
            double NewPortionBalance = Math.Round(Account.Balance * PortionPC_, 2);

            if (CountTotalOpen == 0 || PortionChange == enumPortChgs.Any || (PortionChange != enumPortChgs.Any && NewPortionBalance > PortionBalance))
                PortionBalance = NewPortionBalance;

            if (Pb + Ph < 0)
                DrawDownPC = -(Pb + Ph) / PortionBalance;
            // opb
            if (!FirstRun && DrawDownPC >= MaxDDPercent / 100)
            {
                ExitTrades(enumPosType.All, "Equity StopLoss Reached");

                return;
            }

            if (-(Pb + Ph) > MaxDD)
                MaxDD = -(Pb + Ph);

            MaxDDPer = Math.Max(MaxDDPer, DrawDownPC * 100);
        }

        private void CalculateProfit()
        {
            if (LbT > 0)
            {
                BEb = Math.Round(BEb / LbT, Symbol.Digits);

                if (BCa < 0)
                    BEb -= Math.Round(BCa / PipVal2 / (LbB - LbS), Symbol.Digits);

                if (Pb > PbMax || PbMax == 0)
                    PbMax = Pb;

                if (Pb < PbMin || PbMin == 0)
                    PbMin = Pb;

            }
            else if (TradesOpen)
            {
                PbMax = 0;
                PbMin = 0;
                PhC = 0;
                PaC = 0;
                CaL = 0;
                bTS = 0;

                hDDStart = HedgeStart_;

            }

            if (LhT > 0)
            {
                BEh = Math.Round(BEh / LhT, Symbol.Digits);
            }
            else
            {
                SLh = 0;
            }
        }

        private void GetPendingOrders()
        {
            CountTotalPending = 0;
            CountBuyLimit = 0;
            CountSellLimit = 0;
            CountBuyStop = 0;
            CountSellStop = 0;

            foreach (var order in PendingOrders)
            {
                if (order.Label == str_BuyLimit)
                {
                    CountBuyLimit++;
                    OPpBL = order.TargetPrice;
                }

                if (order.Label == str_SellLimit)
                {
                    CountSellLimit++;
                    OPpSL = order.TargetPrice;
                }

                if (order.Label == str_BuyStop)
                {
                    CountBuyStop++;
                }

                if (order.Label == str_SellStop)
                {
                    CountSellStop++;
                }
            }

            CountTotalPending = CountBuyLimit + CountSellLimit + CountBuyStop + CountSellStop;
        }

        private void GetOrders()
        {
            CountTotalOpen = CountOpenPositions();
            CountOpenBuy = 0;
            CountOpenSell = 0;

            double Ph = 0;
            Pb = 0;
            BCh = 0;
            BEh = 0;
            ThO = 0;
            BCb = 0;
            BEb = 0;
            ChB = 0;
            LhB = 0;
            LbB = 0;
            ChS = 0;
            LhS = 0;
            LbS = 0;

            OrderOpenTimeHO = DateTime.MinValue;

            foreach (var position in Positions)
            {
                if (position.SymbolName == SymbolName)
                {
                    Ph += position.GrossProfit;
                    BCh += position.Swap + position.Commissions;
                    BEh += position.Quantity * position.EntryPrice;

                    if (
                        (position.EntryTime < OrderOpenTimeHO || OrderOpenTimeHO == DateTime.MinValue)
                        )
                    {
                        OrderOpenTimeHO = position.EntryTime;
                        ThO = position.Id;
                    }

                    Pb += position.GrossProfit;
                    BCb += position.Swap + position.Commissions;
                    BEb += position.Quantity * position.EntryPrice;

                    if (position.TradeType == TradeType.Buy)
                    {
                        if (position.Label == str_HedgeBuy)
                        {
                            ChB++;
                            LhB += position.Quantity;
                        }

                        CountOpenBuy++;
                        LbB += position.Quantity;
                    }
                    else if (position.TradeType == TradeType.Sell)
                    {
                        if (position.Label == str_HedgeSell)
                        {
                            ChS++;
                            LhS += position.Quantity;
                        }

                        CountOpenSell++;
                        LbS += position.Quantity;
                    }

                    if (position.EntryTime >= OrderOpenTimeLast)
                    {
                        OrderOpenTimeLast = position.EntryTime;
                        OPbL = position.EntryPrice;
                    }

                    if (position.EntryTime < OrderOpenTimeFirst || PositionIdFirst == 0)
                    {
                        OrderOpenTimeFirst = position.EntryTime;
                        PositionIdFirst = position.Id;
                        OrderLotsFirst = Symbol.VolumeInUnitsToQuantity(position.VolumeInUnits);
                    }

                    if (position.EntryTime < OrderOpenTimeO || OrderOpenTimeO == DateTime.MinValue)
                    {
                        OrderOpenTimeO = position.EntryTime;
                        PositionIdO = position.Id;
                        OrderOpenPriceO = position.EntryPrice;
                    }

                    //if (UsePowerOutSL && ((POSLPips_ > 0 && position.StopLoss == null) || (POSLPips_ == 0 && position.StopLoss.HasValue)))
                    //    SetPOSL = true;
                }
            }

            LbT = LbB + LbS;
            Pb = Math.Round(Pb + BCb, 2);
            CountTotalHedge = ChB + ChS;

            if (CountTotalHedge == 0)
            {
                hActive = false;
            }
            else hActive = true;

            LhT = LhB + LhS;
            Ph = Math.Round(Ph + BCh, 2);
            BCa = BCb + BCh;
        }

        private void InitSmartGrid()
        {
            if (UseSmartGrid)
            {
                Array.Resize(ref rsi, RSI_Period + RSI_MA_Period);
                Array.Sort(rsi);
            }
        }

        private void InitLotArray()
        {
            double LotStep = Symbol.VolumeInUnitsToQuantity(Symbol.VolumeInUnitsMin);
            LotMult = (int)Math.Round(Math.Max(Lot_, LotStep) / LotStep, 0);

            for (int idx = 0; idx < MaxTrades; idx++)
            {
                if (idx == 0 || Multiplier_ < 1)
                    Lots.Add(Lot_);
                else
                {
                    Lots.Add(Math.Max(Lots[idx - 1] * Multiplier_, Lots[idx - 1] + LotStep));
                }
            }
        }

        private void InitGridTP()
        {
            int GridSet = 0, GridIndex = 0, GridLevel = 0, GridError = 0, GridTemp = 0;

            if (!AutoCal)
            {
                while (GridIndex < MaxTrades)
                {
                    if (SetCountArray_.IndexOf(",") == -1 && GridIndex == 0)
                    {
                        GridError = 1;
                        break;
                    }
                    else
                    {
                        int numValue = 0;
                        if (SetCountArray_.IndexOf(",") > 0)
                        {
                            if (Int32.TryParse(SetCountArray_.Substring(0, SetCountArray_.IndexOf(",")), out numValue))
                                GridSet = numValue;
                        }
                    }

                    if (GridSet > 0)
                    {
                        int numValue = 0;
                        if (SetCountArray_.IndexOf(",") > 0)
                        {
                            SetCountArray_ = SetCountArray_.Substring(SetCountArray_.IndexOf(",") + 1);

                            if (Int32.TryParse(GridSetArray_.Substring(0, GridSetArray_.IndexOf(",")), out numValue))
                            {
                                GridTemp = numValue;
                            }
                        }
                        if (GridSetArray_.IndexOf(",") > 0)
                        {
                            GridSetArray_ = GridSetArray_.Substring(GridSetArray_.IndexOf(",") + 1);
                        }

                        if (TP_SetArray_.IndexOf(",") > 0)
                        {
                            if (Int32.TryParse(TP_SetArray_.Substring(0, TP_SetArray_.IndexOf(",")), out numValue))
                            {
                                GridTP = numValue;
                            }

                            TP_SetArray_ = TP_SetArray_.Substring(TP_SetArray_.IndexOf(",") + 1);
                        }
                    }
                    else
                        GridSet = MaxTrades;

                    if (GridTemp == 0 || GridTP == 0)
                    {
                        GridError = 2;
                        break;
                    }

                    for (GridLevel = GridIndex; GridLevel <= Math.Min(GridIndex + GridSet - 1, MaxTrades - 1); GridLevel++)
                    {
                        GridArray0.Add(GridTemp);
                        GridArray1.Add(GridTP);

                        if (PrintMessagesInLog)
                            Print("GridArray ", (GridLevel + 1), ": [", GridArray0[GridLevel], ", ", GridArray1[GridLevel], "]");
                    }

                    GridIndex = GridLevel;
                }

                if (GridError > 0 || GridArray0[0] == 0 || GridArray1[0] == 0)
                {
                    if (GridError == 1)
                        Print("Grid Array Error. Each value should be separated by a comma.");
                    else
                        Print("Grid Array Error. Check that there is one more 'Grid' and 'TP' entry than there are 'Set' numbers - separated by commas.");

                    AllowTrading = false;
                }
            }
            else
            {
                while (GridIndex < 4)
                {
                    int numValue = 0;

                    if (SetCountArray_.IndexOf(",") >= 0)
                    {
                        if (Int32.TryParse(SetCountArray_.Substring(0, SetCountArray_.IndexOf(",")), out numValue))
                        {
                            GridSet = numValue;
                        }

                        SetCountArray_ = SetCountArray_.Substring(SetCountArray_.IndexOf(DTS(GridSet, 0)) + 2);
                    }
                    if (GridIndex == 0 && GridSet < 1)
                    {
                        GridError = 1;
                        break;
                    }

                    if (GridSet > 0)
                        GridLevel += GridSet;
                    else if (GridLevel < MaxTrades)
                        GridLevel = MaxTrades;
                    else
                        GridLevel = MaxTrades + 1;

                    if (GridIndex == 0)
                        Set1Level = GridLevel;
                    else if (GridIndex == 1 && GridLevel <= MaxTrades)
                        Set2Level = GridLevel;
                    else if (GridIndex == 2 && GridLevel <= MaxTrades)
                        Set3Level = GridLevel;
                    else if (GridIndex == 3 && GridLevel <= MaxTrades)
                        Set4Level = GridLevel;

                    GridIndex++;
                }

                if (GridError == 1 || Set1Level == 0)
                {
                    Print("Error setting up Grid Levels. Check that the SetCountArray contains valid numbers separated by commas.");
                    AllowTrading = false;
                }
            }
        }

        private void InitIndcies()
        {
            int maperiod9 = 9;
            int maperiod18 = 18;

            ima_ = Indicators.MovingAverage(MarketSeries.Close, MAPeriod, MovingAverageType.Simple);
            ima_bb = Indicators.MovingAverage(MarketSeries.Open, BollPeriod, MovingAverageType.Simple);
            std_bb = Indicators.StandardDeviation(MarketSeries.Open, BollPeriod, MovingAverageType.Simple);
            iStochastic_ = Indicators.StochasticOscillator(MarketSeries, KPeriod, Slowing, DPeriod, MovingAverageType.Triangular);

            var rsi_Series = MarketData.GetSeries(RSI_TF);

            switch (RSI_Price)
            {
                case appliedPrice.Close:
                    rsi_ = Indicators.RelativeStrengthIndex(rsi_Series.Close, RSI_Period);
                    break;
                case appliedPrice.High:
                    rsi_ = Indicators.RelativeStrengthIndex(rsi_Series.High, RSI_Period);
                    break;
                case appliedPrice.Low:
                    rsi_ = Indicators.RelativeStrengthIndex(rsi_Series.Low, RSI_Period);
                    break;
                case appliedPrice.Median:
                    rsi_ = Indicators.RelativeStrengthIndex(rsi_Series.Median, RSI_Period);
                    break;
                case appliedPrice.Open:
                    rsi_ = Indicators.RelativeStrengthIndex(rsi_Series.Open, RSI_Period);
                    break;
                case appliedPrice.Typical:
                    rsi_ = Indicators.RelativeStrengthIndex(rsi_Series.Typical, RSI_Period);
                    break;
                case appliedPrice.Weighted:
                    rsi_ = Indicators.RelativeStrengthIndex(rsi_Series.Weighted, RSI_Period);
                    break;
            }

            var macd_Series = MarketData.GetSeries(MACD_TF);
            macd_ = Indicators.MacdCrossOver(macd_Series.Close, SlowPeriod, FastPeriod, SignalPeriod);

            var atr_Series = MarketData.GetSeries(ATRTF);
            atr_ = Indicators.AverageTrueRange(atr_Series, ATRPeriods, MovingAverageType.Simple);


            var cciSeries_5M = MarketData.GetSeries(TimeFrame.Minute5);
            var cciSeries_15M = MarketData.GetSeries(TimeFrame.Minute15);
            var cciSeries_30M = MarketData.GetSeries(TimeFrame.Minute30);
            var cciSeries_1H = MarketData.GetSeries(TimeFrame.Hour);
            cci_1 = Indicators.CommodityChannelIndex(cciSeries_5M, CCIPeriod);
            cci_2 = Indicators.CommodityChannelIndex(cciSeries_15M, CCIPeriod);
            cci_3 = Indicators.CommodityChannelIndex(cciSeries_30M, CCIPeriod);
            cci_4 = Indicators.CommodityChannelIndex(cciSeries_1H, CCIPeriod);

            var ichiSeries = MarketData.GetBars(ICHI_TF);

            ichimoku_ = Indicators.IchimokuKinkoHyo(ichiSeries, Tenkan_Sen, Kijun_Sen, Senkou_Span);
        }

        private void InitVars()
        {
            UseHedge_ = UseHedge;
            HedgeStart_ = HedgeStart;
            SetCountArray_ = SetCountArray;
            EntryOffset_ = EntryOffset;
            BollDistance_ = BollDistance;
            POSLPips_ = POSLPips;
            CloseTPPips_ = CloseTPPips;
            ForceTPPips_ = ForceTPPips;
            MinTPPips_ = MinTPPips;
            BEPlusPips_ = BEPlusPips;
            ProfitSet_ = ProfitSet;
            PortionPC_ = PortionPC;
            MADistance_ = MADistance;
            hTakeProfit_ = hTakeProfit;
        }

        private void InitTradeSettings()
        {
            Pip = Symbol.PipSize;

            PipVal2 = (double)((int)(Symbol.PipValue * 100000)) / Pip;

            if (UseAnyEntry)
                UAE = "||";
            else
                UAE = "&&";

            if (NanoAccount)
                AccountType_ = 10;
            else
                AccountType_ = 1;

            InitialAB = Account.Balance;
            StopTradeBalance = InitialAB * (1 - StopTradePercent_);
            MinLotSize = Symbol.VolumeInUnitsToQuantity(Symbol.VolumeInUnitsMin);
            MinLot = Symbol.VolumeInUnitsToQuantity(Symbol.VolumeInUnitsMin);
            //ProfitSet_;
            StopTradePercent_ /= 100;
            PortionPC_ /= 100;
            //EEHoursPC_ /= 100;
            //EELevelPC_ /= 100;         

            if (MinLotSize > Lot_)
            {
                Print("Lot is less than minimum lot size permitted for this account.  Minimum Lot Size: " + MinLotSize.ToString());
                AllowTrading = false;
            }

            LotMult = (int)Math.Round(Math.Max(Lot_, MinLotSize) / MinLotSize, 0);
            MinMult = LotMult;
            Lot_ = MinLotSize;

            if (MinLotSize < 0.01)
                LotDecimal = 3;
            else if (MinLotSize < 0.1)
                LotDecimal = 2;
            else if (MinLotSize < 1)
                LotDecimal = 1;
            else
                LotDecimal = 0;

            MoveTP_ = Math.Round(MoveTP * Pip, Symbol.Digits);
            EntryOffset_ = Math.Round(EntryOffset_ * Pip, Symbol.Digits);
            MADistance_ = Math.Round(MADistance_ * Pip, Symbol.Digits);
            BollDistance_ = Math.Round(BollDistance_ * Pip, Symbol.Digits);
            POSLPips_ = Math.Round(POSLPips_ * Pip, Symbol.Digits);
            hTakeProfit_ = Math.Round(hTakeProfit_ * Pip, Symbol.Digits);
            CloseTPPips_ = Math.Round(CloseTPPips_ * Pip, Symbol.Digits);
            ForceTPPips_ = Math.Round(ForceTPPips_ * Pip, Symbol.Digits);
            MinTPPips_ = Math.Round(MinTPPips_ * Pip, Symbol.Digits);
            BEPlusPips_ = Math.Round(BEPlusPips_ * Pip, Symbol.Digits);
            SLPips_ = Math.Round(SLPips_ * Pip, Symbol.Digits);
            //TSLPips_ = Math.Round(TSLPips * Pip, Symbol.Digits);
            TSLPipsMin_ = Math.Round(TSLPipsMin_ * Pip, Symbol.Digits);


            if (UseHedge_)
            {
                HedgeStart_ /= 100;
                hDDStart = HedgeStart_;
            }
        }

        private  void ReadStats()
        {
            double lastPosId = ReadRegistryKeyValue("LastPositionId");
            Position thisPos = FindPositionsByPositionId((int)Math.Round(lastPosId, 0));

            if (thisPos != null)
            {
                OrderOpenTimeFirst = thisPos.EntryTime;
                OrderLotsFirst = Symbol.VolumeInUnitsToQuantity(thisPos.VolumeInUnits);
                LotMult = (int)Math.Max(1, OrderLotsFirst / MinLot);
                ProfitTotalClosed = FindClosedPL(enumPosType.Basket);
                PhC = FindClosedPL(enumPosType.Hedge);
            }
            else
            {
                WriteRegistryKeyValue("LastPositionId", 0);
                OrderOpenTimeFirst = DateTime.MinValue;
                OrderLotsFirst = 0;
            }
        }

        #endregion Bot Functions

        //---------------------------------------------------------------------- Reuseable Functions ----------------------------------------------------------------------\\
        #region Reuseable Functions

        private double GetProfitOnAllOpenPositions()
        {
            double posProfit = 0;
            foreach (var position in Positions)
            {
                if (position.SymbolName == SymbolName)
                {
                    posProfit += position.NetProfit;
                }
            }

            return (posProfit);
        }

        private double LotSize(double NewLot)
        {
            NewLot = Math.Min(NewLot, Symbol.VolumeInUnitsToQuantity(Symbol.VolumeInUnitsMax));
            NewLot = Math.Max(NewLot, Symbol.VolumeInUnitsToQuantity(Symbol.VolumeInUnitsMin));

            return (NewLot);
        }

        private int GetOldestOpenPosition()
        {
            DateTime EarliestEntryTime = DateTime.Now.AddHours(1);
            int EarliestOpenPositionId = 0;

            foreach (var position in Positions)
            {
                if (position.EntryTime < EarliestEntryTime && position.SymbolName == SymbolName)
                {
                    EarliestEntryTime = position.EntryTime;
                    EarliestOpenPositionId = position.Id;
                }
            }

            return (EarliestOpenPositionId);
        }

        private int CountOpenPositions()
        {
            int positioncount = 0;
            foreach (var position in Positions)
            {
                if (position.SymbolName == SymbolName)
                {
                    positioncount += 1;
                }
            }
            return (positioncount);
        }

        private string DTS(double Value, int Precision)
        {
            return (Math.Round(Value, Precision).ToString());
        }

        private void WriteRegistryKeyValue(string variableName, double variabledoubleValue)
        {
            const string userRoot = "HKEY_CURRENT_USER";
            const string subkey = str_BotRegistryKey;
            string subkey_level1 = Symbol.Name.ToString();
            string keyName = userRoot + "\\" + subkey + "\\" + subkey_level1;
            Registry.SetValue(keyName, variableName, variabledoubleValue, RegistryValueKind.QWord);
        }

        private double ReadRegistryKeyValue(string variableName)
        {
            const string userRoot = "HKEY_CURRENT_USER";
            const string subkey = str_BotRegistryKey;
            string subkey_level1 = Symbol.Name.ToString();
            string keyName = userRoot + "\\" + subkey + "\\" + subkey_level1;

            double tDouble = 0;
            try
            {
                tDouble = (double)Registry.GetValue(keyName, variableName, -1);
            }
            catch (Exception e)
            {
                //do nothing
            }

            return tDouble;
        }

        private Position FindPositionsByPositionId(int positionId)
        {
            foreach (var position in Positions)
            {
                if (position.Id == positionId && position.SymbolName == SymbolName)
                {
                    return (position);
                }
            }
            return (null);
        }

        private double FindClosedPL(enumPosType posType)
        {
            double ClosedProfit = 0;

            if (posType == enumPosType.Basket && UseCloseOldest)
                CountTotalClosed = 0;

            if (OrderOpenTimeFirst > DateTime.MinValue)
            {
                foreach (var hist in History)
                {
                    if ((hist.SymbolName == SymbolName) && (hist.EntryTime >= OrderOpenTimeFirst))
                    {
                        if (posType == enumPosType.Hedge)
                        {
                            if (hist.Label == str_HedgeBuy || hist.Label == str_HedgeSell)
                            {
                                ClosedProfit += hist.GrossProfit + hist.Swap + hist.Commissions;
                            }
                        }
                        else
                        {
                            ClosedProfit += hist.GrossProfit + hist.Swap + hist.Commissions;

                            if (UseCloseOldest)
                                CountTotalClosed++;
                        }
                    }
                }
            }

            return (ClosedProfit);
        }

        #endregion Reuseable Functions

        //---------------------------------------------------------------------- Market Functions ----------------------------------------------------------------------\\
        #region Market Functions

        private int SendMarketOrder(TradeType trType, double OLot, string tradeComment, Color clr)
        {
            //+-----------------------------------------------------------------+
            //| Open Order Function                                             |
            //+-----------------------------------------------------------------+

            long volume = Symbol.QuantityToVolume(OLot);
            volume = Symbol.NormalizeVolume(volume);

            var tr_result = ExecuteMarketOrder(trType, Symbol, volume, tradeComment, null, null, MaxSlippagePips, "");
            if (tr_result.IsSuccessful)
            {
                return (tr_result.Position.Id);
            }
            else
                Print("Order error: " + tr_result.Error.ToString());

            return (0);
        }

        private int SendStopOrder(TradeType trType, double OLot, double OPrice, Color clr)
        {
            long volume = Symbol.QuantityToVolume(OLot);
            volume = Symbol.NormalizeVolume(volume);
            string orderlabel = "";
            double OrderPrice = 0;

            switch (trType)
            {
                case TradeType.Buy:
                    orderlabel = str_BuyStop;
                    OrderPrice = Math.Round(Symbol.Ask + OPrice, Symbol.Digits);
                    break;
                case TradeType.Sell:
                    orderlabel = str_SellStop;
                    OrderPrice = Math.Round(Symbol.Bid + OPrice, Symbol.Digits);
                    break;
            }

            var tr_result = PlaceStopOrder(trType, Symbol, volume, OrderPrice, orderlabel);
            if (tr_result.IsSuccessful)
            {
                return (tr_result.PendingOrder.Id);
            }
            else
                Print("Order error: " + tr_result.Error.ToString());

            return (0);
        }

        private int SendLimitOrder(TradeType trType, double OLot, double OPrice, Color clr)
        {
            long volume = Symbol.QuantityToVolume(OLot);
            volume = Symbol.NormalizeVolume(volume);
            string orderlabel = "";
            double OrderPrice = 0;

            switch (trType)
            {
                case TradeType.Buy:
                    orderlabel = str_BuyLimit;
                    OrderPrice = Math.Round(Symbol.Ask + OPrice, Symbol.Digits);
                    break;
                case TradeType.Sell:
                    orderlabel = str_SellLimit;
                    OrderPrice = Math.Round(Symbol.Bid + OPrice, Symbol.Digits);
                    break;
            }

            var tr_result = PlaceLimitOrder(trType, Symbol, volume, OrderPrice, orderlabel);
            if (tr_result.IsSuccessful)
            {
                return (tr_result.PendingOrder.Id);
            }
            else
                Print("Order error: " + tr_result.Error.ToString());

            return (0);
        }

        private void ExitTrades(enumPosType PosType, string Reason, int positionId = 0)
        {
            CloseOrdersOfType = PosType;

            //Close specific order
            switch (CloseOrdersOfType)
            {
                case enumPosType.Ticket:
                    foreach (var position in Positions)
                    {
                        if (position.Id == positionId && position.SymbolName == SymbolName)
                        {
                            TradeResult trResult = position.Close();

                            if (!trResult.IsSuccessful)
                            {
                                Print("Error closing Position, Position id: " + position.Id.ToString());

                            }
                            break;
                        }
                    }
                    if (PrintMessagesInLog)
                    {
                        Print("Close Trade Reason: " + Reason);
                    }
                    break;

                case enumPosType.Pending:
                    foreach (var order in PendingOrders)
                    {
                        if (order.SymbolName == SymbolName)
                        {
                            TradeResult trResult = order.Cancel();
                            if (!trResult.IsSuccessful)
                            {
                                Print("Error cancelling Pending Order, Pending Order id: " + order.Id.ToString());
                            }
                        }
                    }
                    break;
                default:

                    //All other types of orders            
                    foreach (var position in Positions)
                    {
                        if (position.SymbolName == SymbolName)
                        {
                            TradeResult trResult = position.Close();

                            if (!trResult.IsSuccessful)
                            {
                                Print("Error closing Position, Position id: " + position.Id.ToString());
                            }
                        }
                    }
                    if (PrintMessagesInLog)
                    {
                        Print("Close Trade Reason: " + Reason);
                    }
                    break;
            }
        }

        #endregion Market Functions

    }
}