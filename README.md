
+ # 🌿 Ecofy – Smart Expense Tracker & Price Comparison Platform

🚀 **Try Ecofy Live**: [https://graduation-projectechofi.vercel.app/login](https://graduation-projectechofi.vercel.app/login)  
📦 **GitHub Repo**: [Ahmed-0sama/GraduationProject](https://github.com/Ahmed-0sama/GraduationProject)

Ecofy is a comprehensive expense tracking and price comparison web application that leverages AI-powered receipt processing, real-time price scraping, and intelligent expense categorization to help users manage their finances effectively.

[![Version](https://img.shields.io/badge/Version-1.0.0-blue)](https://github.com/Ahmed-0sama/GraduationProject/releases) 
[![.NET](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/) 
[![React](https://img.shields.io/badge/React-18.2-blue)](https://reactjs.org/) 
[![Azure](https://img.shields.io/badge/Azure-Cloud-blue)](https://azure.microsoft.com/)

## 🌟 Features

### Core Functionality
- 📸 **Receipt Upload & OCR**: Camera-based receipt capture with intelligent text extraction
- 🤖 **AI-Powered Expense Classification**: Automatic categorization of expenses (food, transport, shopping, etc.)
- 💰 **Real-time Price Comparison**: Live price scraping from major e-commerce platforms
- 📊 **Budget Tracking & Analytics**: Comprehensive expense analytics and budget management
- 🔔 **Smart Notifications**: Email alerts for budget thresholds and spending patterns
- ⚡ **Real-time Updates**: Live data synchronization using SignalR

### Technical Features
- 🔐 **JWT Authentication**: Secure user authentication and authorization via REST APIs
- 📱 **Responsive Design**: Mobile-first design with Tailwind CSS
- 🐳 **Containerized Deployment**: Full Docker support for Azure deployment
- ☁️ **Azure Cloud Integration**: Fully deployed on Azure with scalable architecture

## 🛠️ Tech Stack

### Backend
- ASP.NET Core (.NET 8)
- Entity Framework Core + LINQ
- SQL Server
- SignalR (Real-time communication)
- JWT (Authentication)
- RESTful APIs

### Frontend
- React.js
- JavaScript (ES6+)
- Tailwind CSS
- Axios

### AI & Automation
- OCR Engine (for text extraction)
- AI Classification Model (for expense categories)
- Selenium WebDriver (for price scraping)

### E-commerce Integration
- Jumia, Noon, Amazon

### DevOps & Deployment
- Docker
- Azure App Service
- Azure SQL Database
- Azure Blob Storage
- Vercel (Frontend Hosting)

## 🚀 Getting Started

### Prerequisites
- .NET 8.0 SDK
- Node.js (v18+)
- SQL Server or LocalDB
- Docker Desktop
- Azure CLI
- Visual Studio 2022 or VS Code

### Installation

1. Clone the repository
```bash
git clone https://github.com/Ahmed-0sama/GraduationProject.git
cd GraduationProject
```

2. Backend Setup
```bash
cd backend
dotnet restore
dotnet ef database update
dotnet run --project SmartExpenseTracker.API
```

3. Frontend Setup
```bash
cd frontend
npm install
npm start
```

4. Environment Configuration

Create `.env` files in both backend and frontend directories (as shown in original message).

### 🐳 Docker Deployment
```bash
docker-compose up --build
```

## 📚 REST API Documentation

(Include all API routes as previously outlined)

## 📊 Features in Detail

- OCR + AI Pipeline for receipts
- Multi-platform price scraping (Jumia, Noon, Amazon)
- Budget alerts and expense summaries
- SignalR + email notifications

## 🧪 Testing

Backend: `dotnet test`  
Frontend: `npm test`

## 📈 Monitoring

- Azure Application Insights
- Azure Monitor
## Built with ❤️ using ASP.NET Core, React.js, and Azure Cloud — helping you **Ecofing** your finances.
