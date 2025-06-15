using System.Xml.Serialization;
using Bible.Backend.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bible.Backend
{
    public class ParsingStrategist : IStrategist<string, ParsingStrategy>
    {
        private readonly ILogger<ParsingStrategist> _logger;

        public ParsingStrategist(ILoggerFactory? loggerFactory = null, ILogger<ParsingStrategist>? logger = null)
        {
            _logger = logger ?? loggerFactory?.CreateLogger<ParsingStrategist>() ?? NullLogger<ParsingStrategist>.Instance;
        }

        public ParsingStrategy? ChooseStrategy(string filePath)
        {

            // parse the files looking for 1CO, 
            switch (filePath)
            {
                case "A":
                    return new UsxParser();
                default:
                    return null;
            }
        }
    }
}