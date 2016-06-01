using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Pipes;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.IO;

namespace Sadovinki
{

    class ServerSide
    { 
        private Gardens _gardens;
        private List<CoordinatesWithData> _coordinates;
        private NamedPipeServerStream pipeServer;

        private byte[] data = {0, 0, 0, 0, 0};


        private int _index = 0;
        private int _row;
        private int _theNumberOfGardens;
        enum State
        {
            Start,
            WaitDifferentClients,
            EndWork,
        }


        public ServerSide(int row, int countOfGardens)
        {
            Console.WriteLine("Создан сервер Pipe: {0}.", "pipe_sadovniky_server");
            if (row > 255 && countOfGardens > 255)
            {
                Console.WriteLine("Диапазон вводимых чисел не должен превышать 255!");
                Console.ReadLine();
                return;
            }
            this._row = row;
            this._theNumberOfGardens = countOfGardens;
            PopulateData();
            _gardens.SetGardensCenter(this._theNumberOfGardens);
            _gardens.SetNewField(this._row);

        }

        private void ShowTable(int row, int column)
        {
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < column; j++)
                {
                    Console.Write("{0, 5}", _gardens.theField[i, j]);
                }
                Console.WriteLine();
            }
        }

        private void PopulateData()
        {
            this._gardens = new Gardens(this._theNumberOfGardens);
            this._coordinates = new List<CoordinatesWithData>();
        }

        private static NamedPipeServerStream CreatePipeServer()
        {
            return new NamedPipeServerStream(
                "pipe_sadovniky",
                PipeDirection.InOut,
                NamedPipeServerStream.MaxAllowedServerInstances,
                PipeTransmissionMode.Message,
                PipeOptions.Asynchronous
            );
        }

        private void SendCoordinatesToClient(int index)
        {
            this._coordinates.Add(new CoordinatesWithData());
            this._coordinates[_index].SetEqual(this._row);
            data[0] = (byte)this._coordinates[_index].theRow;
            data[1] = (byte)this._coordinates[_index].theColumn;
            //TODO: send center where garden is start
            for (int count = index; count <= _theNumberOfGardens; count = count + 4)
            {
                data[2] = (byte)this._gardens.GetCenter(index + 1);
            }
            data[3] = (byte)(this._index + 1);
            data[4] = (byte)State.WaitDifferentClients;
            pipeServer.Write(data, 0, data.Length);
            pipeServer.WaitForPipeDrain();
        }

        private void WorkServer()
        {

            for(int count = 0; count < this._coordinates.Count; count++)
            {
                if (this._theNumberOfGardens > _coordinates[count].theRow)
                {
                    Console.WriteLine("Число рабочих больше длины поля!");
                    Console.ReadLine();
                    return;
                }
            }

            Console.Clear();
            this._index = 0;
            bool flag = true;
            while (flag)
            {
                Console.SetCursorPosition(0, 0);
                pipeServer = CreatePipeServer();
                pipeServer.WaitForConnection();
                pipeServer.Read(data, 0 , data.Length);
                int row = data[0];
                if (data.Length == 1)
                {
                    if (data[0] == (byte)State.EndWork)
                    {
                        if (this._index != this._theNumberOfGardens)
                        {
                            this._index++;
                        }
                        else
                        {
                            flag = false;
                        }
                    }
                }
                int column = data[1];
                int index = data[2];
                _gardens.SetIndexField(row, column, index);
                pipeServer.WaitForPipeDrain();
                ShowTable(this._row, this._row);
            }


            Console.WriteLine("Садовников отработало: {0}", this._theNumberOfGardens);

            ShowTable(this._row, this._row);

            Console.ReadLine();
        }

        public void Start()
        {
            while (_index != _theNumberOfGardens)
            {
                pipeServer = CreatePipeServer();
                pipeServer.WaitForConnection();
                Console.WriteLine("Подсоединен клиент с номером: {0}", _index + 1);
                SendCoordinatesToClient(_index);
                _index++;

                if (_index != _theNumberOfGardens)
                {
                    Console.WriteLine("Указанное количество подключенных клиентов неподходит: {0}, должно быть:  {1}", _index, _theNumberOfGardens);
                }
            }

            Console.WriteLine("Клиенты успешно подсоединены!");

            _index = 0;
            while (_index != _theNumberOfGardens)
            {
                pipeServer = CreatePipeServer();
                pipeServer.WaitForConnection();
                Console.WriteLine("Стартует клиент {0}", _index + 1);
                pipeServer.WriteByte((byte)State.Start);
                pipeServer.WaitForPipeDrain();
                _index++;
            }

            try
            {
                WorkServer();
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadLine();
            }
        }
    }
}
