# L1JR-Server Connector and Launcher System Analysis

## Overview
This document details how the L1JR-Server expects clients to connect, including the connector system, authentication flow, and custom launcher requirements.

**Date**: 2025-11-11
**Working Directory**: D:\L1R Project\L1JR-Server

---

## 1. Server Connection Architecture

### 1.1 xnetwork Package - Low-Level Networking

**Location**: `D:\L1R Project\L1JR-Server\src\xnetwork\`

The server uses a custom NIO-based networking layer:

#### Key Classes:
- **Acceptor.java** - Handles incoming connections on port 2000 (default)
- **SelectorThread.java** - Manages I/O operations using Java NIO selectors
- **Connection.java** - Represents individual client connections
- **ConnectionHandler.java** - Interface for handling connection events

#### Connection Process (Acceptor.java):
```java
// Location: D:\L1R Project\L1JR-Server\src\xnetwork\Acceptor.java (lines 20-30)
public void startAccept() throws IOException {
    _ssc = ServerSocketChannel.open();
    InetSocketAddress isa = new InetSocketAddress(_listenPort);
    _ssc.socket().bind(isa, 100);  // Backlog of 100 connections

    _selector.registerChannelLater(_ssc, SelectionKey.OP_ACCEPT, this,
        new CallbackErrorHandler() {
            public void handleError(Exception ex) {
                _handler.onError(Acceptor.this, ex);
            }
        });
}
```

### 1.2 LoginServer - Connection Acceptance

**Location**: `D:\L1R Project\L1JR-Server\src\l1j\server\server\LoginServer.java`

The LoginServer manages incoming client connections:

#### Configuration (server.properties):
```properties
LOGIN_SERVER_PORT = 2000
LOGIN_SERVER_ADDRESS = 127.0.0.1
```

#### Connection Flow (LoginServer.java, lines 46-94):
1. **Accept Connection** - Server accepts socket on port 2000
2. **IP Validation** - Checks if IP is banned (IpTable)
3. **Port Validation** - Blocks connections from well-known ports (0-1023)
4. **DDoS Protection** - Monitors connection rate per IP
5. **Client Creation** - Creates GameClient instance
6. **Encryption Init** - Sends FIRST_KEY and initializes encryption

```java
// Location: D:\L1R Project\L1JR-Server\src\l1j\server\server\LoginServer.java (lines 46-60)
@Override
public void onAccept(Acceptor acceptor, final SocketChannel sc) {
    Socket connection = sc.socket();
    String host = connection.getInetAddress().getHostAddress();
    int port = connection.getPort();

    // IP ban check
    if (IpTable.isBannedIp(host)) {
        connection.close();
        return;
    }

    // Well-known port check (blocks ports 0-1023)
    if (port <= Config.SERVER.WELLKNOWN_PORT) {
        IpTable.getInstance().insert(host, BanIpReason.WELLKNOWN_PORT);
        connection.close();
        return;
    }
    // ... continued
}
```

### 1.3 GameClient Initialization

**Location**: `D:\L1R Project\L1JR-Server\src\l1j\server\server\GameClient.java`

#### Initial Packet Exchange (GameClient.java, lines 74-87):
```java
protected void start(SocketChannel socketChannel, SelectorThread selector) throws IOException {
    System.out.println(String.format("[Connecting] [%s] IP : %s Memory : %d MB",
        FormatterUtil.get_formatter_time(), _ip, SystemUtil.getUsedMemoryMB()));

    _connection = new Connection(socketChannel, selector, this);

    if (Config.SERVER.AUTOMATIC_KICK > 0) {
        _observer = new ClientThreadObserver(Config.SERVER.AUTOMATIC_KICK * 60000);
        _observer.start();
    }

    _dollobserver = new DollObserver(15000);
    _dollobserver.start();

    // CRITICAL: Send encryption key to client
    _connection.send(Config.VERSION.FIRST_KEY);
    _cryption.initKeys(Config.VERSION.FIRST_KEY_SEED);
    _connection.resumeRecv();
}
```

**FIRST_KEY Configuration** (version.properties):
```properties
# Initial connection key (9-digit packet before server version call)
FIRST_KEY = e7, 7f, 69, fb, 2e, 47, e1, a3, 45
```

---

## 2. Connector System (Launcher Integration)

### 2.1 Web Server for Launcher Communication

**Location**: `D:\L1R Project\L1JR-Server\src\l1j\server\web\WebServer.java`

The server runs an embedded Netty-based web server for launcher communication:

#### Web Server Configuration (web.properties):
```properties
WEB_SERVER_ENABLE = true
WEB_SERVER_PORT = 80
```

#### Key Endpoints:

**1. Connector Info Endpoint**: `/api/connector/info`
- **Purpose**: Provides launcher with server configuration
- **Handler**: `ConnectorInfoDefine.java`
- **Returns**: Encrypted JSON with server details

**2. Login Endpoint**: `/outgame/login`
- **Purpose**: Launcher submits credentials
- **Handler**: `HttpAccountManager.java`
- **Creates**: `HttpLoginSession` with auth token

**3. Account Creation**: `/outgame/create`
- **Purpose**: New account registration via launcher

### 2.2 Connector Configuration

**Location**: `D:\L1R Project\L1JR-Server\config\connector.properties`

#### Key Settings:
```properties
# Encryption key for launcher communication
CONNECTOR_ENCRYPT_KEY = mOIjQ7ffyEV6w1SodWVqfwoU7qJCxzIhsqw6IM30okU=

# Session encryption key
CONNECTOR_SESSION_KEY = linoffice1234

# Client access key (must match launcher)
CONNECTOR_CLIENT_SIDE_KEY = 711666385

# MS_DLL password
CONNECTOR_DLL_PASSWORD = 2052201973

# Server connection details
LOGIN_SERVER_ADDRESS = 127.0.0.1
LOGIN_SERVER_PORT = 2000

# Web browser URL for launcher
CONNECTOR_BROWSER_URL = http://127.0.0.1:80/

# Lin.bin file configuration
CONNECTOR_LINBIN_PATH = Linbin2303281701.bin
CONNECTOR_LINBIN_SIZE = 17020256

# Client version (must match version.properties)
CLIENT_VERSION = 2303281701
```

### 2.3 Connector Files Structure

**Location**: `D:\L1R Project\L1JR-Server\appcenter\connector\`

```
appcenter/connector/
├── linbin/
│   ├── Linbin2303081701.bin
│   ├── Linbin2303141703.bin
│   ├── Linbin2303211701.bin
│   └── Linbin2303281701.bin (CURRENT)
├── libcocos/
│   └── libcocos2d.zip
├── msdll/
│   └── 210916.asi
├── boxer/
│   └── boxer.dll
├── loader/
│   └── RemastedLoader.bin
└── patch/
    ├── patch_1.zip
    ├── patch_2.zip
    ├── patch_3.zip
    ├── patch_4.zip
    ├── patch_5.zip
    ├── patch_english_lang.zip
    └── patch_korean_lang.zip
```

---

## 3. Authentication Flow

### 3.1 Launcher-Based Login (Recommended)

**Flow Diagram**:
```
Launcher                    Web Server                     Game Server
   |                            |                               |
   |---(1) GET /api/connector/info---------------------->|     |
   |<--(2) Encrypted config (IP, port, opcodes)----------|     |
   |                            |                               |
   |---(3) POST /outgame/login (credentials)------------>|     |
   |       {account, password, hwid, mac, etc}           |     |
   |<--(4) AuthToken (encrypted)-------------------------|     |
   |                            |                               |
   |---(5) Connect TCP 127.0.0.1:2000---------------------------->|
   |<--(6) FIRST_KEY (e7 7f 69 fb 2e 47 e1 a3 45)----------------|
   |---(7) C_EXTENDED_PROTOBUF: A_NpLogin (with AuthToken)------>|
   |       (Opcode 0x8f, contains encrypted auth token)  |       |
   |                            |                               |
   |                            |<--(8) Validate token---------|
   |                            |---(9) Account exists-------->|
   |                            |                               |
   |<--(10) S_LoginResult (success)------------------------------|
   |<--(11) Character list---------------------------------------|
```

#### Step-by-Step Process:

**Step 1-4: Web Server Authentication**

**Location**: `D:\L1R Project\L1JR-Server\src\l1j\server\web\http\connector\HttpLoginSession.java`

```java
// Lines 85-109: Login validation
public HttpLoginValidation validation(boolean merge) {
    if (!checkParameters()) {
        return HttpLoginValidation.FAIL_NOT_FOUND_PARAMETERS;
    }

    sessionAccount = Account.load(account);
    if (sessionAccount == null) {
        return HttpLoginValidation.FAIL_NOT_FOUND_ACCOUNT;
    }
    if (!sessionAccount.validatePassword(password)) {
        return HttpLoginValidation.FAIL_INVALID_ACCOUNT;
    }

    // Check if already connected
    GameClient client = LoginController.getInstance().getClientByAccount(account);
    if (client != null) {
        if (!merge) {
            return HttpLoginValidation.FAIL_MERGE_ACCOUNT;
        }
        mergeClient(client); // Disconnect existing connection
    }
    return HttpLoginValidation.SUCCESS;
}

// Lines 135-140: Create auth token
public void makeAuthToken() {
    byte[] encoded = String.format("%s.%d", this.account, System.currentTimeMillis())
        .getBytes(CharsetUtil.UTF_8);
    byte[] password = this.password.getBytes(CharsetUtil.UTF_8);
    CommonUtil.encode_xor(encoded, password, 0, encoded.length);
    authToken = String.format("%s=", Base64.getEncoder().encodeToString(encoded));
}
```

**HttpLoginSession Required Parameters**:
```java
// Lines 33-46: Session data
private String clientIp;
private int clientPort;
private String account;
private String password;
private String authToken;
private String macAddress;   // Required
private String hddId;        // Required
private String boardId;      // Required
private String nicId;        // Required
private String process;      // Running processes (for anti-cheat)
```

**Step 7: Game Server Login Packet**

**Location**: `D:\L1R Project\L1JR-Server\src\l1j\server\server\clientpackets\proto\A_NpLogin.java`

```java
// Lines 27-55: Parse login packet
@Override
protected void doWork() throws Exception {
    readP(1);           // 0x08
    readBit();          // IP

    readP(1);           // 0x10
    readBit();          // OTP

    readP(1);           // 0x1a
    int sublength = readC();
    readP(sublength);   // AUTH NP

    readP(1);           // 0x22
    int auth_length = readBit();
    byte[] auth_bytes = readByte(auth_length);  // ENCRYPTED AUTH TOKEN

    readP(1);           // 0x2a
    int maclength = readC();
    readP(maclength);   // MAC_HASH

    // Decrypt auth token
    String authToken;
    try {
        authToken = Base64.decryptToKey(
            new String(auth_bytes, CharsetUtil.UTF_8),
            Config.LAUNCHER.CONNECTOR_ENCRYPT_KEY
        );
    } catch(Exception e) {
        _client.sendPacket(S_CommonNews.OTHER_CONNECTOR);
        GeneralThreadPool.getInstance().schedule(new DelayClose(_client), 1500L);
        return;
    }

    // Retrieve login session from web server
    HttpLoginSession session = HttpAccountManager.get(authToken);
    if (session == null) {
        _client.sendPacket(S_CommonNews.SESSOION_EMPTY);
        return;
    }

    // Authorize the client
    _client.setLoginSession(session);
    Authorization.getInstance().auth(_client,
        session.getAccount(),
        session.getPassword(),
        _client.getIp(),
        _client.getHostname()
    );
}
```

**Step 8-11: Authorization Process**

**Location**: `D:\L1R Project\L1JR-Server\src\l1j\server\server\clientpackets\Authorization.java`

```java
// Lines 36-160: Complete authorization flow
public synchronized void auth(final GameClient client, String accountName,
                             String password, String ip, String host) throws IOException {
    // IP ban check
    if (IpTable.isBannedIp(ip)) {
        client.sendPacket(S_CommonNews.IP_BAN_CHECK);
        return;
    }

    // VPN check (if enabled)
    if (Config.SERVER.IP_PROTECT && !AuthIP.isWhiteIp(ip)) {
        client.sendPacket(S_CommonNews.IP_VPN_CHECK);
        return;
    }

    // Multi-PC check
    LoginController login = LoginController.getInstance();
    int loginCount = login.getIpCount(ip);
    if (!Config.SERVER.ALLOW_2PC && loginCount > 0) {
        client.sendPacket(S_CommonNews.IP_CHECK_FAIL);
        return;
    } else if (loginCount >= Config.SERVER.ALLOW_2PC_IP_COUNT) {
        client.sendPacket(S_CommonNews.IP_COUNT_MAX);
        return;
    }

    // Load and validate account
    Account account = Account.load(accountName);
    if (account == null || !account.validatePassword(password)) {
        client.sendPacket(S_LoginResult.ACCOUNT_FAIL);
        return;
    }

    // Check if banned
    if (account.isBanned()) {
        client.sendPacket(S_CommonNews.ACCOUNT_BAN_CHECK);
        return;
    }

    // Development mode check
    if (Config.SERVER.CONNECT_DEVELOP_LOCK && !account.isGameMaster()) {
        client.sendPacket(S_CommonNews.CONNECT_LOCK);
        return;
    }

    // Queue system check
    if (Config.SERVER.ACCESS_STANBY &&
        !EntranceQueue.getInstance().isStanby(client, account)) {
        return; // Client enters queue
    }

    try {
        login.login(client, account);
        account.updateLastActive(ip);
        client.setAccount(account);
        client.setEnterReady(true);
        entered(client); // Send character list
    } catch (GameServerFullException e) {
        client.sendPacket(S_CommonNews.MAX_USER);
        client.kick();
    } catch (AccountAlreadyLoginException e) {
        client.sendPacket(S_CommonNews.RE_LOGIN);
        client.kick();
    }
}
```

### 3.2 Direct Client Login (Legacy, Currently Disabled)

**Configuration** (server.properties):
```properties
# Login method (true connector / false client)
LOGIN_TYPE = false
```

**Opcode**: `C_LOGIN = 0x55`

**Note**: This method is currently unused. The server expects launcher-based authentication.

---

## 4. Opcodes and Packets

### 4.1 Connection-Related Opcodes

**Location**: `D:\L1R Project\L1JR-Server\src\l1j\server\server\Opcodes.java`

#### Client Opcodes (sent by client):
```java
public static final int C_LOGIN = 0x55;                    // Legacy direct login (unused)
public static final int C_LOGIN_RESULT = 0x12;             // Login result from client
public static final int C_LOGIN_TEST = 0x48;               // Login test packet
public static final int C_LOGOUT = 0x99;                   // Logout request
public static final int C_CHANNEL = 0x37;                  // Channel selection
public static final int C_EXTENDED_PROTOBUF = 0x8f;        // Extended protobuf packets
```

#### Server Opcodes (sent by server):
```java
public static final int S_KEY = 0x00;                      // Encryption key
public static final int S_LOGIN_RESULT = 0x01;             // Login result
public static final int S_DISCONNECT = 0x02;               // Disconnect client
public static final int S_EXTENDED_PROTOBUF = 0x92;        // Extended protobuf packets
```

### 4.2 Protobuf Extended Packets

The A_NpLogin packet is sent via C_EXTENDED_PROTOBUF (0x8f):

**Packet Structure**:
```
[Header]
byte    opcode           = 0x8f (C_EXTENDED_PROTOBUF)
short   packet_length    = total packet size

[Protobuf Content]
byte    0x08
varint  ip_value
byte    0x10
varint  otp_value
byte    0x1a
byte    sublength
bytes   auth_np_data
byte    0x22
varint  auth_token_length
bytes   encrypted_auth_token  // Base64 + CONNECTOR_ENCRYPT_KEY
byte    0x2a
byte    mac_length
bytes   mac_hash
```

### 4.3 Connector Info Response

**Location**: `D:\L1R Project\L1JR-Server\src\l1j\server\web\http\connector\HttpConnectorInfoResult.java`

The server sends encrypted configuration to the launcher:

```java
// Lines 61-125: Response structure
public HttpConnectorInfoResult(...) {
    // All values encrypted with CONNECTOR_ENCRYPT_KEY
    this.serverIp = Base64.encryptToBase64(_serverIp, encryptKey);
    this.serverPort = Base64.encryptToBase64(Integer.toString(_serverPort), encryptKey);
    this.browserUrl = Base64.encryptToBase64(_browserUrl, encryptKey);
    this.linbin = Base64.encryptToBase64(_linbin, encryptKey);
    this.linbinSize = Base64.encryptToBase64(Integer.toString(_linbinSize), encryptKey);
    this.linbinVersion = Base64.encryptToBase64(_linbinVersion, encryptKey);

    // Opcode configuration for client compatibility
    this.C_CHANNEL = Base64.encryptToBase64(Integer.toString(Opcodes.C_CHANNEL), encryptKey);
    this.C_LOGIN = Base64.encryptToBase64(Integer.toString(Opcodes.C_LOGIN), encryptKey);
    this.C_LOGOUT = Base64.encryptToBase64(Integer.toString(Opcodes.C_LOGOUT), encryptKey);
    this.S_KEY = Base64.encryptToBase64(Integer.toString(Opcodes.S_KEY), encryptKey);
    this.S_EXTENDED_PROTO_BUF = Base64.encryptToBase64(
        Integer.toString(Opcodes.S_EXTENDED_PROTOBUF), encryptKey);
    this.C_EXTENDED_PROTO_BUF = Base64.encryptToBase64(
        Integer.toString(Opcodes.C_EXTENDED_PROTOBUF), encryptKey);

    // Client configuration
    this.clientSideKey = Base64.encryptToBase64(
        Integer.toString(_clientSideKey), encryptKey);
    this.dllPassword = Base64.encryptToBase64(
        Integer.toString(_dllPassword), encryptKey);
}
```

**JSON Response Format**:
```json
{
  "result_code": "",
  "serverIp": "encrypted_base64",
  "serverPort": "encrypted_base64",
  "browserUrl": "encrypted_base64",
  "linbin": "encrypted_base64",
  "linbinSize": "encrypted_base64",
  "linbinVersion": "encrypted_base64",
  "libcocos": "encrypted_base64",
  "libcocosSize": "encrypted_base64",
  "libcocosVersion": "encrypted_base64",
  "msdll": "encrypted_base64",
  "msdllSize": "encrypted_base64",
  "msdllVersion": "encrypted_base64",
  "boxdll": "encrypted_base64",
  "craft": "encrypted_base64",
  "patch": "encrypted_base64",
  "patchVersion": "encrypted_base64",
  "engines": "encrypted_base64",
  "log": "encrypted_base64",
  "clientCount": "encrypted_base64",
  "createUri": "encrypted_base64",
  "loginUri": "encrypted_base64",
  "clientSideKey": "encrypted_base64",
  "dllPassword": "encrypted_base64",
  "C_CHANNEL": "encrypted_base64",
  "C_LOGIN": "encrypted_base64",
  "C_LOGOUT": "encrypted_base64",
  "S_CUSTOM_MESSAGE_BOX": "encrypted_base64",
  "S_KEY": "encrypted_base64",
  "S_EXTENDED_PROTO_BUF": "encrypted_base64",
  "C_EXTENDED_PROTO_BUF": "encrypted_base64"
}
```

---

## 5. Lin.bin Requirements

### 5.1 Version Validation

**Location**: `D:\L1R Project\L1JR-Server\config\version.properties`

```properties
# Allowed client version (LinBin version)
CLIENT_VERSION = 2303281701
MSDL_VERSION = 1006
LIBCOCOS_VERSION = 2304031701

# Initial connection key
FIRST_KEY = e7, 7f, 69, fb, 2e, 47, e1, a3, 45

# Connection server number (Server unique ID)
SERVER_NUMBER = 2

# Client setting switch
CLIENT_SETTING_SWITCH = 889191819
```

### 5.2 Lin.bin Files Available

**Location**: `D:\L1R Project\L1JR-Server\appcenter\connector\linbin\`

```
Linbin2303081701.bin
Linbin2303141703.bin
Linbin2303211701.bin
Linbin2303281701.bin  ← CURRENT VERSION
```

**File Size**: 17,020,256 bytes (16.2 MB)

### 5.3 Version Checking Flow

The server validates client version in multiple stages:

1. **Connector Info** - Returns expected version to launcher
2. **Initial Connection** - Sends FIRST_KEY based on version
3. **Login Packet** - Validates client version matches `CLIENT_VERSION`
4. **Opcode Routing** - Uses version-specific opcode files if needed

**Version-Specific Opcode Files**:
```
Opcodes.java              (default, 2022.04.08)
Opcodes_220121.java       (2022.01.21)
Opcodes_211006.java       (2021.10.06)
Opcodes_201028.java       (2020.10.28)
Opcodes_paladin.java      (paladin update)
Opcodes_dragonKnight.java (dragon knight update)
```

---

## 6. Security and Anti-Cheat

### 6.1 Hardware ID Validation

The server collects and validates multiple hardware identifiers:

**Required Hardware IDs** (HttpLoginSession.java):
```java
private String macAddress;  // MAC address hash
private String hddId;       // Hard drive ID
private String boardId;     // Motherboard ID
private String nicId;       // Network interface card ID
```

**Multi-Client Prevention** (server.properties):
```properties
# Allow 2 PCs (simultaneous access from the same IP)
ALLOW_2PC = true

# Allowed simultaneous access per IP
ALLOW_2PC_IP_COUNT = 5

# Allowed simultaneous access per HDD
ALLOW_2PC_HDD_COUNT = 5
```

### 6.2 Process Monitoring

**Location**: `D:\L1R Project\L1JR-Server\config\connector.properties`

```properties
# Update user process information (launcher every cycle ==> in-game ==> client)
CONNECTOR_PROCESS_MERGE = true

# User process information update cycle (unit: minutes)
CONNECTOR_PROCESS_INTERVAL = 10

# Engine names to be blocked
CONNECTOR_ENGINE_NAMES = cheet engine, decoder, hack, revolution, linmaster, \
    linfree, linpago, nalyitqs, flba, wireshark, virtualbox, mixmaster, \
    parallels, qemu, hyper-v
```

The launcher periodically sends running process information to the server. If a blacklisted process is detected, the client is disconnected.

### 6.3 Encryption

#### Packet Encryption:
- **Class**: `l1j.server.server.encryptions.lin380.LineageEncryption`
- **Location**: `D:\L1R Project\L1JR-Server\src\l1j\server\server\GameClient.java` (line 132)
- **Key Init**: Based on FIRST_KEY from version.properties

#### Auth Token Encryption:
- **Method**: XOR encoding + Base64
- **Key**: CONNECTOR_ENCRYPT_KEY
- **Format**: `{account}.{timestamp}` XOR with password, then Base64 encoded

### 6.4 IP Protection

**Configuration** (server.properties):
```properties
# IP approval system (based on Korea Internet & Security Agency / foreign IP blocking)
IP_PROTECT = false

# API key for Korean IP validation
IP_INFORMATION_API_KEY = %2BaJsH73n2oarvDBoOfEsObXKBfTVQkNvEf020XA4vb9n1Om7BQ2XQeWt167iPJOW6YnoX7lIaXyMiT9BRXTCoQ%3D%3D
```

**Validation** (Authorization.java, lines 46-54):
```java
if (Config.SERVER.IP_PROTECT && !AuthIP.isWhiteIp(ip)) {
    System.out.println("Block VPN IP access! Account=" + accountName + " IP=" + ip);
    client.sendPacket(S_CommonNews.IP_VPN_CHECK);
    GeneralThreadPool.getInstance().schedule(new DelayClose(client), 1500L);
    return;
}
```

---

## 7. Custom Launcher Requirements

### 7.1 Minimum Requirements

A custom launcher MUST implement the following:

#### 1. HTTP Communication with Web Server
- **GET** `/api/connector/info` - Retrieve server configuration
- **POST** `/outgame/login` - Submit credentials
- **POST** `/outgame/create` - Account creation (optional)

#### 2. Decryption Capability
- Decrypt connector info response using CONNECTOR_ENCRYPT_KEY
- Extract server IP, port, opcodes, and configuration

#### 3. Hardware ID Collection
- MAC address (hashed)
- Hard drive ID
- Motherboard ID
- Network interface card ID

#### 4. Lin.bin Management
- Download Lin.bin from connector endpoint
- Verify file size matches CONNECTOR_LINBIN_SIZE
- Execute Lin.bin with proper parameters

#### 5. TCP Connection to Game Server
- Connect to server IP:port from decrypted config
- Receive FIRST_KEY (9 bytes)
- Initialize packet encryption with FIRST_KEY

#### 6. Authentication Packet
- Send C_EXTENDED_PROTOBUF packet (0x8f)
- Include A_NpLogin protobuf data
- Embed encrypted auth token from web server

### 7.2 Recommended Features

#### 1. Patch Management
- Download and apply patch files from `/connector/patch/`
- Support multi-part patches (patch_1.zip, patch_2.zip, etc.)
- Handle language patches (patch_english_lang.zip)

#### 2. Auto-Update
- Check CONNECTOR_PATCH_VERSION
- Compare with local version
- Download and apply updates automatically

#### 3. Process Monitoring
- Collect running process list
- Send to `/outgame/process` every CONNECTOR_PROCESS_INTERVAL minutes
- Avoid running blacklisted processes

#### 4. Integrated Browser
- Embedded Chromium/WebView for app center
- Navigate to CONNECTOR_BROWSER_URL
- Handle in-game browser integration

### 7.3 Example Launcher Flow (C#)

```csharp
// Step 1: Get connector info
var connectorInfoUrl = "http://127.0.0.1:80/api/connector/info";
var connectorInfo = await GetConnectorInfoAsync(connectorInfoUrl);

// Step 2: Decrypt configuration
var encryptKey = "mOIjQ7ffyEV6w1SodWVqfwoU7qJCxzIhsqw6IM30okU=";
var serverIp = DecryptBase64(connectorInfo.serverIp, encryptKey);
var serverPort = int.Parse(DecryptBase64(connectorInfo.serverPort, encryptKey));

// Step 3: Login via web server
var loginData = new {
    account = username,
    password = password,
    mac_address = GetMacAddress(),
    hdd_id = GetHardDriveId(),
    board_id = GetMotherboardId(),
    nic_id = GetNetworkInterfaceId(),
    process = GetRunningProcesses()
};

var loginUrl = "http://127.0.0.1:80/outgame/login";
var loginResponse = await PostLoginAsync(loginUrl, loginData);
var authToken = loginResponse.auth_token;

// Step 4: Launch Lin.bin
var linBinPath = await DownloadLinBinAsync(connectorInfo.linbin);
var linBinProcess = Process.Start(linBinPath, $"--server={serverIp} --port={serverPort}");

// Step 5: Monitor and maintain session
while (linBinProcess.HasNotExited) {
    await Task.Delay(TimeSpan.FromMinutes(CONNECTOR_PROCESS_INTERVAL));
    await SendProcessInfoAsync("/outgame/process", GetRunningProcesses());
}
```

---

## 8. Configuration Files Summary

### 8.1 server.properties
**Location**: `D:\L1R Project\L1JR-Server\config\server.properties`

**Key Settings**:
```properties
LOGIN_SERVER_PORT = 2000
LOGIN_SERVER_ADDRESS = 127.0.0.1
AUTO_CREATE_ACCOUNTS = true
MAX_ONLINE_USERS = 1000
ALLOW_2PC = true
ALLOW_2PC_IP_COUNT = 5
ALLOW_2PC_HDD_COUNT = 5
LOGIN_TYPE = false  # false = launcher-based
IP_PROTECT = false  # VPN/foreign IP blocking
CONNECT_DEVELOP_LOCK = false  # GM-only mode
```

### 8.2 connector.properties
**Location**: `D:\L1R Project\L1JR-Server\config\connector.properties`

**Key Settings**:
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
CONNECTOR_CLIENT_MAX_COUNT = 0  # 0 = unlimited
CONNECTOR_LOG = true
```

### 8.3 version.properties
**Location**: `D:\L1R Project\L1JR-Server\config\version.properties`

**Key Settings**:
```properties
FIRST_KEY = e7, 7f, 69, fb, 2e, 47, e1, a3, 45
SERVER_NUMBER = 2
CLIENT_VERSION = 2303281701
MSDL_VERSION = 1006
LIBCOCOS_VERSION = 2304031701
CLIENT_SETTING_SWITCH = 889191819
```

### 8.4 web.properties (if exists)
**Expected Settings**:
```properties
WEB_SERVER_ENABLE = true
WEB_SERVER_PORT = 80
```

---

## 9. Troubleshooting

### 9.1 Common Connection Issues

#### Issue: "IP blocked user attempted to connect"
**Cause**: Client IP is in ban table
**Solution**: Check `app_ip` database table, remove ban entry

#### Issue: "Session empty" error
**Cause**: Auth token not found or expired
**Solution**: Ensure launcher sends auth token within session lifetime

#### Issue: "Other connector" error
**Cause**: Auth token decryption failed
**Solution**: Verify CONNECTOR_ENCRYPT_KEY matches between launcher and server

#### Issue: Client can't connect to port 2000
**Cause**: Firewall blocking, or server not listening
**Solution**: Check firewall rules, verify LoginServer.initialize() was called

### 9.2 Debug Logging

**Enable Opcode Logging** (server.properties):
```properties
OPCODES_PRINT_C = true   # Log client packets
OPCODES_PRINT_S = true   # Log server packets
PROTO_CLIENT_CODE_FIND = true
DEBUG_SERVER = true
CLIENT_FUNCTION_LOG_PRINT = true
```

**Enable Connector Logging** (connector.properties):
```properties
CONNECTOR_LOG = true
```

**Log Locations**:
- Game Server: `D:\L1R Project\L1JR-Server\log\`
- Connector: `C:\Users\{user}\AppData\Local\LinOffice\Connector\log\`

### 9.3 Testing Connection

#### Manual Connection Test:
```bash
# Test web server
curl http://127.0.0.1:80/api/connector/info

# Test game server (using netcat)
nc -v 127.0.0.1 2000
```

#### Expected Response:
1. Web server returns encrypted JSON
2. Game server sends 9-byte FIRST_KEY immediately
3. Connection stays open waiting for client packets

---

## 10. Security Considerations

### 10.1 Production Deployment

**CRITICAL**: Change these values before production:

```properties
# connector.properties
CONNECTOR_ENCRYPT_KEY = [GENERATE NEW KEY]
CONNECTOR_SESSION_KEY = [GENERATE NEW KEY]
CONNECTOR_CLIENT_SIDE_KEY = [GENERATE NEW VALUE]
CONNECTOR_DLL_PASSWORD = [GENERATE NEW VALUE]

# server.properties
DB_PASSWORD = [SET SECURE PASSWORD]
LOGIN_SERVER_ADDRESS = [PUBLIC IP]

# version.properties
FIRST_KEY = [GENERATE NEW KEY]
```

### 10.2 Network Security

**Recommended Firewall Rules**:
```
ALLOW TCP 2000 (Game Server)
ALLOW TCP 80/443 (Web Server)
DENY all other incoming connections
```

**Consider Using**:
- DDoS protection service (Cloudflare, etc.)
- Rate limiting on web endpoints
- IP whitelist for admin accounts
- SSL/TLS for web server (HTTPS)

### 10.3 Database Security

**server.properties**:
```properties
DB_LOGIN = root
DB_PASSWORD =   # CHANGE THIS!
```

**Recommendations**:
- Create dedicated database user (not root)
- Use strong password
- Limit database user permissions
- Enable SSL for MySQL connections
- Regular database backups

---

## 11. File Reference Index

### Core Server Files
- **LoginServer**: `D:\L1R Project\L1JR-Server\src\l1j\server\server\LoginServer.java`
- **GameServer**: `D:\L1R Project\L1JR-Server\src\l1j\server\server\GameServer.java`
- **GameClient**: `D:\L1R Project\L1JR-Server\src\l1j\server\server\GameClient.java`
- **Acceptor**: `D:\L1R Project\L1JR-Server\src\xnetwork\Acceptor.java`
- **PacketHandler**: `D:\L1R Project\L1JR-Server\src\l1j\server\server\PacketHandler.java`

### Authentication Files
- **Authorization**: `D:\L1R Project\L1JR-Server\src\l1j\server\server\clientpackets\Authorization.java`
- **A_NpLogin**: `D:\L1R Project\L1JR-Server\src\l1j\server\server\clientpackets\proto\A_NpLogin.java`
- **C_LoginToServer**: `D:\L1R Project\L1JR-Server\src\l1j\server\server\clientpackets\C_LoginToServer.java`
- **HttpLoginSession**: `D:\L1R Project\L1JR-Server\src\l1j\server\web\http\connector\HttpLoginSession.java`
- **HttpAccountManager**: `D:\L1R Project\L1JR-Server\src\l1j\server\web\http\connector\HttpAccountManager.java`

### Web Server Files
- **WebServer**: `D:\L1R Project\L1JR-Server\src\l1j\server\web\WebServer.java`
- **ConnectorInfoDefine**: `D:\L1R Project\L1JR-Server\src\l1j\server\web\dispatcher\response\define\ConnectorInfoDefine.java`
- **HttpConnectorInfoResult**: `D:\L1R Project\L1JR-Server\src\l1j\server\web\http\connector\HttpConnectorInfoResult.java`

### Configuration Files
- **server.properties**: `D:\L1R Project\L1JR-Server\config\server.properties`
- **connector.properties**: `D:\L1R Project\L1JR-Server\config\connector.properties`
- **version.properties**: `D:\L1R Project\L1JR-Server\config\version.properties`
- **LauncherConfigure**: `D:\L1R Project\L1JR-Server\src\l1j\server\configure\LauncherConfigure.java`

### Opcode Files
- **Opcodes.java**: `D:\L1R Project\L1JR-Server\src\l1j\server\server\Opcodes.java`
- **Opcodes_220121**: `D:\L1R Project\L1JR-Server\src\l1j\server\server\Opcodes_220121.java`
- **Opcodes_211006**: `D:\L1R Project\L1JR-Server\src\l1j\server\server\Opcodes_211006.java`
- **Opcodes_201028**: `D:\L1R Project\L1JR-Server\src\l1j\server\server\Opcodes_201028.java`

### Connector Files
- **Connector Directory**: `D:\L1R Project\L1JR-Server\appcenter\connector\`
- **Lin.bin Files**: `D:\L1R Project\L1JR-Server\appcenter\connector\linbin\`
- **Patch Files**: `D:\L1R Project\L1JR-Server\appcenter\connector\patch\`
- **Existing Launcher**: `D:\L1R Project\L1JR-Server\connector\LWLauncher_local\`

---

## Conclusion

The L1JR-Server uses a sophisticated two-stage authentication system:

1. **Web Server Stage** (HTTP) - Launcher authenticates via REST API, receives encrypted auth token
2. **Game Server Stage** (TCP) - Client connects with auth token, server validates session

This approach provides:
- Separation of concerns (web auth vs game connection)
- Enhanced security (hardware ID validation, process monitoring)
- Flexible launcher integration (web-based app center)
- Version control and auto-update capability

For a custom launcher to work, it must:
- Decrypt connector configuration using CONNECTOR_ENCRYPT_KEY
- Submit valid hardware IDs during login
- Send encrypted auth token via A_NpLogin packet
- Match client version (2303281701)
- Initialize encryption with FIRST_KEY

All configuration values are Base64-encoded and encrypted, requiring proper decryption keys for launcher development.
