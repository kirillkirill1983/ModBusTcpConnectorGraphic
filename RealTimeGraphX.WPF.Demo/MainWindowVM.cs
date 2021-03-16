using EasyModbus;
using RealTimeGraphX.DataPoints;
using RealTimeGraphX.Renderers;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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
            //modbusClient = new ModbusClient("192.168.79.56", 502);
            modbusClient = new ModbusClient("127.0.0.1", 502);
            modbusClient.Connect();

            Stopwatch watch = new Stopwatch();
            
            TimeSpan myData = DateTime.Now.TimeOfDay;
            watch.Start();
            string ConnectionString = @"Data Source=.\SQLEXPRESS; Initial Catalog = test_DB; integrated Security=True; Connect Timeout = 30";
            SqlConnection connection = new SqlConnection(ConnectionString);
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    int[] readHoldingRegisters = modbusClient.ReadHoldingRegisters (1, 1);
                    int y = readHoldingRegisters[0];
                    double res; 
                    res =0.01869 * y + 700;
                    int value = Convert.ToInt32(res) ;
                    var x = (watch.Elapsed);
                    TimeSpan newtime =myData+x;
                    Controller.PushData(newtime, res);

                    DateTime date_ = DateTime.Now; ;
                    //string ConnectionString = @"Data Source=.\SQLEXPRESS; Initial Catalog = test_DB; integrated Security=True; Connect Timeout = 30";
                    string sqlExpression = "INSERT INTO Table_Teast2 (value, Time) VALUES (@value, @date_)";
                    //SqlConnection connection = new SqlConnection(ConnectionString);
                    try
                    {

                        connection.Open();

                        SqlCommand command = new SqlCommand(sqlExpression, connection);

                        SqlParameter dateParam = new SqlParameter("@date_", date_);
                        command.Parameters.Add(dateParam);

                        SqlParameter valueParam = new SqlParameter("@value", value);
                        command.Parameters.Add(valueParam);

                        command.ExecuteNonQuery();

                    }
                    catch (SqlException ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    finally
                    {
                        // закрываем подключение
                        connection.Close();
                    }
                    Thread.Sleep(250);
                }
            });
        }
    }
}
