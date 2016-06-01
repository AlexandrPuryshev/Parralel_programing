using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sadovinki
{
    class CoordinatesWithData
    {


        private int _row;
        private int _column;
        public int theRow
        {
            get{return this._row;}
            set{this._row = value;}
        }

        public int theColumn
        {
            get{return this._column;}
            set{this._column = value;}
        }

        public void SetEqual(int data)
        {
            this._row = data;
            this._column = data;
        }

    }
        
}
