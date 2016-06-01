using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Pipes;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace Sadovinki
{
    class ClientSide
    {

        enum State
        {
            Start,
            WaitDifferentClients,
            EndWork,
        }

        enum Direction
        {
            Up,
            Down,
            Left,
            Right,
        }


        private Gardens _gardens;
        private NamedPipeClientStream pipeClient;
        private int _idGarden;
        private CoordinatesWithData _coordinates;

        private int _center;



        public ClientSide()
        {

        }

        private void PopulateData()
        {
            this._gardens = new Gardens(1);
            this._coordinates = new CoordinatesWithData();
        }

        public static NamedPipeClientStream CreatePipeClientStream()
        {
            return new NamedPipeClientStream(
                ".",
                "pipe_sadovniky",
                PipeDirection.InOut,
                PipeOptions.Asynchronous
            );
        }


        public void Start()
        {

            var random = new Random();
            var restTime = random.Next(1000, 5000);
            Thread.Sleep(restTime);

            try
            {
                SendData();

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Thread.Sleep(1000);
                Console.ReadLine();
            }
        }

        private void SendCommand(byte[] data)
        {
            pipeClient.Write(data, 0, data.Length);
            pipeClient.WaitForPipeDrain();
        }

        public static void TryConnect(NamedPipeClientStream pipeStream)
        {
            try
            {
                pipeStream.Connect();
            }
            catch (FileNotFoundException)
            {
                Thread.Sleep(2000);
                TryConnect(pipeStream);
            }
        }



        private void SetGardenFunction(int index, int center)
        {   
            int currentIndex = 0;
            if (index > 5)
            {
                currentIndex = index % 5;
                if (index % 5 == 0)
                {
                    currentIndex = 5;
                }
                Console.WriteLine("Clear cursor function, garden function is {0}", currentIndex);
            }
            else
            {
                currentIndex = index;
            }
            switch (currentIndex)
            {
                case 1:
                    this._gardens.getNextCoordinates1(center, this._coordinates, index, pipeClient, currentIndex);
                    break;
                case 2:
                    this._gardens.getNextCoordinates2(center, this._coordinates, index, pipeClient, currentIndex);
                    break;
                case 3:
                    this._gardens.getNextCoordinates3(center, this._coordinates, index, pipeClient, currentIndex);
                    break;
                case 4:
                    this._gardens.getNextCoordinates4(center, this._coordinates, index, pipeClient, currentIndex);
                    break;
                case 5:
                    this._gardens.getNextCoordinates5(center, this._coordinates, index, pipeClient, currentIndex);
                    break;
            }
        }

        private void SendData()
        {
            PopulateData();
            pipeClient = CreatePipeClientStream();
            TryConnect(pipeClient);
            byte[] data = new byte[5];           
            pipeClient.Read(data, 0, data.Length);
            _coordinates.theRow = (int)data[0];
            _coordinates.theColumn = (int)data[1];
            this._center = (int)data[2];
            this._idGarden = (int)data[3];
            byte waitServer = data[4];
            pipeClient.WaitForPipeDrain();
            //TODO: whait 5 s when all clients is conneted
            Thread.Sleep(5000);
            pipeClient = CreatePipeClientStream();
            TryConnect(pipeClient);
            Console.WriteLine("Клиент номер: {0}, жду других подключений", this._idGarden);
            waitServer = (byte)pipeClient.ReadByte();
            pipeClient.WaitForPipeDrain();

            if ((State)waitServer == State.Start)
            {
                Console.WriteLine("Готов работать!");
                Console.WriteLine("Получены данные: смещение {0}", this._center);
                Thread.Sleep(5000);
                try
                {
                    SetGardenFunction(this._idGarden, this._center);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Thread.Sleep(1000);
                    Console.ReadLine();
                }
                Console.WriteLine("Садовник под номером: {0} отработал", _idGarden);
                pipeClient = CreatePipeClientStream();
                TryConnect(pipeClient);
                byte[] sendData = {(byte)State.EndWork};
                SendCommand(sendData);
                Console.ReadLine();
            }
        }
    }
}
