﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestGenerator.AsyncReaderWriter
{
    public interface IAsyncReader
    {
        Task<string> ReadDataAsync(string path);
    }
}
