using System;
using System.Net;
using System.IO;

namespace HttpServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var settingsPath = @"./site/settings.json";
            var server = new HttpServer(settingsPath);

            while (true)
            {
                Input(Console.ReadLine(), server);
            }
        }

        static void Input(string input, HttpServer server)
        {
            switch (input)
            {
                case "start":
                    server.Start();
                    break;

                case "stop":
                    server.Stop();
                    break;

                case "restart":
                    server.Stop();
                    server.Start();
                    break;

                default:
                    Console.WriteLine("Нет такой команды!");
                    break;
            }
        }
    }
}
