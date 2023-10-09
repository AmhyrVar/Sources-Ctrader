using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cAlgo.API;
using cAlgo.API.Collections;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;




[Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
public class MultiTimeframeBot : Robot
{
    private readonly Dictionary<TimeFrame, Bars> _barsByTimeframe = new Dictionary<TimeFrame, Bars>();
    private readonly Dictionary<TimeFrame, IndicatorDataSeries> _rsiByTimeframe = new Dictionary<TimeFrame, IndicatorDataSeries>();
    
     private RelativeStrengthIndex rsi_Minute;
     private RelativeStrengthIndex rsi_Minute5;
     private RelativeStrengthIndex rsi_Hour;
      private RelativeStrengthIndex rsi_Hour4;
       private RelativeStrengthIndex rsi_Daily;

    protected override void OnStart()
    {
        // Define the timeframes to be checked
        var timeframes = new[] { TimeFrame.HeikinMinute, TimeFrame.HeikinMinute5, TimeFrame.HeikinHour, TimeFrame.HeikinHour4, TimeFrame.HeikinDaily };

        var rsiOverbought = 70;

        rsi_Minute = Indicators.RelativeStrengthIndex(MarketData.GetBars(TimeFrame.HeikinMinute).ClosePrices, 14);
         rsi_Minute5 = Indicators.RelativeStrengthIndex(MarketData.GetBars(TimeFrame.HeikinMinute5).ClosePrices, 14);
         rsi_Hour = Indicators.RelativeStrengthIndex(MarketData.GetBars(TimeFrame.HeikinHour).ClosePrices, 14);
         rsi_Hour4 = Indicators.RelativeStrengthIndex(MarketData.GetBars(TimeFrame.HeikinHour4).ClosePrices, 14);
         rsi_Daily = Indicators.RelativeStrengthIndex(MarketData.GetBars(TimeFrame.HeikinDaily).ClosePrices, 14);
        }

protected override void OnBar()
{
    var openpo = Positions.FindAll("Heikin", SymbolName);
       //Buy
       Print("Uptrend" ,UpTrend(), " Up Onetwo ",  UpOne()&& UpTwo()&&UpThree()&&UpFour()&&UpFive() , "RSI over" , RSI_Over());
       if (UpTrend() && openpo.Length == 0)
       {
        ExecuteMarketOrder(TradeType.Buy,SymbolName,1000,"Heikin",50,50);
       }
    
}

private bool UpOne()
{
      var Minute = MarketData.GetBars(TimeFrame.HeikinMinute).Last(1);
 
    
 
    
    if (Minute.Open > Minute.Close &&  rsi_Minute.Result.LastValue > 70  )
    {Print("NODE1");
    return true;
    }
    else {return false;}
}

private bool UpTwo()

{
var Minute = MarketData.GetBars(TimeFrame.HeikinMinute5).Last(1);
 
   
    var rsi5 =  Indicators.RelativeStrengthIndex(MarketData.GetBars(TimeFrame.HeikinMinute5).ClosePrices, 14);
  
    
    if (Minute.Open > Minute.Close  )
    {Print("NODE2");
    return true;}
    else {return false;}
}
private bool UpThree()

{
var Minute = MarketData.GetBars(TimeFrame.HeikinHour).Last(1);
 
    
   
    
    if (Minute.Open > Minute.Close  &&  rsi_Hour.Result.LastValue > 70 )
    {Print("NODE3");
    return true;}
    else {return false;}
}
private bool UpFour()

{
var Minute = MarketData.GetBars(TimeFrame.HeikinHour4).Last(1);
 
    
   
    
    if (Minute.Open > Minute.Close  &&  rsi_Hour4.Result.LastValue > 70 )
    {Print("NODE4");
    return true;}
    else {return false;}
}
private bool UpFive()

{
var Minute = MarketData.GetBars(TimeFrame.Daily).Last(1);
 
    
   
    
    if (Minute.Open > Minute.Close &&  rsi_Daily.Result.LastValue > 70  )
    {Print("NODE5");
    return true;}
    else {return false;}
}


private bool UpTrend()
{

    
    if (UpOne()&& UpTwo()&&UpThree()&&UpFive()&& UpFour() )
    {return true;}
    else {return false;}
    
}
private bool RSI_Over()
{
    if (  rsi_Minute.Result.LastValue > 70 && rsi_Minute5.Result.LastValue > 70 
    && rsi_Hour.Result.LastValue > 70 && rsi_Hour4.Result.LastValue > 70 && rsi_Daily.Result.LastValue > 70)
    {return true;}
    else {return false;}
}

}

   

