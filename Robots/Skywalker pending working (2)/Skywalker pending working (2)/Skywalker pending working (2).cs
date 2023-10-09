using System;

using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;


namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
    public class Skywalkerpendingworking : Robot
    {

        public int Year;


        public int Month;


        public int Day;
        public int Hour;
        public int Minute;
        public int Second;



        public string MyLine;


        [Parameter("File directory", DefaultValue = "F:\\Skywalker.log")]
        public string Log_File { get; set; }

        protected override void OnStart()
        {


            //format EURUSD 14/06/2021 08:00:00
                        /*string sentence = "Example sentence";
string [] sentenses = sentence.Split(' ');

string one = sentenses[0];
string two = sentenses[1];*/

            //read file get symbolname line 
            //slice that line
            //dump to Y M H M S

List<string> arrLine = File.ReadAllLines(Log_File).ToList();

            int i = 0;

            while (i < arrLine.Count)
            {
                if (arrLine[i].Contains(SymbolName))
                {
                    MyLine = arrLine[i];
                    Print("MyLine is " + MyLine);

                }

                i = i + 1;
            }

            string[] stuff = MyLine.Split(' ');

            var one = stuff[0];
            var two = stuff[1];
            var three = stuff[2];


            Print("1 -" + one);
            Print("2 -" + two);
            Print("3 -" + three);



            string[] myDate = two.Split('/');
            //day month year



            Month = Convert.ToInt32(myDate[0]);
            Day = Convert.ToInt32(myDate[1]);
            Year = Convert.ToInt32(myDate[2]);


            string[] myTime = three.Split(':');

            Hour = Convert.ToInt32(myTime[0]);
            Minute = Convert.ToInt32(myTime[1]);
            Second = Convert.ToInt32(myTime[2]);









            var date = new DateTime(Year, Month, Day, Hour, Minute, Second);
            Print("Initialization to " + date);
            while (Bars.OpenTimes[0] > date)
                Bars.LoadMoreHistory();
            Chart.ScrollXTo(date);
            Print("DONE");









        }

        protected override void OnTick()
        {
            // Put your core logic here
        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }
    }
}
