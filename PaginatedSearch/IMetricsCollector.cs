using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace PaginatedSearch
{
    // Production could integrate with Prometheus/StatsD, etc.
    public interface IMetricsCollector
    {
        void Started();
        void Finished();
    }

    public class LoggingMetricsCollector : IMetricsCollector
    {
        private readonly ILogger? _logger;
        private readonly Stopwatch _stopwatch = new();
        public LoggingMetricsCollector(ILogger<LoggingMetricsCollector>? logger = null) { _logger = logger; }

        public void Started()
        {
            _stopwatch.Restart();
            _logger?.LogTrace("Metric: page fetch started");
        }

        public void Finished()
        {
            _stopwatch.Stop();
            var duration = _stopwatch.Elapsed;
            _logger?.LogDebug("Metric: page fetch took {ms}ms", duration.TotalMilliseconds);
        }
    }
}