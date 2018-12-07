using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestGenerator.CSCodeDataStructures
{
    public class PathInformation
    {
        public string Path { get; protected set; }
        public string Data { get; protected set; }

        public PathInformation(string path, string data)
        {
            if ((path == null) || (data == null))
                throw new ArgumentException("Arguments can't be null!");
            Path = path;
            Data = data;
        }
    }
}
