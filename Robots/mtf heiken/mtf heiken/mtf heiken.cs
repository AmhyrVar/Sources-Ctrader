using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cAlgo.API;
using cAlgo.API.Collections;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;



using cAlgo.API;

[Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
public class MultiTimeframeBot : Robot
{
    protected override void OnStart()
    {
        // Define the timeframes to be checked
        var timeframes = new[] { TimeFrame.Minute, TimeFrame.Minute5, TimeFrame.Hour, TimeFrame.Hour4, TimeFrame.Daily };

        // Define the RSI parameters
        var rsiPeriod = 14;
        var rsiOverbought = 70;

        // Subscribe to the DataReceived event of each timeframe
        foreach (var timeframe in timeframes)
        {
            var bars = MarketData.GetBars(timeframe);
            var indicator = Indicators.RelativeStrengthIndex(bars.ClosePrices, rsiPeriod);

            bars.DataReceived += (s, e) =>
            {
                // Check if the last bar is in an uptrend
                if (bars.ClosePrices.LastValue > bars.ClosePrices.Last(1))
                {
                    // Check if the RSI is overbought
                    if (indicator.Result.LastValue > rsiOverbought)
                    {
                        // Place a buy order
                        var volume = Symbol.QuantityToVolume(10000);
                        var stopLoss = bars.LowPrices.LastValue - Symbol.PipValue * 10;
                        var takeProfit = bars.HighPrices.LastValue + Symbol.PipValue * 20;

                        var result = ExecuteMarketOrder(TradeType.Buy, Symbol, volume, "MultiTFBot", stopLoss, takeProfit);
                        if (result.IsSuccessful)
                        {
                            Print("Buy order placed at {0}", result.Order.OpenTime);
                        }
                        else
                        {
                            Print("Failed to place buy order: {0}", result.Error);
                        }
                    }
                }
            };
        }
    }
}
