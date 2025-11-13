using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LineageLauncher.Core.Interfaces;
using LineageLauncher.Core.Entities;
using LineageLauncher.Network;
using LineageLauncher.Launcher;
using Microsoft.Extensions.Logging;

namespace LineageLauncher.App.ViewModels;

/// <summary>
/// ViewModel for the main launcher window.
/// Handles patch checking, game launching, and user session management.
/// </summary>
public partial class MainLauncherViewModel : ObservableObject
{
    // TEST MODE: Set to true to bypass connector and use hardcoded values
    private const bool TEST_MODE = false;

    private readonly IPatchService _patchService;
    private readonly ILauncherService _launcherService;
    private readonly ConnectorApiClient _connectorClient;
    private readonly IDllDeploymentService _dllDeploymentService;
    private readonly HardwareIdCollector? _hardwareIdCollector;
    private readonly ILogger<MainLauncherViewModel> _logger;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _patchStatus = "Initializing...";

    [ObservableProperty]
    private double _patchProgress;

    [ObservableProperty]
    private bool _isPatching;

    [ObservableProperty]
    private bool _isGameReady;

    [ObservableProperty]
    private string _detailedStatus = string.Empty;

    [ObservableProperty]
    private bool _isCheckingPatches;

    [ObservableProperty]
    private bool _isServerOnline;

    [ObservableProperty]
    private string _serverStatusMessage = "Checking server...";

    [ObservableProperty]
    private string _dllDeploymentStatus = string.Empty;

    [ObservableProperty]
    private double _dllDeploymentProgress;

    [ObservableProperty]
    private bool _isDllDeploymentComplete;

    public User? CurrentUser { get; private set; }
    public ServerInfo? ServerInfo { get; private set; }
    public DecryptedConnectorInfo? ConnectorInfo { get; private set; }
    public event EventHandler? LogoutRequested;

    public MainLauncherViewModel(
        IPatchService patchService,
        ILauncherService launcherService,
        ConnectorApiClient connectorClient,
        IDllDeploymentService dllDeploymentService,
        ILogger<MainLauncherViewModel> logger,
        HardwareIdCollector? hardwareIdCollector = null)
    {
        _patchService = patchService ?? throw new ArgumentNullException(nameof(patchService));
        _launcherService = launcherService ?? throw new ArgumentNullException(nameof(launcherService));
        _connectorClient = connectorClient ?? throw new ArgumentNullException(nameof(connectorClient));
        _dllDeploymentService = dllDeploymentService ?? throw new ArgumentNullException(nameof(dllDeploymentService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _hardwareIdCollector = hardwareIdCollector; // Optional on non-Windows platforms

        // DIAGNOSTIC: Verify RelayCommand generation
        System.Diagnostics.Debug.WriteLine("[DIAGNOSTIC] MainLauncherViewModel constructor complete");
