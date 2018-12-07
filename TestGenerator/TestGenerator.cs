using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using TestGenerator.CSCodeDataStructures;

namespace TestGenerator
{
    public class TestGenerator : ITestGenerator
    {
        private readonly GeneratorConfig _configGenerator;

        public TestGenerator(GeneratorConfig config) => _configGenerator = 
            config ?? throw new ArgumentException("Config can't be null!");

        public async Task Generate()
        {
            DataflowLinkOptions linkOptions = new DataflowLinkOptions
            {
                PropagateCompletion = true
            };
            ExecutionDataflowBlockOptions processOptions = new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = _configGenerator.ProcessThreadCount
            };
            ExecutionDataflowBlockOptions readOptions = new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = _configGenerator.ReaderThreadCount
            };
            ExecutionDataflowBlockOptions writeOptions = new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = _configGenerator.WriterThreadCount
            };
            TransformBlock<string, Task<string>> transformBlock = 
                new TransformBlock<string, Task<string>>(
                    (readPath) => _configGenerator.AsyncReader.ReadDataAsync(readPath), readOptions);
            TransformManyBlock<Task<string>, PathInformation> sourceCodeToTestTransform = 
                new TransformManyBlock<Task<string>, PathInformation>(
                    (readSourceTask) => 
                    _configGenerator.PatternGenerator.GenerateCode(readSourceTask.Result), processOptions);
            ActionBlock<PathInformation> write = new ActionBlock<PathInformation>(
                (path) => _configGenerator.AsyncWriter.WriteDataAsync(path).Wait(), writeOptions);
            transformBlock.LinkTo(sourceCodeToTestTransform, linkOptions);
            sourceCodeToTestTransform.LinkTo(write, linkOptions);
            foreach (string path in _configGenerator.Paths)
                await transformBlock.SendAsync(path);
            transformBlock.Complete();
            await write.Completion;
        }
    }
}
