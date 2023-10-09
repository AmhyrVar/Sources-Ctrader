using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;
using ClickAlgo.Data.Logger;

namespace cAlgo
{
    /// <summary>
    /// Your algo will need full access to create the log files.
    /// </summary>
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
    public class DataLoggerExample : Robot
    {
        [Parameter("Show Log File", DefaultValue = true)]
        public bool ShowLogFile { get; set; }

        #region indicator declarations

        private RelativeStrengthIndex _rsi;

        #endregion

        /// <summary>
        /// This method is called when cTrader first starts
        /// </summary>
        protected override void OnStart()
        {
            // initialize the indicator
            _rsi = Indicators.RelativeStrengthIndex(MarketSeries.Close, 14);

            // path will be created if it does not exist
            TradeLogger.SetLogDir("c:\\\\ClickAlgo\\logs\\");

            // if you do not have excel installed, this will create a text file instead
            TradeLogger.Extension = "txt";
        }

        protected override void OnTick()
        {
            // shows how we can log to file when account balance is low
            if (Account.Equity < 100)
            {
                TradeLogger.Warning("Account balance low");
                TradeLogger.StopLogging();
            }
        }

        /// <summary>
        /// This method is called at the close of each candle (bar)
        /// </summary>
        protected override void OnBar()
        {
            try
            {
                // Logging indicator values.
                if (_rsi.Result.LastValue > 80)
                {
                    TradeLogger.Info("RSI > 80 " + _rsi.Result.LastValue.ToString("0.0000"), Server.Time);
                }

                if (_rsi.Result.LastValue < 20)
                {
                    TradeLogger.Info("RSI < 20 " + _rsi.Result.LastValue.ToString("0.0000"), Server.Time);
                }
            } catch (Exception ex)
            {
                // if an error occurs, we log the error
                TradeLogger.Error(ex.Message);
            }
        }

        /// <summary>
        /// This method is called when the robot stops
        /// </summary>
        protected override void OnStop()
        {
            // We automatically show the log file using excel, notepad or any other application, this depends on the extension you have set.
            if (ShowLogFile)
            {
                TradeLogger.ShowLogFile();
            }
        }
    }
}
