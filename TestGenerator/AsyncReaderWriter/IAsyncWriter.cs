using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestGenerator.CSCodeDataStructures;

namespace TestGenerator.AsyncReaderWriter
{
    public interface IAsyncWriter
    {
        Task WriteDataAsync(PathInformation pathInformation);
    }
}
