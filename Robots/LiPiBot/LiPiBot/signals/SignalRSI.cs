using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo {
    class SignalRSI : ISignal {

        private LPBSRSI robot;

        private RelativeStrengthIndex rsi;

        public SignalRSI(LiPiBotBase robot) {
            this.robot = (LPBSRSI)robot;
            Init();
        }

        private void Init() {
            DataSeries source = this.robot.RSI_Source;
            int periods = this.robot.RSI_Periods;

            this.rsi = this.robot.Indicators.RelativeStrengthIndex(source, periods);
        }

        public SIGNAL GetSignal() {
            int levelMin = robot.RSI_LevelMin;
            int levelMax = 100 - robot.RSI_LevelMin;

            if (rsi.Result.LastValue <= levelMin) {
                return SIGNAL.BUY;
            } else if (rsi.Result.LastValue >= levelMax) {
                return SIGNAL.SELL;
            }
            return SIGNAL.NONE;
        }
        public void OnPositionOpen(TradeResult position) {
            
        }
    }
}


