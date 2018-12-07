using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestGenerator.AsyncReaderWriter;
using TestGenerator.AsyncReaderWriter.Reader;
using TestGenerator.AsyncReaderWriter.Writer;
using TestGenerator.PatternGenerator;

namespace TestGenerator
{
    public class GeneratorConfig
    {
        private IAsyncReader _asyncReader;
        private IAsyncWriter _asyncWriter;
        private IEnumerable<string> _paths;
        private IPatternGenerator _patternGenerator;
        private int _readerThreadCount;
        private int _processThreadCount;
        private int _writerThreadCount;

        public IAsyncReader AsyncReader
        {
            get => _asyncReader;
            set => _asyncReader = value ?? throw new ArgumentException("Reader can't be null!");
        }

        public IAsyncWriter AsyncWriter
        {
            get => _asyncWriter;
            set => _asyncWriter = value ?? throw new ArgumentException("Writer can't be null!");
        }

        public IEnumerable<string> Paths
        {
            get => new List<string>(_paths);
            set
            {
                if (value == null)
                    throw new ArgumentException("Paths can't be null!");
                _paths = new List<string>(value);
            }
        }

        public IPatternGenerator PatternGenerator
        {
            get => _patternGenerator;
            set => _patternGenerator = value ?? throw new ArgumentException("Template generator can't be null!");
        }

        public int ReaderThreadCount
        {
            get => _readerThreadCount;
            set
            {
                if (value < 1)
                    throw new ArgumentException("There can be at least 1 thread!");
                _readerThreadCount = value;
            }
        }

        public int ProcessThreadCount
        {
            get => _processThreadCount;
            set
            {
                if (value < 1)
                    throw new ArgumentException("There can be at least 1 thread!");
                _processThreadCount = value;
            }
        }

        public int WriterThreadCount
        {
            get => _writerThreadCount;
            set
            {
                if (value < 1)
                    throw new ArgumentException("There can be at least 1 thread!");
                _writerThreadCount = value;
            }
        }

        public GeneratorConfig()
        {
            _readerThreadCount = 1;
            _writerThreadCount = 1;
            _processThreadCount = Environment.ProcessorCount;
            _asyncReader = new AsyncFileDataReader();
            _asyncWriter = new AsyncFileDataWriter();
            _paths = new List<string>();
            _patternGenerator = new PatternGenerator.PatternGenerator(new CSCodeAnalyzer.CSCodeAnalyzer());
        }
    }
}
