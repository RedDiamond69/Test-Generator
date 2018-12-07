using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestGenerator.CSCodeDataStructures
{
    public class ParameterInformation
    {
        public string Name { get; protected set; }
        public TypeInformation Type { get; protected set; }

        public ParameterInformation(string name, TypeInformation typeInformation)
        {
            Name = name ?? throw new ArgumentException("Parameter name can't be null!");
            Type = typeInformation ?? throw new ArgumentException("Parameter type can't be null!");
        }
    }
}
