using EffectiveMobileTestTask.Classes;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Globalization;
using NLog;
using NLog.Targets;
using NLog.Config;

namespace EffectiveMobileTestTask
{
    public class Program
    {
        static List<Order> ordersList = new List<Order>();
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static void Main(string[] args)
        {
            ConfigureLogging();

            string[] parameters;
            if (args.Length != 3)
            {
                parameters = File.ReadAllText("config.txt").Split(" ");
            }
            else
            {
                parameters = args;
            }

            string cityDistrict = parameters[0];
            DateTime firstDeliveryDateTime;
            if (!DateTime.TryParse($"{parameters[1]} {parameters[2]}", out firstDeliveryDateTime))
            {
                logger.Error("Invalid date format for _firstDeliveryDateTime.");
                Console.WriteLine("Invalid date format for _firstDeliveryDateTime.");
                return;
            }

            if (!WriteDownOrders())
                return;

            FilterAndSaveOrders(cityDistrict, firstDeliveryDateTime);
            DisplayOrders();
        }

        public static bool WriteDownOrders()
        {
            IFormatProvider formatter = new NumberFormatInfo { NumberDecimalSeparator = "." };
            List<string> lines = new List<string>();

            try
            {
                using (StreamReader reader = new StreamReader("Orders.txt"))
                {
                    Regex regex = new Regex("^\\d* \\d*(\\.\\d*)? [A-Za-zА-Яа-я0-9_]* \\d\\d\\d\\d-\\d\\d?-\\d\\d? \\d\\d?:\\d\\d?:\\d\\d?$");

                    string? line;

                    while ((line = reader.ReadLine()) != null)
                    {
                        if (regex.IsMatch(line.Trim()))
                        {
                            lines.Add(line);
                        }
                        else
                        {
                            logger.Error("Input values have incorrect format! Fix them and try agian!");
                            Console.WriteLine("Input values have incorrect format! Fix them and try agian!");
                            return false;
                        }
                    }
                }

                foreach (var line in lines)
                {
                    string[] details = line.Split(" ");
                    int id = int.Parse(details[0]);
                    double weight = Math.Round(double.Parse(details[1], formatter), 3);
                    string area = details[2];
                    DateTime orderDate = Convert.ToDateTime($"{details[3]} {details[4]}");

                    Order order = new Order(id, weight, area, orderDate);

                    ordersList.Add(order);

                    logger.Info($"Order with ID: {id} succesfully added!");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "An error occurred while processing orders.");
                return false;
            }

            return true;
        }

        public static void DisplayOrders()
        {
            Console.Clear();
            Console.WriteLine($"{"Id", -7}{"Weight(kg)", -15}{"Area", -15}{"Time", -20}");
            foreach (var order in ordersList)
            {
                Console.WriteLine($"{order.Id, -7}{order.Weight, -15:F3}{order.Area, -15}{order.OrderTime, -20}");
            }
        }

        private static void ConfigureLogging()
        {
            var config = new LoggingConfiguration();

            var fileTarget = new FileTarget("fileTarget")
            {
                FileName = "_deliveryLog.txt",
                Layout = "${longdate} | ${level:uppercase=true} | ${message}"
            };

            config.AddRule(LogLevel.Info, LogLevel.Fatal, fileTarget);

            LogManager.Configuration = config;
        }

        public static void FilterAndSaveOrders(string cityDistrict, DateTime firstDeliveryDateTime)
        {
            DateTime endDeliveryTime = firstDeliveryDateTime.AddMinutes(30);

            var filteredOrders = ordersList
                .Where(order => order.Area == cityDistrict &&
                                order.OrderTime >= firstDeliveryDateTime &&
                                order.OrderTime <= endDeliveryTime)
                .ToList();

            using (StreamWriter writer = new StreamWriter("_deliveryOrders.txt"))
            {
                string separator = $"|{new string('-', 7)}+{new string('-', 15)}+{new string('-', 15)}+{new string('-', 20)}|";
                writer.WriteLine($"Orders for delivery in {cityDistrict} between {firstDeliveryDateTime} and {endDeliveryTime}:\n");
                writer.WriteLine($"|{"Id",-7}|{"Weight(kg)",-15}|{"Area",-15}|{"Time",-20}|");
                writer.WriteLine(separator);


                foreach (var order in filteredOrders)
                {
                    writer.WriteLine($"|{order.Id,-7}|{order.Weight,-15:F3}|{order.Area,-15}|{order.OrderTime,-20}|");
                    writer.WriteLine(separator);
                }
            }

            logger.Info($"{filteredOrders.Count} orders filtered and saved to _deliveryOrders.txt");
        }
    }
}