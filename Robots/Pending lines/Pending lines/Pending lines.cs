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
    public class Pendinglines : Robot
    {

        [Parameter("File directory", DefaultValue = "F:\\Skywalker.log")]
        public string Log_File { get; set; }
        [Parameter("Use comment in vertical line", DefaultValue = false)]
        public bool UseComment { get; set; }
        [Parameter("Vertical line comment", DefaultValue = "a")]
        public string Comment { get; set; }

        public DateTime XAxis;
        public string OPut;
        public bool Contain = false;




//we first need a contain check

        protected override void OnStart()
        {

            if (!UseComment)
                Print("There we go");
            {
                var lines = Chart.FindAllObjects<ChartVerticalLine>();



                foreach (var line in lines)
                {
                    XAxis = line.Time;



                }

                OPut = (SymbolName + " " + XAxis);



                List<string> arrLine = File.ReadAllLines(Log_File).ToList();

                var i = 0;

                Print("Arrlength = " + arrLine.Count);

                while (i < arrLine.Count)
                {
                    Print("one ride");
                    if (arrLine[i].Contains(SymbolName) && arrLine[i] != OPut)
                    {

                        arrLine[i] = OPut;


                        Contain = true;
                        File.WriteAllLines(Log_File, arrLine.ToArray());
                        Print("VISITEEEEEEEEEED");

                        Stop();
                    }
                    if (arrLine[i].Contains(SymbolName) && arrLine[i] == OPut)
                    {
                        Print("We broke");
                        Contain = true;

                        Stop();
                    }



                    i = i + 1;
                }

                if (Contain == false)
                {
                    Print("We Add");
                    arrLine.Add(OPut);
                    Contain = true;
                    File.WriteAllLines(Log_File, arrLine.ToArray());

                    Stop();
                }
            }


            if (UseComment)
                Print("There we go");
            {
                var lines = Chart.FindAllObjects<ChartVerticalLine>();



                foreach (var line in lines)
                {
                    if (line.Comment == Comment)
                    {
                        XAxis = line.Time;
                    }


                }

                OPut = (SymbolName + " " + XAxis);



                List<string> arrLine = File.ReadAllLines(Log_File).ToList();

                var i = 0;

                Print("Arrlength = " + arrLine.Count);

                while (i < arrLine.Count)
                {
                    Print("one ride");
                    if (arrLine[i].Contains(SymbolName) && arrLine[i] != OPut)
                    {

                        arrLine[i] = OPut;


                        Contain = true;
                        File.WriteAllLines(Log_File, arrLine.ToArray());
                        Print("VISITEEEEEEEEEED");

                        Stop();
                    }
                    if (arrLine[i].Contains(SymbolName) && arrLine[i] == OPut)
                    {
                        Print("We broke");
                        Contain = true;

                        Stop();
                    }



                    i = i + 1;
                }

                if (Contain == false)
                {
                    Print("We Add");
                    arrLine.Add(OPut);
                    Contain = true;
                    File.WriteAllLines(Log_File, arrLine.ToArray());

                    Stop();
                }
            }

        }

        protected override void OnTick()
        {





        }






        protected override void OnStop()
        {


        }
    }
}
