using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sadovinki
{
    class Program
    {
        static void ArgumentsError()
        {
            Console.WriteLine("Используйте:");
            Console.WriteLine("Sadovniki.exe server <размер поля> <число садовников>");
            Console.WriteLine("Sadovniki.exe client");
            Environment.Exit(1);
        }


        static void Main(string[] args)
        {  
            if (args.Length == 0)
            {
                ArgumentsError();
            }
            switch (args[0])
            {
                case "server":
                {
                    var server = new ServerSide(int.Parse(args[1]), int.Parse(args[2]));
                    server.Start();
                    break;
                }
                case "client":
                {
                    var client = new ClientSide();
                    client.Start();
                    break;
                }
            }
        }

    }
}
