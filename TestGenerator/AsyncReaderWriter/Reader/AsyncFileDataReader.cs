using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestGenerator.AsyncReaderWriter.Reader
{
    public class AsyncFileDataReader : IAsyncReader
    {
        public async Task<string> ReadDataAsync(string path)
        {
            if (path == null)
                throw new ArgumentException("Path can't be null!");
            using (StreamReader streamReader = new StreamReader(path))
                return await streamReader.ReadToEndAsync();
        }

        public AsyncFileDataReader()
        {
        }
    }
}
