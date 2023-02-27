using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkersModsPerformanceTester
{
    internal class CsvBuilder
    {
        private char _columnSeparator;
        private string[] _columnsNames = new string[]{ };
        private StringBuilder _stringBuilder = new StringBuilder();

        public CsvBuilder(char columnSeparator = ';')
        {
            _columnSeparator = columnSeparator;
        }

        public void SetUpColumns(params string[] columnsNames)
        {
            if(columnsNames == null)
            {
                throw new ArgumentNullException(nameof(columnsNames));
            }
            if(columnsNames.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(columnsNames));
            }
            if(_columnsNames.Length > 0 || _stringBuilder.Length > 0)
            {
                throw new InvalidOperationException();
            }
            BuildRow(columnsNames);
        }

        public void AddRow(params string[] records)
        {
            BuildRow(records);
        }

        public override string ToString()
        {
            return _stringBuilder.ToString();
        }

        private void BuildRow(string[] records)
        {
            for (int i = 0; i < records.Length; i++)
            {
                _stringBuilder.Append(records[i]);
                if (i != records.Length - 1)
                {
                    _stringBuilder.Append(_columnSeparator);
                }
            }
            _stringBuilder.Append('\n');
        }
    }
}
