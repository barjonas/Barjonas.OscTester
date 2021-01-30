using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq;
using VirtualSeat.DownlinkLauncher.Model;
using System.Net;

namespace Barjonas.OscTester
{
    public class Sys
    {
        private readonly ILogger _logger;
        public Sys(ILogger<Sys> logger)
        {
            _logger = logger;
        }

        internal void Run()
        {
            string defaultString = "224.0.0.154:7825";
            IPEndPoint? endPoint = null;
            while (endPoint == null)
            {
                Console.WriteLine($"Enter endpoint or press enter for default ({defaultString})");
                string? requestString = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(requestString))
                {
                    requestString = defaultString;
                }
                if (!IPEndPoint.TryParse(requestString, out endPoint))
                {
                    Console.WriteLine($"{requestString} could not be parsed.");
                }
            }

            _ = new OscWrapper(endPoint, _logger);
            Console.WriteLine("Press any escape to exit");
            while (Console.ReadKey().Key != ConsoleKey.Escape)
            { }
        }
    }
}
