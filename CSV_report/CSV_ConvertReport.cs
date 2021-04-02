using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace SCVtiSQL.CSV_DB
{
    /// <summary>
    /// Объект класса читает данные файла CSV, формируемого интернет банкингом Белапб
    /// из потока StreamReader и заполняет ими таблицу.  
    /// </summary>
    class CSV_ConvertReport
    {
        private readonly DataTable dtIdName;
        private readonly DataTable dtData;
        
        private StreamReader streamReader;

        CSV_ConvertReport(StreamReader sr)
        {
            streamReader = sr;

            dtData = new DataTable();

            SetDataColumnsName(dtData);
        }

        private void SetDataColumnsName(DataTable dt)
        {
            //DataColumn d
        }
    }
}
