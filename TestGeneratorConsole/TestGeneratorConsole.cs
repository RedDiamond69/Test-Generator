using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestGenerator;
using TestGenerator.AsyncReaderWriter.Writer;

namespace TestGeneratorConsole
{
    public class TestGeneratorConsole
    {
        static void Main(string[] args)
        {
            GeneratorConfig config = new GeneratorConfig
            {
                Paths = new List<string>
                {
                    "..\\..\\TestSourceFiles\\SimpleClass.cs",
                    "..\\..\\TestSourceFiles\\NotSimpleClass.cs"
                },
                AsyncWriter = new AsyncFileDataWriter()
                {
                    Directory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                },
                ReaderThreadCount = 2,
                WriterThreadCount = 2
            };
            new TestGenerator.TestGenerator(config).Generate().Wait();
            Console.WriteLine("Generation completed!");
            Console.ReadKey();
        }
    }
}
