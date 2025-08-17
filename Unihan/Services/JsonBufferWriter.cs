namespace Bible.Backend.Services;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

public class JsonBufferWriter<T> : IDisposable, IAsyncDisposable
{
    private Task? _backgroundTask;
    private readonly string _nl = "\n"; // Environment.NewLine;
    private readonly JsonBufferWriterOptions _options;
    private readonly ConcurrentQueue<T> _buffer = new();
    private readonly CancellationTokenSource _cts = new();
    private readonly SemaphoreSlim _fileLock = new(1, 1);
    private bool _isFirstEntry = true;
    private FileStream? _fileStream;
    private StreamWriter? _writer;
    private bool _disposed = false;

    public JsonBufferWriter(IOptions<JsonBufferWriterOptions> options)
    {
        _options = options.Value;
    }

    private void CheckIsInitialized()
    {
        if (_backgroundTask == null)
        {
            // Initialize file and write opening bracket for JSON array
            InitializeFileAsync().GetAwaiter().GetResult();
        }
    }

    private async Task InitializeFileAsync()
    {
        if (string.IsNullOrEmpty(_options.OutputPath))
        {
            throw new ArgumentNullException(nameof(_options.OutputPath));
        }
        await _fileLock.WaitAsync();
        try
        {
            if (_backgroundTask != null) return;
            // Create or overwrite file, write '[' as start of JSON array
            _fileStream = new FileStream(_options.OutputPath, FileMode.Create, FileAccess.Write, FileShare.Read, 4096, true);
            _writer = new StreamWriter(_fileStream);
            await _writer.WriteAsync($"[{_nl}");
            await _writer.FlushAsync();
            // Start background flushing task
            _backgroundTask = Task.Run(FlushLoopAsync);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public void AddEntry(T entry)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(JsonBufferWriter<T>));
        _buffer.Enqueue(entry);
        CheckIsInitialized();
    }

    private async Task FlushLoopAsync()
    {
        try
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                await Task.Delay(_options.FlushInterval, _cts.Token);
                await FlushAsync();
            }
        }
        catch (TaskCanceledException)
        {
            // Expected on cancellation
        }
    }

    public async Task FlushAsync()
    {
        if (_writer == null) return;
        if (_buffer.IsEmpty) return;
        List<T> entriesToWrite = new();

        while (_buffer.TryDequeue(out var entry))
        {
            entriesToWrite.Add(entry);
        }

        if (entriesToWrite.Count == 0) return;

        await _fileLock.WaitAsync();
        try
        {
            foreach (var entry in entriesToWrite)
            {
                // Add comma before every entry except the first one
                if (!_isFirstEntry)
                {
                    await _writer.WriteAsync($",{_nl}");
                }
                else
                {
                    _isFirstEntry = false;
                }

                var jsonSerializerOptions = new JsonSerializerOptions {
                    // Serialize enums as strings
                    Converters = { new JsonStringEnumConverter() },
                    // Write unicode characters as they are, not escaped
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    // Optional output formatting
                    WriteIndented = false
                };
                string json = JsonSerializer.Serialize(entry, jsonSerializerOptions);
                //{ "Key":"\uD840\uDC00","Field":"kJapanese","Value":"カ"},
                await _writer.WriteAsync(json);
            }
            await _writer.FlushAsync();
        }
        finally
        {
            _fileLock.Release();
        }
    }

    private async Task CloseFileAsync()
    {
        if (_writer == null) return;

        await _fileLock.WaitAsync();
        try
        {
            // Write closing bracket for JSON array
            await _writer.WriteAsync($"{_nl}]");
            await _writer.FlushAsync();
            await _writer.DisposeAsync();
            await _fileStream!.DisposeAsync();
            _writer = null;
            _fileStream = null;
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public void Dispose()
    {
        DisposeAsync().GetAwaiter().GetResult();
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;

        _disposed = true;

        _cts.Cancel();

        if (_backgroundTask != null)
        {
            try
            {
                await _backgroundTask;
            }
            catch (TaskCanceledException)
            {
                // ignore
            }
        }

        await FlushAsync();
        await CloseFileAsync();

        _cts.Dispose();
        _fileLock.Dispose();
    }
}

public class JsonBufferWriterOptions : IOptions<JsonBufferWriterOptions>
{
    public string? OutputPath { get; set; }
    public TimeSpan FlushInterval { get; set; } = TimeSpan.FromSeconds(5);

    public static implicit operator JsonBufferWriterOptions(string value) =>
        new JsonBufferWriterOptions { OutputPath = value };

    public static implicit operator string?(JsonBufferWriterOptions text) =>
        text?.OutputPath;

    public JsonBufferWriterOptions Value
    {
        get
        {
            if (string.IsNullOrEmpty(OutputPath))
            {
                throw new ArgumentNullException(nameof(OutputPath));
            }
            return this;
        }
    }
}