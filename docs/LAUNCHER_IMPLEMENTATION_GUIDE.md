# L1R Custom Launcher - Comprehensive Implementation Guide

**Version:** 1.0
**Date:** November 11, 2025
**Purpose:** Complete implementation roadmap for L1R-CustomLauncher based on Korean/Chinese research and L1JR-Server analysis

---

## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [Research Synthesis](#2-research-synthesis)
3. [Lin.bin Launch Methods](#3-linbin-launch-methods)
4. [L1JR-Server Integration](#4-l1jr-server-integration)
5. [Architecture Recommendations](#5-architecture-recommendations)
6. [Technical Requirements](#6-technical-requirements)
7. [Implementation Roadmap](#7-implementation-roadmap)
8. [Security Considerations](#8-security-considerations)
9. [Testing Strategy](#9-testing-strategy)
10. [Deployment Guide](#10-deployment-guide)

---

## 1. Executive Summary

### 1.1 Project Goals

This guide synthesizes research from Korean/Chinese Lineage launcher implementations with the specific requirements of the L1JR-Server to create a modern, secure, and efficient custom launcher for Lineage Remastered.

### 1.2 Key Findings

**Korean/Chinese Launcher Approaches:**
- **Memory Injection**: Direct process memory manipulation to bypass Lin.bin checks
- **Config File Method**: Login.ini files with encrypted credentials (Base64 + XOR)
- **Environment Variables**: Passing server info via process environment
- **Hybrid Approach**: Combination of config files + process spawning

**L1JR-Server Requirements:**
- **Two-Stage Authentication**: HTTP Web Server → TCP Game Server
- **Connector API**: REST endpoints for configuration and authentication
- **Hardware ID Collection**: MAC, HDD, Board, NIC for anti-cheat
- **Encrypted Communication**: Base64 + AES/XOR encryption throughout
- **Process Monitoring**: Anti-cheat via blacklist detection

### 1.3 Recommended Approach

**Hybrid Method:**
1. HTTP authentication via Connector API (receives auth token)
2. Create Login.ini with encrypted credentials
3. Launch Lin.bin with environment variables pointing to config
4. Monitor process and maintain session
5. Periodic hardware/process reporting to server

**Why This Approach:**
- ✅ Works with existing L1JR-Server infrastructure
- ✅ No client modification required
- ✅ Compatible with Korean/Chinese launcher patterns
- ✅ Maintains security via server-side validation
- ✅ Supports auto-update and patching

---

## 2. Research Synthesis

### 2.1 Korean Launcher Patterns

**Common Features Found in Korean Launchers:**

1. **ServerData Encryption**
   - Base64 encoding + XOR cipher
   - Key stored in launcher binary
   - Decoded at runtime before Lin.bin launch

2. **Login.ini Format**
   ```ini
   [User]
   name=encrypted_base64_username
   password=encrypted_base64_password

   [Options]
   RememberUser=1
   PatchVersion=2
   Language=1
   ```

3. **Lin.bin Launch Parameters**
   - No direct command-line parameters (by design)
   - Reads from Login.ini in same directory
   - Server IP/Port via memory injection or pre-configured

4. **Anti-Cheat Integration**
   - Process list scanning
   - Memory protection checks
   - Hardware ID validation
   - DLL injection detection

**Example from LWLauncher_local.cfg:**
```ini
[User]
name=uSIWeYp+ghQqqhTKVkhRBkbrl0BwG6OeqYGQW08JEUg=
password=tUuq7v3nkIxLEvRDT9E5hA==
```
*Note: These are encrypted credentials using server's encryption key*

### 2.2 Chinese Launcher Patterns

**Common Features from Chinese Forums (Lineage45.com, CSDN):**

1. **Patch Management**
   - Parallel download system
   - MD5 checksum verification
   - Incremental patching (only changed files)
   - Patch manifest JSON format

2. **Embedded Browser**
   - CEF (Chromium Embedded Framework) most common
   - Used for news, shop, account management
   - Size: 40-60MB typical

3. **Auto-Update System**
   - Version check on startup
   - Self-update capability
   - Rollback mechanism for failed updates

4. **UI Patterns**
   - Windows Forms (older launchers)
   - WPF (modern launchers)
   - Full-screen background with game art
   - Progress bars for patching
   - Remember username checkbox

### 2.3 L1JR-Server Specific Requirements

**Connector System Analysis (from SERVER_CONNECTION_ANALYSIS.md):**

1. **HTTP Web Server (Port 80)**
   - Endpoint: `/api/connector/info`
   - Returns: Encrypted JSON with server config
   - Encryption: Base64 + CONNECTOR_ENCRYPT_KEY

2. **Authentication Flow**
   ```
   Launcher → POST /outgame/login → Receives AuthToken
   Launcher → Launch Lin.bin
   Lin.bin → Connect TCP 127.0.0.1:2000
   Lin.bin → Send C_EXTENDED_PROTOBUF with AuthToken
   Server → Validates token → Returns character list
   ```

3. **Required Data Collection**
   ```csharp
   public class LoginRequest
   {
       public string account { get; set; }
       public string password { get; set; }
       public string mac_address { get; set; }  // Required
       public string hdd_id { get; set; }       // Required
       public string board_id { get; set; }     // Required
       public string nic_id { get; set; }       // Required
       public string process { get; set; }      // Running processes
   }
   ```

4. **Connector Configuration (connector.properties)**
   ```properties
   CONNECTOR_ENCRYPT_KEY = mOIjQ7ffyEV6w1SodWVqfwoU7qJCxzIhsqw6IM30okU=
   CONNECTOR_SESSION_KEY = linoffice1234
   CONNECTOR_CLIENT_SIDE_KEY = 711666385
   CONNECTOR_DLL_PASSWORD = 2052201973
   CONNECTOR_LINBIN_PATH = Linbin2303281701.bin
   CONNECTOR_LINBIN_SIZE = 17020256
   ```

---

## 3. Lin.bin Launch Methods

### 3.1 Method Comparison

| Method | Complexity | Reliability | Client Modification | Detection Risk |
|--------|------------|-------------|---------------------|----------------|
| Config File (Login.ini) | Low | High | None | Low |
| Environment Variables | Medium | Medium | None | Low |
| Memory Injection | High | Medium | None | Medium |
| DLL Injection | Very High | Low | Yes | High |

### 3.2 Recommended: Config File Method

**How It Works:**

1. Launcher creates `Login.ini` in Lin.bin directory:
   ```ini
   [User]
   name=<encrypted_username>
   password=<encrypted_password>

   [Server]
   ip=127.0.0.1
   port=2000

   [Options]
   RememberUser=1
   PatchVersion=5
   Language=1
   ```

2. Encryption for credentials matches L1JR-Server:
   ```csharp
   // Pseudo-code for encryption
   string EncryptCredential(string value, string key)
   {
       byte[] valueBytes = Encoding.UTF8.GetBytes(value);
       byte[] keyBytes = Convert.FromBase64String(key);
       byte[] encrypted = XorEncrypt(valueBytes, keyBytes);
       return Convert.ToBase64String(encrypted);
   }
   ```

3. Launch Lin.bin:
   ```csharp
   ProcessStartInfo startInfo = new ProcessStartInfo
   {
       FileName = "Linbin2303281701.bin",
       WorkingDirectory = clientDirectory,
       UseShellExecute = false,
       RedirectStandardOutput = false,
       RedirectStandardError = false
   };

   Process linProcess = Process.Start(startInfo);
   ```

4. Lin.bin reads Login.ini automatically on startup
5. Launcher monitors process and maintains session

**Advantages:**
- ✅ No client modification required
- ✅ Compatible with existing Lin.bin behavior
- ✅ Easy to debug and test
- ✅ Low detection risk
- ✅ Matches Korean launcher patterns

**Disadvantages:**
- ⚠️ Config file visible on disk (mitigated by encryption)
- ⚠️ Requires file I/O permissions

### 3.3 Alternative: Environment Variables Method

**Implementation:**
```csharp
ProcessStartInfo startInfo = new ProcessStartInfo
{
    FileName = "Linbin2303281701.bin",
    WorkingDirectory = clientDirectory,
    UseShellExecute = false
};

// Set environment variables
startInfo.EnvironmentVariables["L1_SERVER_IP"] = "127.0.0.1";
startInfo.EnvironmentVariables["L1_SERVER_PORT"] = "2000";
startInfo.EnvironmentVariables["L1_AUTH_TOKEN"] = authToken;
startInfo.EnvironmentVariables["L1_USERNAME"] = username;

Process.Start(startInfo);
```

**Advantages:**
- ✅ No file I/O required
- ✅ Hidden from casual inspection
- ✅ Process-specific (not system-wide)

**Disadvantages:**
- ⚠️ Lin.bin may not read environment variables
- ⚠️ Requires Lin.bin code analysis to confirm
- ⚠️ Less common pattern in Korean launchers

### 3.4 Advanced: Memory Injection (Not Recommended)

**Overview:**
Direct memory modification of Lin.bin process after launch.

**Why Not Recommended:**
- ❌ High complexity
- ❌ Anti-cheat detection
- ❌ Requires reverse engineering Lin.bin
- ❌ Fragile across client versions
- ❌ Legal/ethical concerns

**Only Consider If:**
- Config file method fails
- Lin.bin has anti-debugging
- Server requires specific memory patterns

---

## 4. L1JR-Server Integration

### 4.1 Authentication Flow (Detailed)

```
┌──────────┐         ┌────────────────┐         ┌─────────────┐
│ Launcher │         │ Web Server     │         │ Game Server │
│          │         │ (Port 80)      │         │ (Port 2000) │
└────┬─────┘         └────────┬───────┘         └──────┬──────┘
     │                        │                        │
     │ 1. GET /api/connector/info                      │
     │─────────────────────────>                       │
     │                        │                        │
     │ 2. Encrypted Config    │                        │
     │<─────────────────────────                       │
     │ {serverIp, port, opcodes, etc}                  │
     │                        │                        │
     │ 3. Decrypt Config      │                        │
     │ using CONNECTOR_ENCRYPT_KEY                     │
     │                        │                        │
     │ 4. POST /outgame/login │                        │
     │ {account, password, hwid, mac, etc}             │
     │─────────────────────────>                       │
     │                        │                        │
     │                        │ 5. Validate Account    │
     │                        │ Check IP/HDD/MAC       │
     │                        │                        │
     │ 6. AuthToken           │                        │
     │<─────────────────────────                       │
     │ {auth_token, session_key}                       │
     │                        │                        │
     │ 7. Create Login.ini    │                        │
     │ with encrypted creds   │                        │
     │                        │                        │
     │ 8. Launch Lin.bin      │                        │
     │────────┐               │                        │
     │        │               │                        │
     │<───────┘               │                        │
     │                        │                        │
     │                        │    9. TCP Connect      │
     │                        │      127.0.0.1:2000    │
     │                        │<───────────────────────┤
     │                        │                        │
     │                        │    10. FIRST_KEY       │
     │                        │      (9 bytes)         │
     │                        │───────────────────────>│
     │                        │                        │
     │                        │    11. C_EXTENDED_PROTOBUF │
     │                        │      A_NpLogin packet  │
     │                        │      with AuthToken    │
     │                        │<───────────────────────┤
     │                        │                        │
     │                        │<───12. Validate Token───│
     │                        │                        │
     │                        │────13. Account Exists──>│
     │                        │                        │
     │                        │    14. S_LoginResult   │
     │                        │      (success)         │
     │                        │───────────────────────>│
     │                        │                        │
     │                        │    15. Character List  │
     │                        │───────────────────────>│
     │                        │                        │
```

### 4.2 HTTP Client Implementation

**Required NuGet Packages:**
```xml
<PackageReference Include="System.Net.Http" Version="8.0.0" />
<PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
```

**Connector Info Request:**
```csharp
public class ConnectorClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _encryptKey;

    public ConnectorClient(string baseUrl, string encryptKey)
    {
        _baseUrl = baseUrl;
        _encryptKey = encryptKey;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    public async Task<ConnectorInfo> GetConnectorInfoAsync()
    {
        var response = await _httpClient.GetAsync("/api/connector/info");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var encryptedInfo = JsonConvert.DeserializeObject<EncryptedConnectorInfo>(json);

        // Decrypt all fields
        return new ConnectorInfo
        {
            ServerIp = DecryptBase64(encryptedInfo.serverIp, _encryptKey),
            ServerPort = int.Parse(DecryptBase64(encryptedInfo.serverPort, _encryptKey)),
            BrowserUrl = DecryptBase64(encryptedInfo.browserUrl, _encryptKey),
            LinBinPath = DecryptBase64(encryptedInfo.linbin, _encryptKey),
            LinBinSize = int.Parse(DecryptBase64(encryptedInfo.linbinSize, _encryptKey)),
            ClientSideKey = int.Parse(DecryptBase64(encryptedInfo.clientSideKey, _encryptKey)),
            DllPassword = int.Parse(DecryptBase64(encryptedInfo.dllPassword, _encryptKey)),
            // ... decrypt all other fields
        };
    }

    private string DecryptBase64(string encryptedBase64, string key)
    {
        byte[] encryptedBytes = Convert.FromBase64String(encryptedBase64);
        byte[] keyBytes = Convert.FromBase64String(key);
        byte[] decrypted = XorDecrypt(encryptedBytes, keyBytes);
        return Encoding.UTF8.GetString(decrypted);
    }

    private byte[] XorDecrypt(byte[] data, byte[] key)
    {
        byte[] result = new byte[data.Length];
        for (int i = 0; i < data.Length; i++)
        {
            result[i] = (byte)(data[i] ^ key[i % key.Length]);
        }
        return result;
    }
}
```

**Login Request:**
```csharp
public async Task<LoginResponse> LoginAsync(LoginRequest request)
{
    var json = JsonConvert.SerializeObject(request);
    var content = new StringContent(json, Encoding.UTF8, "application/json");

    var response = await _httpClient.PostAsync("/outgame/login", content);
    response.EnsureSuccessStatusCode();

    var responseJson = await response.Content.ReadAsStringAsync();
    return JsonConvert.DeserializeObject<LoginResponse>(responseJson);
}

public class LoginRequest
{
    public string account { get; set; }
    public string password { get; set; }
    public string mac_address { get; set; }
    public string hdd_id { get; set; }
    public string board_id { get; set; }
    public string nic_id { get; set; }
    public string process { get; set; }
}

public class LoginResponse
{
    public string result_code { get; set; }
    public string auth_token { get; set; }
    public string session_key { get; set; }
}
```

### 4.3 Hardware ID Collection

**Required NuGet Packages:**
```xml
<PackageReference Include="System.Management" Version="8.0.0" />
```

**Implementation:**
```csharp
using System.Management;
using System.Net.NetworkInformation;
using System.Security.Cryptography;

public class HardwareIdCollector
{
    public string GetMacAddress()
    {
        var interfaces = NetworkInterface.GetAllNetworkInterfaces();
        var activeInterface = interfaces.FirstOrDefault(i =>
            i.OperationalStatus == OperationalStatus.Up &&
            i.NetworkInterfaceType != NetworkInterfaceType.Loopback);

        if (activeInterface != null)
        {
            var macBytes = activeInterface.GetPhysicalAddress().GetAddressBytes();
            return HashSHA256(BitConverter.ToString(macBytes));
        }

        return string.Empty;
    }

    public string GetHardDriveId()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
            foreach (ManagementObject drive in searcher.Get())
            {
                var serialNumber = drive["SerialNumber"]?.ToString();
                if (!string.IsNullOrEmpty(serialNumber))
                {
                    return HashSHA256(serialNumber.Trim());
                }
            }
        }
        catch (Exception ex)
        {
            // Log error
        }

        return string.Empty;
    }

    public string GetMotherboardId()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard");
            foreach (ManagementObject board in searcher.Get())
            {
                var serialNumber = board["SerialNumber"]?.ToString();
                if (!string.IsNullOrEmpty(serialNumber))
                {
                    return HashSHA256(serialNumber.Trim());
                }
            }
        }
        catch (Exception ex)
        {
            // Log error
        }

        return string.Empty;
    }

    public string GetNetworkInterfaceId()
    {
        try
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces();
            var physicalInterfaces = interfaces
                .Where(i => i.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .Select(i => i.Id)
                .ToList();

            if (physicalInterfaces.Any())
            {
                var combined = string.Join("|", physicalInterfaces);
                return HashSHA256(combined);
            }
        }
        catch (Exception ex)
        {
            // Log error
        }

        return string.Empty;
    }

    public string GetRunningProcesses()
    {
        try
        {
            var processes = Process.GetProcesses()
                .Select(p => p.ProcessName)
                .OrderBy(name => name)
                .ToList();

            return string.Join(",", processes);
        }
        catch (Exception ex)
        {
            // Log error
            return string.Empty;
        }
    }

    private string HashSHA256(string input)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }
}
```

### 4.4 Encryption Implementation

**XOR Cipher (matching L1JR-Server):**
```csharp
public class XorCrypto
{
    private readonly byte[] _key;

    public XorCrypto(string base64Key)
    {
        _key = Convert.FromBase64String(base64Key);
    }

    public string Encrypt(string plaintext)
    {
        byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        byte[] encrypted = XorOperation(plaintextBytes);
        return Convert.ToBase64String(encrypted);
    }

    public string Decrypt(string ciphertext)
    {
        byte[] ciphertextBytes = Convert.FromBase64String(ciphertext);
        byte[] decrypted = XorOperation(ciphertextBytes);
        return Encoding.UTF8.GetString(decrypted);
    }

    private byte[] XorOperation(byte[] data)
    {
        byte[] result = new byte[data.Length];
        for (int i = 0; i < data.Length; i++)
        {
            result[i] = (byte)(data[i] ^ _key[i % _key.Length]);
        }
        return result;
    }
}
```

**AuthToken Encryption (matching HttpLoginSession.java):**
```csharp
public class AuthTokenGenerator
{
    public string GenerateAuthToken(string account, string password)
    {
        // Format: "{account}.{timestamp}"
        string tokenData = $"{account}.{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";

        byte[] tokenBytes = Encoding.UTF8.GetBytes(tokenData);
        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

        // XOR encode with password
        byte[] encoded = new byte[tokenBytes.Length];
        for (int i = 0; i < tokenBytes.Length; i++)
        {
            encoded[i] = (byte)(tokenBytes[i] ^ passwordBytes[i % passwordBytes.Length]);
        }

        // Base64 encode with trailing "="
        return Convert.ToBase64String(encoded) + "=";
    }
}
```

---

## 5. Architecture Recommendations

### 5.1 Project Structure

```
L1R-CustomLauncher/
├── src/
│   ├── LineageLauncher.App/              # WPF Application (Main EXE)
│   │   ├── Views/
│   │   │   ├── MainWindow.xaml
│   │   │   ├── LoginView.xaml
│   │   │   ├── PatchingView.xaml
│   │   │   └── SettingsView.xaml
│   │   ├── ViewModels/
│   │   │   ├── MainViewModel.cs
│   │   │   ├── LoginViewModel.cs
│   │   │   ├── PatchingViewModel.cs
│   │   │   └── SettingsViewModel.cs
│   │   ├── Services/
│   │   │   └── NavigationService.cs
│   │   └── App.xaml
│   │
│   ├── LineageLauncher.Core/             # Domain Models & Interfaces
│   │   ├── Models/
│   │   │   ├── ConnectorInfo.cs
│   │   │   ├── LoginRequest.cs
│   │   │   ├── LoginResponse.cs
│   │   │   ├── PatchInfo.cs
│   │   │   └── ServerInfo.cs
│   │   ├── Interfaces/
│   │   │   ├── IConnectorClient.cs
│   │   │   ├── ICryptoService.cs
│   │   │   ├── IPatcherService.cs
│   │   │   ├── ILauncherService.cs
│   │   │   └── IHardwareIdCollector.cs
│   │   └── Exceptions/
│   │       ├── AuthenticationException.cs
│   │       ├── PatchingException.cs
│   │       └── LauncherException.cs
│   │
│   ├── LineageLauncher.Crypto/           # Encryption & Security
│   │   ├── XorCrypto.cs
│   │   ├── AuthTokenGenerator.cs
│   │   ├── PasswordHasher.cs (Argon2id)
│   │   └── CryptoService.cs (implements ICryptoService)
│   │
│   ├── LineageLauncher.Network/          # HTTP API Client
│   │   ├── ConnectorClient.cs (implements IConnectorClient)
│   │   ├── Models/
│   │   │   ├── EncryptedConnectorInfo.cs
│   │   │   └── ApiResponse.cs
│   │   └── Exceptions/
│   │       └── NetworkException.cs
│   │
│   ├── LineageLauncher.Patcher/          # File Patching Engine
│   │   ├── PatcherService.cs (implements IPatcherService)
│   │   ├── FileDownloader.cs
│   │   ├── PatchApplicator.cs
│   │   ├── ChecksumValidator.cs
│   │   └── Models/
│   │       ├── PatchManifest.cs
│   │       └── FileEntry.cs
│   │
│   ├── LineageLauncher.Launcher/         # Process Manager
│   │   ├── LauncherService.cs (implements ILauncherService)
│   │   ├── LinBinLauncher.cs
│   │   ├── LoginIniGenerator.cs
│   │   ├── ProcessMonitor.cs
│   │   └── HardwareIdCollector.cs (implements IHardwareIdCollector)
│   │
│   └── LineageLauncher.Infrastructure/   # Infrastructure Services
│       ├── Logging/
│       │   └── FileLogger.cs
│       ├── Configuration/
│       │   └── LauncherConfig.cs
│       └── Storage/
│           └── SecureStorage.cs
│
├── tests/
│   ├── LineageLauncher.UnitTests/
│   │   ├── Crypto/
│   │   ├── Network/
│   │   ├── Patcher/
│   │   └── Launcher/
│   └── LineageLauncher.IntegrationTests/
│       └── EndToEndTests.cs
│
└── tools/
    └── PatchManifestGenerator/
        └── Program.cs
```

### 5.2 Dependency Injection Setup

**Program.cs:**
```csharp
public static class Program
{
    [STAThread]
    public static void Main()
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Configuration
                services.AddSingleton<IConfiguration>(context.Configuration);

                // Core Services
                services.AddSingleton<ICryptoService, CryptoService>();
                services.AddSingleton<IHardwareIdCollector, HardwareIdCollector>();

                // HTTP Client
                services.AddHttpClient<IConnectorClient, ConnectorClient>(client =>
                {
                    client.BaseAddress = new Uri(context.Configuration["ServerUrl"]);
                    client.Timeout = TimeSpan.FromSeconds(30);
                });

                // Business Logic Services
                services.AddSingleton<IPatcherService, PatcherService>();
                services.AddSingleton<ILauncherService, LauncherService>();

                // ViewModels
                services.AddTransient<MainViewModel>();
                services.AddTransient<LoginViewModel>();
                services.AddTransient<PatchingViewModel>();
                services.AddTransient<SettingsViewModel>();

                // Views
                services.AddSingleton<MainWindow>();
            })
            .Build();

        var app = new App();
        var mainWindow = host.Services.GetRequiredService<MainWindow>();
        app.Run(mainWindow);
    }
}
```

### 5.3 Clean Architecture Layers

```
┌─────────────────────────────────────────────┐
│          Presentation Layer                 │
│  (LineageLauncher.App - WPF UI)            │
│  - Views (XAML)                            │
│  - ViewModels (MVVM)                       │
│  - User Input Handling                     │
└──────────────┬──────────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────────┐
│          Application Layer                  │
│  (LineageLauncher.Core - Interfaces)       │
│  - Business Logic Interfaces               │
│  - Domain Models                           │
│  - Use Cases                               │
└──────────────┬──────────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────────┐
│          Implementation Layer               │
│  (Crypto, Network, Patcher, Launcher DLLs) │
│  - ICryptoService → CryptoService          │
│  - IConnectorClient → ConnectorClient      │
│  - IPatcherService → PatcherService        │
│  - ILauncherService → LauncherService      │
└──────────────┬──────────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────────┐
│          Infrastructure Layer               │
│  (LineageLauncher.Infrastructure)          │
│  - Logging                                 │
│  - Configuration                           │
│  - File System                             │
│  - Secure Storage                          │
└─────────────────────────────────────────────┘
```

**Benefits:**
- ✅ Testable (interfaces allow mocking)
- ✅ Maintainable (separation of concerns)
- ✅ Extensible (easy to add features)
- ✅ Decoupled (dependencies only on abstractions)

### 5.4 MVVM Pattern

**Example ViewModel:**
```csharp
public class LoginViewModel : ViewModelBase
{
    private readonly IConnectorClient _connectorClient;
    private readonly ICryptoService _cryptoService;
    private readonly IHardwareIdCollector _hardwareCollector;

    private string _username;
    private string _password;
    private bool _rememberMe;
    private bool _isLoggingIn;
    private string _statusMessage;

    public LoginViewModel(
        IConnectorClient connectorClient,
        ICryptoService cryptoService,
        IHardwareIdCollector hardwareCollector)
    {
        _connectorClient = connectorClient;
        _cryptoService = cryptoService;
        _hardwareCollector = hardwareCollector;

        LoginCommand = new AsyncRelayCommand(LoginAsync, CanLogin);
    }

    public string Username
    {
        get => _username;
        set => SetProperty(ref _username, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public IAsyncRelayCommand LoginCommand { get; }

    private bool CanLogin() => !string.IsNullOrEmpty(Username) &&
                               !string.IsNullOrEmpty(Password) &&
                               !_isLoggingIn;

    private async Task LoginAsync()
    {
        IsLoggingIn = true;
        StatusMessage = "Connecting to server...";

        try
        {
            // Collect hardware IDs
            var loginRequest = new LoginRequest
            {
                account = Username,
                password = Password,
                mac_address = _hardwareCollector.GetMacAddress(),
                hdd_id = _hardwareCollector.GetHardDriveId(),
                board_id = _hardwareCollector.GetMotherboardId(),
                nic_id = _hardwareCollector.GetNetworkInterfaceId(),
                process = _hardwareCollector.GetRunningProcesses()
            };

            // Send login request
            var response = await _connectorClient.LoginAsync(loginRequest);

            if (response.result_code == "success")
            {
                StatusMessage = "Login successful!";
                // Navigate to patching view
                // Store auth token for Lin.bin launch
            }
            else
            {
                StatusMessage = $"Login failed: {response.result_code}";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoggingIn = false;
        }
    }
}
```

---

## 6. Technical Requirements

### 6.1 Development Environment

**Required Software:**
- Windows 10/11 (64-bit)
- .NET 8.0 SDK
- Visual Studio 2022 or JetBrains Rider
- Git for version control

**Recommended Tools:**
- Fiddler or Wireshark (network debugging)
- Process Monitor (file/registry monitoring)
- Visual Studio Code (documentation)

### 6.2 NuGet Packages

**LineageLauncher.App:**
```xml
<PackageReference Include="ModernWpfUI" Version="0.9.6" />
<PackageReference Include="Microsoft.Web.WebView2" Version="1.0.2210.55" />
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Hosting" Version="8.0.0" />
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.2" />
```

**LineageLauncher.Core:**
```xml
<PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
```

**LineageLauncher.Crypto:**
```xml
<PackageReference Include="Konscious.Security.Cryptography.Argon2" Version="1.3.0" />
```

**LineageLauncher.Network:**
```xml
<PackageReference Include="System.Net.Http" Version="8.0.0" />
<PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
<PackageReference Include="Polly" Version="8.2.0" />
```

**LineageLauncher.Patcher:**
```xml
<PackageReference Include="SharpZipLib" Version="1.4.2" />
<PackageReference Include="System.IO.Compression" Version="8.0.0" />
```

**LineageLauncher.Launcher:**
```xml
<PackageReference Include="System.Management" Version="8.0.0" />
<PackageReference Include="System.Diagnostics.Process" Version="8.0.0" />
```

### 6.3 Configuration File Format

**appsettings.json:**
```json
{
  "Launcher": {
    "ServerUrl": "http://127.0.0.1:80",
    "EncryptionKey": "mOIjQ7ffyEV6w1SodWVqfwoU7qJCxzIhsqw6IM30okU=",
    "SessionKey": "linoffice1234",
    "ClientDirectory": "C:\\Lineage\\Client",
    "LinBinFileName": "Linbin2303281701.bin",
    "PatchDirectory": "C:\\Lineage\\Patches",
    "LogDirectory": "C:\\Lineage\\Logs",
    "RememberCredentials": true,
    "AutoUpdate": true,
    "Language": "en"
  },
  "Patching": {
    "ParallelDownloads": 4,
    "RetryAttempts": 3,
    "RetryDelayMs": 1000,
    "VerifyChecksums": true,
    "BackupBeforePatch": true
  },
  "AntiCheat": {
    "ProcessMonitoringEnabled": true,
    "ProcessMonitoringIntervalMs": 60000,
    "BlacklistedProcesses": [
      "cheat engine",
      "wireshark",
      "fiddler",
      "x64dbg",
      "ollydbg"
    ]
  },
  "Logging": {
    "LogLevel": "Information",
    "LogToFile": true,
    "MaxLogFileSizeMB": 10,
    "MaxLogFileCount": 5
  }
}
```

### 6.4 Login.ini Format (Generated by Launcher)

```ini
[User]
; Encrypted username (Base64 + XOR)
name=uSIWeYp+ghQqqhTKVkhRBkbrl0BwG6OeqYGQW08JEUg=

; Encrypted password (Base64 + XOR)
password=tUuq7v3nkIxLEvRDT9E5hA==

[Server]
; Server connection details (encrypted)
ip=encrypted_base64_ip
port=encrypted_base64_port

; Auth token from HTTP login
auth_token=encrypted_base64_token

[Options]
; Remember username checkbox
RememberUser=1

; Current patch version
PatchVersion=5

; Update client language (0=Korean, 1=English)
UpdateClientLanguage=1

; Show assistant (tutorial)
ShowAssistant=0

; Installation step
StepAssistant=0

; Client installation complete
InstallClient=1

; Language (0=Korean, 1=English, 2=Chinese)
Language=1
```

### 6.5 Patch Manifest Format (JSON)

```json
{
  "version": 5,
  "timestamp": "2025-11-11T12:00:00Z",
  "files": [
    {
      "path": "data/data.pak",
      "url": "http://127.0.0.1:80/connector/patch/data_pak.zip",
      "size": 15728640,
      "checksum": "sha256:abc123...",
      "compressed": true,
      "priority": 1
    },
    {
      "path": "sprite/sprite00.pak",
      "url": "http://127.0.0.1:80/connector/patch/sprite00_pak.zip",
      "size": 52428800,
      "checksum": "sha256:def456...",
      "compressed": true,
      "priority": 2
    }
  ],
  "required_version": 4,
  "notes": "November 2025 patch - English translations"
}
```

---

## 7. Implementation Roadmap

### 7.1 Phase 1: Foundation (Week 1-2)

**Goals:**
- Set up .NET solution
- Create all project structures
- Implement core interfaces
- Basic DI configuration

**Tasks:**
1. Create .NET 8 solution
2. Add all project files (.csproj)
3. Configure NuGet packages
4. Define core interfaces:
   - `IConnectorClient`
   - `ICryptoService`
   - `IPatcherService`
   - `ILauncherService`
   - `IHardwareIdCollector`
5. Create domain models
6. Set up dependency injection
7. Configure logging infrastructure

**Deliverables:**
- ✅ Buildable solution
- ✅ All interfaces defined
- ✅ Unit test projects created
- ✅ Basic configuration system

### 7.2 Phase 2: Crypto & Network (Week 3-4)

**Goals:**
- Implement encryption/decryption
- HTTP client for connector API
- Hardware ID collection

**Tasks:**
1. **LineageLauncher.Crypto:**
   - XOR cipher (matching L1JR-Server)
   - AuthToken generation
   - Base64 encoding/decoding
   - Unit tests for all crypto operations

2. **LineageLauncher.Network:**
   - `ConnectorClient` implementation
   - GET `/api/connector/info`
   - POST `/outgame/login`
   - Retry logic with Polly
   - Network error handling
   - Integration tests

3. **LineageLauncher.Launcher:**
   - `HardwareIdCollector` implementation
   - MAC address collection
   - HDD ID collection
   - Motherboard ID collection
   - NIC ID collection
   - Process enumeration

**Deliverables:**
- ✅ Working encryption/decryption
- ✅ HTTP API client
- ✅ Hardware ID collection
- ✅ 90%+ test coverage

### 7.3 Phase 3: Authentication (Week 5)

**Goals:**
- Complete login flow
- Session management
- Credential storage

**Tasks:**
1. Implement `LoginViewModel`
2. Create login UI (XAML)
3. Integrate with connector API
4. Store credentials securely (Windows Credential Manager)
5. Handle authentication errors
6. Remember username feature
7. Unit and integration tests

**Deliverables:**
- ✅ Working login screen
- ✅ Successful authentication with L1JR-Server
- ✅ Secure credential storage
- ✅ Error handling and user feedback

### 7.4 Phase 4: Lin.bin Launcher (Week 6)

**Goals:**
- Generate Login.ini
- Launch Lin.bin process
- Monitor process status

**Tasks:**
1. **LoginIniGenerator:**
   - Encrypt username/password
   - Write Login.ini to disk
   - Set appropriate file permissions

2. **LinBinLauncher:**
   - Process.Start() with correct working directory
   - Environment variable setup (if needed)
   - Process monitoring
   - Handle process exit/crash

3. **ProcessMonitor:**
   - Detect if Lin.bin is running
   - Restart on crash (optional)
   - Cleanup on exit

4. Testing with actual Lin.bin

**Deliverables:**
- ✅ Login.ini generation
- ✅ Successful Lin.bin launch
- ✅ Client connects to L1JR-Server
- ✅ Process monitoring

### 7.5 Phase 5: Patching System (Week 7-8)

**Goals:**
- Download patch files
- Apply patches to client
- Progress tracking

**Tasks:**
1. **PatcherService:**
   - Download patch manifest from server
   - Compare local version with server version
   - Parallel file downloads
   - Progress reporting

2. **PatchApplicator:**
   - Extract ZIP files
   - Copy files to client directory
   - Backup original files
   - Rollback on error

3. **ChecksumValidator:**
   - SHA256 checksum verification
   - Validate after download
   - Validate after extraction

4. **UI Integration:**
   - Progress bar
   - File count display
   - Speed display
   - Cancel button

**Deliverables:**
- ✅ Working patch system
- ✅ Parallel downloads
- ✅ Checksum validation
- ✅ Progress UI

### 7.6 Phase 6: UI Polish (Week 9-10)

**Goals:**
- Modern WPF UI
- WebView2 browser
- Settings panel
- Animations and transitions

**Tasks:**
1. **Main Window:**
   - Background image
   - Navigation menu
   - News/announcements area
   - Server status display

2. **Login View:**
   - Username/password fields
   - Remember me checkbox
   - Login button
   - Error messages
   - Loading spinner

3. **Patching View:**
   - Progress bars
   - File list
   - Speed/ETA display
   - Cancel button

4. **Settings View:**
   - Client directory selection
   - Language selection
   - Auto-update toggle
   - Clear cache button

5. **WebView2 Integration:**
   - Embedded browser for news
   - Navigate to CONNECTOR_BROWSER_URL
   - Handle navigation events

**Deliverables:**
- ✅ Complete modern UI
- ✅ Smooth animations
- ✅ Embedded browser
- ✅ Settings persistence

### 7.7 Phase 7: Testing & Bug Fixes (Week 11)

**Goals:**
- End-to-end testing
- Bug fixes
- Performance optimization
- Documentation

**Tasks:**
1. End-to-end integration tests
2. Load testing (multiple accounts)
3. Error scenario testing
4. Performance profiling
5. Memory leak detection
6. Security audit
7. User documentation
8. Developer documentation

**Deliverables:**
- ✅ All tests passing
- ✅ No critical bugs
- ✅ Performance acceptable
- ✅ Documentation complete

### 7.8 Phase 8: Deployment (Week 12)

**Goals:**
- Build for production
- Create installer
- Code signing
- Distribution

**Tasks:**
1. Create self-contained build
2. Create installer with Inno Setup or NSIS
3. Code signing certificate
4. Auto-update system
5. Crash reporting integration
6. Release to production

**Deliverables:**
- ✅ Production-ready launcher
- ✅ Installer package
- ✅ Code-signed executable
- ✅ Auto-update capability

---

## 8. Security Considerations

### 8.1 Credential Storage

**DO NOT:**
- ❌ Store plaintext passwords
- ❌ Store passwords in config files
- ❌ Use weak encryption

**DO:**
- ✅ Use Windows Credential Manager for stored credentials
- ✅ Hash passwords with Argon2id before storage
- ✅ Clear password from memory after use

**Implementation:**
```csharp
using System.Security.Cryptography;
using Windows.Security.Credentials;

public class SecureCredentialStorage
{
    private const string ResourceName = "LineageLauncher";

    public void StoreCredentials(string username, string password)
    {
        var vault = new PasswordVault();
        var credential = new PasswordCredential(ResourceName, username, password);
        vault.Add(credential);
    }

    public (string username, string password) RetrieveCredentials()
    {
        var vault = new PasswordVault();
        var credentials = vault.FindAllByResource(ResourceName);
        if (credentials.Count > 0)
        {
            var credential = credentials[0];
            credential.RetrievePassword();
            return (credential.UserName, credential.Password);
        }
        return (null, null);
    }

    public void DeleteCredentials(string username)
    {
        var vault = new PasswordVault();
        var credential = vault.Retrieve(ResourceName, username);
        vault.Remove(credential);
    }
}
```

### 8.2 Anti-Cheat Integration

**Process Monitoring:**
```csharp
public class AntiCheatMonitor
{
    private readonly List<string> _blacklist;
    private readonly ILogger _logger;
    private Timer _monitorTimer;

    public AntiCheatMonitor(IConfiguration config, ILogger logger)
    {
        _blacklist = config.GetSection("AntiCheat:BlacklistedProcesses")
            .Get<List<string>>();
        _logger = logger;
    }

    public void StartMonitoring(int intervalMs)
    {
        _monitorTimer = new Timer(_ => CheckForBlacklistedProcesses(),
            null, 0, intervalMs);
    }

    public void StopMonitoring()
    {
        _monitorTimer?.Dispose();
    }

    private void CheckForBlacklistedProcesses()
    {
        var processes = Process.GetProcesses();
        foreach (var process in processes)
        {
            var processName = process.ProcessName.ToLower();
            if (_blacklist.Any(b => processName.Contains(b.ToLower())))
            {
                _logger.LogWarning($"Blacklisted process detected: {processName}");
                OnBlacklistedProcessDetected?.Invoke(processName);
            }
        }
    }

    public event Action<string> OnBlacklistedProcessDetected;
}
```

### 8.3 Network Security

**HTTPS Enforcement:**
```csharp
public class SecureHttpClient
{
    private readonly HttpClient _httpClient;

    public SecureHttpClient(string baseUrl)
    {
        var handler = new HttpClientHandler
        {
            // Enforce HTTPS
            ServerCertificateCustomValidationCallback =
                (message, cert, chain, errors) =>
                {
                    // In production, validate certificate properly
                    return errors == SslPolicyErrors.None;
                }
        };

        _httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri(baseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };

        // Add security headers
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "LineageLauncher/1.0");
        _httpClient.DefaultRequestHeaders.Add("X-Client-Version", "1.0.0");
    }
}
```

### 8.4 Code Obfuscation

**Recommended Tool:**
- ConfuserEx (free)
- Dotfuscator (commercial)

**What to Obfuscate:**
- ✅ Encryption keys (embed in code, obfuscate)
- ✅ API endpoints
- ✅ Algorithm implementations
- ✅ Anti-cheat logic

**What NOT to Obfuscate:**
- ❌ Public APIs
- ❌ Exception messages
- ❌ Log messages

---

## 9. Testing Strategy

### 9.1 Unit Tests

**Coverage Goals:**
- Core: 90%+
- Crypto: 100%
- Network: 80%+
- Patcher: 85%+
- Launcher: 75%+

**Example Test:**
```csharp
[TestClass]
public class XorCryptoTests
{
    private XorCrypto _crypto;

    [TestInitialize]
    public void Setup()
    {
        // Use test encryption key
        _crypto = new XorCrypto("mOIjQ7ffyEV6w1SodWVqfwoU7qJCxzIhsqw6IM30okU=");
    }

    [TestMethod]
    public void Encrypt_PlainText_ReturnsBase64()
    {
        // Arrange
        string plaintext = "test@example.com";

        // Act
        string encrypted = _crypto.Encrypt(plaintext);

        // Assert
        Assert.IsTrue(IsBase64String(encrypted));
        Assert.AreNotEqual(plaintext, encrypted);
    }

    [TestMethod]
    public void Decrypt_EncryptedText_ReturnsOriginal()
    {
        // Arrange
        string plaintext = "test@example.com";
        string encrypted = _crypto.Encrypt(plaintext);

        // Act
        string decrypted = _crypto.Decrypt(encrypted);

        // Assert
        Assert.AreEqual(plaintext, decrypted);
    }

    [TestMethod]
    public void EncryptDecrypt_KoreanCharacters_PreservesText()
    {
        // Arrange
        string korean = "테스트계정";

        // Act
        string encrypted = _crypto.Encrypt(korean);
        string decrypted = _crypto.Decrypt(encrypted);

        // Assert
        Assert.AreEqual(korean, decrypted);
    }
}
```

### 9.2 Integration Tests

**Test Scenarios:**
1. Full login flow (launcher → web server → game server)
2. Patch download and application
3. Lin.bin launch and monitoring
4. Hardware ID collection
5. Session management
6. Error handling (network errors, invalid credentials, etc.)

**Example Integration Test:**
```csharp
[TestClass]
public class LoginFlowIntegrationTests
{
    private IConnectorClient _connectorClient;
    private IHardwareIdCollector _hardwareCollector;

    [TestInitialize]
    public void Setup()
    {
        // Use test server URL
        _connectorClient = new ConnectorClient(
            "http://127.0.0.1:80",
            "mOIjQ7ffyEV6w1SodWVqfwoU7qJCxzIhsqw6IM30okU="
        );
        _hardwareCollector = new HardwareIdCollector();
    }

    [TestMethod]
    public async Task LoginFlow_ValidCredentials_ReturnsAuthToken()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            account = "testuser",
            password = "testpass",
            mac_address = _hardwareCollector.GetMacAddress(),
            hdd_id = _hardwareCollector.GetHardDriveId(),
            board_id = _hardwareCollector.GetMotherboardId(),
            nic_id = _hardwareCollector.GetNetworkInterfaceId(),
            process = _hardwareCollector.GetRunningProcesses()
        };

        // Act
        var response = await _connectorClient.LoginAsync(loginRequest);

        // Assert
        Assert.IsNotNull(response);
        Assert.IsNotNull(response.auth_token);
        Assert.AreEqual("success", response.result_code);
    }
}
```

### 9.3 End-to-End Tests

**Test Scenarios:**
1. Fresh install → first launch → login → patch → play
2. Existing install → update check → apply patch → play
3. Invalid credentials → error message → retry
4. Network error → retry → success
5. Patch failure → rollback → success

---

## 10. Deployment Guide

### 10.1 Build Configuration

**Release Build:**
```xml
<PropertyGroup Condition="'$(Configuration)'=='Release'">
  <OutputType>WinExe</OutputType>
  <Platform>x64</Platform>
  <PublishSingleFile>true</PublishSingleFile>
  <SelfContained>true</SelfContained>
  <RuntimeIdentifier>win-x64</RuntimeIdentifier>
  <PublishTrimmed>true</PublishTrimmed>
  <TrimMode>link</TrimMode>
  <IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract>
  <DebugType>none</DebugType>
  <DebugSymbols>false</DebugSymbols>
</PropertyGroup>
```

**Build Command:**
```bash
dotnet publish src/LineageLauncher.App/LineageLauncher.App.csproj \
  --configuration Release \
  --runtime win-x64 \
  --self-contained true \
  --output publish/
```

### 10.2 Installer Creation

**Inno Setup Script:**
```iss
[Setup]
AppName=Lineage Remastered Launcher
AppVersion=1.0.0
DefaultDirName={pf}\LineageRemastered
DefaultGroupName=Lineage Remastered
OutputBaseFilename=LineageLauncher_Setup
Compression=lzma2
SolidCompression=yes
SetupIconFile=icon.ico
UninstallDisplayIcon={app}\LineageLauncher.exe

[Files]
Source: "publish\*"; DestDir: "{app}"; Flags: recursesubdirs

[Icons]
Name: "{group}\Lineage Remastered"; Filename: "{app}\LineageLauncher.exe"
Name: "{commondesktop}\Lineage Remastered"; Filename: "{app}\LineageLauncher.exe"

[Run]
Filename: "{app}\LineageLauncher.exe"; Description: "Launch Lineage Remastered"; Flags: postinstall nowait skipifsilent
```

### 10.3 Auto-Update System

**Update Check:**
```csharp
public class AutoUpdateService
{
    private readonly HttpClient _httpClient;
    private readonly string _updateUrl;

    public async Task<UpdateInfo> CheckForUpdatesAsync()
    {
        var response = await _httpClient.GetAsync($"{_updateUrl}/version.json");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var updateInfo = JsonConvert.DeserializeObject<UpdateInfo>(json);

        var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
        var latestVersion = Version.Parse(updateInfo.Version);

        if (latestVersion > currentVersion)
        {
            return updateInfo;
        }

        return null;
    }

    public async Task DownloadAndInstallUpdateAsync(UpdateInfo updateInfo)
    {
        // Download new version
        var updateFile = Path.Combine(Path.GetTempPath(), "LineageLauncher_Update.exe");
        using (var response = await _httpClient.GetAsync(updateInfo.DownloadUrl))
        {
            using (var fs = File.Create(updateFile))
            {
                await response.Content.CopyToAsync(fs);
            }
        }

        // Launch updater and exit
        Process.Start(updateFile, "/SILENT /CLOSEAPPLICATIONS");
        Application.Current.Shutdown();
    }
}

public class UpdateInfo
{
    public string Version { get; set; }
    public string DownloadUrl { get; set; }
    public string ReleaseNotes { get; set; }
    public bool Required { get; set; }
}
```

### 10.4 Code Signing

**Sign Executable:**
```bash
signtool sign /f "certificate.pfx" /p "password" /t http://timestamp.digicert.com "LineageLauncher.exe"
```

**Verify Signature:**
```bash
signtool verify /pa "LineageLauncher.exe"
```

---

## Appendix A: Configuration Reference

### A.1 L1JR-Server Configuration Files

**connector.properties:**
```properties
CONNECTOR_ENCRYPT_KEY = mOIjQ7ffyEV6w1SodWVqfwoU7qJCxzIhsqw6IM30okU=
CONNECTOR_SESSION_KEY = linoffice1234
CONNECTOR_CLIENT_SIDE_KEY = 711666385
CONNECTOR_DLL_PASSWORD = 2052201973
CONNECTOR_BROWSER_URL = http://127.0.0.1:80/
CONNECTOR_LINBIN_PATH = Linbin2303281701.bin
CONNECTOR_LINBIN_SIZE = 17020256
CONNECTOR_PATCH_VERSION = 5
CONNECTOR_PROCESS_MERGE = true
CONNECTOR_PROCESS_INTERVAL = 10
CONNECTOR_CLIENT_MAX_COUNT = 0
CONNECTOR_LOG = true
```

**version.properties:**
```properties
FIRST_KEY = e7, 7f, 69, fb, 2e, 47, e1, a3, 45
SERVER_NUMBER = 2
CLIENT_VERSION = 2303281701
MSDL_VERSION = 1006
LIBCOCOS_VERSION = 2304031701
CLIENT_SETTING_SWITCH = 889191819
```

**server.properties:**
```properties
LOGIN_SERVER_PORT = 2000
LOGIN_SERVER_ADDRESS = 127.0.0.1
AUTO_CREATE_ACCOUNTS = true
MAX_ONLINE_USERS = 1000
ALLOW_2PC = true
ALLOW_2PC_IP_COUNT = 5
ALLOW_2PC_HDD_COUNT = 5
LOGIN_TYPE = false
IP_PROTECT = false
CONNECT_DEVELOP_LOCK = false
```

### A.2 Launcher Configuration

**appsettings.json (complete):**
```json
{
  "Launcher": {
    "ServerUrl": "http://127.0.0.1:80",
    "EncryptionKey": "mOIjQ7ffyEV6w1SodWVqfwoU7qJCxzIhsqw6IM30okU=",
    "SessionKey": "linoffice1234",
    "ClientDirectory": "C:\\Lineage\\Client",
    "LinBinFileName": "Linbin2303281701.bin",
    "PatchDirectory": "C:\\Lineage\\Patches",
    "LogDirectory": "C:\\Lineage\\Logs",
    "RememberCredentials": true,
    "AutoUpdate": true,
    "Language": "en",
    "ClientSideKey": 711666385,
    "DllPassword": 2052201973
  },
  "Patching": {
    "ParallelDownloads": 4,
    "RetryAttempts": 3,
    "RetryDelayMs": 1000,
    "VerifyChecksums": true,
    "BackupBeforePatch": true,
    "BackupDirectory": "C:\\Lineage\\Backups"
  },
  "AntiCheat": {
    "ProcessMonitoringEnabled": true,
    "ProcessMonitoringIntervalMs": 60000,
    "BlacklistedProcesses": [
      "cheat engine",
      "decoder",
      "hack",
      "revolution",
      "linmaster",
      "linfree",
      "linpago",
      "nalyitqs",
      "flba",
      "wireshark",
      "virtualbox",
      "mixmaster",
      "parallels",
      "qemu",
      "hyper-v",
      "x64dbg",
      "ollydbg",
      "ida",
      "ghidra"
    ]
  },
  "Logging": {
    "LogLevel": "Information",
    "LogToFile": true,
    "MaxLogFileSizeMB": 10,
    "MaxLogFileCount": 5,
    "EnableDebugMode": false
  },
  "UI": {
    "Theme": "Dark",
    "AccentColor": "#007ACC",
    "BackgroundImage": "background.jpg",
    "ShowNews": true,
    "ShowServerStatus": true
  }
}
```

---

## Appendix B: Error Codes

### B.1 L1JR-Server Error Codes

| Code | Message | Cause | Solution |
|------|---------|-------|----------|
| `FAIL_NOT_FOUND_PARAMETERS` | Missing parameters | Required field missing | Check all hardware IDs present |
| `FAIL_NOT_FOUND_ACCOUNT` | Account not found | Invalid username | Verify account exists |
| `FAIL_INVALID_ACCOUNT` | Invalid password | Wrong password | Check credentials |
| `FAIL_MERGE_ACCOUNT` | Already logged in | Duplicate login | Force disconnect or wait |
| `IP_BAN_CHECK` | IP banned | IP in ban table | Contact admin |
| `IP_VPN_CHECK` | VPN detected | Foreign IP | Disable VPN |
| `IP_CHECK_FAIL` | Multi-PC not allowed | Same IP | Enable ALLOW_2PC |
| `IP_COUNT_MAX` | Too many connections | IP limit reached | Wait or contact admin |
| `ACCOUNT_BAN_CHECK` | Account banned | Account banned | Contact admin |
| `CONNECT_LOCK` | Server locked | Dev mode enabled | Wait for server open |
| `MAX_USER` | Server full | Max users reached | Wait or VIP access |
| `RE_LOGIN` | Duplicate session | Already connected | Force disconnect |
| `SESSION_EMPTY` | Invalid auth token | Token expired/invalid | Re-login |
| `OTHER_CONNECTOR` | Decryption failed | Wrong encryption key | Check CONNECTOR_ENCRYPT_KEY |

### B.2 Launcher Error Codes

| Code | Message | Cause | Solution |
|------|---------|-------|----------|
| `LAUNCHER_001` | Network error | Cannot reach server | Check internet connection |
| `LAUNCHER_002` | Decryption error | Invalid encryption key | Check config |
| `LAUNCHER_003` | Lin.bin not found | Missing client file | Reinstall client |
| `LAUNCHER_004` | Lin.bin launch failed | Permission error | Run as admin |
| `LAUNCHER_005` | Patch download failed | Network timeout | Retry |
| `LAUNCHER_006` | Patch apply failed | File locked | Close all clients |
| `LAUNCHER_007` | Checksum mismatch | Corrupted download | Re-download |
| `LAUNCHER_008` | Hardware ID error | Cannot collect HWID | Check permissions |
| `LAUNCHER_009` | Process monitoring error | Cannot enumerate processes | Check permissions |
| `LAUNCHER_010` | Blacklisted process detected | Cheat tool running | Close blacklisted process |

---

## Appendix C: Troubleshooting

### C.1 Common Issues

**Issue: "Cannot connect to server"**
```
Cause: Web server not running or wrong URL
Solution:
1. Check L1JR-Server is running
2. Verify WEB_SERVER_ENABLE = true in web.properties
3. Check firewall allows port 80
4. Test: curl http://127.0.0.1:80/api/connector/info
```

**Issue: "Invalid auth token"**
```
Cause: Token decryption failed
Solution:
1. Verify CONNECTOR_ENCRYPT_KEY matches between launcher and server
2. Check token format: base64 string ending with "="
3. Enable debug logging to see decryption attempts
```

**Issue: "Lin.bin won't launch"**
```
Cause: Missing Lin.bin or wrong working directory
Solution:
1. Verify Lin.bin exists in client directory
2. Check working directory in Process.Start()
3. Run launcher as administrator
4. Check Windows event log for errors
```

**Issue: "Patching stuck at 0%"**
```
Cause: Network issue or invalid patch URL
Solution:
1. Check internet connection
2. Verify patch URLs in server response
3. Test download manually: curl <patch_url>
4. Check firewall/antivirus blocking downloads
```

### C.2 Debug Logging

**Enable Debug Mode:**
```json
{
  "Logging": {
    "LogLevel": "Debug",
    "EnableDebugMode": true
  }
}
```

**Log Locations:**
- Launcher: `C:\Lineage\Logs\launcher.log`
- Server: `D:\L1R Project\L1JR-Server\log\`

**Useful Log Patterns:**
```csharp
_logger.LogDebug($"[ConnectorClient] Requesting /api/connector/info");
_logger.LogDebug($"[ConnectorClient] Response: {json}");
_logger.LogDebug($"[Crypto] Decrypting: {encryptedData}");
_logger.LogDebug($"[Crypto] Decrypted: {decryptedData}");
_logger.LogDebug($"[LauncherService] Launching Lin.bin: {linBinPath}");
_logger.LogDebug($"[LauncherService] Working directory: {workingDir}");
```

---

## Conclusion

This comprehensive guide provides everything needed to implement the L1R-CustomLauncher:

1. **Korean/Chinese launcher patterns** have been analyzed and synthesized
2. **L1JR-Server integration** requirements are fully documented
3. **Architecture recommendations** follow clean architecture and MVVM
4. **Technical requirements** are specified with code examples
5. **Implementation roadmap** provides a 12-week schedule
6. **Security considerations** address credential storage and anti-cheat
7. **Testing strategy** ensures quality and reliability
8. **Deployment guide** covers building, installing, and auto-update

**Next Steps:**
1. Review this guide thoroughly
2. Set up .NET solution (Phase 1)
3. Implement crypto and network layers (Phase 2)
4. Build authentication system (Phase 3)
5. Create Lin.bin launcher (Phase 4)
6. Add patching system (Phase 5)
7. Polish UI (Phase 6)
8. Test and deploy (Phases 7-8)

**Key Success Factors:**
- ✅ Use config file method (Login.ini) for Lin.bin launch
- ✅ Match L1JR-Server encryption exactly (XOR + Base64)
- ✅ Collect all required hardware IDs
- ✅ Follow two-stage authentication flow
- ✅ Implement robust error handling
- ✅ Test thoroughly before production

This launcher will be faster, smaller, and more maintainable than the existing LWLauncher while maintaining full compatibility with L1JR-Server's connector system.

---

**Document Version:** 1.0
**Last Updated:** November 11, 2025
**Author:** Claude Code + L1R Development Team
**Status:** Ready for Implementation
