using DarkSigil.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkSigil.Modules.Cal
{
    public class cal : ICommands
    {
        public void Execute(string[] args)
        {
            string param = args.Length > 0 ? args[0] : "";

            if (param.EndsWith("?") || param.EndsWith("help"))
            {
                Console.WriteLine("Usage: cal [month]\n");
                Console.WriteLine("Example: cal January or Cal February\n");
                return;
            }

            if (args.Length == 0)
            {
                Default();
                return;
            }

            if (args.Length > 0 && !param.EndsWith("?"))
            {
                SpecifiedMonth(args[0]);
                return;
            }

        }

        private void Default()
        {
            DateTime currentDate = DateTime.Now;

            int currentMonth = currentDate.Month;
            int currentYear = currentDate.Year;
            int currentDay = currentDate.Day;

            Console.ForegroundColor = ConsoleColor.Cyan;

            DateTime firstDayOfMoth = new DateTime(currentYear, currentMonth, 1);
            int daysInMonth = DateTime.DaysInMonth(currentYear, currentMonth);

            int startDayOfWeek = (int)firstDayOfMoth.DayOfWeek;

            Console.WriteLine($"Calendar for {currentDate.ToString("MMMM yyyy")}");

            Console.ResetColor();

            Console.WriteLine("Sun Mon Tue Wed Thu Fri Sat");

            for (int i = 0; i < startDayOfWeek; i++)
            {
                Console.Write("    ");
            }

            for (int a = 1; a <= daysInMonth; a++)
            {
                if (a == currentDay)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(a.ToString("D2") + "  ");
                    Console.ResetColor();
                }
                else
                {
                    Console.Write(a.ToString("D2") + "  ");
                }

                if ((a + startDayOfWeek) % 7 == 0)
                {
                    Console.WriteLine();
                }
            }

            Console.WriteLine();
        }

        private void SpecifiedMonth(string month)
        {
            try
            {
                DateTime currentDate = DateTime.Now;

                int currentYear = currentDate.Year;

                int specifiedMonth = DateTime.ParseExact(month, "MMMM", System.Globalization.CultureInfo.InvariantCulture).Month;

                if (specifiedMonth < 1 || specifiedMonth > 12)
                {
                    Console.WriteLine("Invalid month. Please enter a valid month name.");
                    return;
                }

                Console.ForegroundColor = ConsoleColor.Cyan;

                DateTime firstDayOfMoth = new DateTime(currentYear, specifiedMonth, 1);
                int daysInMonth = DateTime.DaysInMonth(currentYear, specifiedMonth);

                int startDayOfWeek = (int)firstDayOfMoth.DayOfWeek;

                Console.WriteLine($"Calendar for {month} {currentYear}");

                Console.ResetColor();

                Console.WriteLine("Sun Mon Tue Wed Thu Fri Sat");

                for (int i = 0; i < startDayOfWeek; i++)
                {
                    Console.Write("    ");
                }

                for (int a = 1; a <= daysInMonth; a++)
                {
                    Console.Write(a.ToString("D2") + "  ");

                    if ((a + startDayOfWeek) % 7 == 0)
                    {
                        Console.WriteLine();
                    }
                }

                Console.WriteLine();
            }
            catch (FormatException)
            {
                Console.WriteLine("Invalid month. Please enter a valid month name.");
            }
            catch (ArgumentOutOfRangeException)
            {
                Console.WriteLine("Invalid month. Please enter a valid month name.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

        }
    }
}
