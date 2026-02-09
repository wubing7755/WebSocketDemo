# WebSocket Demo（Blazor WebAssembly）

![Blazor WASM](https://img.shields.io/badge/Blazor-WebAssembly-blueviolet)
![.NET](https://img.shields.io/badge/.NET-6.0%2B-blue)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![EN](https://img.shields.io/badge/Language-English-blue)](README.en-US.md)
[![CN](https://img.shields.io/badge/语言-中文-red)](README.md)

一个基于 **Blazor WebAssembly（Hosted）WebSocket** 的实时聊天室示例项目，用于演示 WebSocket 在 .NET / Blazor 场景下的实现。

---

## ✨ 功能特性

* 🔌 WebSocket 实时通信
* 💬 实时聊天室（多用户）
* 👤 用户加入 / 离开通知
* 📜 消息历史记录（浏览器本地存储）
* 🔄 自动重连机制
* 🟢 在线用户列表与状态同步

---

## 🏗 项目结构

```
WebSocketDemo/
├── Client/        # Blazor WebAssembly 客户端
├── Server/        # ASP.NET Core WebSocket 服务端
└── Shared/        # 共享模型
```

---

## 🧩 技术栈

* .NET 6 / ASP.NET Core
* Blazor WebAssembly（Hosted）
* WebSocket
* JSON 序列化
* 浏览器 LocalStorage

---

## 🚀 快速开始

### 1️⃣ 克隆项目

```bash
git clone https://github.com/yourname/WebSocketDemo.git
cd WebSocketDemo
```

### 2️⃣ 启动服务端

```bash
dotnet run --project Server
```

### 3️⃣ 打开浏览器

```
https://localhost:7235 or http://localhost:5015
```

> 打开多个浏览器窗口或标签页即可模拟多用户聊天

---

## 🖥 使用说明

1. 输入用户名（自定义）
2. 连接 WebSocket 服务器
3. 发送消息，实时广播给所有在线用户
4. 查看在线用户列表与系统通知
5. 刷新页面后可自动加载历史消息

---

## 🧠 核心实现说明

### WebSocket 服务端

* 使用 ASP.NET Core 中间件处理 WebSocket 连接
* 统一管理在线用户与连接生命周期
* 支持消息广播与用户状态通知

### 客户端通信

* 使用 `ClientWebSocket`
* 自动序列化 / 反序列化消息
* 内置断线重连与错误处理机制

### 数据持久化

* 使用浏览器 LocalStorage
* 自动保存最近聊天记录
* 支持清除历史数据

---

## 📂 关键文件

| 文件                       | 说明               |
| ------------------------ | ---------------- |
| `ChatRoom.razor`         | 聊天室主界面           |
| `WebSocketService.cs`    | 客户端 WebSocket 服务 |
| `WebSocketMiddleware.cs` | 服务端 WebSocket 处理 |
| `ChatMessage.cs`         | 聊天消息模型           |

---

## 🧪 测试建议

* 单用户 / 多用户聊天
* 用户加入 / 离开通知
* 网络断开后的自动重连
* 多浏览器兼容性测试

---

## 🔒 注意事项

* 本项目为学习 / 示例用途
* 未实现用户鉴权与权限控制
* 生产环境请启用 HTTPS 并加强安全配置

---

## 📄 License

MIT
