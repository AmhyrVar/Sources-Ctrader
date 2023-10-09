using cAlgo.API;
using cAlgo.API.Indicators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cAlgo {
    class SignalStochastic : ISignal {

        private LPBSStochastic robot;

        private StochasticOscillator stochasticOscillator;

        public SignalStochastic(LiPiBotBase robot) {
            this.robot = (LPBSStochastic)robot;
            Init();
        }

        private void Init() {
            int periodsK = this.robot.Stochastic_PeriodsK;
            int periodsD = this.robot.Stochastic_PeriodsD;
            int slowingK = this.robot.Stochastic_SlowingK;
            MovingAverageType maType = this.robot.Stochastic_MovingAverageType;
            Bars bars = this.robot.MarketData.GetBars(this.robot.TimeFrame);

            this.stochasticOscillator = this.robot.Indicators.StochasticOscillator(bars, periodsK, slowingK, periodsD, maType);
        }

        public SIGNAL GetSignal() {
            int levelMin = robot.Stochastic_LevelMin;
            int levelMax = 100 - robot.Stochastic_LevelMin;

            /*
             * Puvodni kod hledani signalu s prekriyenim Stoch.D a Stoch.K uvnitr Level Area.
            if (stochasticOscillator.PercentK.HasCrossedAbove(stochasticOscillator.PercentD, 0) && stochasticOscillator.PercentK.Last(1) <= levelMin) {
                return SIGNAL.BUY;
            } else if (stochasticOscillator.PercentK.HasCrossedBelow(stochasticOscillator.PercentD, 0) && stochasticOscillator.PercentK.Last(1) >= levelMax) {
                return SIGNAL.SELL;
            }
            return SIGNAL.NONE;
            */

            SIGNAL signal = SIGNAL.NONE;
            if (robot.StochasticSignal_CrossRedAndGreenLines == LiPiBotBase.Instrument_Signal.Yes) {
                SIGNAL signalTmp = SIGNAL.NONE;
                if (stochasticOscillator.PercentK.HasCrossedAbove(stochasticOscillator.PercentD, 0)) {
                    signalTmp = SIGNAL.BUY;
                } else if (stochasticOscillator.PercentK.HasCrossedBelow(stochasticOscillator.PercentD, 0)) {
                    signalTmp = SIGNAL.SELL;
                }
                if (signalTmp == SIGNAL.NONE) return SIGNAL.NONE;
                if (signal != SIGNAL.NONE && signal != signalTmp) return SIGNAL.NONE;
                signal = signalTmp;
            }

            if (robot.StochasticSignal_GreenLineInLevelArea == LiPiBotBase.Instrument_Signal.Yes) {
                SIGNAL signalTmp = SIGNAL.NONE;
                if (stochasticOscillator.PercentK.Last(1) <= levelMin) {
                    signalTmp = SIGNAL.BUY;
                } else if (stochasticOscillator.PercentK.Last(1) >= levelMax) {
                    signalTmp = SIGNAL.SELL;
                }
                if (signalTmp == SIGNAL.NONE) return SIGNAL.NONE;
                if (signal != SIGNAL.NONE && signal != signalTmp) return SIGNAL.NONE;
                signal = signalTmp;
            }

            if (robot.StochasticSignal_GreenLineCrossLineIntoLevelArea == LiPiBotBase.Instrument_Signal.Yes) {
                SIGNAL signalTmp = SIGNAL.NONE;
                if (stochasticOscillator.PercentK.Last(2) > levelMin && stochasticOscillator.PercentK.Last(1) <= levelMin) {
                    signalTmp = SIGNAL.BUY;
                } else if (stochasticOscillator.PercentK.Last(2) < levelMax && stochasticOscillator.PercentK.Last(1) >= levelMax) {
                    signalTmp = SIGNAL.SELL;
                }
                if (signalTmp == SIGNAL.NONE) return SIGNAL.NONE;
                if (signal != SIGNAL.NONE && signal != signalTmp) return SIGNAL.NONE;
                signal = signalTmp;
            }

            if (robot.StochasticSignal_GreenLineCrossLineFromLevelArea == LiPiBotBase.Instrument_Signal.Yes) {
                SIGNAL signalTmp = SIGNAL.NONE;
                if (stochasticOscillator.PercentK.Last(2) <= levelMin && stochasticOscillator.PercentK.Last(1) > levelMin) {
                    signalTmp = SIGNAL.BUY;
                } else if (stochasticOscillator.PercentK.Last(2) >= levelMax && stochasticOscillator.PercentK.Last(1) < levelMax) {
                    signalTmp = SIGNAL.SELL;
                }
                if (signalTmp == SIGNAL.NONE) return SIGNAL.NONE;
                if (signal != SIGNAL.NONE && signal != signalTmp) return SIGNAL.NONE;
                signal = signalTmp;
            }

            return signal;
        }

        public void OnPositionOpen(TradeResult position) {
            
        }
    }
}
