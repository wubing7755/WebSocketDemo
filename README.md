
# WebSocket Demo 项目实现文档

## 项目概述

这是一个基于 Blazor WebAssembly 框架的 WebSocket 示例项目，旨在演示如何使用 WebSocket 协议实现实时聊天室功能。项目采用典型的 Blazor WebAssembly 托管架构，包含三个主要部分：

- **Client** (`WebSocketDemo.Client`)：Blazor WebAssembly 客户端应用
- **Server** (`WebSocketDemo.Server`)：ASP.NET Core 服务器端应用
- **Shared** (`WebSocketDemo.Shared`)：共享模型和组件

## 当前项目状态

✅ **已完成**
- 基础项目结构搭建
- Blazor WebAssembly 托管项目模板
- 示例页面（主页、计数器、天气预报）

⬜ **待实现**
- WebSocket 聊天室功能

## 功能实现步骤

### 第 1 步：创建共享数据模型
- [x] 在 `Shared` 项目中创建聊天消息模型
- [x] 创建用户连接信息模型
- [x] 定义 WebSocket 消息类型枚举

#### 功能要求
1. **聊天消息模型 (`ChatMessage.cs`)**
   - 包含以下属性：
     - `MessageId` (Guid)：消息唯一标识符
     - `UserId` (string)：发送者用户ID
     - `UserName` (string)：发送者昵称
     - `Content` (string)：消息内容
     - `Timestamp` (DateTime)：消息发送时间
     - `MessageType` (MessageType)：消息类型（文本、系统通知等）

2. **用户连接信息模型 (`UserInfo.cs`)**
   - 包含以下属性：
     - `UserId` (string)：用户唯一标识符
     - `UserName` (string)：用户昵称
     - `ConnectionId` (string)：WebSocket连接ID
     - `IsOnline` (bool)：用户在线状态
     - `LastActiveTime` (DateTime)：最后活动时间

3. **WebSocket消息类型枚举 (`MessageType.cs`)**
   - 定义以下消息类型：
     - `TextMessage`：普通文本消息
     - `UserJoined`：用户加入聊天室
     - `UserLeft`：用户离开聊天室
     - `SystemNotification`：系统通知
     - `Error`：错误消息

#### 注意事项
- 所有模型类都应包含适当的构造函数和属性初始化
- 考虑使用 `[Serializable]` 特性以支持JSON序列化
- 模型设计应简洁，避免过度复杂化，满足基本聊天室需求即可

### 第 2 步：实现服务器端 WebSocket 处理器
- [x] 在 `Server` 项目中创建 WebSocket 中间件或控制器
- [x] 实现连接管理（连接、断开、消息广播）
- [x] 添加 WebSocket 端点配置

#### 功能要求
1. **创建 WebSocket 中间件或控制器**
   - 实现一个 WebSocket 中间件类或控制器类
   - 支持接受 WebSocket 连接请求
   - 处理 WebSocket 握手和协议升级
   - 提供统一的连接入口点

2. **实现连接管理**
   - 维护所有活跃连接的集合
   - 处理新用户连接：验证、创建用户信息、广播加入通知
   - 处理用户断开连接：清理资源、广播离开通知
   - 实现消息广播功能：将消息发送给所有连接用户
   - 支持向特定用户发送消息

3. **添加 WebSocket 端点配置**
   - 在 Program.cs 中配置 WebSocket 中间件或控制器路由
   - 设置 WebSocket 选项（如保持连接时间、缓冲区大小等）
   - 配置 CORS 策略以支持 WebSocket 连接
   - 确保 WebSocket 端点可以通过标准 URL 访问

#### 注意事项
- 使用线程安全的数据结构管理连接集合
- 考虑连接异常处理（网络断开、超时等）
- 实现适当的资源清理（关闭连接、释放资源）
- 添加日志记录以便调试和监控

### 第 3 步：实现客户端 WebSocket 服务
- [x] 在 `Client` 项目中创建 WebSocket 服务类
- [x] 实现连接管理、消息发送和接收
- [x] 添加错误处理和重连机制

#### 功能要求
1. **创建 WebSocket 服务接口 (`IWebSocketService.cs`)**
   - 定义连接状态、消息接收、在线用户更新等事件
   - 包含连接、断开、发送消息、重连等方法
   - 提供当前连接状态、用户信息和在线用户列表属性

2. **创建 WebSocket 服务实现类 (`WebSocketService.cs`)**
   - 实现 `IWebSocketService` 接口
   - 使用 `ClientWebSocket` 类进行底层连接
   - 处理 WebSocket 消息的序列化和反序列化
   - 维护连接状态和在线用户列表

3. **实现连接管理**
   - 支持连接到指定 WebSocket 服务器
   - 自动处理连接建立和断开
   - 维护当前用户信息和连接状态
   - 支持通过查询参数传递用户名

4. **实现消息发送和接收**
   - 支持发送文本消息和 ChatMessage 对象
   - 自动消息序列化为 JSON 格式
   - 持续接收服务器消息（聊天消息和在线用户更新）
   - 解析不同类型的消息并触发相应事件

5. **添加错误处理和重连机制**
   - 实现自动重连机制（最大重连次数：5次）
   - 重连延迟策略（指数退避）
   - 处理 WebSocket 异常和连接中断
   - 提供错误事件通知 UI 层

#### 注意事项
- 使用 `IJSRuntime` 支持 Blazor WebAssembly 环境
- 实现 `IDisposable` 接口以确保资源正确释放
- 使用线程安全的数据结构管理内部状态
- 考虑浏览器环境下 WebSocket 的限制和特性

### 第 4 步：创建聊天室用户界面
- [ ] 设计聊天室主页面布局
- [ ] 实现消息显示区域
- [ ] 添加消息输入和发送控件
- [ ] 创建用户列表显示

### 第 5 步：添加用户身份管理
- [ ] 实现简单的用户昵称输入
- [ ] 添加用户连接/断开状态显示
- [ ] 实现用户列表更新

### 第 6 步：完善功能和优化
- [ ] 添加消息时间戳显示
- [ ] 实现消息历史记录
- [ ] 优化 UI 响应性和用户体验
- [ ] 添加错误处理和用户反馈

### 第 7 步：测试和部署
- [ ] 功能测试（单用户、多用户聊天）
- [ ] 性能测试（连接稳定性、消息延迟）
- [ ] 部署配置和发布

## 技术要点

1. **WebSocket 协议**：实现全双工通信，支持实时消息传递
2. **Blazor WebAssembly**：在浏览器中运行 .NET 代码，无需插件
3. **ASP.NET Core 中间件**：处理 WebSocket 连接请求
4. **响应式 UI**：使用 Blazor 组件实现动态更新

## 文件结构规划

```
WebSocketDemo/
├── Shared/
│   ├── Models/
│   │   ├── ChatMessage.cs
│   │   ├── UserInfo.cs
│   │   └── MessageType.cs
│   └── Services/
│       └── IWebSocketService.cs
├── Server/
│   ├── Controllers/
│   │   └── WebSocketController.cs
│   ├── Middleware/
│   │   └── WebSocketMiddleware.cs
│   └── Services/
│       └── ChatService.cs
└── Client/
    ├── Pages/
    │   ├── ChatRoom.razor
    │   └── ChatRoom.razor.css
    ├── Services/
    │   └── WebSocketService.cs
    └── Shared/
        └── Components/
            └── MessageList.razor
```

## 更新记录

| 日期 | 步骤 | 状态 | 备注 |
|------|------|------|------|
| - | 项目初始化 | ✅ 完成 | 创建基础项目结构 |
| 2026-02-09 | 第 1 步：创建共享数据模型 | ✅ 完成 | 创建了 ChatMessage.cs、UserInfo.cs 和 MessageType.cs 模型 |
| 2026-02-09 | 第 2 步：实现服务器端 WebSocket 处理器 | ✅ 完成 | 创建了 WebSocketMiddleware、ChatService 并更新了 Program.cs 配置 |
| 2026-02-09 | 第 3 步：实现客户端 WebSocket 服务 | ✅ 完成 | 创建了 IWebSocketService 接口和 WebSocketService 实现类 |
| - | 第 4 步：创建聊天室用户界面 | ⬜ 待开始 | |
| - | 第 5 步：添加用户身份管理 | ⬜ 待开始 | |
| - | 第 6 步：完善功能和优化 | ⬜ 待开始 | |
| - | 第 7 步：测试和部署 | ⬜ 待开始 | |

---

*本文件将随着项目进展不断更新。每完成一个实现步骤，都会在此文档中记录进度和实现细节。*
