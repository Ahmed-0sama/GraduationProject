Smart Expense Tracker & Price Comparison Platform
🚀 Live Demo: https://graduation-projectechofi.vercel.app/login
A comprehensive expense tracking and price comparison web application that leverages AI-powered receipt processing, real-time price scraping, and intelligent expense categorization to help users manage their finances effectively.
🌟 Features
Core Functionality

📸 Receipt Upload & OCR: Camera-based receipt capture with intelligent text extraction
🤖 AI-Powered Expense Classification: Automatic categorization of expenses (food, transport, shopping, etc.)
💰 Real-time Price Comparison: Live price scraping from major e-commerce platforms
📊 Budget Tracking & Analytics: Comprehensive expense analytics and budget management
🔔 Smart Notifications: Email alerts for budget thresholds and spending patterns
⚡ Real-time Updates: Live data synchronization using SignalR

Technical Features

🔐 JWT Authentication: Secure user authentication and authorization via REST APIs
📱 Responsive Design: Mobile-first design with Tailwind CSS
🐳 Containerized Deployment: Full Docker support for Azure deployment
☁️ Azure Cloud Integration: Fully deployed on Azure with scalable architecture

🛠️ Tech Stack
Backend

ASP.NET Core - REST API framework
Entity Framework Core - ORM for database operations
SQL Server - Primary database
LINQ - Data querying
SignalR - Real-time communication
JWT - Authentication tokens
REST APIs - RESTful web services architecture

Frontend

React.js - User interface framework
JavaScript (ES6+) - Primary frontend language
Tailwind CSS - Utility-first CSS framework
Axios - HTTP client for REST API communication

AI & Automation

OCR Engine - Receipt text extraction
AI Classification Model - Expense categorization
Selenium WebDriver - Price scraping automation

E-commerce Integration

Jumia - Price comparison
Noon - Price comparison
Amazon - Price comparison

DevOps & Deployment

Docker - Containerization
Azure - Primary cloud hosting platform
Azure App Service - Backend hosting
Azure SQL Database - Cloud database
Azure Storage - File and blob storage
Vercel - Frontend deployment (current)

🚀 Getting Started
Prerequisites

.NET 8.0 SDK
Node.js (v18+)
SQL Server (LocalDB for development, Azure SQL for production)
Docker Desktop (for containerization)
Azure CLI (for deployment)
Visual Studio 2022 or VS Code

Installation

Clone the repository
bashgit clone https://github.com/yourusername/smart-expense-tracker.git
cd smart-expense-tracker

Backend Setup
bashcd backend
dotnet restore
dotnet ef database update
dotnet run --project SmartExpenseTracker.API

Frontend Setup
bashcd frontend
npm install
npm start

Environment Configuration
Create .env files in both backend and frontend directories:
Backend (.env)
envConnectionStrings__DefaultConnection=Server=your-azure-sql-server.database.windows.net;Database=SmartExpenseTrackerDB;User Id=your-username;Password=your-password;Encrypt=True;
ConnectionStrings__LocalConnection=Server=(localdb)\\mssqllocaldb;Database=SmartExpenseTrackerDB;Trusted_Connection=true
JwtSettings__Secret=your-super-secret-jwt-key-here
JwtSettings__Issuer=SmartExpenseTracker
JwtSettings__Audience=SmartExpenseTrackerUsers
EmailSettings__SmtpServer=smtp.gmail.com
EmailSettings__SmtpPort=587
EmailSettings__Username=your-email@gmail.com
EmailSettings__Password=your-app-password
OCRService__ApiKey=your-ocr-api-key
AIClassification__ApiKey=your-ai-api-key
Azure__StorageConnectionString=your-azure-storage-connection-string
Frontend (.env)
envREACT_APP_API_BASE_URL=https://your-azure-app-service.azurewebsites.net/api
REACT_APP_SIGNALR_HUB_URL=https://your-azure-app-service.azurewebsites.net/expenseHub
REACT_APP_LOCAL_API_URL=https://localhost:7001/api


🐳 Docker Deployment

Build and run locally with Docker Compose
bashdocker-compose up --build

Deploy to Azure Container Instances
bash# Build and push to Azure Container Registry
az acr build --registry your-registry --image expense-tracker-api ./backend
az acr build --registry your-registry --image expense-tracker-frontend ./frontend

# Deploy to Azure Container Instances
az container create --resource-group your-rg --name expense-tracker-api --image your-registry.azurecr.io/expense-tracker-api:latest

Access the application

Frontend: http://localhost:3000 (local) | https://graduation-projectechofi.vercel.app (production)
Backend API: http://localhost:5000 (local) | https://your-azure-app-service.azurewebsites.net (production)
SignalR Hub: http://localhost:5000/expenseHub (local) | https://your-azure-app-service.azurewebsites.net/expenseHub (production)



📚 REST API Documentation
Authentication Endpoints
httpPOST /api/auth/register
POST /api/auth/login
POST /api/auth/refresh-token
GET  /api/auth/profile
Expense Management
httpGET    /api/expenses
POST   /api/expenses
PUT    /api/expenses/{id}
DELETE /api/expenses/{id}
POST   /api/expenses/upload-receipt
GET    /api/expenses/categories
GET    /api/expenses/by-category/{category}
Price Comparison
httpGET /api/prices/compare?product={productName}
GET /api/prices/track/{productId}
POST /api/prices/track
DELETE /api/prices/track/{id}
Budget Management
httpGET    /api/budgets
POST   /api/budgets
PUT    /api/budgets/{id}
DELETE /api/budgets/{id}
GET    /api/budgets/{id}/analytics
GET    /api/budgets/summary
Notifications
httpGET    /api/notifications
POST   /api/notifications/mark-read/{id}
GET    /api/notifications/settings
PUT    /api/notifications/settings
For detailed API documentation, see docs/api-documentation.md
🧪 Testing
Backend Tests
bashcd backend
dotnet test
Frontend Tests
bashcd frontend
npm test
Integration Tests
bashdocker-compose -f docker-compose.test.yml up --build
📊 Features in Detail
Receipt Processing Flow

User captures receipt using camera
Image is processed through OCR engine
Extracted text is parsed for expense details
AI model classifies the expense category
Data is stored and user can review/edit

Price Scraping System

Multi-platform Support: Jumia, Noon, Amazon
Real-time Updates: Prices updated every 6 hours
Smart Matching: AI-powered product matching across platforms
Historical Tracking: Price history and trend analysis

Notification System

Budget Alerts: Notify when approaching budget limits
Price Drops: Alert users when tracked items go on sale
Monthly Reports: Automated expense summaries
Custom Triggers: User-defined notification rules

🔮 Roadmap
Phase 1 (Current)

 Core expense tracking
 Receipt OCR processing
 Basic price comparison
 JWT authentication
 Real-time updates

Phase 2 (In Development)

 Banking API Integration - Connect bank accounts for automatic transaction import
 PDF Export System - Generate detailed monthly/yearly reports
 Extended Retailer Support - Integration with local supermarkets and shops
 Enhanced AI Models - Improved accuracy for expense classification and receipt processing

Phase 3 (Future)

 Mobile app (React Native)
 Multi-currency support
 Social features (family budget sharing)
 Investment tracking
 Tax calculation assistance
 Subscription management
 Loyalty points tracking

🤝 Contributing
We welcome contributions! Please follow these steps:

Fork the repository
Create a feature branch (git checkout -b feature/AmazingFeature)
Commit your changes (git commit -m 'Add some AmazingFeature')
Push to the branch (git push origin feature/AmazingFeature)
Open a Pull Request

Development Guidelines

Follow C# coding conventions for backend
Use ESLint and Prettier for frontend code formatting
Write unit tests for new features
Update documentation for API changes
Follow semantic versioning for releases

📄 License
This project is licensed under the MIT License - see the LICENSE file for details.
👥 Team

Project Lead: [Your Name]
Backend Developer: [Backend Dev Name]
Frontend Developer: [Frontend Dev Name]
AI/ML Engineer: [ML Engineer Name]

📞 Support

Issues: GitHub Issues
Discussions: GitHub Discussions
Email: support@smartexpensetracker.com

🙏 Acknowledgments

Thanks to all contributors who have helped shape this project
Special thanks to the open-source community for the amazing tools and libraries
OCR services provided by [OCR Provider Name]
AI classification powered by [AI Service Provider]


⭐ Star this repository if you find it helpful!
📱 Try the live demo: https://graduation-projectechofi.vercel.app/login
