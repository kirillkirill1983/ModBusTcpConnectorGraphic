using EasyModbus;
using RealTimeGraphX.DataPoints;
using RealTimeGraphX.Renderers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace RealTimeGraphX.WPF.Demo
{
    public class MainWindowVM
    {
        public WpfGraphController<TimeSpanDataPoint, DoubleDataPoint> Controller { get; set; }
        //public WpfGraphController<DateTimeDataPoint, DoubleDataPoint> Controller { get; set; }


        public MainWindowVM()
        {
            Controller = new WpfGraphController<TimeSpanDataPoint, DoubleDataPoint>();
            //Controller = new WpfGraphController<DateTimeDataPoint, DoubleDataPoint>();
            Controller.Range.MinimumY = 0;
            Controller.Range.MaximumY = 33000;
            Controller.Range.MaximumX = TimeSpan.FromSeconds(30);
            //Controller.Range.MaximumX =DateTime.Now;
            Controller.Range.AutoY = true;
            Controller.Range.AutoYFallbackMode = GraphRangeAutoYFallBackMode.MinMax;

            Controller.DataSeriesCollection.Add(new WpfGraphDataSeries()
            {
                Name = "Coonnect MB Device",
                Stroke = Colors.DarkGreen,
            });

            

            ModbusClient modbusClient;
            modbusClient = new ModbusClient("192.168.79.56", 502);
            //modbusClient = new ModbusClient("127.0.0.1", 502);
            modbusClient.Connect();

            Stopwatch watch = new Stopwatch();
            
            TimeSpan myData = DateTime.Now.TimeOfDay;
            watch.Start();


            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    int[] readHoldingRegisters = modbusClient.ReadHoldingRegisters (1539, 1);
                    int y = readHoldingRegisters[0];
                    double res; 
                    res =0.01869 * y + 700;
                    var x = (watch.Elapsed);
                    TimeSpan newtime =myData+x;
                    Controller.PushData(newtime, res);
                    Thread.Sleep(10);
                }
            });
        }
    }
}
