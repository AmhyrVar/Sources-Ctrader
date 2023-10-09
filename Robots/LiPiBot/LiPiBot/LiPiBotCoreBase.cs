using System;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo {

    public abstract class LiPiBotCoreBase : LiPiBotBase {

        public override string GetLabel() {
            string label = GetBotName() + /*" " + SymbolName +*/ " " + TimeFrame.ShortName;
            if (BotPositionLabelSuffix.Trim().Length == 0) return label;
            return label + " " + BotPositionLabelSuffix.Trim();
        }


        #region BOT PARAMETERS
        /*
         * LiPiBot
         * */
        public virtual Bot_Action BotAction { get; set; }
        public virtual Bot_Position_Action BotPositionAction { get; set; }
        public virtual Bot_Trade_Type BotTradeType { get; set; }
        public virtual Bot_Check_Profit_From BotCheckProfitFrom { get; set; }
        public virtual string BotPositionLabelSuffix { get; set; }
        public virtual Log_On_Position_Event BotTradingTypeSignalMessageBox { get; set; }
        public virtual Log_On_Position_Event BotTradingTypeSignalConsole { get; set; }
        //public virtual Log_Signals BotTradingTypeSignalSound { get; set; }

        public virtual Yes_No BotTradingHourStartEndMessageBoxShow { get; set; }

        //public virtual Yes_No CloseAllPositionWhenBotStops { get; set; } // v OnStop bude zavreni vsech pozic ?? pridat dialog pro potvrzeni - zobrazit pouze pokud netestuju (backtesting) // Only_In_Backtesting ???

        //public virtual Yes_No DiableInconsistentParametersWarningDialog  { get; set; } // Pokud bude nalezena validačni chyba u parametru, nebude zobrayen dialog, ale spusti se bot i s temito chybne/nepresne zadanymi parametry

        /*
         * LiPiBot - Backtesting
         * */
        public virtual Yes_No BotBacktestingCloseAllPositionsOnStop { get; set; }
        public virtual int BotBacktestingStopOnMaximumBalanceDrop { get; set; }
        public virtual int BotBacktestingStopOnMaximumEquinityDrop { get; set; }
        public virtual int BotBacktestingStopOnMaximumLosingTrades { get; set; }
        public virtual Yes_No BotBacktestingStopOnStartValidationError { get; set; }


        /*
         * Position
         * */
        public virtual double PositionVolumeInLots { get; set; } // 
        public virtual double PositionStopLossInPips { get; set; } // 
        public virtual double PositionTakeProfitInPips { get; set; } // 
        public virtual Yes_No PositionActiveTrailingStopLoss { get; set; } //

        /*
         * Move Stop Loss to Profit
         * */
        public virtual double MoveStopLossToProfitAtPips { get; set; } // Hodnota musi byt vetsi nez Symbol Spread, protoze jinak kdyz je SL nastaven do spreadu je okamzite pri nastaveni TSL nastaveny SL ihned aktivovan a pozice uzavrena.
        public virtual double MoveStopLossToProfitSetValue { get; set; }
        public virtual Move_Stop_Loss_Type MoveStopLossToProfitType { get; set; }
        public virtual Yes_No RemoveTakeProfitOnMoveStopLossToProfit { get; set; }
        
        /*
         * Position - Close
         * */
        public virtual Close_Positions_If_Opposite_Signal_Exists ClosePositionsIfOppositeSignalExists { get; set; }
        public virtual Closing_Aggregated_Positions ClosingAggregatedAdditionalPositions { get; set; }
        public virtual Close_Positions_On_Start_Trading_Hour ClosePositionsOnStartTradingHour { get; set; }
        public virtual Close_Positions_On_End_Trading_Hour ClosePositionsOnEndTradingHour { get; set; }

        // ClosePositionsOnStartTradingHour + ClosePositionsOnEndTradingHour : uzavreni posuneme pred (minusove hodnoty) nebo po (plusove hodnoty) Start nebo End Trading Hours
        // public virtual int ClosePositionsOnStartTradingHourShiftMinutes { get; set; }

        // zavrit pozice podle stari = tj. pozice starsi nez x minut
        public virtual Close_Positions_By_Age ClosePositionsByAge { get; set; }
        public virtual int ClosePositionByAgeValue { get; set; }
        // cas v minutach, kdy zavrit pozice - tj. je-li hodnota 120 minut, pak po 120 minutach od pocatku - pokus o uyavreni - nyni se ceka dalscih 120 minuit a dalsi pokus o uzavreni - opet cekani 120 minut, ....
        public virtual int ClosePositionByAgeOnTimeInterval { get; set; }

        public virtual Close_Positions_Last_Trading_Day_Of_Week ClosePositionsLastTradingDayOfWeek { get; set; }
        public virtual int ClosePositionsLastTradingDayOfWeekBeforeEndTradingValue { get; set; }
        

        // Pokud je mozne zjistit, kdy se market uzavirat, pak pred jeho uzavrenim muzeme uzavrit pozice:
        // pokud neni aktivni TradingHour, ktery by ukoncil trading pred uzavrenim trhu, muzeme pozice uzavrit
        // Pokud je Trading Hours aktivni a ukonci trading pred uzavrenim marketu, pak tato hodnota nic nedela (musime pouzit Close on Trading Hours End)
        // public virtual Yes_No/In_Profit/In_Loss ClosePositionsOnMarketClosing { get; set; }
        // pocet minut pred uzavrenim trhu uzavrit pozice
        // public virtual int ClosePositionsOnMarketClosingMinutesBefore { get; set; }

        /*
         * Position - Open
         * */
        public virtual Yes_No OpenOppositePrimePositionIfOppositeSignalExists { get; set; }


        /*
         * Position Prime
         * */
        public virtual int PrimePositionsMax { get; set; }
        public virtual double PrimePositionMinProfitPips { get; set; }
        public virtual double PrimePositionMinProfitPipsMultiplicator { get; set; }
        public virtual double PrimePositionMinLossPips { get; set; }
        public virtual double PrimePositionMinLossPipsMultiplicator { get; set; }
        public virtual int PrimePositionMinAge { get; set; }
        public virtual double PrimePositionMinAgeMultiplicator { get; set; }

        /*
         * Position Additional
         * */
        public virtual int AdditionalPositionsMax { get; set; }
        public virtual Additional_Positions_Type AdditionalPositionsType { get; set; } // OpenAdditionalPositions = OpenMoreSamePositions
        public virtual double AdditionalPositionsMinLossPips { get; set; } // OpenMoreSamePositionsMinLossPips
        public virtual double AdditionalPositionsMinLossPipsMultiplicator { get; set; }
        public virtual double AdditionalPositionsMinProfitPips { get; set; } // OpenMoreSamePositionsMinProfitPips
        public virtual double AdditionalPositionsMinProfitPipsMultiplicator { get; set; }
        public virtual Yes_No AdditionalPositionsSignalRequired { get; set; } // OpenMoreSamePositionsSignalRequired
        public virtual int AdditionalPositionsVolumeMultiply { get; set; } // OpenMoreSamePositionsVolumeMultiply
        public virtual Multiply_Volume_Type AdditionalPositionsVolumeMultiplyType { get; set; } // OpenMoreSamePositionsVolumeMultiplyType


        /*
         * Trading Days
         * */
        // public virtual Yes_No TradingDayMonday { get; set; }
        // public virtual Yes_No TradingDayTuesday { get; set; }
        // public virtual Yes_No TradingDayWednesday { get; set; }
        // public virtual Yes_No TradingDayThursday { get; set; }
        // public virtual Yes_No TradingDayFriday { get; set; }
        // public virtual Yes_No/In_Profit/In_Loss ClosePositionsOnYourLastTradingDay { get; set; }
        
        // pocet minut pred koncem Tradingu uzavrit pozice (pokud neni nastaveny Trading Hours cislo znamena pocet minut pred uzavrenim burzy - v tom pripade se doporucuje nedavat 0).
        // public virtual int ClosePositionsOnYourLastTradingDayMinutesBeforeEnd { get; set; }


        /*
         * Trading Hours
         * */
        public virtual Yes_No TradingHoursActive { get; set; }
        public virtual int TradingHourStart { get; set; }
        public virtual int TradingHourEnd { get; set; }
        #endregion


        public enum Close_Position_Cause {
            END_TRADING_HOUR,
            START_TRADING_HOUR,
            CLOSE_AGGREGATED_POSITIONS,
            OPPOSITE_SIGNAL_EXISTS,
            AGE,
            LAST_TRADING_DAY_IN_WEEK
        }

        public enum Open_Position_Cause_Type {
            PRIME_POSITION,
            ADDITIONAL_POSITION
        }

        public bool IsMoveStopLossToProfitActive() {
            return MoveStopLossToProfitAtPips != 0;
        }

        /*
        [Parameter(DefaultValue = WORKER_TYPE.ONLY_SIGNAL)]
        public WORKER_TYPE WorkerType { get; set; }
        */

        public ISignal SignalWorker;

        // Parameter slouzi k zapamatovani si maximalni Balance, abychom mohli zjistit, propad Balance od tohoto zaznamenaneho maxima k aktualni Balance.
        private double backtestingMaximumBalance = 0.0;

        protected override void OnTick() {
            if (RunningMode != RunningMode.RealTime && BotBacktestingStopOnMaximumEquinityDrop > 0) {
                double equnity = Account.Balance - Account.Equity;
                if (equnity > 0) {
                    if (equnity >= BotBacktestingStopOnMaximumEquinityDrop) {
                        Print("Bot Backtesting Stopped : Equinity Dropped [" + equnity + "].");
                        Stop();
                        return;
                    }
                }
            }

            if (RunningMode != RunningMode.RealTime && BotBacktestingStopOnMaximumBalanceDrop > 0) {
                double balance = Account.Balance;
                if (balance > backtestingMaximumBalance) {
                    backtestingMaximumBalance = balance;
                } else if (backtestingMaximumBalance - balance > BotBacktestingStopOnMaximumBalanceDrop) {
                    Print("Bot Backtesting Stopped : Balance Dropped [" + (backtestingMaximumBalance - balance) + "].");
                    Stop();
                    return;
                }
            }

            base.OnTick();
        }

        protected override void OnStart() {
            // Povolíme spuštění BOTa pouze pro Backtesting a Optimalization.
            if (RunningMode != RunningMode.SilentBacktesting && RunningMode != RunningMode.VisualBacktesting && RunningMode != RunningMode.Optimization) {
                Print("Only Backtesting and Optimalization is allowed.");
                StopBot();
                MessageBox.Show("Only Backtesting and Optimalization is allowed.", "Bot Stopped", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (IsInvalidParameterExists() == false) {
                SignalWorker = CreateSignalWorker();
                if (IsTradingHour(false) == false) {
                    if (IsMessageBoxShowEnabled() && BotTradingHourStartEndMessageBoxShow == LiPiBotBase.Yes_No.Yes) {
                        new Thread(new ThreadStart(delegate {
                            // Aby MessageBox nebyl zobrazen jako modal, zobrazíme ho v Threadu.
                            MessageBox.Show("Trading Hour : [" + GetLabel() + " : " + SymbolName + "]" 
                                + Environment.NewLine + "Time: " + Server.TimeInUtc.AddHours(TimeZoneInfo.Local.GetUtcOffset(Server.TimeInUtc).Hours)
                                + Environment.NewLine + "Trading will not be active until Start Trading Hour [" + (TradingHourStart + TimeZoneInfo.Local.GetUtcOffset(Server.TimeInUtc).Hours) + ":00].",
                                //+ Environment.NewLine + "Trading will not be active until Start Trading Hour [" + (TradingHourStart + TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow).Hours) + ":00].",
                                GetBotName() + " : Trading Hours", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        })).Start();
                    }
                }
                base.OnStart();
            }
        }

        protected override void OnStop() {
            base.OnStop();

            if (RunningMode != RunningMode.RealTime && BotBacktestingCloseAllPositionsOnStop == LiPiBotBase.Yes_No.Yes) {
                if (worker != null) {
                    Print("Closing positions [" + GetPositionsAll().Count + "] on Backtesting Stopped.");
                }
                // Pokud provadime backtestineg nebo optimalization, uzavreme na konci vsechny pozice.
                foreach (Position postion in GetPositionsAll()) {
                    postion.Close();
                }
            }
        }

        protected override void OnBar() {
            base.OnBar();
            if (TradingHourAlreadytStarted == true 
                && IsClosePositionsOnEndTradingHourProcessed == false 
                && IsTradingHour(true) == false) {
                // Právě bylo ukončen Trading (nastala End Trading hour) a tak zkontrolujeme, zda nemáme nastaveno, aby i pozice byly uzavreny.
                TradingHourEndClosePositions();
            }
        }

        private DateTime closePositionsLastTradingDayTime = DateTime.MinValue;
        protected override void ClosePositionsLastTradingDay() {
            if (ClosePositionsLastTradingDayOfWeek != Close_Positions_Last_Trading_Day_Of_Week.None) {
                if (closePositionsLastTradingDayTime <= Server.TimeInUtc) {
                    if (closePositionsLastTradingDayTime != DateTime.MinValue) {
                        LastTradingDayInWeekClosePositions();
                    }
                    closePositionsLastTradingDayTime = GetTimeForClosePositionsLastTradingDayInWeek();
                }
            }
        }

        private DateTime GetTimeForClosePositionsLastTradingDayInWeek() {
            DateTime result = DateTime.MaxValue;

            DateTime d = Server.TimeInUtc;
            //DateTime dt = d.AddHours(d.Hour * -1 + 1).AddMinutes(d.Minute * -1).AddSeconds(d.Second * -1).AddMilliseconds(d.Millisecond * -1);
            DateTime dt = new DateTime(d.Year, d.Month, d.Day, 0, 0, 0);
            //dt = dt.AddDays(1);

            if (closePositionsLastTradingDayTime != DateTime.MinValue) {
                // Pokud jsme jiz hledali Last Trading Time In Week, pak zacneme v case, kdy melo dojit k uzavreni pozic.
                // Tento cas, jeste musi byt market aktivni a proto hledame minutu, kdy je market neaktivni.
                dt = closePositionsLastTradingDayTime;
                while (Symbol.MarketHours.IsOpened(dt) == true) {
                    dt = dt.AddMinutes(1);
                }
            }

            if (Symbol.MarketHours.IsOpened(dt) == false) {
                // Pokud jsme robota spustili v dobe, kdy neni aktivni market, najdeme cas, kdy market aktivni bude.
                while (Symbol.MarketHours.IsOpened(dt) == false) {
                    dt = dt.AddMinutes(1);
                }
            }
            
            while (dt.DayOfWeek != DayOfWeek.Saturday) {
                if (Symbol.MarketHours.IsOpened(dt)) {
                    result = dt;
                }
                dt = dt.AddMinutes(1);
            }

            if (TradingHoursActive == Yes_No.Yes) {
                // Pokud mame nastaveny Trading Hours, pak nastavime cas pred konec Trading Hours
                result = new DateTime(result.Year, result.Month, result.Day, TradingHourEnd, 0, 0);
            }
            result = result.AddMinutes(-1 * ClosePositionsLastTradingDayOfWeekBeforeEndTradingValue);
            return result;
        }

        private void LastTradingDayInWeekClosePositions() {
            if (ClosePositionsLastTradingDayOfWeek == Close_Positions_Last_Trading_Day_Of_Week.All) {
                //worker.P("ClosePositionsOnEndTradingHour LastTradingDaInWeek A");
                worker.CloseOnEndTradingHour(GetPositionsAll(), LiPiBotCoreBase.Close_Position_Cause.LAST_TRADING_DAY_IN_WEEK);
            } else if (ClosePositionsLastTradingDayOfWeek == Close_Positions_Last_Trading_Day_Of_Week.Only_In_Profit) {
                //worker.P("ClosePositionsOnEndTradingHour LastTradingDaInWeek B");
                worker.CloseOnEndTradingHour(GetPositionsAll().FindAll(item => GetPositionProfit(item) > 0), LiPiBotCoreBase.Close_Position_Cause.LAST_TRADING_DAY_IN_WEEK);
            } else if (ClosePositionsLastTradingDayOfWeek == Close_Positions_Last_Trading_Day_Of_Week.Only_In_Loss) {
                //worker.P("ClosePositionsOnEndTradingHour LastTradingDaInWeek C");
                worker.CloseOnEndTradingHour(GetPositionsAll().FindAll(item => GetPositionProfit(item) < 0), LiPiBotCoreBase.Close_Position_Cause.LAST_TRADING_DAY_IN_WEEK);
            }
        }

        private void TradingHourEndClosePositions() {
            IsClosePositionsOnEndTradingHourProcessed = true;

            if (ClosePositionsOnEndTradingHour == Close_Positions_On_End_Trading_Hour.All) {
                //worker.P("ClosePositionsOnEndTradingHour A");
                worker.CloseOnEndTradingHour(GetPositionsAll(), LiPiBotCoreBase.Close_Position_Cause.END_TRADING_HOUR);
            } else if (ClosePositionsOnEndTradingHour == Close_Positions_On_End_Trading_Hour.Only_In_Profit) {
                //worker.P("ClosePositionsOnEndTradingHour B");
                worker.CloseOnEndTradingHour(GetPositionsAll().FindAll(item => GetPositionProfit(item) > 0), LiPiBotCoreBase.Close_Position_Cause.END_TRADING_HOUR);
            } else if (ClosePositionsOnEndTradingHour == Close_Positions_On_End_Trading_Hour.Only_In_Loss) {
                //worker.P("ClosePositionsOnEndTradingHour C");
                worker.CloseOnEndTradingHour(GetPositionsAll().FindAll(item => GetPositionProfit(item) < 0), LiPiBotCoreBase.Close_Position_Cause.END_TRADING_HOUR);
            }
        }
        private void TradingHourStartClosePositions() {
            if (ClosePositionsOnStartTradingHour == Close_Positions_On_Start_Trading_Hour.All) {
                //worker.P("ClosePositionsOnStartTradingHour A");
                worker.CloseOnEndTradingHour(GetPositionsAll(), LiPiBotCoreBase.Close_Position_Cause.START_TRADING_HOUR);
            } else if (ClosePositionsOnStartTradingHour == Close_Positions_On_Start_Trading_Hour.Only_In_Profit) {
                //worker.P("ClosePositionsOnStartTradingHour B");
                worker.CloseOnEndTradingHour(GetPositionsAll().FindAll(item => GetPositionProfit(item) > 0), LiPiBotCoreBase.Close_Position_Cause.START_TRADING_HOUR);
            } else if (ClosePositionsOnStartTradingHour == Close_Positions_On_Start_Trading_Hour.Only_In_Loss) {
                //worker.P("ClosePositionsOnStartTradingHour C");
                worker.CloseOnEndTradingHour(GetPositionsAll().FindAll(item => GetPositionProfit(item) < 0), LiPiBotCoreBase.Close_Position_Cause.START_TRADING_HOUR);
            }
        }

        public double GetPositionProfit(Position postion) {
            if (BotCheckProfitFrom.Equals(Bot_Check_Profit_From.Pips)) {
                return postion.Pips;
            }
            return postion.NetProfit;
        }

        protected abstract ISignal CreateSignalWorker();


        public bool IsInvalidParameterExistsForRealMode() {
            //if (BotPositionLabelSuffix.Trim().Length > 5) return StopBot("Maximalni pocet znaku pro LevelSuffix je 5.");
            if (BotPositionLabelSuffix.Trim().Length > 5) return StopBotOnValidationError("Maximum length of LevelSuffix is 5.");

            return false;
        }

        public bool IsInvalidParameterExistsForBacktestingMode() {
            if (BotBacktestingStopOnStartValidationError == Yes_No.No) {
                return false;
            }

            /*
             * Povolit pouze vybrane TimeFramy: 5m, 1h, 1D
             * TimeFrame 1m is not allowed for Demo.
             * */
            /*
             * Kazdemu povolenemu TimeFramu nastavit maximalni delku pro backtestovani (zjistit cas pri startu a zastavit robota po urcitem case).
             * TimeFrame 5m is allowed for 3 days = 864 candles. (10 days = 2880 candles ??)
             * TimeFrame 1h is allowed for 1 month = 720 candles. (4 months = 2880 candles ??)
             * TimeFrame 1D is allowed for 2 years = 730 candles. (8 years = 2920 candles ??)
             * */
            /*
             * Neni dovoleno spustit na Demo uctu s obchodovanim v realnem case (jak to vypada pri obchodovani se muzete podivat na Copy strance).
             * Demo is not allowed for using in realtime trading for Real or Demo account.
             * */

            return false;
        }



        protected virtual bool IsInvalidParameterExists() {
            if (BotBacktestingStopOnStartValidationError == Yes_No.No) {
                return false;
            }

            /*
             * TODO: Kontrolu rozdelit na 2 metody.
             * 1: pri optimalizaci, nezobrazovat dialogy, ale jenom ignorovat nesmyslne nastaveni
             * 2: pri spousteni nebo backtestovani zobrazit dialog, ze toto nastaveni nema smysl
             * */


            if (PositionStopLossInPips > 0 && MoveStopLossToProfitAtPips > 0) {
                double valueTSL1 = Math.Round(MoveStopLossToProfitAtPips + -1 * PositionStopLossInPips, 1, MidpointRounding.AwayFromZero);
                double valueTSL2 = Math.Round(MoveStopLossToProfitAtPips * -1 * MoveStopLossToProfitSetValue / 100, 1, MidpointRounding.AwayFromZero) * -1; // musime vynasobit -1, protoze porovnavame kladna cisla (původni -1 je kvuli API, kde zadavame SL do kladnych hodnot = tj. musi to byt zaporne cislo)

                /*
                 * Mame nastaveny inicializacni TSL na 20 (tj. zaciname na -20).
                 * Aktivovany TSL je na 90 a nastaveni hodnoty na 50% (90*50/100 = 45).
                 * Ve chvili, kdy dosahneme 90ti pipu, je TSL na hodnote 70 pipu (-20 + 90) a aktivovany TSL by chtel nastavit TSL na 45 pipu - neni mozne nastavit TSL na mensi pocet pipu.
                 * */
                //if (valueTSL1 >= valueTSL2) return StopBot("TSL nebude mastaven, protoze inicializovany TSL bude ve chvili dosazeni Pipu pro aktivovani TSL ve vyssi hodnote [" + valueTSL1 + "] nez TSL, ktery by mel byt nastaven [" + valueTSL2 + "].");
                if (valueTSL1 >= valueTSL2) return StopBotOnValidationError("Move Stop Loss to Profit is wrong. Trailing Stop Loss will be above the current price (try to change Profit at Pips or % of Current Pips).");
            }

            //if (BotPositionLabelSuffix.Trim().Length > 5) return StopBot("Maximalni pocet znaku pro LevelSuffix je 5.");
            if (BotPositionLabelSuffix.Trim().Length > 5) return StopBotOnValidationError("Maximum length of LevelSuffix is 5.");

            //if (AdditionalPositionsType != Additional_Positions_Type.Aggregated && ClosingAggregatedAdditionalPositions != Closing_Aggregated_Positions.All) return StopBot("Pokud nemame aktivovane agregovane Pozice, musime nastavit zavirani agregovanzch pozic na ALL.");
            if (AdditionalPositionsType != Additional_Positions_Type.Aggregated && ClosingAggregatedAdditionalPositions != Closing_Aggregated_Positions.All) return StopBotOnValidationError("Positions Type is not Aggregated - Closing Aggregated Position have to be = ALL.");

            //if (IsMoveStopLossToProfitActive() == false && MoveStopLossToProfitSetValue != 5) return StopBot("Neni aktivni TSL - Set TSL musi byt 5.");
            if (IsMoveStopLossToProfitActive() == false && MoveStopLossToProfitSetValue != 5) return StopBotOnValidationError("Trailing Stop Loss is not active - SL to Profit Value have to be = 5.");
            //if (IsMoveStopLossToProfitActive() && PositionTakeProfitInPips > 0 && MoveStopLossToProfitAtPips > PositionTakeProfitInPips) return StopBot("TSL je vetsi nez TS = TS nebude nikdy aktivovan.");
            if (IsMoveStopLossToProfitActive() && PositionTakeProfitInPips > 0 && MoveStopLossToProfitAtPips > PositionTakeProfitInPips) return StopBotOnValidationError("Trailing Stop Loss is above Take Profit.");

            //if (AdditionalPositionsType == Additional_Positions_Type.No && AdditionalPositionsMax != 1) return StopBot("Otevreni dalsich pozic neni povolen - Max. Pozic musi byt = 1");
            //if (OpenMoreSamePositionsAllowed == Allow_Open_More_Positions.No && OpenMoreSamePositionsMinLossPips != 10) return StopBot("Otevreni dalsich pozic neni povolen - Max. Pozic musi byt = 10");
            //if (AdditionalPositionsType == Additional_Positions_Type.No && AdditionalPositionsVolumeMultiply != 1) return StopBot("Otevreni dalsich pozic neni povolen - Multiply musi byt = 1");
    //        if (AdditionalPositionsMax < 1 && AdditionalPositionsVolumeMultiply != 1) return StopBot("Otevreni dalsich pozic neni povolen - Multiply musi byt = 1");
            //if (AdditionalPositionsType == Additional_Positions_Type.No && AdditionalPositionsVolumeMultiplyType != Multiply_Volume_Type.Linear) return StopBot("Otevreni dalsich pozic neni povolen - Multiply type musi byt = Linear");

            //if (AdditionalPositionsMax < 1 && AdditionalPositionsVolumeMultiplyType != Multiply_Volume_Type.Linear) return StopBot("Otevreni dalsich pozic neni povolen - Multiply type musi byt = Linear");
            if (AdditionalPositionsMax < 1 && AdditionalPositionsVolumeMultiplyType != Multiply_Volume_Type.Linear) return StopBotOnValidationError("Open New Positions is not allowed - Multiply Type have to be = Linear");
            //if (AdditionalPositionsVolumeMultiply == 0 && AdditionalPositionsVolumeMultiplyType != Multiply_Volume_Type.Linear) return StopBot("Mupltiply Volume novych poyic je = 0 - Multiply type musi byt = Linear");
            if (AdditionalPositionsVolumeMultiply == 0 && AdditionalPositionsVolumeMultiplyType != Multiply_Volume_Type.Linear) return StopBotOnValidationError("Volume Multiply of New Positions is = 0 - Multiply Type have to be = Linear");

            if (RunningMode == RunningMode.SilentBacktesting || RunningMode == RunningMode.VisualBacktesting || RunningMode == RunningMode.Optimization) {
                // IsBacktesting
                // https://ctrader.com/forum/cbot-support/4674
                // You can use IsBacktesting in optimization as well. It is true for both backtesting and optimization.
                //if (TradingHoursActive == Yes_No.No && (TradingHourStart > 0 || TradingHourEnd > 0)) return StopBot("Tading Hours neni aktivni - Start a End musi mit hodnotu 0.");
                if (TradingHoursActive == Yes_No.No && (TradingHourStart > 0 || TradingHourEnd > 0)) return StopBotOnValidationError("Trading Hours is not active - Start and End of Trading Hours have to be = 0.");
                //if (TradingHourStart == -1 && TradingHourEnd != 0) return StopBot("Pocatecni Trading hodina je -1 a koncova Trading hodina musi byt 0");
            }
            //if (TradingHoursActive == Yes_No.Yes && TradingHourStart == TradingHourEnd) return StopBot("Pocatecni Trading hodina nesmi byt stejna jako koncova trading hodina");
            if (TradingHoursActive == Yes_No.Yes && TradingHourStart == TradingHourEnd) return StopBotOnValidationError("Start Trading hour have to be different from Stop Trading Hour.");

            //if (ClosePositionsIfOppositeSignalExists != Close_Positions_If_Opposite_Signal_Exists.None && AdditionalPositionsType == Additional_Positions_Type.Separated) return StopBot("Pokud uzavirame Pozice pri existenci opacneho signalu, pak neni mozne pouzit SEPARATE dalsi pozice, protoze se vzdy uzavrou vsechnz");
            if (ClosePositionsIfOppositeSignalExists != Close_Positions_If_Opposite_Signal_Exists.None && AdditionalPositionsType == Additional_Positions_Type.Separated) return StopBotOnValidationError("Close Position if Opposite Signal Exists is active - Additional Positions Type cannot be = SEPARATED");

            //if ((AdditionalPositionsType != Additional_Positions_Type.No
            //if ((AdditionalPositionsMax > 1
              //      && AdditionalPositionsMinLossPips == 0 && AdditionalPositionsMinProfitPips == 0) && AdditionalPositionsSignalRequired == LBotBase.Yes_No.No) return StopBot("Mame aktivni otevirani novzch pociz pri 0 poctu Loss i Profit Pipu a tak (nemame-li nastavene zadne Pipy) je nutne mit aktivni otvirat novou pozici pri nalezeni signalu.");
            if ((AdditionalPositionsMax > 1
                    && AdditionalPositionsMinLossPips == 0 && AdditionalPositionsMinProfitPips == 0) && AdditionalPositionsSignalRequired == LiPiBotBase.Yes_No.No) return StopBotOnValidationError("Minimum Loss and Minimum Profit to Open New Additinal Position is = 0 - Signal Required to Open New Additional Position have to be = YES.");

            // 0 nekontrolujeme, proto 0 = disabled kontrolu pipu
            //if (AdditionalPositionsType == Additional_Positions_Type.No && AdditionalPositionsMinLossPips != 0 && AdditionalPositionsMinLossPips >= 15) return StopBot("Otevreni dalsich pozic neni povolen - Minimalni pocet Loss pipu je 15");
            //if (AdditionalPositionsType == Additional_Positions_Type.No && AdditionalPositionsMinProfitPips != 0 && AdditionalPositionsMinProfitPips >= 15) return StopBot("Otevreni dalsich pozic neni povolen - Minimalni pocet Propfit pipu je 15");
            
            //if (AdditionalPositionsMax < 1 && AdditionalPositionsMinLossPips != 0 && AdditionalPositionsMinLossPips >= 15) return StopBot("Otevreni dalsich pozic neni povolen - Minimalni pocet Loss pipu je 15");
            if (AdditionalPositionsMax < 1 && AdditionalPositionsMinLossPips != 0 && AdditionalPositionsMinLossPips >= 15) return StopBotOnValidationError("To Open New Position is required Minimum Loss to Open New Additional Position have to be at least = 15");
            //if (AdditionalPositionsMax < 1 && AdditionalPositionsMinProfitPips != 0 && AdditionalPositionsMinProfitPips >= 15) return StopBot("Otevreni dalsich pozic neni povolen - Minimalni pocet Propfit pipu je 15");
            if (AdditionalPositionsMax < 1 && AdditionalPositionsMinProfitPips != 0 && AdditionalPositionsMinProfitPips >= 15) return StopBotOnValidationError("To Open New Position is required Minimum Profit to Open New Additional Position have to be at least = 15");

            return false;
        }

        protected override bool IsTradingHour(bool ShowDialogTradingHourIsActive) {
            /*
             * ShowDialogTradingHourIsActive = TRUE = při přechodu na Trading Hours nebo z Trading Hours, bude zobrazen dialog o této skutečnosti.
             * 
             * Zobrazení dialogu chceme zakázat především při spuštění Bot během Tradign Hours, kdy při spuštění máme nastavené parametry jako, že nejsme v Trading Hours.
             * Ale při startu bude zjištěno, že jsme v Trading Hours a tak by ihned při spuštění byl zobrazen Dialog, že jsme přešli do Trading Hours - my v nich ale jsme a nechceme zobrazovat tuto informaci.
             * Zobrazujeme pouze informaci, že jsme pustili Bota mimo Trading Hours a potom když zobrazujeme informaci, že právě nastaly Trading Hours a potom že skončili Trading Hours.
             * */

            if (TradingHoursActive == Yes_No.No) return true;

            int currentHour = Server.TimeInUtc.Hour;

            /*
             * Je-li currentHour = 10, pak skutečná hodina je 12 (tj. +2 pro letní čas - v květnu)
             * */

            if (TradingHourStart < TradingHourEnd) {
                // Trading Hour <07:00 - 13:00>
                if (TradingHourStart <= currentHour && currentHour < TradingHourEnd) {
                    if (TradingHourStarted == false && TradingHourStopped) {
                        IsClosePositionsOnEndTradingHourProcessed = false;
                        TradingHourStarted = true;
                        TradingHourStopped = false;
                        TradingHourAlreadytStarted = true;
                        if (ShowDialogTradingHourIsActive) {
                            ShowTradingHourDialog(true);
                            TradingHourStartClosePositions();
                        }
                    }
                    return true;
                }
            } else if (TradingHourStart > TradingHourEnd) {
                // Trading Hour <17:00 - 03:00>
                if (TradingHourStart <= currentHour || currentHour < TradingHourEnd) {
                    if (TradingHourStarted == false && TradingHourStopped) {
                        IsClosePositionsOnEndTradingHourProcessed = false;
                        TradingHourStarted = true;
                        TradingHourStopped = false;
                        TradingHourAlreadytStarted = true;
                        if (ShowDialogTradingHourIsActive) {
                            ShowTradingHourDialog(true);
                            TradingHourStartClosePositions();
                        }
                    }
                    return true;
                }
            }

            if (TradingHourStopped == false && TradingHourStarted) {
                TradingHourStarted = false;
                TradingHourStopped = true;
                if (ShowDialogTradingHourIsActive) ShowTradingHourDialog(false);
            }
            return false;
        }

        /*
         * TRUE = při přechodu z aktivního Tradigu během Trading Hours na neaktivní Trading došlo ke kontrole, zda se mají při ukončení Trading Hours zavřít otevřené poozice.
         * Při dalším přechodu do aktivního Tradigku (nastanou Trading Hours) se opět změní na výchozí FALSE hodnotu.
         * */
        private bool IsClosePositionsOnEndTradingHourProcessed = false;
        /*
         * TradingHourAlreadytStarted = Zda již byly aktivovány Trading Hours po spuštění Bota.
         * Při spuštění Bota je hodnota FALSE.
         * Ve chvíli, se aktivuje Trading Hours, hosnota se změní na TRUE a již tak zůstane.
         * Při dalším spuštění Bota je hodnota opět FALSE až do chvíle Trading Hours.
         * */
        private bool TradingHourAlreadytStarted = false;
        private bool TradingHourStarted = false;
        private bool TradingHourStopped = true;
        private void ShowTradingHourDialog(bool isTradinghour) {
            if (IsMessageBoxShowEnabled() && BotTradingHourStartEndMessageBoxShow == LiPiBotBase.Yes_No.Yes) {
                if (isTradinghour) {
                    new Thread(new ThreadStart(delegate {
                        // Aby MessageBox nebyl zobrazen jako modal, zobrazíme ho v Threadu.
                        MessageBox.Show(@"Trading Hour : [" + GetLabel() + " : " + SymbolName + "]"
                            + Environment.NewLine + "Time: " + Server.TimeInUtc.AddHours(TimeZoneInfo.Local.GetUtcOffset(Server.TimeInUtc).Hours)
                            + Environment.NewLine + "Trading is active.",
                            GetBotName() + " : Trading Hours", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    })).Start();
                } else {
                    new Thread(new ThreadStart(delegate {
                        // Aby MessageBox nebyl zobrazen jako modal, zobrazíme ho v Threadu.
                        MessageBox.Show(@"Trading Hour : [" + GetLabel() + " : " + SymbolName + "]"
                            + Environment.NewLine + "Time: " + Server.TimeInUtc.AddHours(TimeZoneInfo.Local.GetUtcOffset(Server.TimeInUtc).Hours)
                            + Environment.NewLine + "Trading is not active.", GetBotName() + " : Trading Hours",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    })).Start();
                }
            }
        }
    }
}