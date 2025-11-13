using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace LineageLauncher.Launcher.IPC;

/// <summary>
/// Manages named pipes for bidirectional communication with the game client.
/// </summary>
public sealed class PipeManager : IDisposable
{
    private readonly ILogger<PipeManager> _logger;
    private readonly string _pipeNameOut;
    private readonly string _pipeNameIn;

    private NamedPipeServerStream? _pipeOut;
    private NamedPipeServerStream? _pipeIn;
    private bool _disposed;

    public bool IsConnected =>
        _pipeOut?.IsConnected == true && _pipeIn?.IsConnected == true;

    public PipeManager(
        ILogger<PipeManager> logger,
        string? pipeBaseName = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var baseName = pipeBaseName ?? "LineageLauncher";
        _pipeNameOut = $"{baseName}_Pipe1";
        _pipeNameIn = $"{baseName}_Pipe2";
    }

    /// <summary>
    /// Creates the named pipes for IPC.
    /// </summary>
    public Task CreatePipesAsync(CancellationToken cancellationToken = default)
    {
        if (_pipeOut != null || _pipeIn != null)
        {
            throw new InvalidOperationException("Pipes already created");
        }

        try
        {
            _logger.LogInformation("Creating named pipes...");
            _logger.LogDebug("Pipe Out (Launcher → Game): {PipeName}", _pipeNameOut);
            _logger.LogDebug("Pipe In (Game → Launcher): {PipeName}", _pipeNameIn);

            // Pipe 1: Launcher → Game (Out)
            _pipeOut = new NamedPipeServerStream(
                _pipeNameOut,
                PipeDirection.Out,
                maxNumberOfServerInstances: 1,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous);

            // Pipe 2: Game → Launcher (In)
            _pipeIn = new NamedPipeServerStream(
                _pipeNameIn,
                PipeDirection.In,
                maxNumberOfServerInstances: 1,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous);

            _logger.LogInformation("Named pipes created successfully");
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create named pipes");
            Dispose();
            throw;
        }
    }

    /// <summary>
    /// Waits for the game client to connect to both pipes.
    /// </summary>
    public async Task WaitForConnectionAsync(
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        if (_pipeOut == null || _pipeIn == null)
        {
            throw new InvalidOperationException("Pipes not created");
        }

        _logger.LogInformation(
            "Waiting for game client connection (timeout: {Timeout})...",
            timeout);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(timeout);

        try
        {
            // Wait for both pipes to connect
            await Task.WhenAll(
                _pipeOut.WaitForConnectionAsync(cts.Token),
                _pipeIn.WaitForConnectionAsync(cts.Token));

            _logger.LogInformation("Game client connected to both pipes");
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(
                "Pipe connection timeout after {Timeout}",
                timeout);
            throw new TimeoutException(
                $"Game client did not connect within {timeout}");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Pipe connection cancelled");
            throw;
        }
    }

    /// <summary>
    /// Reads a message from the game client.
    /// </summary>
    public async Task<byte[]?> ReadMessageAsync(
        CancellationToken cancellationToken = default)
    {
        if (_pipeIn == null)
        {
            throw new InvalidOperationException("Pipe not created");
        }

        if (!_pipeIn.IsConnected)
        {
            _logger.LogWarning("Pipe not connected");
            return null;
        }

        try
        {
            var buffer = new byte[4096];
            var bytesRead = await _pipeIn.ReadAsync(
                buffer,
                0,
                buffer.Length,
                cancellationToken);

            if (bytesRead == 0)
            {
                _logger.LogDebug("Client disconnected (0 bytes read)");
                return null;
            }

            var message = new byte[bytesRead];
            Array.Copy(buffer, message, bytesRead);

            _logger.LogDebug("Received {BytesRead} bytes", bytesRead);
            return message;
        }
        catch (IOException ex)
        {
            _logger.LogWarning(ex, "Pipe read error");
            return null;
        }
    }

    /// <summary>
    /// Writes a message to the game client.
    /// </summary>
    public async Task WriteMessageAsync(
        byte[] data,
        CancellationToken cancellationToken = default)
    {
        if (_pipeOut == null)
        {
            throw new InvalidOperationException("Pipe not created");
        }

        if (!_pipeOut.IsConnected)
        {
            throw new InvalidOperationException("Pipe not connected");
        }

        try
        {
            await _pipeOut.WriteAsync(data, 0, data.Length, cancellationToken);
            await _pipeOut.FlushAsync(cancellationToken);

            _logger.LogDebug("Sent {BytesSent} bytes", data.Length);
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "Pipe write error");
            throw;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        _pipeOut?.Dispose();
        _pipeIn?.Dispose();

        _pipeOut = null;
        _pipeIn = null;

        _disposed = true;
        _logger.LogDebug("PipeManager disposed");
    }
}
