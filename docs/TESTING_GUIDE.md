# L1R Custom Launcher - Comprehensive Testing Guide

**Last Updated:** 2025-11-11
**Version:** 0.1.0-alpha
**Purpose:** Complete testing plan for validating launcher functionality

---

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Environment Setup](#environment-setup)
3. [Component Testing](#component-testing)
4. [Integration Testing](#integration-testing)
5. [End-to-End Testing](#end-to-end-testing)
6. [Troubleshooting Guide](#troubleshooting-guide)
7. [Test Checklist](#test-checklist)

---

## Prerequisites

### Required Software

✓ **Windows 10/11** (64-bit)
✓ **.NET 8.0 Runtime** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
✓ **Visual C++ 2015-2019 Redistributable (x86)** - [Download](https://aka.ms/vs/17/release/vc_redist.x86.exe)
✓ **DirectX End-User Runtime (June 2010)** - [Download](https://www.microsoft.com/en-us/download/details.aspx?id=8109)
✓ **Java 8 JDK** - For running L1JR-Server

### Required Files

✓ **Lin.bin** - Game client executable
  Location: `D:\L1R Project\L1R-Client\bin64\Lin.bin`
  Size: ~21MB

✓ **L1JR-Server** - Game server (must be running)
  Location: `D:\L1R Project\L1JR-Server`
  Ports: 2000 (game), 8085 (web/connector API)

✓ **DLL Files** (hosted on connector server):
  - `210916.asi` (MS_DLL)
  - `boxer.dll`
  - `libcocos2d.zip`

### Server Configuration

The L1JR-Server must have these properties configured:

**`config/webserver.properties`:**
```properties
WEB_SERVER_ENABLE = true
WEB_SERVER_PORT = 8085
```

**`config/connector.properties`:**
```properties
CONNECTOR_ENCRYPT_KEY = mOIjQ7ffyEV6w1SodWVqfwoU7qJCxzIhsqw6IM30okU=
CONNECTOR_DLL_PASSWORD = 2052201973
CONNECTOR_CLIENT_SIDE_KEY = 711666385
```

**DLL Files Location:**
```
D:\L1R Project\L1JR-Server\appcenter\connector\
├── msdll\
│   └── 210916.asi
├── boxer\
│   └── boxer.dll
└── libcocos\
    └── libcocos2d.zip
```

---

## Environment Setup

### 1. Verify L1JR-Server is Running

```bash
# Check if game server is running on port 2000
netstat -ano | findstr :2000

# Check if web server is running on port 8085
netstat -ano | findstr :8085

# Both should show LISTENING with same PID (java.exe)
```

**Expected Output:**
```
TCP    0.0.0.0:2000           0.0.0.0:0              LISTENING       16848
TCP    0.0.0.0:8085           0.0.0.0:0              LISTENING       16848
```

### 2. Verify Lin.bin Exists

```bash
ls -lh "D:\L1R Project\L1R-Client\bin64\Lin.bin"
```

**Expected Output:**
```
-r-xr-xr-x 1 User 197121 21M Mar 26  2023 Lin.bin
```

### 3. Build Custom Launcher

```bash
cd "D:\L1R Project\L1R-CustomLauncher"
dotnet build --configuration Release
```

**Expected Output:**
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

---

## Component Testing

### Test 1: Base64 XOR Encryption/Decryption

**Purpose:** Verify encryption utilities work correctly

**Test Steps:**
1. Open `LineageLauncher.Crypto.Tests` project
2. Run encryption tests:
   ```bash
   dotnet test --filter "Category=Encryption"
   ```

**Expected Results:**
- ✓ `DecryptFromBase64_WithValidInput_ReturnsDecryptedString`
- ✓ `EncryptToBase64_ThenDecrypt_ReturnsOriginalString`
- ✓ `DecryptInt_WithValidInput_ReturnsDecryptedInteger`

**Manual Test:**
```csharp
var encryptedKey = "mOIjQ7ffyEV6w1SodWVqfwoU7qJCxzIhsqw6IM30okU=";
var encrypted = "someBase64EncryptedValue";
var decrypted = Base64Crypto.DecryptFromBase64(encrypted, encryptedKey);
// Should return decrypted plaintext
```

---

### Test 2: Connector API Client

**Purpose:** Verify HTTP communication with L1JR-Server connector

**Prerequisites:**
- L1JR-Server running on port 8085
- Connector endpoint configured

**Test Steps:**

#### 2.1 Test Connector Info Endpoint (Manual)

```bash
# Test with curl (may hang if web server not responding)
curl -X GET "http://127.0.0.1:8085/api/connector/info"
```

**Expected Response:**
```json
{
  "result_code": "",
  "serverIp": "ENCRYPTED_BASE64_VALUE",
  "serverPort": "ENCRYPTED_BASE64_VALUE",
  "msdll": "ENCRYPTED_BASE64_VALUE",
  "libcocos": "ENCRYPTED_BASE64_VALUE",
  "boxdll": "ENCRYPTED_BASE64_VALUE",
  "dllPassword": "ENCRYPTED_BASE64_VALUE",
  "clientSideKey": "ENCRYPTED_BASE64_VALUE",
  ...
}
```

#### 2.2 Test Connector API Client (Unit Test)

```csharp
[Test]
public async Task GetConnectorInfoAsync_ReturnsDecryptedInfo()
{
    var client = new ConnectorApiClient(httpClient, logger);
    var info = await client.GetConnectorInfoAsync();

    Assert.NotNull(info);
    Assert.NotEmpty(info.ServerIp);
    Assert.Greater(info.ServerPort, 0);
}
```

**Expected Results:**
- ✓ Connector info fetched successfully
- ✓ All encrypted fields decrypted properly
- ✓ Server IP: `127.0.0.1` or configured IP
- ✓ Server Port: `2000` (default game port)
- ✓ DLL Password: `2052201973`
- ✓ Client Side Key: `711666385`

---

### Test 3: Hardware ID Collector (Windows Only)

**Purpose:** Verify hardware identification works correctly

**Test Steps:**

```csharp
[Test]
[Platform("Win")]
public void GetMacAddress_ReturnsSHA256Hash()
{
    var collector = new HardwareIdCollector(logger);
    var macHash = collector.GetMacAddress();

    Assert.NotNull(macHash);
    Assert.AreEqual(64, macHash.Length); // SHA256 hex = 64 chars
}

[Test]
[Platform("Win")]
public void GetHardDriveId_ReturnsSHA256Hash()
{
    var collector = new HardwareIdCollector(logger);
    var hddHash = collector.GetHardDriveId();

    Assert.NotNull(hddHash);
    Assert.AreEqual(64, hddHash.Length);
}
```

**Expected Results:**
- ✓ MAC address collected and hashed (64-char hex)
- ✓ HDD serial collected and hashed
- ✓ Motherboard serial collected and hashed
- ✓ NIC ID collected and hashed
- ✓ Running processes list (comma-separated, max 50)

**Manual Verification:**
```bash
# View debug logs to see actual hardware IDs (before hashing)
# Should output: MAC Address: <value>, HDD Serial: <value>, etc.
```

---

### Test 4: DLL Deployment Service

**Purpose:** Verify DLL download and extraction works

**Prerequisites:**
- Connector API accessible
- DLL files hosted at configured URLs

**Test Steps:**

#### 4.1 Verify DLL File URLs

```bash
# Test MS_DLL download
curl -I "http://127.0.0.1:8085/connector/msdll/210916.asi"

# Test Boxer DLL download
curl -I "http://127.0.0.1:8085/connector/boxer/boxer.dll"

# Test Libcocos ZIP download
curl -I "http://127.0.0.1:8085/connector/libcocos/libcocos2d.zip"
```

**Expected Response:**
```
HTTP/1.1 200 OK
Content-Type: application/octet-stream
Content-Length: <file_size>
```

#### 4.2 Test DLL Deployment

```csharp
[Test]
public async Task DeployDllsAsync_DownloadsAllRequiredFiles()
{
    var service = new DllDeploymentService(connectorClient, logger);
    var clientDir = "D:\\L1R Project\\L1R-Client\\bin64";

    var result = await service.DeployDllsAsync(clientDir, progressReporter);

    Assert.IsTrue(result);
    Assert.IsTrue(File.Exists(Path.Combine(clientDir, "210916.asi")));
    Assert.IsTrue(File.Exists(Path.Combine(clientDir, "boxer.dll")));
    Assert.IsTrue(File.Exists(Path.Combine(clientDir, "libcocos2d.dll")));
}
```

**Expected Results:**
- ✓ MS_DLL (210916.asi) downloaded successfully
- ✓ Boxer DLL downloaded successfully
- ✓ Libcocos ZIP downloaded and extracted
- ✓ File sizes match connector-provided values
- ✓ Progress reported for each file (0-100%)

**Manual Verification:**
```bash
# Check if DLL files exist in client directory
ls -lh "D:\L1R Project\L1R-Client\bin64"/*.dll
ls -lh "D:\L1R Project\L1R-Client\bin64"/*.asi
```

---

### Test 5: Lin.bin Launcher Service

**Purpose:** Verify game client launches correctly

**Test Steps:**

#### 5.1 Test Login.ini Creation

```csharp
[Test]
public void CreateLoginIni_GeneratesCorrectConfiguration()
{
    var launcher = new LinBinLauncher(logger);
    var clientDir = "D:\\L1R Project\\L1R-Client\\bin64";

    // This is called internally by LaunchGameAsync
    // Verify Login.ini is created with correct content

    var loginIniPath = Path.Combine(clientDir, "Login.ini");
    Assert.IsTrue(File.Exists(loginIniPath));

    var content = File.ReadAllText(loginIniPath);
    Assert.Contains("[Server]", content);
    Assert.Contains("ip=127.0.0.1", content);
    Assert.Contains("port=2000", content);
}
```

**Expected Login.ini:**
```ini
[Server]
ip=127.0.0.1
port=2000

[Options]
RememberUser=1
Language=1
```

#### 5.2 Test Process Launch

```csharp
[Test]
public async Task LaunchGameAsync_StartsLinBinProcess()
{
    var launcher = new LinBinLauncher(logger);
    var serverInfo = new ServerInfo
    {
        ServerName = "Test Server",
        ServerAddress = "127.0.0.1",
        ServerPort = 2000
    };
    var user = new User { Username = "test_user" };

    var processId = await launcher.LaunchGameAsync(serverInfo, user);

    Assert.Greater(processId, 0);

    // Verify process is running
    var process = Process.GetProcessById(processId);
    Assert.NotNull(process);
    Assert.AreEqual("Lin", process.ProcessName);
}
```

**Expected Results:**
- ✓ Login.ini created in client directory
- ✓ Lin.bin process started successfully
- ✓ Process ID returned (> 0)
- ✓ Environment variables set (L1_DLL_PASSWORD, L1_CLIENT_SIDE_KEY)
- ✓ Process visible in Task Manager as "Lin.bin"

**Manual Verification:**
```bash
# Check if Lin.bin is running
tasklist | findstr "Lin"

# Should show:
# Lin.bin                      <PID> Console                    1     <Memory>
```

---

## Integration Testing

### Test 6: Full Authentication Flow

**Purpose:** Test complete login → connector fetch → DLL deploy → launch flow

**Test Steps:**

1. **Start Launcher**
   ```bash
   cd "D:\L1R Project\L1R-CustomLauncher\src\LineageLauncher.App\bin\Release\net8.0-windows"
   .\LineageLauncher.exe
   ```

2. **Login Screen**
   - Enter username: `test_user`
   - Enter password: `test_password`
   - Click "Login"

3. **Monitor Console Output:**
   ```
   [INFO] Authenticating...
   [INFO] Login successful!
   [INFO] Fetching server configuration...
   [INFO] Connected to 127.0.0.1:2000
   [INFO] Checking for updates...
   [INFO] Game is up to date!
   ```

4. **Click "Start Game"**

5. **Monitor DLL Deployment:**
   ```
   [INFO] Collecting hardware information...
   [INFO] Gathering system identifiers for anti-cheat...
   [INFO] Hardware IDs collected
   [INFO] Preparing game files...
   [INFO] Downloading 210916.asi... 0%
   [INFO] Downloading 210916.asi... 50%
   [INFO] Downloading 210916.asi... 100%
   [INFO] Downloading boxer.dll... 0%
   ...
   [INFO] DLL deployment complete
   [INFO] Starting game...
   [INFO] Launching Lin.bin...
   [INFO] Game launched successfully! PID: 12345
   ```

**Expected Results:**
- ✓ Login succeeds
- ✓ Connector info fetched and decrypted
- ✓ Server status shows "Connected"
- ✓ Patch check completes
- ✓ "Start Game" button enabled
- ✓ DLL files downloaded (first launch only)
- ✓ Hardware IDs collected
- ✓ Lin.bin launches successfully
- ✓ Process ID displayed

---

### Test 7: Error Handling

**Purpose:** Verify graceful error handling

#### 7.1 Server Offline

**Test Steps:**
1. Stop L1JR-Server
2. Launch custom launcher
3. Login

**Expected Results:**
- ✗ Connection error displayed
- ✗ "Failed to connect to server"
- ✓ No crash
- ✓ User can retry

#### 7.2 DLL Download Failure

**Test Steps:**
1. Remove DLL files from connector directory
2. Launch custom launcher
3. Login and click "Start Game"

**Expected Results:**
- ✗ "Failed to deploy required DLL files"
- ✓ Error message displayed to user
- ✓ Can retry download

#### 7.3 Lin.bin Missing

**Test Steps:**
1. Rename or move Lin.bin
2. Click "Start Game"

**Expected Results:**
- ✗ FileNotFoundException thrown
- ✗ "Lin.bin not found at: <path>"
- ✓ Clear error message to user

#### 7.4 Invalid Credentials

**Test Steps:**
1. Enter incorrect username/password
2. Click "Login"

**Expected Results:**
- ✗ "Invalid username or password"
- ✓ Password field cleared
- ✓ Can retry login

---

## End-to-End Testing

### Test 8: Complete User Journey

**Scenario:** First-time user launching the game

**Steps:**

1. ✓ User double-clicks `LineageLauncher.exe`
2. ✓ Launcher window appears
3. ✓ User enters credentials
4. ✓ Clicks "Login"
5. ✓ Authentication succeeds
6. ✓ Main launcher window appears
7. ✓ Status: "Connecting to server..."
8. ✓ Status: "Fetching server configuration..."
9. ✓ Status: "Connected to 127.0.0.1:2000"
10. ✓ Status: "Checking for updates..."
11. ✓ Status: "Game is up to date!"
12. ✓ "Start Game" button enabled
13. ✓ User clicks "Start Game"
14. ✓ Status: "Collecting hardware information..."
15. ✓ Status: "Preparing game files..."
16. ✓ Progress bar shows DLL download (210916.asi)
17. ✓ Progress bar shows DLL download (boxer.dll)
18. ✓ Progress bar shows DLL download (libcocos2d.zip)
19. ✓ Status: "Starting game..."
20. ✓ Status: "Game launched successfully! PID: <process_id>"
21. ✓ Lin.bin window appears
22. ✓ Login screen visible in game client
23. ✓ User can login to game

**Total Time:** ~30-60 seconds (first launch with DLL download)

---

### Test 9: Subsequent Launches

**Scenario:** User launching game again (DLLs already deployed)

**Steps:**

1. ✓ User double-clicks `LineageLauncher.exe`
2. ✓ Login screen appears
3. ✓ User enters credentials
4. ✓ Clicks "Login"
5. ✓ Main launcher window appears
6. ✓ Connector info fetched
7. ✓ Patch check completes
8. ✓ User clicks "Start Game"
9. ✓ Hardware IDs collected
10. ✓ **DLL deployment skipped** (already complete)
11. ✓ Lin.bin launches immediately
12. ✓ Game login screen appears

**Total Time:** ~5-10 seconds (subsequent launches)

---

## Troubleshooting Guide

### Issue: "Failed to connect to server"

**Possible Causes:**
- L1JR-Server not running
- Web server port incorrect
- Firewall blocking port 8085

**Solutions:**
1. Verify server is running:
   ```bash
   netstat -ano | findstr :8085
   ```
2. Check `webserver.properties`:
   ```
   WEB_SERVER_PORT = 8085
   ```
3. Check firewall rules

---

### Issue: "Failed to deploy required DLL files"

**Possible Causes:**
- DLL files not in connector directory
- Incorrect file paths in connector config
- Download URL construction error

**Solutions:**
1. Verify DLL files exist:
   ```bash
   ls "D:\L1R Project\L1JR-Server\appcenter\connector\msdll\210916.asi"
   ls "D:\L1R Project\L1JR-Server\appcenter\connector\boxer\boxer.dll"
   ls "D:\L1R Project\L1JR-Server\appcenter\connector\libcocos\libcocos2d.zip"
   ```
2. Check connector configuration
3. Test download URLs manually with curl

---

### Issue: "Lin.bin not found"

**Possible Causes:**
- Client path hardcoded incorrectly
- Lin.bin moved or deleted

**Solutions:**
1. Verify Lin.bin location:
   ```bash
   ls "D:\L1R Project\L1R-Client\bin64\Lin.bin"
   ```
2. Update path in `LinBinLauncher.cs` if needed

---

### Issue: Lin.bin launches but crashes immediately

**Possible Causes:**
- Missing VC++ Runtime
- Missing DirectX
- Missing PAK files
- Corrupted DLL files

**Solutions:**
1. Install VC++ 2015-2019 Redistributable (x86)
2. Install DirectX End-User Runtime
3. Verify PAK files exist in client directory
4. Re-download DLL files

---

## Test Checklist

### Pre-Launch Checklist

- [ ] L1JR-Server running (ports 2000, 8085)
- [ ] Lin.bin exists at `D:\L1R Project\L1R-Client\bin64\Lin.bin`
- [ ] DLL files in connector directory
- [ ] VC++ Runtime installed
- [ ] DirectX Runtime installed
- [ ] Custom launcher built successfully
- [ ] No compilation errors/warnings

### Component Test Checklist

- [ ] Base64 XOR encryption/decryption works
- [ ] Connector API client fetches info
- [ ] Hardware ID collector runs (Windows)
- [ ] DLL deployment service downloads files
- [ ] Lin.bin launcher creates Login.ini
- [ ] Lin.bin launcher starts process

### Integration Test Checklist

- [ ] Login authentication succeeds
- [ ] Connector info fetched and decrypted
- [ ] Server status shows "Connected"
- [ ] Patch check completes
- [ ] "Start Game" button enabled
- [ ] Hardware IDs collected
- [ ] DLL files downloaded (first launch)
- [ ] Lin.bin launches successfully
- [ ] Game login screen appears

### Error Handling Checklist

- [ ] Server offline handled gracefully
- [ ] DLL download failure shows error
- [ ] Lin.bin missing shows clear message
- [ ] Invalid credentials rejected
- [ ] No crashes on any error condition

### Performance Checklist

- [ ] First launch: < 60 seconds
- [ ] Subsequent launches: < 10 seconds
- [ ] Memory usage reasonable (< 200MB)
- [ ] No memory leaks
- [ ] CPU usage normal

---

## Test Results Template

Use this template to record test results:

```markdown
## Test Session: [Date]

**Tester:** [Name]
**Environment:** Windows [Version]
**Build:** [Launcher Version]
**Server:** L1JR-Server [Version]

### Test Results

| Test ID | Test Name | Status | Notes |
|---------|-----------|--------|-------|
| 1 | Base64 Encryption | ✓ PASS | All tests green |
| 2 | Connector API | ✓ PASS | Response time: 200ms |
| 3 | Hardware IDs | ✓ PASS | SHA256 hashes correct |
| 4 | DLL Deployment | ✓ PASS | All files downloaded |
| 5 | Lin.bin Launch | ✓ PASS | PID: 12345 |
| 6 | Auth Flow | ✓ PASS | Login successful |
| 7 | Error Handling | ✓ PASS | Graceful failures |
| 8 | E2E First Launch | ✓ PASS | 45 seconds total |
| 9 | E2E Subsequent | ✓ PASS | 8 seconds total |

### Issues Found

1. [Issue description]
   - Severity: High/Medium/Low
   - Status: Open/Fixed
   - Notes: [Details]

### Recommendations

- [Any suggestions for improvement]
```

---

## Next Steps

After successful testing:

1. [ ] Document any bugs found
2. [ ] Create GitHub issues for failures
3. [ ] Update this test guide with new findings
4. [ ] Plan next release features
5. [ ] Performance optimization if needed

---

**Test Plan Version:** 1.0
**Last Reviewed:** 2025-11-11
**Next Review:** After major feature additions
