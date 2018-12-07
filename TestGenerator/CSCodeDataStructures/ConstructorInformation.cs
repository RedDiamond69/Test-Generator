using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestGenerator.CSCodeDataStructures
{
    public class ConstructorInformation
    {
        public List<ParameterInformation> ParametersDeclaration { get; protected set; }

        public ConstructorInformation() => ParametersDeclaration = new List<ParameterInformation>();
    }
}
