using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;


namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
    public class fractalis : Robot
    {


        [Parameter(DefaultValue = 1000)]
        public int Volume { get; set; }

        public double close;
        public Position _position;
        private Fractals i_fractal;
        List<double> UpFrac = new List<double>();








        protected override void OnStart()
        {

            double close = Bars.ClosePrices.LastValue;
            i_fractal = Indicators.GetIndicator<Fractals>(10);



        }

        protected override void OnStop()
        {

            //foreach (Part aPart in parts)
            foreach (double upfrac in UpFrac)
            {

                string FractalString = "" + upfrac;

                string[] lines = 
                {
                    FractalString
                };


                System.IO.File.AppendAllLines("C:\\\\Users\\\\Amir\\\\Desktop\\\\log.txt", lines);

            }
        }

        //int totalPositions = Positions.Count;



        bool IsPosOpen()
        {
            if (Positions.Count == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }





        protected override void OnBar()
        {


            bool IPO = IsPosOpen();
            UpFrac.Add(i_fractal.UpFractal.LastValue);

        }
    }
    /* if (UpFrac.Count == 0)
            {
                UpFrac.Add(i_fractal.UpFractal.LastValue);
            }
            else if (i_fractal.UpFractal.LastValue != UpFrac[0])
            {
                UpFrac.Add(i_fractal.UpFractal.LastValue);
            }
        }*/
    //{

    //}


    //Créer une var qui parcourt la liste puis printer cette liste avec un index









}
/*if (!IPO)
            {
                string PBOT = " LastFractal-1 " + i_fractal.UpFractal.LastValue;

                string[] lines = 
                {
                    PBOT
                    //var someItem = list[yourIndex]; 


                };


                System.IO.File.AppendAllLines("C:\\\\Users\\\\Amir\\\\Desktop\\\\log.txt", lines);


                if (i_fractal.UpFractal.Last(0) < i_fractal.UpFractal.Last(1) && i_fractal.UpFractal.Last(1) > i_fractal.UpFractal.Last(2))
                {
                    ExecuteMarketOrder(TradeType.Sell, Symbol, Volume, "Sell");


                }
            }*/






/*foreach (var position in Positions)
            {
                if (IPO && )
                {
                    ClosePosition(position);
                }
            }*/




