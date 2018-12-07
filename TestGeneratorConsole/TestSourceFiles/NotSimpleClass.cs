using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestGeneratorConsole.TestSourceFiles1
{
    public class NotSimpleClass1
    {
        public int GetInt() => 0;

        public void DoSomething(int param1, double param2) { }

        public NotSimpleClass1(int param) { }

        public NotSimpleClass1(int p1, double p2) { }

        public NotSimpleClass1() { }
    }
}

namespace TestGeneratorConsole.TestSourceFiles2
{
    public interface ITestClassInterface { }

    public class NotSimpleClass2
    {
        public ITestClassInterface GetInterface() => null;

        public void SetInterface(ITestClassInterface classInterface) { }

        protected ITestClassInterface GetProtectedInterface() => null;

        public NotSimpleClass2(ITestClassInterface myTestInterface) { }
    }
}
