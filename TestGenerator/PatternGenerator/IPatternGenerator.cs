using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestGenerator.CSCodeDataStructures;

namespace TestGenerator.PatternGenerator
{
    public interface IPatternGenerator
    {
        IEnumerable<PathInformation> GenerateCode(string source);
    }
}
