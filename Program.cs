﻿using EffectiveMobileTestTask.Classes;
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
        static List<Order> ordersList = new List<Order>(); // Список всех заказов
        private static readonly Logger logger = LogManager.GetCurrentClassLogger(); // Подключение логгера

        public static void Main(string[] args)
        {
            ConfigureLogging();

            string[] parameters = ValidateParameters(args);

            if (parameters.Length == 0)
            {
                return;
            }

            string cityDistrict = parameters[0];
            DateTime firstDeliveryDateTime = Convert.ToDateTime(parameters[1]);

            if (!WriteDownOrders())
                return;

            FilterAndSaveOrders(cityDistrict, firstDeliveryDateTime);
        }

        public static string[] ValidateParameters(string[] args) // Валидация входных параметров
        {
            try
            {
                string[] parameters;
                if (args.Length != 3)
                {
                    logger.Error("Invalid number of input parameters! Using data from config file instead...");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Invalid number of input parameters! Using data from config file instead...");
                    Console.ForegroundColor = ConsoleColor.White;
                    Thread.Sleep(1000);
                    parameters = File.ReadAllText("_config.txt").Split(" ");
                }
                else
                {
                    parameters = args;
                }

                string cityDistrict = parameters[0];
                DateTime firstDeliveryDateTime;
                if (!DateTime.TryParse($"{parameters[1]} {parameters[2]}", out firstDeliveryDateTime))
                {
                    logger.Error("Invalid date format for _firstDeliveryDateTime. Pattern: \"area\" yyyy-mm-dd HH:MM:SS");
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid date format for _firstDeliveryDateTime. Pattern: \"area\" yyyy-mm-dd HH:MM:SS");
                    Console.ForegroundColor = ConsoleColor.White;
                    throw new Exception();
                }

                return new string[] { cityDistrict, firstDeliveryDateTime.ToString("yyyy-MM-dd HH:mm:ss") };
            }
            catch (Exception ex)
            {
                logger.Error("Something went wrong while validating parameters!");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Something went wrong while validating parameters!");
                Console.ForegroundColor = ConsoleColor.White;
                return new string[] { };
            }
        }

        public static bool WriteDownOrders() // Запись заказов в список заказов ordersList
        {
            IFormatProvider formatter = new NumberFormatInfo { NumberDecimalSeparator = "." }; // Форматтер для указания точки в качестве разделителя дробной части
            List<string> lines = new List<string>();

            try
            {
                if (!ValidateOrders(lines))
                {
                    throw new Exception();
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

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Order with ID: {id} succesfully added!");
                    Console.ForegroundColor = ConsoleColor.White;
                    logger.Info($"Order with ID: {id} succesfully added!");
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("An error occurred while processing orders.");
                Console.ForegroundColor = ConsoleColor.White;
                logger.Error("An error occurred while processing orders.");
                Console.ReadLine();
                return false;
            }

            return true;
        }

        public static bool ValidateOrders(List<string>lines) // Валидация каждого заказа
        {
            using (StreamReader reader = new StreamReader("_Orders.txt"))
            {
                // Регулярное выражение для валидации каждой строки
                Regex regex = new Regex("^\\d* \\d*(\\.\\d*)? [A-Za-zА-Яа-я0-9_]* \\d\\d\\d\\d-[0-1]?\\d-[0-3]?\\d [0-2]?\\d:[0-5]?\\d:[0-5]?\\d$");

                string? line;

                while ((line = reader.ReadLine()) != null)
                {
                    if (regex.IsMatch(line.Trim()))
                    {
                        lines.Add(line);
                    }
                    else
                    {
                        logger.Error("Input values have incorrect format! Fix them and try again!");
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Input values have incorrect format! Fix them and try again!");
                        Console.ForegroundColor = ConsoleColor.White;
                        return false;
                    }
                }

                return true;
            }
        }

        private static void ConfigureLogging() // Настройка логгирования
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

        public static void FilterAndSaveOrders(string cityDistrict, DateTime firstDeliveryDateTime) // Поиск и сохранение заказов по заданному условию
        {
            DateTime endDeliveryTime = firstDeliveryDateTime.AddMinutes(30);

            var filteredOrders = ordersList
                .Where(order => order.Area == cityDistrict &&
                                order.OrderTime >= firstDeliveryDateTime &&
                                order.OrderTime <= endDeliveryTime)
                .ToList();

            try
            {
                using (StreamWriter writer = new StreamWriter("_deliveryOrders.txt", false))
                {
                    string separator = $"|{new string('-', 7)}+{new string('-', 15)}+{new string('-', 15)}+{new string('-', 20)}|";
                    writer.WriteLine($"Orders for delivery in {cityDistrict} between {firstDeliveryDateTime} and {endDeliveryTime}:\n");
                    writer.WriteLine($"|{"Id",-7}|{"Weight(kg)",-15}|{"Area",-15}|{"Time",-20}|");
                    writer.WriteLine(separator);


                    foreach (var order in filteredOrders)
                    {
                        writer.WriteLine($"|{order.Id,-7}|{order.Weight,-15:F3}|{order.Area,-15}|{order.OrderTime.ToString("yyyy-MM-dd HH:mm:ss"),-20}|");
                        writer.WriteLine(separator);
                    }
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{filteredOrders.Count} orders filtered and saved to _deliveryOrders.txt");
                Console.ForegroundColor = ConsoleColor.White;
                logger.Info($"{filteredOrders.Count} orders filtered and saved to _deliveryOrders.txt");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                logger.Error("Something went wrong while writting to _deliveryOrders.txt");
                Console.WriteLine("Something went wrong while writting to _deliveryOrders.txt");
            }
        }
    }
}