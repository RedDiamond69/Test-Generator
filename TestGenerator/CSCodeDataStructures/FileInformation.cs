using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestGenerator.CSCodeDataStructures
{
    public class FileInformation
    {
        public List<string> UsingsDeclaration { get; protected set; }
        public List<ClassInformation> ClassesDeclaration { get; protected set; }

        public FileInformation()
        {
            UsingsDeclaration = new List<string>();
            ClassesDeclaration = new List<ClassInformation>();
        }
    }
}
