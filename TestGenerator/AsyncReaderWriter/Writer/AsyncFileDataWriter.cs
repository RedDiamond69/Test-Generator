using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestGenerator.CSCodeDataStructures;

namespace TestGenerator.AsyncReaderWriter.Writer
{
    public class AsyncFileDataWriter : IAsyncWriter
    {
        private string _directory;

        public string Directory
        {
            get => _directory;
            set
            {
                if (value == null)
                    throw new ArgumentException("Directory can't be null!");
                _directory = Path.GetFullPath(value);
            }
        }

        public async Task WriteDataAsync(PathInformation pathInformation)
        {
            if (pathInformation == null)
                throw new ArgumentException("Path information can't be null!");
            if (!System.IO.Directory.Exists(_directory))
                System.IO.Directory.CreateDirectory(_directory);
            using (StreamWriter writer = new StreamWriter(Directory + 
                Path.DirectorySeparatorChar + pathInformation.Path))
                await writer.WriteAsync(pathInformation.Data);
        }

        public AsyncFileDataWriter() => Directory = System.IO.Directory.GetCurrentDirectory();
    }
}
