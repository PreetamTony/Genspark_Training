# 🚌 NexBus - Modern Bus Booking Application

A comprehensive, full-stack bus booking application built with Angular 21 and .NET 8, featuring real-time seat selection, interactive maps, AI-powered chatbot, and more.

## 🚀 Features

### 🎯 Core Functionality
- **User Authentication**: Secure login system with role-based access (Admin, Operator, Customer)
- **Bus Search & Booking**: Intelligent search with filters for routes, dates, and preferences
- **Real-time Seat Selection**: Interactive seat maps with gender-based passenger details
- **Payment Processing**: Professional payment interface with coupon support
- **Operator Dashboard**: Complete fleet management, scheduling, and analytics

### 🌟 Advanced Features
- **🗺️ Interactive Maps**: Real-time route visualization using Leaflet.js and OpenStreetMap
- **🤖 AI Chatbot**: "Nexbot" - Intelligent assistant powered by Groq API (Llama 3.3)
- **💳 Smart Coupons**: Dynamic discount system with operator-specific promotions
- **⭐ Rating System**: Comprehensive bus reviews and ratings
- **📱 Responsive Design**: Mobile-first approach with modern UI/UX

### 🛠️ Technical Highlights
- **Standalone Components**: Modern Angular architecture
- **PostgreSQL Database**: Production-ready persistent storage
- **RESTful APIs**: Clean, scalable backend architecture
- **Professional UI**: Glass-panel design with TailwindCSS
- **Real-time Updates**: Dynamic seat availability and booking status

## 📋 System Requirements

### Prerequisites
- **Node.js** (v18 or higher)
- **.NET 8 SDK**
- **PostgreSQL** (v12 or higher)
- **Angular CLI** (v17 or higher)

### Development Environment
- **Frontend**: Angular 21, TypeScript, TailwindCSS
- **Backend**: .NET 8, Entity Framework Core, PostgreSQL
- **Tools**: Git, VS Code (recommended)

## 🛠️ Installation & Setup

### 1. Clone the Repository
```bash
git clone <repository-url>
cd NexBus
```

### 2. Backend Setup (.NET 8)
```bash
# Navigate to backend directory
cd backend

# Restore dependencies
dotnet restore

# Update database connection in appsettings.Development.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=busbooking;Username=postgres;Password=postgres"
  },
  "UseInMemoryDatabase": false
}

# Apply database migrations
dotnet ef database update

# Start the backend server
dotnet run
```

### 3. Frontend Setup (Angular 21)
```bash
# Navigate to frontend directory
cd frontend

# Install dependencies
npm install

# Start the development server
ng serve
```

### 4. Database Setup (PostgreSQL)
```bash
# Create database
createdb busbooking

# Start PostgreSQL service
brew services start postgresql@18  # macOS
# or
sudo systemctl start postgresql     # Linux

# Verify connection
psql -U postgres -d busbooking
```

## 🌐 Access Points

### Development URLs
- **Frontend**: http://localhost:4200
- **Backend API**: http://localhost:5047
- **API Documentation**: http://localhost:5047/swagger

### Default Accounts
- **Admin**: admin@nexbus.com / admin123
- **Operator**: operator@nexbus.com / operator123
- **Customer**: customer@nexbus.com / customer123

## 📁 Project Structure

```
NexBus/
├── backend/                          # .NET 8 Web API
│   ├── Controllers/                  # API endpoints
│   ├── Models/                      # Data models
│   ├── Services/                    # Business logic
│   ├── DTOs/                        # Data transfer objects
│   ├── Migrations/                  # Database migrations
│   └── Program.cs                   # Application entry point
├── frontend/                        # Angular 21 Application
│   ├── src/
│   │   ├── app/                     # Main application
│   │   ├── components/              # Angular components
│   │   ├── services/                # HTTP services
│   │   ├── models/                  # TypeScript models
│   │   └── assets/                  # Static assets
│   ├── angular.json                 # Angular configuration
│   └── package.json                 # Node.js dependencies
├── .gitignore                       # Git ignore rules
├── README.md                        # This file
└── docker-compose.yml               # Docker configuration (optional)
```

## 🗄️ Database Schema

### Core Tables
- **Users**: User authentication and profiles
- **Locations**: Cities and geographical points
- **Routes**: Source-destination connections
- **Buses**: Fleet information and specifications
- **Schedules**: Timetables and availability
- **Bookings**: User booking transactions
- **Seats**: Individual seat configurations
- **Coupons**: Discount codes and promotions
- **Reviews**: User ratings and feedback

### Relationships
- Users → Bookings (One-to-Many)
- Buses → Schedules (One-to-Many)
- Routes → Schedules (One-to-Many)
- Bookings → Passengers (One-to-Many)
- Bookings → BookingSeats (One-to-Many)

## 🔧 Configuration

### Backend Configuration
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=busbooking;Username=postgres;Password=postgres"
  },
  "JwtSettings": {
    "SecretKey": "your-secret-key",
    "ExpiryDays": 7
  },
  "GroqSettings": {
    "ApiKey": "your-groq-api-key"
  }
}
```

### Frontend Configuration
```typescript
// src/environments/environment.ts
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5047/api'
};
```

## 🚀 Running the Application

### Development Mode
```bash
# Terminal 1 - Backend
cd backend
dotnet run

# Terminal 2 - Frontend
cd frontend
ng serve
```

### Production Build
```bash
# Frontend build
cd frontend
ng build --configuration production

# Backend publish
cd backend
dotnet publish -c Release
```

## 🧪 Testing

### Backend Tests
```bash
cd backend
dotnet test
```

### Frontend Tests
```bash
cd frontend
ng test
ng e2e
```

## 📊 API Endpoints

### Authentication
- `POST /api/auth/login` - User login
- `POST /api/auth/register` - User registration
- `GET /api/auth/profile` - Get user profile

### Bus Operations
- `GET /api/schedules/search` - Search bus schedules
- `GET /api/buses/{id}` - Get bus details
- `POST /api/bookings` - Create booking
- `GET /api/routes` - Get available routes

### Operator Management
- `GET /api/operator/dashboard` - Operator dashboard data
- `POST /api/operator/coupons` - Create coupons
- `GET /api/operator/schedules` - Manage schedules

### AI Chatbot
- `POST /api/chatbot/chat` - Chat with Nexbot
- `GET /api/chatbot/context` - Get system context

## 🔐 Security Features

- **JWT Authentication**: Token-based authentication
- **Role-Based Access**: Admin, Operator, Customer roles
- **Password Hashing**: Secure password storage
- **Input Validation**: Comprehensive data validation
- **CORS Configuration**: Cross-origin resource sharing
- **SQL Injection Protection**: Entity Framework parameterization

## 🎨 UI/UX Features

- **Glass Panel Design**: Modern, translucent UI elements
- **Responsive Layout**: Mobile-first design approach
- **Interactive Seat Maps**: Real-time seat selection
- **Loading States**: Professional loading indicators
- **Error Handling**: User-friendly error messages
- **Dark/Light Mode**: Theme support (planned)

## 🤖 AI Integration

### Nexbot Chatbot
- **Model**: Llama 3.3 70B (Groq API)
- **Capabilities**: Route information, booking assistance, policies
- **Features**: Real-time streaming, contextual responses
- **Integration**: Seamless chat interface

### Map Integration
- **Technology**: Leaflet.js + OpenStreetMap
- **Features**: Route visualization, distance calculation
- **Coverage**: 19+ major Indian cities
- **Interactive**: Clickable markers and route paths

## 📈 Performance Optimization

- **Lazy Loading**: Component-level lazy loading
- **Image Optimization**: Compressed and optimized images
- **Caching**: API response caching
- **Database Indexing**: Optimized database queries
- **Bundle Optimization**: Angular build optimizations

## 🚀 Deployment

### Docker Deployment
```bash
# Build and run with Docker Compose
docker-compose up --build
```

### Manual Deployment
```bash
# Backend deployment
cd backend
dotnet publish -c Release -o ./publish

# Frontend deployment
cd frontend
ng build --configuration production
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🐛 Bug Reporting

Report bugs and issues through the GitHub Issues page. Please include:
- Detailed description of the issue
- Steps to reproduce
- Expected vs actual behavior
- Screenshots (if applicable)

## 📞 Support

For support and questions:
- **Email**: support@nexbus.com
- **Documentation**: [Project Wiki](wiki-link)
- **Issues**: [GitHub Issues](issues-link)

## 🔄 Version History

- **v2.0.0** - Current version with AI chatbot and maps
- **v1.0.0** - Initial release with core booking functionality

## 🎯 Future Enhancements

- [ ] Mobile App (React Native)
- [ ] Payment Gateway Integration
- [ ] Real-time GPS Tracking
- [ ] Multi-language Support
- [ ] Advanced Analytics Dashboard
- [ ] SMS/Email Notifications
- [ ] Loyalty Program Integration

---

**Built with ❤️ by the NexBus Team**
