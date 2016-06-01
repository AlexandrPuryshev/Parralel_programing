using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Pipes;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace Sadovinki
{
    class Gardens
    {

        private int _countOfGardeners;
        private int[] _centers;

        private List<int> _gardenes;
        private int[,] _field;

        private const double countGardensOnOneField = 5.0;


        public int theCountOfGardeners
        {
            set {_countOfGardeners = value;}
            get {return _countOfGardeners;}
        }

        public List<int> getGardenes
        {
            get { return _gardenes; }
        }

        
        public Gardens(int countOfGardens)
        {
            _gardenes = new List<int>();
            this._centers = new int[Convert.ToInt16(Math.Ceiling(countOfGardens / countGardensOnOneField))];
        }

        #region client side

        private void sendDataRowColumn(int index, NamedPipeClientStream pipeClient, int row, int column, bool swap, byte[] data, int currentIndex)
        {
            pipeClient = ClientSide.CreatePipeClientStream();
            ClientSide.TryConnect(pipeClient);
            if(swap){
                 data[0] = (byte)column;
                 data[1] = (byte)row;
            }
            else {
                data[0] = (byte)row;
                data[1] = (byte)column;
            }
            data[2] = (byte)index;

            Console.WriteLine("Перехожу на участок row {0}, column {1}, номер {2}, функция садовника {3}", row, column, index, currentIndex);
            pipeClient.Write(data, 0, data.Length);
            pipeClient.WaitForPipeDrain();
            var random = new Random();
            Thread.Sleep(random.Next(1000, 3000));
        }




        public void getNextCoordinates1(int center, CoordinatesWithData coordinates, int index, NamedPipeClientStream pipeClient, int currentIndex)
        {
            byte[] data = new byte[3];
            for (int i = center; i < coordinates.theRow; i++)
            {
                for (int j = center; j < coordinates.theColumn; j++)
                {
                    sendDataRowColumn(index, pipeClient, i, j, false, data, currentIndex);
                }
            }
        }

        public void getNextCoordinates2(int center, CoordinatesWithData coordinates, int index, NamedPipeClientStream pipeClient,int currentIndex)
        {
            byte[] data = new byte[3];
            for (int i = coordinates.theRow - 1; i > center; i--)
            {
                for (int j = coordinates.theColumn - 1; j > center; j--)
                {
                    sendDataRowColumn(index, pipeClient, i, j, true, data, currentIndex);

                }
            }
        }

        public void getNextCoordinates3(int center, CoordinatesWithData coordinates, int index, NamedPipeClientStream pipeClient, int currentIndex)
        {
            byte[] data = new byte[3];
            for (int i = center; i < coordinates.theRow; i++)
            {
                for (int j = coordinates.theColumn - 1; j > center; j--)
                {
                    sendDataRowColumn(index, pipeClient, i, j, false, data, currentIndex);
                }
            }
        }

        public void getNextCoordinates4(int center, CoordinatesWithData coordinates, int index, NamedPipeClientStream pipeClient, int currentIndex)
        {
            byte[] data = new byte[3];
            for (int i = coordinates.theRow - 1; i > center; i--)
            {
                for (int j = coordinates.theColumn - 1; j > center; j--)
                {
                    sendDataRowColumn(index, pipeClient, i, j, false, data, currentIndex);
                }
            }
        }

        public void getNextCoordinates5(int center, CoordinatesWithData coordinates, int index, NamedPipeClientStream pipeClient, int currentIndex)
        {
            byte[] data = new byte[3];
            for (int i = 1; i < coordinates.theRow; i++)
            {
                for (int j = center; j < coordinates.theColumn; j++)
                {
                    sendDataRowColumn(index, pipeClient, i, j, true, data, currentIndex);
                }
            }
        }

        #endregion




        #region ServerSide

        public void SetIndexField(int row, int column, int index)
        {
            if(this._field[row, column] == 0) this._field[row, column] = index;
        }

        public int[,] theField
        {
            get { return _field; }
            set { _field = value; }
        }

        public int GetCenter(int indexAferFiveGardens)
        {
            if (indexAferFiveGardens > 5)
            {
                double temp = (indexAferFiveGardens) / 5;
                int center = Convert.ToInt16(Math.Ceiling(temp));
                return center;
            }
            return 0;

        }

        //устанавливаем число центров для садовинков
        public void SetGardensCenter(int countOfGardens)
        {
            this._centers[0] = 0;
            for(int count = 1; count < countOfGardens; count = count + 5)
            {
               this._gardenes.Add(countOfGardens);
               this._centers[_countOfGardeners] = _countOfGardeners++;
            }
        }

        public void SetNewField(int row)
        {
            _field = new int[row, row];
        }

        #endregion



    }
}
