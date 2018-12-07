using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestGenerator.AsyncReaderWriter.Reader
{
    public class AsyncFileDataReader : IAsyncReader
    {
        public Task<string> ReadDataAsync(string path)
        {
            throw new NotImplementedException();
        }

        public AsyncFileDataReader()
        {
        }
    }
}
