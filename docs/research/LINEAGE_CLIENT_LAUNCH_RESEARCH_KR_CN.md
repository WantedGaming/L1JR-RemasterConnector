# 리니지 리마스터 클라이언트 실행 연구 보고서
# Lineage Remaster Client Launch Research Report
# 天堂重制版客户端启动研究报告

**연구 날짜 (Research Date / 研究日期):** 2025-01-11
**출처 언어 (Source Languages / 来源语言):** 한국어 (Korean) & 中文 (Chinese)
**연구 범위 (Research Scope / 研究范围):** 한국 및 중국 커뮤니티 출처만 사용 (Korean & Chinese sources only)

---

## 개요 (Summary / 摘要)

이 연구는 리니지 리마스터 게임 클라이언트를 실행하는 방법에 대한 한국어 및 중국어 출처의 기술 정보를 수집했습니다. 연구 결과, 리니지 클라이언트 실행은 주로 **커스텀 런처(접속기/登入器)**를 통해 이루어지며, 이 런처들은 IP 주소와 포트를 인코딩하여 클라이언트에 전달하는 방식을 사용합니다.

This research collected technical information from Korean and Chinese sources about launching the Lineage Remaster game client. The findings show that Lineage client execution primarily occurs through **custom launchers (접속기/登入器)** that encode and pass IP addresses and ports to the client.

本研究从韩文和中文来源收集了关于启动天堂重制版游戏客户端的技术信息。研究发现，天堂客户端启动主要通过**自定义启动器（接续机/登入器）**进行，这些启动器将IP地址和端口编码后传递给客户端。

---

## 1. Lin.bin 실행 파일 (Lin.bin Executable / Lin.bin执行文件)

### 1.1 Lin.bin이란? (What is Lin.bin? / 什么是Lin.bin?)

**한국어 출처:**
- Lin.bin은 리니지 클라이언트의 핵심 실행 파일입니다
- 이전에는 "스피드핵" 및 커스텀 스킨을 위해 무단으로 수정되었으며, NC소프트는 이를 약관 위반으로 간주
- 리니지 UI에 Lin.bin 파일 복구 기능 추가됨

**中文来源:**
- Lin.bin (也称为 TW13081901.bin, Lin.bin.exe, TW13081901.bin.exe) 是游戏的主核心执行文件
- 登入器(Login.exe)启动游戏时，就是针对这个 TW13081901.bin 文件
- 不同版本：
  - Lin.bin ver12010402 (351_S3DS2X) - L1J-TW-99nets 使用
  - Lin.bin ver12011702 (351_S3DS2Y) - 其他版本使用
  - ver12011702 版本默认开启穿人功能，并读取 CollisionZone.xml 配置无碰撞区域

### 1.2 버전 정보 (Version Information / 版本信息)

**主要版本对应关系:**

| 版本名称 | 文件名 | 用途 |
|---------|--------|------|
| 351_S3DS2X | ver12010402 | L1J-TW-99nets |
| 351_S3DS2Y | ver12011702 | 其他私服版本 |
| 380a | TW13081901.bin | Login_v380a 对应 |
| 1.63 | Lin270.exe | 旧版本客户端 |

### 1.3 커맨드라인 파라미터 (Command-Line Parameters / 命令行参数)

**한국어 출처:**
- Lin.bin은 일반적으로 **직접 커맨드라인 파라미터를 받지 않습니다**
- 대신, 런처(접속기)가 메모리 패치나 설정 파일을 통해 서버 정보를 주입합니다

**中文来源:**
- Lin.bin 不直接接受命令行参数
- IP地址和端口配置主要通过登入编码器（Encode.exe）生成配置文件完成
- 配置信息存储在 Login.ini 文件中，而不是通过命令行传递

**주요 발견 사항 (Key Finding / 关键发现):**
Lin.bin은 전통적인 의미의 커맨드라인 파라미터를 사용하지 않습니다. 대신:
1. 설정 파일 (Login.ini, Login.cfg)을 통한 구성
2. 런처에 의한 메모리 주입
3. 인코딩된 ServerData를 통한 서버 정보 전달

---

## 2. 설정 파일 (Configuration Files / 配置文件)

### 2.1 주요 설정 파일들 (Main Configuration Files / 主要配置文件)

#### Login.ini (登入器连接配置)

**파일 구조 (File Structure / 文件结构):**

```ini
[Server1]
ServerName=快樂天堂1
ServerData=LB0yci4fLho0ADAeLxY4CTgPIGA8Dj4OPhFRDT0SUg4+EVENPQ09ElIO...

[Server2]
ServerName=快樂天堂2
ServerData=LB0yci4fLho0ADAeLxY4CTgPIGA8Dj4OPhFRDT0SUg4+EVENPQ09ElIO...
```

**한국어 설명:**
- `[ServerN]`: 서버 섹션 헤더 (Server1-Server8까지 지원)
- `ServerName`: 런처의 서버 리스트에 표시되는 이름
- `ServerData`: **암호화된 서버 연결 정보** (IP, 포트, 버전 등 포함)

**中文说明:**
- ServerData 字段包含加密的配置内容
- 登入器解密 ServerData 以获取连接信息
- 加密内容通过 Encode.exe 编码器生成

**ServerData 암호화 내용 (Encrypted Content / 加密内容):**
- IP 주소 (IP Address / IP地址)
- 포트 번호 (Port Number / 端口号)
- 클라이언트 버전 (Client Version / 客户端版本)
- 기타 연결 파라미터 (Other connection parameters / 其他连接参数)

#### Login.cfg (登入器设置配置)

**파일 구조 (File Structure / 文件结构):**

```ini
[Setting]
BeanfunLogin=0
EatFileName=eat.exe
UpdateInfo=[编码后的更新配置]

[Hyperlink]
HyperlinkText1=官方网站
HyperlinkHttp1=http://server.com
HyperlinkTextColor1=0xFFE7C6

HyperlinkText2=论坛
HyperlinkHttp2=http://forum.server.com
HyperlinkTextColor2=0xFFE7C6

[Background]
Enable=1
Bulletin=skin\Bulletin.bmp
Serverlist=skin\Serverlist.bmp
Button=skin\Button.bmp
ButtonTextColor=0xFFFFFF
```

**설정 섹션 설명 (Setting Section / 设置区说明):**

| 설정 키 | 설명 (Korean) | 说明 (Chinese) | 기본값 |
|---------|--------------|---------------|--------|
| BeanfunLogin | 직접 로그인 모드 토글 | 直接登录模式开关 | 0/1 |
| EatFileName | 패치 유틸리티 실행 파일 이름 | 补丁工具可执行文件名 | eat.exe |
| UpdateInfo | 인코딩된 업데이트 구성 | 编码后的更新配置 | 自动生成 |

**하이퍼링크 섹션 (Hyperlink Section / 超链接区):**
- 최대 5개의 커스텀 링크 지원
- 각 링크에는 텍스트, URL, 색상 지정 가능

**배경 섹션 (Background Section / 背景区):**
- 커스텀 UI 스킨 지원
- .bmp 이미지 파일 경로 설정

#### Update.ini (更新配置)

```ini
[Update]
Version=3
1=patch001.zip
2=patch002.zip
3=patch003.zip
```

**업데이트 프로세스 (Update Process / 更新流程):**

1. Login.exe 시작 시 Login.cfg 읽어 자동 업데이트 위치 확인
2. 원격 Update.ini의 Version과 로컬 Login.ini.Updated의 version 비교
3. 로컬 버전 < 원격 버전이면 업데이트 실행
4. 순차적으로 패치 파일 다운로드 및 적용

### 2.2 서버 측 설정 파일 (Server-Side Config / 服务器端配置)

#### server.properties (Java 서버 설정)

```properties
# Database Configuration
Driver=com.mysql.jdbc.Driver
URL=jdbc:mysql://localhost/l1jdb?useUnicode=true&characterEncoding=euckr&autoReconnect=true
Login=root
Password=[your_password]

# Network Configuration
GameserverHostname=*
GameserverPort=2000
```

**주요 설정:**
- `GameserverPort`: 클라이언트 연결 포트 (기본값: 2000)
- `URL`: 데이터베이스 연결 문자열
- Hostname에 `*`는 모든 인터페이스에서 수신

---

## 3. 서버 연결 (Server Connection / 服务器连接)

### 3.1 연결 프로세스 (Connection Process / 连接流程)

**한국어 출처 - 클라이언트 측:**

1. **런처 실행 (Launcher Start)**
   - Login.exe 시작
   - Login.cfg 읽어 설정 로드
   - Login.ini에서 서버 리스트 로드

2. **서버 선택 (Server Selection)**
   - 사용자가 서버 리스트에서 선택
   - ServerData 복호화하여 연결 정보 추출

3. **클라이언트 시작 (Client Launch)**
   - Lin.bin 프로세스 생성 (CreateProcess)
   - 메모리에 서버 IP/포트 주입
   - 또는 임시 설정 파일 생성

4. **서버 인증 (Server Authentication)**
   - 클라이언트가 서버에 연결
   - 버전 확인 패킷 전송
   - 인증 패킷 교환

**中文来源 - 编码器工作流程:**

1. **打开 Encode.exe 编码器**
   - 从 Login_v342 编码器文件夹运行

2. **选择天堂版本**
   - 在工具菜单中选择版本 (例如 351_S3DS2X)
   - 确保与服务器核心版本匹配

3. **配置选项**
   - 启用多开功能
   - 启用变身档 (S3DS2X)
   - 启用内置喝水辅助程序

4. **编码服务器信息**
   - 输入服务器 IP 地址
   - 输入端口号 (通常 2000 或其他)
   - 点击编码按钮生成 Login.ini

5. **部署文件**
   - 将生成的 Login.cfg 和 Login.ini 移动到游戏客户端目录
   - 复制 Login.exe, eat.dll, eat.exe 等文件
   - 复制皮肤文件夹 (如果使用自定义界面)

### 3.2 IP 주소 및 포트 설정 (IP/Port Configuration / IP端口配置)

**한국어 방법:**

**방법 1: 직접 설정 파일 수정**
```
기본적으로 l1.ini 파일이나 system 폴더 내 관련 설정 파일을 수정
자체 런처를 사용하는 경우 해당 부분에서도 접속 IP를 설정
```

**방법 2: Ctool 사용**
```
1. Ctool 실행
2. 접속 주소: 127.0.0.1 (로컬) 또는 공인 IP
3. 접속 포트: 서버 포트 입력
4. 실행 파일: Lin.bin 경로 지정
5. 생성된 3개 파일을 클라이언트 폴더에 복사
```

**中文方法:**

**IP地址配置示例:**

| 连接类型 | IP地址 | 说明 |
|---------|--------|------|
| 本地测试 | 127.0.0.1 | 同一台电脑测试 |
| 局域网 | 192.168.x.x | 内网连接 |
| 外网连接 | 公网IP或域名 | 需要端口转发 |

**常见连接问题:**

**한국어:**
- 외부 접속 시 "이 서버는 사용할 수 없습니다" 메시지
  - DMZ 설정 오류 (공유기 사용자)
  - 사설 IP(192.168.xxx.xx) 또는 로컬 IP(127.0.0.1) 사용

**中文:**
- 外网客户端无法连接显示"此服务器无法使用"
  - 路由器 DMZ 设置错误
  - 使用了内网 IP (192.168.xxx.xx) 或本地 IP (127.0.0.1)
  - 需要配置端口转发

---

## 4. 인증 플로우 (Authentication Flow / 认证流程)

### 4.1 클라이언트-서버 인증 프로토콜 (Client-Server Auth Protocol / 客户端服务器认证协议)

**한국어 출처 정보:**
- 리니지 클라이언트는 자체 암호화 프로토콜 사용
- 패킷은 암호화되어 전송됨
- 소켓 파일에는 "게임 접속 시 적용되는 코드" 포함

**中文来源信息:**

**登入器认证功能:**

1. **RSA 包加密**
   - 登入器与模拟器之间支持 RSA 数据包加密
   - 需要通过"工具"标签的密钥生成功能生成密钥
   - 需要修改模拟器代码（参考"包加密核心修改"文档）

2. **服务器状态检测**
   - 绿灯/红灯指示服务器连通性
   - 实时检测服务器是否在线

3. **防外挂保护**
   - 基础防护选项
   - 高级防护选项

### 4.2 자격 증명 전달 방식 (Credential Passing / 凭证传递方式)

**发现的方法:**

**방법 1: 설정 파일 기반 (Config File Based / 基于配置文件)**
```
- Login.ini의 ServerData에 인코딩됨
- 런처가 복호화하여 사용
- 커맨드라인으로 전달되지 않음
```

**방법 2: 메모리 주입 (Memory Injection / 内存注入)**
```
- 런처가 Lin.bin 프로세스 시작
- CreateProcess로 프로세스 생성
- 메모리에 직접 IP/포트 쓰기
- 코드 패칭으로 서버 주소 변경
```

**방법 3: 임시 파일 (Temporary Files / 临时文件)**
```
- 런처가 임시 설정 파일 생성
- Lin.bin이 시작 시 읽음
- 연결 후 파일 삭제
```

### 4.3 세션 관리 (Session Management / 会话管理)

**서버 측 세션 토큰 시스템:**

```java
// L1J Server Example
public class GameClient {
    private String accountName;
    private String sessionKey; // 세션 키
    private long loginTime;
    private InetAddress clientIp;

    // 인증 확인
    public boolean isAuthenticated() {
        return sessionKey != null && accountName != null;
    }
}
```

**인증 흐름:**
1. 클라이언트가 로그인 패킷 전송
2. 서버가 계정 확인 및 세션 키 생성
3. 세션 키를 클라이언트에 반환
4. 이후 모든 패킷에 세션 키 포함

---

## 5. 런처 통합 (Launcher Integration / 启动器集成)

### 5.1 커스텀 런처 구조 (Custom Launcher Architecture / 自定义启动器架构)

**한국어 출처 - 런처 제작 도구:**

#### CtoolNt (씨툴엔티)

**지원 사항:**
- 운영 체제: Windows XP/Vista/7/8/10
- 언어: 한국어
- 제거 지원: 예
- 파일 크기: 4.45 MB

**사용 방법:**
1. Ctool.exe 실행
2. 접속 주소 입력 (127.0.0.1 또는 서버 IP)
3. 접속 포트 입력
4. 실행 파일 경로 (Lin.bin) 지정
5. 생성된 파일들을 클라이언트 폴더에 복사

**重要设置:**
```
접속주소: 로컬은 127.0.0.1
접속포트: 서버 포트 번호
실행파일: 리니지 클라이언트 폴더의 LIN.BIN
```

**中文来源 - Login.exe 组件结构:**

```
Login_v380/
├── Login.exe           # 主启动器程序
├── Encode.exe          # 编码器工具
├── Login.cfg           # 启动器设置
├── Login.ini           # 服务器列表配置
├── eat.exe             # 补丁工具
├── eat.dll             # 补丁库
├── LinHelperZ.txt      # 内置辅助宏配置
├── S3DS2X.bin          # 变身文件
├── Microsoft.VC90.CRT/ # VC++ 运行库
└── skin/               # 自定义界面皮肤
    ├── Bulletin.bmp
    ├── Serverlist.bmp
    └── Button.bmp
```

### 5.2 런처 실행 프로세스 (Launcher Execution Process / 启动器执行流程)

**详细启动流程:**

```
1. Login.exe 启动
   ↓
2. 读取 Login.cfg 获取设置
   ↓
3. 读取 Login.ini 获取服务器列表
   - 解密 ServerData 字段
   - 提取 IP, 端口, 版本信息
   ↓
4. 显示服务器选择界面
   - 检查服务器在线状态（绿灯/红灯）
   - 显示服务器名称
   ↓
5. 用户选择服务器并点击"开始游戏"
   ↓
6. 检查更新
   - 比较 Login.ini.Updated 和远程 Update.ini
   - 如果版本不同，下载补丁
   ↓
7. 启动 Lin.bin (TW13081901.bin)
   - 使用 CreateProcess 创建进程
   - 注入服务器连接信息到内存
   - 或创建临时配置文件
   ↓
8. Lin.bin 连接到服务器
   - 发送版本检查数据包
   - 发送认证数据包
   ↓
9. 游戏开始
```

### 5.3 파라미터 전달 메커니즘 (Parameter Passing Mechanism / 参数传递机制)

**발견된 방법들:**

#### 방법 1: 메모리 주입 (Memory Injection / 内存注入)

**한국어 설명:**
```
런처가 Lin.bin 프로세스를 일시 중지된 상태로 생성
프로세스 메모리에 접근하여 IP 주소를 직접 쓰기
특정 메모리 주소의 바이트를 패치
프로세스 재개
```

**技术实现概念 (未找到具体代码):**
```c++
// 伪代码示例 (Pseudo Code Example)
PROCESS_INFORMATION pi;
STARTUPINFO si;

// 创建暂停的进程
CreateProcess("Lin.bin", NULL, ..., CREATE_SUSPENDED, ...);

// 写入内存
DWORD serverIP = inet_addr("192.168.1.100");
DWORD serverPort = 2000;

WriteProcessMemory(pi.hProcess,
    (LPVOID)IP_ADDRESS_OFFSET,
    &serverIP,
    sizeof(DWORD),
    NULL);

WriteProcessMemory(pi.hProcess,
    (LPVOID)PORT_OFFSET,
    &serverPort,
    sizeof(DWORD),
    NULL);

// 恢复进程
ResumeThread(pi.hThread);
```

**重要内存偏移 (需要逆向工程确定):**
- IP 地址偏移: 0x????????
- 端口偏移: 0x????????
- 这些偏移随客户端版本变化

#### 방법 2: 설정 파일 생성 (Config File Generation / 配置文件生成)

**生成临时配置文件:**
```ini
# 临时配置文件示例
[Connection]
ServerIP=192.168.1.100
ServerPort=2000
ClientVersion=TW13081901
```

**프로세스:**
1. 런처가 임시 파일 생성
2. Lin.bin 시작 시 이 파일 읽기
3. 연결 후 파일 삭제 (보안)

#### 방법 3: 환경 변수 (Environment Variables / 环境变量)

```c++
// 设置环境变量
SetEnvironmentVariable("LINEAGE_SERVER_IP", "192.168.1.100");
SetEnvironmentVariable("LINEAGE_SERVER_PORT", "2000");

// Lin.bin 读取
char* serverIP = getenv("LINEAGE_SERVER_IP");
char* serverPort = getenv("LINEAGE_SERVER_PORT");
```

### 5.4 XOR 암호화 파라미터 (XOR Encrypted Parameters / XOR加密参数)

**한국어 출처 - XOR 암호화 개념:**

**XOR 암호화의 특징:**
- 텍스트 파일을 ASCII 코드로 변환
- 각 바이트를 비밀 키 값으로 XOR 연산
- 암호화와 복호화에 동일한 키 사용 가능
- 단순하지만 쉽게 깨짐
- 리니지에서는 추가 보안과 함께 사용

**중국 출처 - ServerData 加密:**

ServerData 字段使用 SFL 编码器加密:

```ini
ServerData=LB0yci4fLho0ADAeLxY4CTgPIGA8Dj4OPhFRDT0SUg4+EVENPQ09ElIO...
```

**加密内容包括:**
- 服务器 IP 地址
- 端口号
- 客户端版本标识
- 其他连接参数

**解密过程:**
1. Login.exe 读取 ServerData
2. 使用内置密钥解密
3. 提取 IP, 端口等信息
4. 传递给 Lin.bin

### 5.5 클라이언트 업데이트/패칭 처리 (Client Update/Patching / 客户端更新补丁处理)

**自动更新流程:**

```
Login.exe 启动
↓
读取 Login.cfg 中的 UpdateInfo
↓
从远程服务器下载 Update.ini
↓
比较版本号
  Login.ini.Updated (本地版本)
  vs
  Update.ini (远程版本)
↓
如果需要更新:
  下载补丁文件 (1=patch1.zip, 2=patch2.zip, ...)
  ↓
  使用 eat.exe 应用补丁
  ↓
  解压到客户端目录
  ↓
  更新 Login.ini.Updated 版本号
↓
启动游戏
```

**eat.exe 补丁工具:**
- 压缩补丁文件到客户端程序
- 自动应用文件更新
- 支持增量更新

**Update.ini 示例:**
```ini
[Update]
Version=5
1=ui_update.zip
2=sprite_update.zip
3=data_update.zip
4=system_fix.zip
5=balance_patch.zip
```

---

## 6. 한국 사설 서버 커뮤니티 (Korean Private Server Community / 韩国私服社区)

### 6.1 주요 커뮤니티 사이트 (Main Community Sites / 主要社区网站)

**한국어 출처:**

1. **리니지연구소 (Lineage Research Lab)**
   - URL: https://linlab3.com/
   - 내용: 서버 구축, 런처 제작, 기술 자료
   - 특징: 활발한 개발자 커뮤니티

2. **투데이서버 (Today Server)**
   - URL: https://todayserver.net/
   - 내용: 프리서버 정보, 기술 팁
   - 특징: 실행 오류 해결 가이드

3. **리니지 인벤 (Lineage Inven)**
   - URL: https://www.inven.co.kr/board/lineage/
   - 내용: 공식 서버 정보, 런처 오류 해결
   - 특징: 대형 게임 커뮤니티

4. **번개서버 (Bungae Server)**
   - URL: http://bgserver.live/
   - 내용: 프리서버, 프로그래밍, Java
   - 특징: 개발 중심 커뮤니티

**법적 주의사항:**
```
⚠️ 중요: 한국에서 사설 서버 운영은 불법입니다

- 2017년 6월부터 시행
- 5년 이하의 징역 또는 5천만원 이하의 벌금
- 게임산업진흥법 위반
- 저작권법 위반
- 사기죄 적용 가능

이 연구는 학술적/교육적 목적으로만 진행되었습니다.
```

### 6.2 런처 개발 가이드 (Launcher Development Guides / 启动器开发指南)

**한국어 커뮤니티 발견 정보:**

#### 접속기 제작 도구:

1. **CtoolNt (씨툴엔티)**
   - Windows 전용
   - GUI 기반 런처 생성
   - IP/포트 설정 간편

2. **린툴 (Lin Tool)**
   - 게임가드 내장
   - 간편한 프리서버 런처 생성

3. **노딤 클래스**
   - 1.63~2.00 / 리마스터 접속기 제작 방법
   - Visual Studio 2022 필요
   - 비디오 튜토리얼 제공 예정

**제작 프로세스 (一般流程):**

```
1. 개발 환경 설정
   - Visual Studio 2022 설치
   - Windows SDK 설치

2. Lin.bin 분석
   - Ghidra 또는 IDA Pro 사용
   - 서버 연결 코드 위치 찾기
   - IP/포트 하드코딩 위치 확인

3. 런처 프로그램 작성
   - C++ 또는 C# 사용
   - GUI 인터페이스 구현
   - 서버 리스트 관리

4. 메모리 패치 구현
   - CreateProcess로 프로세스 생성
   - WriteProcessMemory로 IP/포트 주입

5. 설정 파일 생성
   - Login.ini, Login.cfg 생성
   - ServerData 암호화

6. 배포
   - 모든 필요한 파일 패키징
   - 설치 가이드 작성
```

---

## 7. 중국 사설 서버 커뮤니티 (Chinese Private Server Community / 中国私服社区)

### 7.1 主要社区网站 (Main Community Sites / 주요 커뮤니티 사이트)

**中文来源:**

1. **45天堂私服论坛**
   - URL: https://lineage45.com/
   - 内容: 新手架设专区, 登入器下载, 技术讨论
   - 特点: 最活跃的中文天堂私服社区

2. **J.J.'s Blogs (MoroseDog GitLab)**
   - URL: https://morosedog.gitlab.io/
   - 内容: 详细的私服架设教学
   - 特点: 系统化的技术文档
   - 主题:
     - 登入器简介和功能说明
     - 登入器设定档说明
     - 客户端和登入器关系分析
     - 自动更新补丁系统

3. **天堂单机论坛**
   - URL: https://www.l2tw.com/
   - 内容: 单机版架设, 教程区
   - 特点: 专注于本地服务器搭建

4. **LinHelper / LinPRO**
   - URL: http://www.linpro.idv.tw/
   - 内容: 专业登入器工具
   - 特点: 支持多版本客户端

**法律声明:**
```
⚠️ 重要: 私服运营在大多数地区都属于违法行为

- 侵犯知识产权
- 违反软件授权协议
- 可能面临法律追责

本研究仅用于学术和教育目的。
```

### 7.2 启动器开发教程 (Launcher Development Tutorials / 런처 개발 튜토리얼)

**从中文社区发现的技术细节:**

#### 编码器 (Encode.exe) 详细使用:

**步骤 1: 选择版本**
```
工具菜单 → 选择天堂版本
选项:
- 351_S3DS2X (L1J-TW-99nets)
- 351_S3DS2Y (其他版本)
- 380a (较新版本)
```

**步骤 2: 配置功能**
```
[编码] 标签页:
☑ 多开功能 (Multi-client)
☑ 变身档 S3DS2X (Transformation files)
☑ 内置喝水辅助 (Built-in helper)
```

**步骤 3: 输入服务器信息**
```
服务器名称: 快乐天堂1
IP地址: 192.168.1.100 (或域名)
端口: 2000
版本: TW13081901
```

**步骤 4: 生成配置文件**
```
点击 [编码] 按钮
生成文件:
- Login.cfg (启动器设置)
- Login.ini (服务器列表)
```

**步骤 5: 部署文件**
```
复制到客户端目录:
- Login.exe
- Login.cfg
- Login.ini
- eat.exe
- eat.dll
- LinHelperZ.txt
- S3DS2X.bin
- skin/ 文件夹
- Microsoft.VC90.CRT/ 文件夹
```

#### LinHelperZ.txt 辅助宏配置:

**文件结构:**
```ini
[AllHP]
Item1=强化红色药水
Item2=强化绿色药水
Item3=治愈药水

[AllState]
Item1=0_速度药水
Item2=1_力量药水
Item3=2_敏捷药水

[AllPolymorphs]
Item1=变身卷轴_黑骑士
Item2=变身卷轴_黑色长老

[AllAntidote]
Item1=解毒剂
Item2=万能药
```

**编号说明:**
- `0_` = 状态增益类
- `1_` = 属性增益类
- `2_` = 特殊效果类

---

## 8. 일반적인 문제들 (Common Issues / 常见问题)

### 8.1 클라이언트 실행 오류 (Client Launch Errors / 客户端启动错误)

**한국어 출처 - 문제 해결:**

#### 문제 1: "실행 파일을 찾을 수 없습니다"

**원인:**
- Lin.bin 파일이 누락되거나 이름 변경됨
- 잘못된 경로 설정

**해결:**
```
1. 리니지 클라이언트 폴더 확인
2. Lin.bin 또는 Lin270.exe 파일 존재 확인
3. 필요시 Lin.bin을 Lin원본.bin으로 복사
4. 런처 설정에서 경로 재설정
```

#### 문제 2: "서버에 연결할 수 없습니다"

**원인:**
- 잘못된 IP/포트 설정
- 방화벽 차단
- 서버가 오프라인

**해결:**
```
1. Login.ini의 ServerData 확인
2. 서버 IP가 올바른지 확인
   - 로컬: 127.0.0.1
   - 공인 IP: 실제 서버 IP
3. 방화벽에서 포트 열기
4. 서버 상태 확인 (초록불/빨간불)
```

#### 문제 3: "버전이 일치하지 않습니다"

**원인:**
- 클라이언트 버전과 서버 버전 불일치
- 잘못된 Lin.bin 버전 사용

**해결:**
```
1. 서버 버전 확인 (351_S3DS2X, 351_S3DS2Y 등)
2. 해당하는 Lin.bin 사용
3. 런처 설정에서 올바른 버전 선택
4. 필요시 클라이언트 재다운로드
```

**中文来源 - 问题解决:**

#### 问题 1: 运行启动器显示"更新补丁失败"

**原因:**
- 计算机日期时间设置不正确
- 网络连接问题
- 更新服务器离线

**解决方案:**
```
1. 检查系统日期和时间
   - 设置为当前日期（不能是过去或未来）
2. 检查网络连接
3. 临时禁用防病毒软件
4. 联系服务器管理员
```

#### 问题 2: 启动器被杀毒软件删除

**原因:**
- 杀毒软件误报
- Windows Defender 实时保护
- 第三方安全软件

**解决方案:**
```
1. 将启动器添加到杀毒软件白名单
2. 临时禁用实时保护
   Windows Defender:
   设置 → 更新和安全 → Windows 安全中心
   → 病毒和威胁防护 → 管理设置
   → 关闭实时保护
3. 重新下载启动器
4. 运行前先添加例外
```

#### 问题 3: 点击"开始游戏"后无反应

**原因:**
- Lin.bin 文件损坏
- 缺少必要的 DLL 文件
- 权限不足

**解决方案:**
```
1. 以管理员身份运行启动器
   - 右键 → 以管理员身份运行
2. 检查客户端完整性
   - 验证所有文件存在
   - 重新安装客户端
3. 安装 Visual C++ 运行库
   - Microsoft.VC90.CRT
   - VC++ 2010 Redistributable
4. 检查游戏路径
   - 路径不能包含中文或特殊字符
```

### 8.2 연결 실패 처리 (Connection Failure Handling / 连接失败处理)

**런처가 구현해야 할 기능:**

```cpp
// 伪代码示例
enum ConnectionResult {
    SUCCESS,
    SERVER_OFFLINE,
    VERSION_MISMATCH,
    NETWORK_ERROR,
    AUTHENTICATION_FAILED,
    TIMEOUT
};

ConnectionResult ConnectToServer(ServerInfo server) {
    // 1. 检查服务器在线状态
    if (!PingServer(server.ip, server.port)) {
        return SERVER_OFFLINE;
    }

    // 2. 启动客户端
    if (!LaunchClient(server)) {
        return NETWORK_ERROR;
    }

    // 3. 等待连接确认
    if (!WaitForConnection(TIMEOUT_SECONDS)) {
        return TIMEOUT;
    }

    // 4. 验证版本
    if (!VerifyVersion(server.version)) {
        return VERSION_MISMATCH;
    }

    return SUCCESS;
}

// 错误处理
void HandleConnectionError(ConnectionResult result) {
    switch (result) {
        case SERVER_OFFLINE:
            ShowMessage("服务器离线或无法访问");
            break;
        case VERSION_MISMATCH:
            ShowMessage("客户端版本不匹配，请更新");
            break;
        case NETWORK_ERROR:
            ShowMessage("网络连接错误");
            break;
        case AUTHENTICATION_FAILED:
            ShowMessage("认证失败，请检查账号密码");
            break;
        case TIMEOUT:
            ShowMessage("连接超时");
            break;
    }
}
```

### 8.3 클라이언트 크래시 처리 (Client Crash Handling / 客户端崩溃处理)

**常见崩溃原因和解决:**

| 崩溃类型 | 原因 | 解决方案 |
|---------|------|---------|
| 启动时崩溃 | 缺少 DLL / 版本不匹配 | 重装 VC++ 运行库 |
| 连接时崩溃 | 网络包错误 / 版本问题 | 检查客户端版本 |
| 游戏中崩溃 | 内存不足 / 数据损坏 | 检查 PAK 文件完整性 |
| 退出时崩溃 | 资源释放问题 | 忽略（不影响使用）|

**런처 구현 - 크래시 감지:**

```cpp
// 监控客户端进程
HANDLE hProcess = OpenProcess(PROCESS_ALL_ACCESS, FALSE, processId);

// 等待进程结束
DWORD exitCode;
WaitForSingleObject(hProcess, INFINITE);
GetExitCodeProcess(hProcess, &exitCode);

if (exitCode != 0) {
    // 非正常退出
    LogCrash(exitCode);
    ShowCrashReport();

    // 提供重启选项
    if (MessageBox("游戏意外退出，是否重新启动？",
                   "错误", MB_YESNO) == IDYES) {
        RestartGame();
    }
}
```

---

## 9. 기술 구현 예시 (Technical Implementation Examples / 技术实现示例)

### 9.1 간단한 런처 의사 코드 (Simple Launcher Pseudo Code / 简单启动器伪代码)

```cpp
// =====================================================
// 简单的天堂游戏启动器实现
// Simple Lineage Game Launcher Implementation
// 간단한 리니지 런처 구현
// =====================================================

#include <windows.h>
#include <string>
#include <vector>

// 服务器信息结构
// Server Information Structure
// 서버 정보 구조체
struct ServerInfo {
    std::string name;        // 服务器名称 / Server Name / 서버 이름
    std::string ip;          // IP地址 / IP Address / IP 주소
    int port;                // 端口 / Port / 포트
    std::string version;     // 客户端版本 / Client Version / 클라이언트 버전
    bool isOnline;           // 在线状态 / Online Status / 온라인 상태
};

// 配置管理类
// Configuration Manager Class
// 설정 관리 클래스
class ConfigManager {
public:
    // 加载 Login.ini
    // Load Login.ini
    // Login.ini 로드
    std::vector<ServerInfo> LoadServerList(const char* configFile) {
        std::vector<ServerInfo> servers;

        // 读取 INI 文件
        // Read INI file
        // INI 파일 읽기
        char buffer[4096];
        GetPrivateProfileSectionNames(buffer, sizeof(buffer), configFile);

        // 解析每个服务器部分
        // Parse each server section
        // 각 서버 섹션 파싱
        char* section = buffer;
        while (*section) {
            if (strncmp(section, "Server", 6) == 0) {
                ServerInfo server;

                // 读取服务器名称
                // Read server name
                // 서버 이름 읽기
                char serverName[256];
                GetPrivateProfileString(section, "ServerName", "",
                                       serverName, sizeof(serverName),
                                       configFile);
                server.name = serverName;

                // 读取并解密 ServerData
                // Read and decrypt ServerData
                // ServerData 읽기 및 복호화
                char serverData[512];
                GetPrivateProfileString(section, "ServerData", "",
                                       serverData, sizeof(serverData),
                                       configFile);

                // 解密 ServerData 获取 IP 和端口
                // Decrypt ServerData to get IP and port
                // ServerData 복호화하여 IP와 포트 얻기
                DecryptServerData(serverData, server);

                servers.push_back(server);
            }
            section += strlen(section) + 1;
        }

        return servers;
    }

    // 解密 ServerData
    // Decrypt ServerData
    // ServerData 복호화
    void DecryptServerData(const char* encrypted, ServerInfo& server) {
        // 实际实现需要逆向工程确定加密算法
        // Actual implementation requires reverse engineering
        // 실제 구현은 역공학으로 암호화 알고리즘 확인 필요

        // 示例: Base64 解码 + XOR 解密
        // Example: Base64 decode + XOR decryption
        // 예시: Base64 디코드 + XOR 복호화
        std::string decoded = Base64Decode(encrypted);
        std::string decrypted = XorDecrypt(decoded, SECRET_KEY);

        // 解析 IP:PORT:VERSION
        // Parse IP:PORT:VERSION
        // IP:PORT:VERSION 파싱
        ParseConnectionString(decrypted, server);
    }

private:
    const char* SECRET_KEY = "YourSecretKeyHere"; // 需要逆向获取 / Needs RE / 역공학 필요
};

// 启动器类
// Launcher Class
// 런처 클래스
class LineageLauncher {
public:
    LineageLauncher(const char* clientPath)
        : m_clientPath(clientPath) {}

    // 启动游戏
    // Launch Game
    // 게임 시작
    bool LaunchGame(const ServerInfo& server) {
        // 方法 1: 内存注入
        // Method 1: Memory Injection
        // 방법 1: 메모리 주입
        return LaunchWithMemoryInjection(server);

        // 方法 2: 配置文件
        // Method 2: Config File
        // 방법 2: 설정 파일
        // return LaunchWithConfigFile(server);

        // 方法 3: 环境变量
        // Method 3: Environment Variables
        // 방법 3: 환경 변수
        // return LaunchWithEnvironmentVars(server);
    }

private:
    std::string m_clientPath;

    // 方法 1: 使用内存注入启动
    // Method 1: Launch with Memory Injection
    // 방법 1: 메모리 주입으로 시작
    bool LaunchWithMemoryInjection(const ServerInfo& server) {
        STARTUPINFO si = { sizeof(si) };
        PROCESS_INFORMATION pi;

        // 创建暂停的进程
        // Create suspended process
        // 일시 중지된 프로세스 생성
        if (!CreateProcess(
            m_clientPath.c_str(),   // Lin.bin 路径 / Path / 경로
            NULL,                    // 命令行参数 / Args / 인자
            NULL,                    // 进程安全属性 / Security / 보안
            NULL,                    // 线程安全属性 / Security / 보안
            FALSE,                   // 继承句柄 / Inherit / 상속
            CREATE_SUSPENDED,        // 创建标志 / Flags / 플래그
            NULL,                    // 环境 / Environment / 환경
            NULL,                    // 当前目录 / Directory / 디렉토리
            &si,                     // 启动信息 / Startup info / 시작 정보
            &pi                      // 进程信息 / Process info / 프로세스 정보
        )) {
            ShowError("无法启动游戏客户端");
            return false;
        }

        // 注入服务器 IP 和端口
        // Inject server IP and port
        // 서버 IP와 포트 주입
        if (!InjectServerInfo(pi.hProcess, server)) {
            TerminateProcess(pi.hProcess, 1);
            ShowError("无法注入服务器信息");
            return false;
        }

        // 恢复进程
        // Resume process
        // 프로세스 재개
        ResumeThread(pi.hThread);

        // 关闭句柄
        // Close handles
        // 핸들 닫기
        CloseHandle(pi.hThread);
        CloseHandle(pi.hProcess);

        return true;
    }

    // 注入服务器信息到进程内存
    // Inject server info into process memory
    // 프로세스 메모리에 서버 정보 주입
    bool InjectServerInfo(HANDLE hProcess, const ServerInfo& server) {
        // ⚠️ 警告: 这些偏移需要通过逆向工程确定
        // ⚠️ Warning: These offsets need to be determined via RE
        // ⚠️ 경고: 이러한 오프셋은 역공학으로 확인 필요

        // 假设的内存偏移（实际值需要逆向工程）
        // Assumed memory offsets (actual values need RE)
        // 가정된 메모리 오프셋 (실제 값은 역공학 필요)
        const DWORD IP_OFFSET = 0x00400000;    // 示例偏移 / Example / 예시
        const DWORD PORT_OFFSET = 0x00400004;  // 示例偏移 / Example / 예시

        // 将 IP 地址转换为网络字节序
        // Convert IP to network byte order
        // IP 주소를 네트워크 바이트 순서로 변환
        DWORD serverIP = inet_addr(server.ip.c_str());
        DWORD serverPort = htons((WORD)server.port);

        // 写入 IP 地址
        // Write IP address
        // IP 주소 쓰기
        SIZE_T bytesWritten;
        if (!WriteProcessMemory(
            hProcess,
            (LPVOID)IP_OFFSET,
            &serverIP,
            sizeof(DWORD),
            &bytesWritten
        )) {
            return false;
        }

        // 写入端口
        // Write port
        // 포트 쓰기
        if (!WriteProcessMemory(
            hProcess,
            (LPVOID)PORT_OFFSET,
            &serverPort,
            sizeof(DWORD),
            &bytesWritten
        )) {
            return false;
        }

        return true;
    }

    // 方法 2: 使用配置文件启动
    // Method 2: Launch with config file
    // 방법 2: 설정 파일로 시작
    bool LaunchWithConfigFile(const ServerInfo& server) {
        // 创建临时配置文件
        // Create temporary config file
        // 임시 설정 파일 생성
        std::string tempConfig = CreateTempConfigFile(server);

        // 启动客户端
        // Launch client
        // 클라이언트 시작
        STARTUPINFO si = { sizeof(si) };
        PROCESS_INFORMATION pi;

        std::string cmdLine = m_clientPath + " -config \"" + tempConfig + "\"";

        if (!CreateProcess(
            NULL,
            (LPSTR)cmdLine.c_str(),
            NULL, NULL, FALSE, 0, NULL, NULL,
            &si, &pi
        )) {
            return false;
        }

        // 等待进程启动
        // Wait for process to start
        // 프로세스 시작 대기
        WaitForInputIdle(pi.hProcess, 5000);

        // 删除临时配置文件
        // Delete temporary config file
        // 임시 설정 파일 삭제
        Sleep(2000);
        DeleteFile(tempConfig.c_str());

        CloseHandle(pi.hThread);
        CloseHandle(pi.hProcess);

        return true;
    }

    // 创建临时配置文件
    // Create temporary config file
    // 임시 설정 파일 생성
    std::string CreateTempConfigFile(const ServerInfo& server) {
        char tempPath[MAX_PATH];
        GetTempPath(MAX_PATH, tempPath);

        std::string configPath = std::string(tempPath) + "lineage_temp.ini";

        // 写入配置
        // Write config
        // 설정 쓰기
        WritePrivateProfileString("Connection", "ServerIP",
                                 server.ip.c_str(),
                                 configPath.c_str());
        WritePrivateProfileString("Connection", "ServerPort",
                                 std::to_string(server.port).c_str(),
                                 configPath.c_str());
        WritePrivateProfileString("Connection", "ClientVersion",
                                 server.version.c_str(),
                                 configPath.c_str());

        return configPath;
    }

    // 显示错误消息
    // Show error message
    // 오류 메시지 표시
    void ShowError(const char* message) {
        MessageBoxA(NULL, message, "启动器错误", MB_OK | MB_ICONERROR);
    }
};

// 主程序入口
// Main program entry
// 메인 프로그램 진입점
int WINAPI WinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance,
                   LPSTR lpCmdLine, int nCmdShow) {
    // 加载配置
    // Load configuration
    // 설정 로드
    ConfigManager config;
    std::vector<ServerInfo> servers = config.LoadServerList("Login.ini");

    if (servers.empty()) {
        MessageBox(NULL, "未找到服务器配置", "错误", MB_OK | MB_ICONERROR);
        return 1;
    }

    // 显示服务器选择对话框（简化版）
    // Show server selection dialog (simplified)
    // 서버 선택 대화상자 표시 (간소화)
    // ...

    // 假设用户选择了第一个服务器
    // Assume user selected first server
    // 사용자가 첫 번째 서버를 선택했다고 가정
    ServerInfo selectedServer = servers[0];

    // 创建启动器实例
    // Create launcher instance
    // 런처 인스턴스 생성
    LineageLauncher launcher("C:\\Lineage\\Lin.bin");

    // 启动游戏
    // Launch game
    // 게임 시작
    if (!launcher.LaunchGame(selectedServer)) {
        MessageBox(NULL, "启动游戏失败", "错误", MB_OK | MB_ICONERROR);
        return 1;
    }

    return 0;
}
```

### 9.2 ServerData 암호화/복호화 예시 (ServerData Encryption/Decryption Example / ServerData加密解密示例)

```cpp
// =====================================================
// ServerData 加密解密实现示例
// ServerData Encryption/Decryption Implementation
// ServerData 암호화/복호화 구현 예시
// =====================================================

#include <string>
#include <vector>
#include <sstream>

// Base64 编码表
// Base64 encoding table
// Base64 인코딩 테이블
static const std::string base64_chars =
    "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
    "abcdefghijklmnopqrstuvwxyz"
    "0123456789+/";

// Base64 编码
// Base64 encode
// Base64 인코딩
std::string Base64Encode(const std::vector<unsigned char>& data) {
    std::string ret;
    int i = 0;
    int j = 0;
    unsigned char char_array_3[3];
    unsigned char char_array_4[4];
    int in_len = data.size();
    int pos = 0;

    while (in_len--) {
        char_array_3[i++] = data[pos++];
        if (i == 3) {
            char_array_4[0] = (char_array_3[0] & 0xfc) >> 2;
            char_array_4[1] = ((char_array_3[0] & 0x03) << 4) +
                             ((char_array_3[1] & 0xf0) >> 4);
            char_array_4[2] = ((char_array_3[1] & 0x0f) << 2) +
                             ((char_array_3[2] & 0xc0) >> 6);
            char_array_4[3] = char_array_3[2] & 0x3f;

            for(i = 0; i < 4; i++)
                ret += base64_chars[char_array_4[i]];
            i = 0;
        }
    }

    if (i) {
        for(j = i; j < 3; j++)
            char_array_3[j] = '\0';

        char_array_4[0] = (char_array_3[0] & 0xfc) >> 2;
        char_array_4[1] = ((char_array_3[0] & 0x03) << 4) +
                         ((char_array_3[1] & 0xf0) >> 4);
        char_array_4[2] = ((char_array_3[1] & 0x0f) << 2) +
                         ((char_array_3[2] & 0xc0) >> 6);

        for (j = 0; (j < i + 1); j++)
            ret += base64_chars[char_array_4[j]];

        while((i++ < 3))
            ret += '=';
    }

    return ret;
}

// Base64 解码
// Base64 decode
// Base64 디코딩
std::vector<unsigned char> Base64Decode(const std::string& encoded_string) {
    int in_len = encoded_string.size();
    int i = 0;
    int j = 0;
    int in_ = 0;
    unsigned char char_array_4[4], char_array_3[3];
    std::vector<unsigned char> ret;

    while (in_len-- && ( encoded_string[in_] != '=') &&
           (isalnum(encoded_string[in_]) ||
            (encoded_string[in_] == '+') ||
            (encoded_string[in_] == '/'))) {
        char_array_4[i++] = encoded_string[in_]; in_++;
        if (i ==4) {
            for (i = 0; i <4; i++)
                char_array_4[i] = base64_chars.find(char_array_4[i]);

            char_array_3[0] = (char_array_4[0] << 2) +
                             ((char_array_4[1] & 0x30) >> 4);
            char_array_3[1] = ((char_array_4[1] & 0xf) << 4) +
                             ((char_array_4[2] & 0x3c) >> 2);
            char_array_3[2] = ((char_array_4[2] & 0x3) << 6) +
                             char_array_4[3];

            for (i = 0; (i < 3); i++)
                ret.push_back(char_array_3[i]);
            i = 0;
        }
    }

    if (i) {
        for (j = 0; j < i; j++)
            char_array_4[j] = base64_chars.find(char_array_4[j]);

        char_array_3[0] = (char_array_4[0] << 2) +
                         ((char_array_4[1] & 0x30) >> 4);
        char_array_3[1] = ((char_array_4[1] & 0xf) << 4) +
                         ((char_array_4[2] & 0x3c) >> 2);

        for (j = 0; (j < i - 1); j++)
            ret.push_back(char_array_3[j]);
    }

    return ret;
}

// XOR 加密/解密
// XOR encrypt/decrypt
// XOR 암호화/복호화
std::vector<unsigned char> XorCrypt(
    const std::vector<unsigned char>& data,
    const std::string& key)
{
    std::vector<unsigned char> result;
    result.reserve(data.size());

    for (size_t i = 0; i < data.size(); i++) {
        unsigned char keyByte = key[i % key.length()];
        result.push_back(data[i] ^ keyByte);
    }

    return result;
}

// ServerData 编码器
// ServerData encoder
// ServerData 인코더
class ServerDataEncoder {
public:
    // 编码服务器信息
    // Encode server info
    // 서버 정보 인코딩
    static std::string Encode(
        const std::string& ip,
        int port,
        const std::string& version)
    {
        // 创建原始数据字符串
        // Create raw data string
        // 원시 데이터 문자열 생성
        std::ostringstream oss;
        oss << ip << ":" << port << ":" << version;
        std::string rawData = oss.str();

        // 转换为字节数组
        // Convert to byte array
        // 바이트 배열로 변환
        std::vector<unsigned char> dataBytes(rawData.begin(), rawData.end());

        // XOR 加密
        // XOR encrypt
        // XOR 암호화
        std::vector<unsigned char> encrypted =
            XorCrypt(dataBytes, SECRET_KEY);

        // Base64 编码
        // Base64 encode
        // Base64 인코딩
        return Base64Encode(encrypted);
    }

    // 解码服务器信息
    // Decode server info
    // 서버 정보 디코딩
    static bool Decode(
        const std::string& serverData,
        std::string& ip,
        int& port,
        std::string& version)
    {
        try {
            // Base64 解码
            // Base64 decode
            // Base64 디코딩
            std::vector<unsigned char> decoded = Base64Decode(serverData);

            // XOR 解密
            // XOR decrypt
            // XOR 복호화
            std::vector<unsigned char> decrypted =
                XorCrypt(decoded, SECRET_KEY);

            // 转换为字符串
            // Convert to string
            // 문자열로 변환
            std::string rawData(decrypted.begin(), decrypted.end());

            // 解析 IP:PORT:VERSION
            // Parse IP:PORT:VERSION
            // IP:PORT:VERSION 파싱
            std::istringstream iss(rawData);
            std::string portStr;

            if (!std::getline(iss, ip, ':') ||
                !std::getline(iss, portStr, ':') ||
                !std::getline(iss, version, ':')) {
                return false;
            }

            port = std::stoi(portStr);
            return true;

        } catch (...) {
            return false;
        }
    }

private:
    // ⚠️ 警告: 这个密钥是示例，实际密钥需要逆向工程获取
    // ⚠️ Warning: This key is an example, actual key needs RE
    // ⚠️ 경고: 이 키는 예시이며, 실제 키는 역공학 필요
    static constexpr const char* SECRET_KEY = "LineageSecretKey2024";
};

// 使用示例
// Usage example
// 사용 예시
void Example_ServerDataEncoding() {
    // 编码服务器信息
    // Encode server info
    // 서버 정보 인코딩
    std::string serverData = ServerDataEncoder::Encode(
        "192.168.1.100",    // IP 地址 / IP Address / IP 주소
        2000,                // 端口 / Port / 포트
        "TW13081901"        // 版本 / Version / 버전
    );

    // serverData 现在可以保存到 Login.ini
    // serverData can now be saved to Login.ini
    // serverData를 이제 Login.ini에 저장 가능
    // [Server1]
    // ServerName=TestServer
    // ServerData=<serverData value here>

    // 解码服务器信息
    // Decode server info
    // 서버 정보 디코딩
    std::string ip;
    int port;
    std::string version;

    if (ServerDataEncoder::Decode(serverData, ip, port, version)) {
        // 成功解码
        // Successfully decoded
        // 성공적으로 디코딩
        // ip = "192.168.1.100"
        // port = 2000
        // version = "TW13081901"
    }
}
```

---

## 10. L1J-Remastered 프로젝트 적용 (Application to L1J-Remastered / L1J-Remastered项目应用)

### 10.1 커스텀 런처 개발 방향 (Custom Launcher Development Direction / 自定义启动器开发方向)

**프로젝트 구조 권장사항:**

```
L1R-CustomLauncher/
├── src/
│   ├── Launcher/
│   │   ├── MainWindow.xaml          # WPF 主界面 / Main UI / 메인 UI
│   │   ├── MainWindow.xaml.cs
│   │   ├── ServerListControl.xaml   # 服务器列表 / Server list / 서버 리스트
│   │   └── SettingsControl.xaml     # 设置界面 / Settings / 설정
│   ├── Core/
│   │   ├── ConfigManager.cs         # 配置管理 / Config / 설정 관리
│   │   ├── ServerDataEncoder.cs     # ServerData 加密 / Encryption / 암호화
│   │   ├── ProcessLauncher.cs       # 进程启动 / Launch / 프로세스 시작
│   │   └── MemoryInjector.cs        # 内存注入 / Injection / 메모리 주입
│   ├── Models/
│   │   ├── ServerInfo.cs            # 服务器信息模型 / Model / 모델
│   │   └── LauncherConfig.cs        # 启动器配置 / Config / 설정
│   └── Utils/
│       ├── Base64Util.cs            # Base64 工具 / Utility / 유틸
│       ├── XorCrypto.cs             # XOR 加密 / Crypto / 암호화
│       └── NetworkUtil.cs           # 网络工具 / Network / 네트워크
├── config/
│   ├── Login.ini                    # 服务器列表 / Server list / 서버 리스트
│   ├── Login.cfg                    # 启动器设置 / Settings / 설정
│   └── Update.ini                   # 更新配置 / Update / 업데이트
├── resources/
│   ├── skin/                        # 界面皮肤 / UI Skin / UI 스킨
│   └── icons/                       # 图标 / Icons / 아이콘
└── docs/
    ├── ARCHITECTURE.md              # 架构文档 / Architecture / 아키텍처
    └── IMPLEMENTATION.md            # 实现指南 / Implementation / 구현 가이드
```

### 10.2 구현 우선순위 (Implementation Priority / 实现优先级)

**阶段 1: 基础功能 (Phase 1: Basic Features / 1단계: 기본 기능)**

1. ✅ 配置文件读取 (Config file reading / 설정 파일 읽기)
   - Login.ini 파싱
   - ServerData 복호화
   - 서버 리스트 로드

2. ✅ 服务器列表显示 (Server list display / 서버 리스트 표시)
   - WPF 데이터그리드
   - 서버 상태 표시 (온라인/오프라인)
   - 서버 선택 기능

3. ✅ 基础启动功能 (Basic launch / 기본 실행 기능)
   - CreateProcess로 Lin.bin 시작
   - 기본 에러 처리

**阶段 2: 高级功能 (Phase 2: Advanced Features / 2단계: 고급 기능)**

4. ⚠️ 内存注入 (Memory injection / 메모리 주입)
   - IP/포트 메모리 패치
   - 버전 문자열 주입
   - ⚠️ 역공학 필요 (오프셋 확인)

5. ✅ 自动更新系统 (Auto-update system / 자동 업데이트 시스템)
   - Update.ini 체크
   - 패치 다운로드
   - 파일 적용

6. ✅ 服务器状态检测 (Server status check / 서버 상태 확인)
   - TCP 포트 핑
   - 연결 가능 여부 확인
   - UI 상태 업데이트

**阶段 3: 优化和完善 (Phase 3: Polish / 3단계: 완성도 향상)**

7. ✅ 用户界面优化 (UI polish / UI 개선)
   - 커스텀 스킨 지원
   - 애니메이션 효과
   - 현대적인 디자인

8. ✅ 错误处理 (Error handling / 에러 처리)
   - 상세한 에러 메시지
   - 로그 파일 생성
   - 크래시 리포팅

9. ✅ 多语言支持 (Multi-language / 다국어 지원)
   - 한국어
   - 中文
   - English

### 10.3 서버 측 설정 (Server-Side Configuration / 服务器端配置)

**L1JR-Server/config/server.properties:**

```properties
# =====================================================
# L1J-Remastered Server Configuration
# 服务器配置
# 서버 설정
# =====================================================

# Network Settings / 网络设置 / 네트워크 설정
# -------------------------------------------------
GameserverHostname=*
GameserverPort=2000
LoginserverHostname=*
LoginserverPort=2000

# 客户端连接应使用此端口
# Clients should connect to this port
# 클라이언트는 이 포트로 연결해야 함

# Database Settings / 数据库设置 / 데이터베이스 설정
# -------------------------------------------------
Driver=com.mysql.jdbc.Driver
URL=jdbc:mysql://localhost/l1j_remastered?useUnicode=true&characterEncoding=utf8&autoReconnect=true
Login=root
Password=your_password_here

# Time Zone / 时区 / 시간대
# -------------------------------------------------
TimeZone=Asia/Seoul

# Client Version / 客户端版本 / 클라이언트 버전
# -------------------------------------------------
# 支持的客户端版本
# Supported client versions
# 지원되는 클라이언트 버전
ClientVersion=220121

# Connector Settings / 连接器设置 / 커넥터 설정
# -------------------------------------------------
# xnetwork 包中的连接器配置
# Connector configuration in xnetwork package
# xnetwork 패키지의 커넥터 설정
UseConnector=true
ConnectorPort=3000

# Thread Pool / 线程池 / 스레드 풀
# -------------------------------------------------
# 总 Java 线程数
# Total Java threads
# 총 Java 스레드 수
SCHEDULED_CORE_POOLSIZE=512

# <500 用户: 512
# 500+ 用户: 1024
# <500 users: 512
# 500+ users: 1024
# <500명 사용자: 512
# 500명 이상: 1024

# Pool Type / 池类型 / 풀 타입
# 1 = normal Thread
# 2 = fixed size (推荐 / recommended / 권장)
# 3 = cached
THREAD_POOL_TYPE=2
```

### 10.4 클라이언트 버전 체크 (Client Version Check / 客户端版本检查)

**서버 측 구현 (L1JR-Server):**

```java
// =====================================================
// 客户端版本检查
// Client Version Check
// 클라이언트 버전 확인
// =====================================================

package l1j.server.server;

public class ClientVersionChecker {

    // 支持的客户端版本列表
    // Supported client versions list
    // 지원되는 클라이언트 버전 목록
    private static final String[] SUPPORTED_VERSIONS = {
        "220121",      // 리마스터 버전 / Remaster version / 重制版版本
        "TW13081901",  // 台湾版本 / Taiwan version / 대만 버전
        "351_S3DS2X",  // 旧版本 / Old version / 구버전
        "351_S3DS2Y"   // 旧版本 / Old version / 구버전
    };

    /**
     * 检查客户端版本是否受支持
     * Check if client version is supported
     * 클라이언트 버전이 지원되는지 확인
     */
    public static boolean isVersionSupported(String clientVersion) {
        if (clientVersion == null || clientVersion.isEmpty()) {
            return false;
        }

        for (String version : SUPPORTED_VERSIONS) {
            if (version.equals(clientVersion)) {
                return true;
            }
        }

        return false;
    }

    /**
     * 处理客户端版本检查数据包
     * Handle client version check packet
     * 클라이언트 버전 확인 패킷 처리
     */
    public static void handleVersionCheck(
        GameClient client,
        String clientVersion)
    {
        // 记录客户端版本
        // Log client version
        // 클라이언트 버전 로그
        _log.info("Client version check: " + clientVersion +
                  " from " + client.getHostname());

        // 检查版本
        // Check version
        // 버전 확인
        if (!isVersionSupported(clientVersion)) {
            // 发送版本不匹配消息
            // Send version mismatch message
            // 버전 불일치 메시지 전송
            client.sendPacket(new S_Disconnect(
                "您的客户端版本不受支持\n" +
                "Your client version is not supported\n" +
                "클라이언트 버전이 지원되지 않습니다"
            ));

            // 断开连接
            // Disconnect
            // 연결 끊기
            client.close();
            return;
        }

        // 版本匹配，继续认证流程
        // Version matched, continue authentication
        // 버전 일치, 인증 계속 진행
        client.setClientVersion(clientVersion);
        proceedToAuthentication(client);
    }
}
```

**런처 측 구현 (L1R-CustomLauncher):**

```csharp
// =====================================================
// 版本注入器
// Version Injector
// 버전 주입기
// =====================================================

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace L1R.CustomLauncher.Core
{
    public class VersionInjector
    {
        // Windows API 导入
        // Windows API imports
        // Windows API 가져오기
        [DllImport("kernel32.dll")]
        private static extern bool WriteProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            byte[] lpBuffer,
            int nSize,
            out int lpNumberOfBytesWritten);

        // ⚠️ 警告: 这个偏移需要通过逆向工程确定
        // ⚠️ Warning: This offset needs to be determined via RE
        // ⚠️ 경고: 이 오프셋은 역공학으로 확인 필요
        private const int VERSION_STRING_OFFSET = 0x00000000; // 示例 / Example / 예시

        /// <summary>
        /// 注入客户端版本字符串
        /// Inject client version string
        /// 클라이언트 버전 문자열 주입
        /// </summary>
        public static bool InjectVersion(
            IntPtr processHandle,
            string version)
        {
            try {
                // 将版本字符串转换为字节数组
                // Convert version string to byte array
                // 버전 문자열을 바이트 배열로 변환
                byte[] versionBytes = Encoding.ASCII.GetBytes(version + "\0");

                // 写入进程内存
                // Write to process memory
                // 프로세스 메모리에 쓰기
                int bytesWritten;
                bool success = WriteProcessMemory(
                    processHandle,
                    new IntPtr(VERSION_STRING_OFFSET),
                    versionBytes,
                    versionBytes.Length,
                    out bytesWritten
                );

                return success && bytesWritten == versionBytes.Length;
            }
            catch (Exception ex) {
                Logger.Error($"版本注入失败: {ex.Message}");
                return false;
            }
        }
    }
}
```

### 10.5 연결 프로세스 구현 (Connection Process Implementation / 连接流程实现)

**完整的连接流程:**

```csharp
// =====================================================
// 完整的游戏启动和连接流程
// Complete game launch and connection process
// 완전한 게임 시작 및 연결 프로세스
// =====================================================

using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace L1R.CustomLauncher.Core
{
    public class GameLauncher
    {
        private ServerInfo _server;
        private string _clientPath;

        public GameLauncher(ServerInfo server, string clientPath)
        {
            _server = server;
            _clientPath = clientPath;
        }

        /// <summary>
        /// 启动游戏的完整流程
        /// Complete game launch process
        /// 게임 시작의 완전한 프로세스
        /// </summary>
        public async Task<bool> LaunchAsync()
        {
            try {
                // 步骤 1: 检查服务器在线状态
                // Step 1: Check server online status
                // 1단계: 서버 온라인 상태 확인
                if (!await CheckServerOnlineAsync()) {
                    ShowError("服务器离线或无法访问\n" +
                             "Server is offline or unreachable\n" +
                             "서버가 오프라인이거나 접근 불가");
                    return false;
                }

                // 步骤 2: 验证客户端文件
                // Step 2: Verify client files
                // 2단계: 클라이언트 파일 확인
                if (!VerifyClientFiles()) {
                    ShowError("客户端文件损坏或缺失\n" +
                             "Client files are corrupted or missing\n" +
                             "클라이언트 파일이 손상되었거나 누락됨");
                    return false;
                }

                // 步骤 3: 创建进程
                // Step 3: Create process
                // 3단계: 프로세스 생성
                Process process = CreateSuspendedProcess();
                if (process == null) {
                    ShowError("无法启动游戏客户端\n" +
                             "Failed to start game client\n" +
                             "게임 클라이언트 시작 실패");
                    return false;
                }

                // 步骤 4: 注入服务器信息
                // Step 4: Inject server info
                // 4단계: 서버 정보 주입
                if (!InjectServerInfo(process)) {
                    process.Kill();
                    ShowError("无法注入服务器信息\n" +
                             "Failed to inject server info\n" +
                             "서버 정보 주입 실패");
                    return false;
                }

                // 步骤 5: 恢复进程
                // Step 5: Resume process
                // 5단계: 프로세스 재개
                ResumeProcess(process);

                // 步骤 6: 监控连接
                // Step 6: Monitor connection
                // 6단계: 연결 모니터링
                bool connected = await MonitorConnectionAsync(process);
                if (!connected) {
                    ShowError("连接服务器超时\n" +
                             "Server connection timeout\n" +
                             "서버 연결 시간 초과");
                    return false;
                }

                // 成功！
                // Success!
                // 성공!
                return true;
            }
            catch (Exception ex) {
                Logger.Error($"启动失败: {ex.Message}");
                ShowError($"启动失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 检查服务器是否在线
        /// Check if server is online
        /// 서버가 온라인인지 확인
        /// </summary>
        private async Task<bool> CheckServerOnlineAsync()
        {
            try {
                using (var client = new TcpClient()) {
                    // 尝试连接，5秒超时
                    // Try to connect, 5 second timeout
                    // 연결 시도, 5초 타임아웃
                    var connectTask = client.ConnectAsync(_server.Ip, _server.Port);
                    var timeoutTask = Task.Delay(5000);

                    var completedTask = await Task.WhenAny(connectTask, timeoutTask);

                    if (completedTask == connectTask && !connectTask.IsFaulted) {
                        return true;
                    }
                }
            }
            catch { }

            return false;
        }

        /// <summary>
        /// 验证客户端文件完整性
        /// Verify client file integrity
        /// 클라이언트 파일 무결성 확인
        /// </summary>
        private bool VerifyClientFiles()
        {
            // 检查必需的文件
            // Check required files
            // 필수 파일 확인
            string[] requiredFiles = {
                "Lin.bin",
                "data.pak",
                "icon.pak",
                "ui.pak",
                "tile.pak"
            };

            foreach (var file in requiredFiles) {
                string fullPath = Path.Combine(
                    Path.GetDirectoryName(_clientPath),
                    file
                );

                if (!File.Exists(fullPath)) {
                    Logger.Error($"缺少文件: {file}");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 创建暂停的进程
        /// Create suspended process
        /// 일시 중지된 프로세스 생성
        /// </summary>
        private Process CreateSuspendedProcess()
        {
            var startInfo = new ProcessStartInfo {
                FileName = _clientPath,
                WorkingDirectory = Path.GetDirectoryName(_clientPath),
                UseShellExecute = false,
                CreateNoWindow = false
            };

            // 注意: C# Process 类不直接支持 CREATE_SUSPENDED
            // 需要使用 P/Invoke 调用 CreateProcess
            // Note: C# Process class doesn't directly support CREATE_SUSPENDED
            // Need to use P/Invoke to call CreateProcess
            // 참고: C# Process 클래스는 CREATE_SUSPENDED를 직접 지원하지 않음
            // CreateProcess를 호출하기 위해 P/Invoke 사용 필요

            return ProcessCreator.CreateSuspended(startInfo);
        }

        /// <summary>
        /// 注入服务器信息到进程
        /// Inject server info into process
        /// 프로세스에 서버 정보 주입
        /// </summary>
        private bool InjectServerInfo(Process process)
        {
            try {
                var injector = new MemoryInjector(process.Handle);

                // 注入 IP 地址
                // Inject IP address
                // IP 주소 주입
                if (!injector.InjectIpAddress(_server.Ip)) {
                    return false;
                }

                // 注入端口
                // Inject port
                // 포트 주입
                if (!injector.InjectPort(_server.Port)) {
                    return false;
                }

                // 注入版本
                // Inject version
                // 버전 주입
                if (!injector.InjectVersion(_server.Version)) {
                    return false;
                }

                return true;
            }
            catch (Exception ex) {
                Logger.Error($"注入失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 恢复进程执行
        /// Resume process execution
        /// 프로세스 실행 재개
        /// </summary>
        private void ResumeProcess(Process process)
        {
            ProcessCreator.ResumeThread(process);
        }

        /// <summary>
        /// 监控客户端连接状态
        /// Monitor client connection status
        /// 클라이언트 연결 상태 모니터링
        /// </summary>
        private async Task<bool> MonitorConnectionAsync(Process process)
        {
            // 等待最多 30 秒
            // Wait up to 30 seconds
            // 최대 30초 대기
            for (int i = 0; i < 30; i++) {
                await Task.Delay(1000);

                // 检查进程是否还在运行
                // Check if process is still running
                // 프로세스가 여전히 실행 중인지 확인
                if (process.HasExited) {
                    Logger.Error("客户端进程意外退出");
                    return false;
                }

                // TODO: 实现更高级的连接检测
                // 例如检查内存中的连接状态标志
                // TODO: Implement more advanced connection detection
                // e.g., check connection status flag in memory
                // TODO: 더 고급 연결 감지 구현
                // 예: 메모리의 연결 상태 플래그 확인
            }

            // 假设 30 秒后仍在运行表示成功连接
            // Assume still running after 30 seconds means successful connection
            // 30초 후에도 실행 중이면 연결 성공으로 간주
            return !process.HasExited;
        }

        private void ShowError(string message)
        {
            System.Windows.MessageBox.Show(
                message,
                "启动器错误 / Launcher Error / 런처 오류",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error
            );
        }
    }
}
```

---

## 11. 结论 (Conclusion / 결론)

### 11.1 主要发现 (Key Findings / 주요 발견 사항)

**한국어 커뮤니티:**

1. **런처 중심 접근**
   - Lin.bin은 직접 커맨드라인 파라미터를 받지 않음
   - 대신 커스텀 런처(접속기)가 메모리 주입이나 설정 파일을 통해 서버 정보 전달
   - Ctool, 린툴 등의 도구가 일반적으로 사용됨

2. **메모리 패치 기술**
   - CreateProcess로 일시 중지 상태로 프로세스 생성
   - WriteProcessMemory로 IP/포트 주입
   - 특정 메모리 오프셋은 역공학 필요

3. **법적 위험**
   - 한국에서 사설 서버는 불법
   - 5년 징역 또는 5천만원 벌금
   - 학술 목적 연구만 허용

**中文社区:**

1. **编码器系统**
   - Encode.exe 用于生成 Login.ini 配置
   - ServerData 字段包含加密的服务器信息
   - Base64 + XOR 加密方案

2. **登入器架构**
   - Login.exe 读取 Login.ini 和 Login.cfg
   - 支持多服务器列表 (Server1-Server8)
   - 自动更新系统通过 Update.ini

3. **版本兼容性**
   - 351_S3DS2X, 351_S3DS2Y, TW13081901 等版本
   - 客户端和服务器版本必须匹配
   - LinHelperZ.txt 提供内置辅助宏

### 11.2 缺失的信息 (Missing Information / 누락된 정보)

**需要逆向工程确定:**

1. **内存偏移**
   - IP 地址存储位置
   - 端口号存储位置
   - 版本字符串位置
   - 这些偏移随客户端版本变化

2. **加密密钥**
   - ServerData 的实际加密密钥
   - XOR 密钥字符串
   - 可能使用更复杂的加密算法

3. **数据包格式**
   - 版本检查数据包结构
   - 认证数据包格式
   - 会话密钥生成算法

**需要进一步研究:**

1. 新版本客户端 (220121 Remaster)
2. 官方 NC Launcher 的工作原理
3. 游戏守护 (GameGuard) 绕过技术
4. PAK 文件加密和客户端补丁

### 11.3 后续步骤 (Next Steps / 다음 단계)

**对于 L1R-CustomLauncher 项目:**

1. **阶段 1: 基础实现**
   - ✅ 实现 Login.ini 解析
   - ✅ 实现 ServerData 解密
   - ✅ 创建基础 WPF UI
   - ✅ 实现简单的进程启动

2. **阶段 2: 逆向工程**
   - ⚠️ 使用 Ghidra 分析 Lin.bin
   - ⚠️ 找到 IP/端口内存偏移
   - ⚠️ 确定版本检查机制
   - ⚠️ 实现内存注入

3. **阶段 3: 完善功能**
   - ⚠️ 实现自动更新系统
   - ⚠️ 添加服务器状态检测
   - ⚠️ 支持多语言界面
   - ⚠️ 添加错误报告系统

4. **阶段 4: 测试和文档**
   - ⚠️ 全面测试各种场景
   - ⚠️ 编写用户文档
   - ⚠️ 创建开发者文档
   - ⚠️ 记录已知问题

### 11.4 法律声明 (Legal Disclaimer / 법적 고지)

```
⚠️⚠️⚠️ 重要法律声明 / IMPORTANT LEGAL DISCLAIMER / 중요 법적 고지 ⚠️⚠️⚠️

本研究报告仅用于学术和教育目的。
This research report is for academic and educational purposes only.
이 연구 보고서는 학술 및 교육 목적으로만 사용됩니다.

在大多数司法管辖区，运营或协助运营未经授权的游戏服务器是非法的，
可能违反版权法、计算机欺诈法和其他法律。
In most jurisdictions, operating or assisting in the operation of
unauthorized game servers is illegal and may violate copyright laws,
computer fraud laws, and other legislation.
대부분의 관할 지역에서 무단 게임 서버를 운영하거나 운영을 지원하는 것은
불법이며 저작권법, 컴퓨터 사기법 및 기타 법률을 위반할 수 있습니다.

作者和贡献者不对本信息的任何误用或非法使用负责。
The author and contributors are not responsible for any misuse or
illegal use of this information.
저자와 기여자는 이 정보의 오용이나 불법적 사용에 대해 책임지지 않습니다.

使用本研究中描述的任何技术或工具的风险由您自己承担。
Use of any techniques or tools described in this research is at your own risk.
이 연구에 설명된 모든 기술이나 도구의 사용은 귀하의 책임입니다.

请始终尊重知识产权并遵守当地法律。
Always respect intellectual property rights and comply with local laws.
항상 지적 재산권을 존중하고 현지 법률을 준수하십시오.
```

---

## 12. 참고 자료 (References / 参考资料)

### 12.1 한국어 출처 (Korean Sources)

**커뮤니티 포럼:**
1. 리니지연구소 - https://linlab3.com/
2. 투데이서버 - https://todayserver.net/
3. 리니지 인벤 - https://www.inven.co.kr/board/lineage/
4. 번개서버 - http://bgserver.live/
5. 노딤 클래스 - https://nodim1.xyz/

**기술 블로그:**
1. "리니지 접속기 제작 프로그램 CtoolNt" - SKYDC
2. "구버전 구축방법[1.63.버전]" - TopServer
3. "초보자 리니지 서버 구축 방법" - 리니지연구소

**공식 문서:**
1. 리니지 파워북 - https://lineage.plaync.com/powerbook/
2. 리니지 다운로드 - https://lineage.plaync.com/download/

### 12.2 中文来源 (Chinese Sources)

**社区论坛:**
1. 45天堂私服论坛 - https://lineage45.com/
2. 天堂单机论坛 - https://www.l2tw.com/
3. 155游戏天堂私服论坛 - https://game155.com/

**技术博客:**
1. J.J.'s Blogs (MoroseDog) - https://morosedog.gitlab.io/
   - 登入器简介
   - 登入器功能说明
   - 登入器设定档说明
   - 客户端和登入器关系分析
2. "天堂Lineage(单机版)从零开始架设教学" - jeremyatchina
3. "天堂1，363版本通用登陆器的全面分析与实践" - CSDN

**工具和软件:**
1. LinHelper / LH2 / Login PRO - https://www.linhelper.com/
2. LinPRO_2016 - http://www.linpro.idv.tw/

### 12.3 기술 문서 (Technical Documentation)

**Windows API:**
1. CreateProcess Function - Microsoft Learn
2. WriteProcessMemory Function - Microsoft Learn
3. Process and Thread Functions - Microsoft Learn

**암호화/복호화:**
1. Base64 Encoding/Decoding
2. XOR Encryption
3. 对称加密算法

### 12.4 관련 프로젝트 (Related Projects)

**오픈소스 에뮬레이터:**
1. L1J (Lineage Java Emulator)
2. L1J-TW (Taiwan version)
3. L1J-Remastered (본 프로젝트 / This project / 本项目)

**역공학 도구:**
1. Ghidra - NSA's reverse engineering tool
2. IDA Pro
3. x64dbg / OllyDbg

---

## 附录 A: 术语对照表 (Appendix A: Terminology / 부록 A: 용어 대조표)

| 한국어 | English | 中文 | 설명 |
|--------|---------|------|------|
| 접속기 | Launcher | 登入器 | 游戏启动器程序 |
| 런처 | Launcher | 启动器 | 同上 |
| 사설 서버 | Private Server | 私服 | 非官方游戏服务器 |
| 프리서버 | Free Server | 免费服务器 | 同上 |
| 메모리 주입 | Memory Injection | 内存注入 | 修改进程内存 |
| 패치 | Patch | 补丁 | 更新文件 |
| 클라이언트 | Client | 客户端 | 游戏客户端程序 |
| 서버 | Server | 服务器 | 游戏服务器 |
| 암호화 | Encryption | 加密 | 数据加密 |
| 복호화 | Decryption | 解密 | 数据解密 |
| 설정 파일 | Config File | 配置文件 | 配置文件 |
| 실행 파일 | Executable | 可执行文件 | .exe 或 .bin 文件 |
| 프로세스 | Process | 进程 | 操作系统进程 |
| 포트 | Port | 端口 | 网络端口 |
| 버전 | Version | 版本 | 软件版本 |
| 인코딩 | Encoding | 编码 | 数据编码 |
| 디코딩 | Decoding | 解码 | 数据解码 |

---

## 附录 B: 常见错误代码 (Appendix B: Error Codes / 부록 B: 오류 코드)

| 错误代码 | 한국어 메시지 | English Message | 中文消息 | 原因 |
|---------|--------------|-----------------|---------|------|
| ERR_001 | 실행 파일을 찾을 수 없습니다 | Executable not found | 找不到可执行文件 | Lin.bin 缺失 |
| ERR_002 | 서버에 연결할 수 없습니다 | Cannot connect to server | 无法连接到服务器 | IP/端口错误 |
| ERR_003 | 버전이 일치하지 않습니다 | Version mismatch | 版本不匹配 | 客户端版本错误 |
| ERR_004 | 메모리 주입 실패 | Memory injection failed | 内存注入失败 | 权限不足 |
| ERR_005 | 설정 파일 오류 | Config file error | 配置文件错误 | Login.ini 损坏 |
| ERR_006 | 업데이트 실패 | Update failed | 更新失败 | 网络问题 |
| ERR_007 | 인증 실패 | Authentication failed | 认证失败 | 账号密码错误 |
| ERR_008 | 서버가 오프라인입니다 | Server is offline | 服务器离线 | 服务器未运行 |
| ERR_009 | 클라이언트 손상 | Client corrupted | 客户端损坏 | PAK 文件损坏 |
| ERR_010 | 관리자 권한 필요 | Admin rights required | 需要管理员权限 | 需要提权 |

---

## 附录 C: 配置文件模板 (Appendix C: Config Templates / 부록 C: 설정 파일 템플릿)

### Login.ini 模板

```ini
; =====================================================
; Lineage Launcher Server List Configuration
; 天堂启动器服务器列表配置
; 리니지 런처 서버 리스트 설정
; =====================================================

[Server1]
ServerName=Test Server 1
ServerData=LB0yci4fLho0ADAeLxY4CTgPIGA8Dj4OPhFRDT0SUg4+EVENPQ09ElIO

[Server2]
ServerName=Test Server 2
ServerData=LB0yci4fLho0ADAeLxY4CTgPIGA8Dj4OPhFRDT0SUg4+EVENPQ09ElIO

[Server3]
ServerName=Development Server
ServerData=LB0yci4fLho0ADAeLxY4CTgPIGA8Dj4OPhFRDT0SUg4+EVENPQ09ElIO

; 最多支持 8 个服务器
; Up to 8 servers supported
; 최대 8개 서버 지원
```

### Login.cfg 模板

```ini
; =====================================================
; Launcher Settings Configuration
; 启动器设置配置
; 런처 설정 구성
; =====================================================

[Setting]
BeanfunLogin=0
EatFileName=eat.exe
UpdateInfo=

[Hyperlink]
HyperlinkText1=官方网站 / Official Site / 공식 사이트
HyperlinkHttp1=http://yourserver.com
HyperlinkTextColor1=0xFFE7C6

HyperlinkText2=论坛 / Forum / 포럼
HyperlinkHttp2=http://forum.yourserver.com
HyperlinkTextColor2=0xFFE7C6

HyperlinkText3=Discord
HyperlinkHttp3=https://discord.gg/yourserver
HyperlinkTextColor3=0xFFE7C6

[Background]
Enable=1
Bulletin=skin\Bulletin.bmp
Serverlist=skin\Serverlist.bmp
Button=skin\Button.bmp
ButtonTextColor=0xFFFFFF
```

### Update.ini 模板

```ini
; =====================================================
; Auto-Update Configuration
; 自动更新配置
; 자동 업데이트 설정
; =====================================================

[Update]
Version=1
1=initial_patch.zip

; 增加新补丁时递增 Version 并添加新行
; Increment Version and add new line when adding patches
; 새 패치 추가 시 Version 증가 및 새 줄 추가
```

---

**研究完成日期 (Research Completed / 연구 완료 날짜):** 2025-01-11
**文档版本 (Document Version / 문서 버전):** 1.0
**作者 (Author / 저자):** Claude (Anthropic) with User Research Direction

---

**END OF DOCUMENT / 文档结束 / 문서 끝**
