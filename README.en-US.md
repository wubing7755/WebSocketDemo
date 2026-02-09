# WebSocket Demo (Blazor WebAssembly)

![Blazor WASM](https://img.shields.io/badge/Blazor-WebAssembly-blueviolet)
![.NET](https://img.shields.io/badge/.NET-6.0%2B-blue)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![EN](https://img.shields.io/badge/Language-English-blue)](README.en-US.md)
[![CN](https://img.shields.io/badge/语言-中文-red)](README.md)

A real-time chat demo project based on **Blazor WebAssembly (Hosted) + WebSocket**, demonstrating how to implement WebSocket communication in **.NET / Blazor** applications.

---

## ✨ Features

* 🔌 Real-time communication via WebSocket
* 💬 Multi-user real-time chat room
* 👤 User join / leave notifications
* 📜 Message history (browser local storage)
* 🔄 Automatic reconnection mechanism
* 🟢 Online user list and status synchronization

---

## 🏗 Project Structure

```
WebSocketDemo/
├── Client/        # Blazor WebAssembly client
├── Server/        # ASP.NET Core WebSocket server
└── Shared/        # Shared models
```

---

## 🧩 Tech Stack

* .NET 6 / ASP.NET Core
* Blazor WebAssembly (Hosted)
* WebSocket
* JSON serialization
* Browser LocalStorage

---

## 🚀 Getting Started

### 1️⃣ Clone the repository

```bash
git clone https://github.com/yourname/WebSocketDemo.git
cd WebSocketDemo
```

### 2️⃣ Run the server

```bash
dotnet run --project Server
```

### 3️⃣ Open in browser

```
https://localhost:7235 or http://localhost:5015
```

> Open multiple browser tabs or windows to simulate multiple users chatting.

---

## 🖥 Usage

1. Enter a username (customizable)
2. Connect to the WebSocket server
3. Send messages and broadcast them to all online users in real time
4. View the online user list and system notifications
5. Refresh the page to automatically load message history

---

## 🧠 Core Implementation Overview

### WebSocket Server

* Uses ASP.NET Core middleware to handle WebSocket connections
* Centralized management of online users and connection lifecycle
* Supports message broadcasting and user status notifications

### Client Communication

* Uses `ClientWebSocket`
* Automatic message serialization / deserialization
* Built-in reconnection and error handling

### Data Persistence

* Uses browser LocalStorage
* Automatically stores recent chat messages
* Supports clearing local history

---

## 📂 Key Files

| File                     | Description                   |
| ------------------------ | ----------------------------- |
| `ChatRoom.razor`         | Chat room main UI             |
| `WebSocketService.cs`    | Client-side WebSocket service |
| `WebSocketMiddleware.cs` | Server-side WebSocket handler |
| `ChatMessage.cs`         | Chat message model            |

---

## 🧪 Testing Suggestions

* Single-user and multi-user chat
* User join / leave notifications
* Automatic reconnection after network interruption
* Cross-browser compatibility testing

---

## 🔒 Notes

* This project is intended for learning and demonstration purposes
* User authentication and authorization are not implemented
* Enable HTTPS and enhance security settings for production use

---

## 📄 License

MIT
