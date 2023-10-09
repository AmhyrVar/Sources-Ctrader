using cAlgo.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace cAlgo {

    public abstract class LiPiBotBase : Robot {

        public static readonly string commentPrime = "PP";
        public static readonly string commentAdditional = "PA";

        public int GetBotBaseVersion() { return BOT_BASE_VERSION; }
        private const int BOT_BASE_VERSION = 1;

        public Worker worker;

        public Stats stats;

        public enum Bot_Action {
            Trade = 0,
            Signal = 1,
            Trade_And_Signal = 2
        }
        
        public enum Bot_Position_Action {
            Open_Positions = 0,
            Close_Positions = 1,
            Open_And_Close_Positions = 2
        }

        public enum Bot_Trade_Type {
            Buy = 0,
            Sell = 1,
            Buy_And_Sell = 2
        }

        public enum Bot_Check_Profit_From {
            Pips = 0,
            Net_Profit = 1
        }

        public enum Multiply_Volume_Type {
            Quadratic = 0,
            Linear = 1
        }

        public enum Close_Positions_If_Opposite_Signal_Exists {
            All = 0,
            Only_In_Profit = 1,
            Only_In_Loss = 2,
            None = 3
        }

        public enum Close_Positions_By_Age {
            All = 0,
            Only_In_Profit = 1,
            Only_In_Loss = 2,
            None = 3
        }

        public enum Close_Positions_On_End_Trading_Hour {
            All = 0,
            Only_In_Profit = 1,
            Only_In_Loss = 2,
            None = 3
        }

        public enum Close_Positions_On_Start_Trading_Hour {
            All = 0,
            Only_In_Profit = 1,
            Only_In_Loss = 2,
            None = 3
        }

        public enum Close_Positions_Last_Trading_Day_Of_Week {
            All = 0,
            Only_In_Profit = 1,
            Only_In_Loss = 2,
            None = 3
        }

        public enum Additional_Positions_Type {
            //No = 0,
            Separated = 0,//1, // Kazda pozie se uzavira samostatne.
            Aggregated = 1//2 // Jakmile je uzavrena jedna z pozic, uzavrou se i vsechny ostatni.
        }

        public enum Instrument_Signal {
            Yes = 0,
            //Yes_Inverted = 2, // Pokud je nalezen SELL signál, budeme ho považovat za opačný (BUY) signál, aby byla otevřena BUY pozice.
            No = 1
        }

        public enum Closing_Aggregated_Positions {
            All = 0,
            Only_In_Profit = 1,
            Only_In_Loss = 2
        }

        public enum Move_Stop_Loss_Type {
            Single_Move = 0,
            Repetitive_Move = 1,
            Trailing_Stop_Loss = 2
        }

        public enum Log_On_Position_Event {
            No = 0,
            Open_Signal = 1, // (nalezen signal LiPiBot)
            Close_Signal = 2, // (nalezen signal LiPiBot)
            Close_Action = 3, // (stop loss, close, ...)
            Open_And_Close_Signal = 4,
            All = 5
        }


        public enum Yes_No {
            Yes = 0,
            No = 1
        }

        protected override void OnStart() {
            // System.Diagnostics.Debugger.Launch();

            Print("START BOT.....: " + SymbolName + " : " + GetLabel() + " v." + GetBotVersion() + " | " + GetBotDate());
            string web = GetBotWeb();
            if (web != null && web.Length > 0) {
                Print("WEB: " + GetBotWeb());
            }

            base.OnStart();

            stats = new Stats(this);
            worker = new Worker(this);
            LockTick = false;
        }

        public bool LockTick = false;
        protected override void OnTick() {
            base.OnTick();
            if (IsTradingHour(true) == false) return;

            if (LockTick == false) {
                LockTick = true;
                ClosePositionsLastTradingDay();
                stats.OnTick();
                worker.OnTick();
                LockTick = false;
            }
        }

        protected override void OnBar() {
            base.OnBar();

            if (IsTradingHour(true) == false) return;
            
            int i = 0;
            bool dialogShown = false;
            while (LockTick) {
                Print("Bar Tick Locked.");
                Thread.Sleep(100);
                if (++i > 100) {
                    Print("Bar Tick Locked Too Many Attempts Error.");
                    if (dialogShown == false) {
                        new Thread(new ThreadStart(delegate {
                            // Aby MessageBox nebyl zobrazen jako modal, zobrazíme ho v Threadu.
                            MessageBox.Show("Bar Tick Locked Too Many Attempts Error.....", GetBotName() + " : Bar Tick Locked Too Many Attempts Error.", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        })).Start();
                        dialogShown = true;
                    }
                    break;
                }
            }

            LockTick = true;
            worker.OnBar();
            LockTick = false;
        }

        protected override void OnStop() {
            base.OnStop();
            if (worker != null) worker.OnStop();

            /*
            if (Account.IsLive == false) {
                Print("Uzavreny vsechny zbyle pozice [" + GetPositions().Count + "] pri ukonceni LiPi Bota.");
                // Uzavreme vsechny pozice, pokud nejsme v Live Account
                foreach (Position postion in GetPositions()) {
                    postion.Close();
                }
            }
            */
            /*
               foreach (Position postion in GetPositions()) {
                    postion.Close();
                }
          */

            if (stats != null) stats.Print();
            Print("STOP BOT: " + SymbolName + " : " + GetLabel() + " v." + GetBotVersion() + " | " + GetBotDate());
            string web = GetBotWeb();
            if (web != null && web.Length > 0) {
                Print("WEB: " + GetBotWeb());
            }
        }

        protected bool StopBotOnValidationError(string message) {
            if (RunningMode != RunningMode.Optimization) {
                MessageBox.Show(message, GetBotName() + " : The Parameter Is Incorrect", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            Print("The Parameter Is Incorrect : " + message);
            return StopBot();
        }

        protected bool StopBot() {
            //Stop();
            return false;
        }

        public virtual bool IsMessageBoxShowEnabled() {
            return RunningMode != RunningMode.Optimization 
                && RunningMode != RunningMode.SilentBacktesting;
        }

        public List<Position> GetPositionsAll() {
            string s = SymbolName;
            List<Position> result = Positions.FindAll(GetLabel(), s).ToList();

            // foreach test, zda mezi pozicemi neni pozice jeho symbolu
            foreach (Position p in result) {
                if (p.SymbolName != s) {
                    worker.P("ERROR: GetPositions ::: Selected Position with Wrong SymbolName [" + s + "] : " + p.Id + " " + p.SymbolName);
                    MessageBox.Show("!!!!!!!!!!!!!!!!!!!!! Selected Position with Wrong SymbolName[" + s + "] : " + p.Id + " " + p.SymbolName, "!!!!!!!!!!!!!!!!!!!!! " + GetBotName() + " : ERROR: GetPositions", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            return result;
        }

        public List<List<Position>> GetPositionsGrouped() {
            string s = SymbolName;
            List<Position> result = GetPositionsAll();

            List<List<Position>> res = new List<List<Position>>();
            /*
            List<Position> pPrimes = result.FindAll(item => item.Comment.StartsWith(LPBBase.commentPrime));
            foreach (Position p in pPrimes) {
                List<Position> ps = new List<Position>() { p };
                ps.AddRange(result.FindAll(item => item.Comment.StartsWith(LPBBase.commentAdditional + ":" + p.Id)));

                res.Add(ps);
            }
            */

            List<List<Position>> groupedCustomerList = result
                .GroupBy(u => {
                    if (u.Comment == LiPiBotBase.commentPrime) {
                        return LiPiBotBase.commentAdditional + ":" + u.Id;
                    };
                    return u.Comment;
                })
                .Select(grp => grp.ToList())
                .ToList();

            //            return res;
            return groupedCustomerList;
        }

        protected abstract bool IsTradingHour(bool ShowDialogTradingHourIsActive);

        protected abstract void ClosePositionsLastTradingDay();

        public abstract string GetBotName();
        public abstract string GetLabel();
        public abstract string GetBotVersion();
        public abstract string GetBotDate();
        public abstract string GetBotWeb();

        public bool IsBotInDemoMode() {
            throw new Exception("Not Implemented.");
        }
    }
}
