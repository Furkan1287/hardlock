#!/bin/bash

# HARDLOCK - Advanced File Security Platform
# Startup Script

set -e

echo "🔒 Starting HARDLOCK - Advanced File Security Platform"
echo "=================================================="

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo "❌ Docker is not running. Please start Docker and try again."
    exit 1
fi

# Check if Docker Compose is available
if ! command -v docker-compose &> /dev/null; then
    echo "❌ Docker Compose is not installed. Please install Docker Compose and try again."
    exit 1
fi

# Create data directory for SQLite
mkdir -p data

# Create .env file if it doesn't exist
if [ ! -f .env ]; then
    echo "📝 Creating .env file with default configuration..."
    cat > .env << EOF
# HARDLOCK Environment Configuration

# Database (SQLite for development)
DATABASE_CONNECTION_STRING=Data Source=hardlock.db

# JWT Configuration
JWT_SECRET=your-super-secret-jwt-key-change-this-in-production
JWT_EXPIRY_MINUTES=15
JWT_ISSUER=HardLock
JWT_AUDIENCE=HardLock

# Redis
REDIS_CONNECTION_STRING=localhost:6379

# AWS Configuration (optional - for production)
AWS_ACCESS_KEY_ID=
AWS_SECRET_ACCESS_KEY=
AWS_REGION=us-east-1
AWS_S3_BUCKET=hardlock-files
AWS_GLACIER_VAULT=hardlock-backup

# SMTP Configuration (optional - for notifications)
SMTP_HOST=
SMTP_PORT=587
SMTP_USERNAME=
SMTP_PASSWORD=

# Elasticsearch
ELASTICSEARCH_URL=http://localhost:9200

# Security Settings
MAX_LOGIN_ATTEMPTS=3
SELF_DESTRUCT_ENABLED=true
EOF
    echo "✅ .env file created. Please review and update the configuration."
fi

# Build and start services
echo "🚀 Building and starting HARDLOCK services..."
docker-compose up -d --build

# Wait for services to be ready
echo "⏳ Waiting for services to be ready..."
sleep 30

# Check service health
echo "🔍 Checking service health..."
services=("redis" "vault" "identity-service" "encryption-service")
for service in "${services[@]}"; do
    if docker-compose ps $service | grep -q "Up"; then
        echo "✅ $service is running"
    else
        echo "❌ $service is not running"
    fi
done

echo ""
echo "🎉 HARDLOCK is now running!"
echo ""
echo "📊 Service URLs:"
echo "   Frontend:        http://localhost:3000"
echo "   API Gateway:     http://localhost:8080"
echo "   Identity Service: http://localhost:5001"
echo "   Encryption Service: http://localhost:5002"
echo "   Storage Service:  http://localhost:5003"
echo "   Access Control:   http://localhost:5004"
echo "   Cold Wallet:      http://localhost:5005"
echo "   Audit Service:    http://localhost:5006"
echo "   Notification:     http://localhost:5007"
echo ""
echo "🔧 Monitoring & Management:"
echo "   Grafana:          http://localhost:3001 (admin/hardlock_admin)"
echo "   Prometheus:       http://localhost:9090"
echo "   Jaeger:           http://localhost:16686"
echo "   Kibana:           http://localhost:5601"
echo "   Elasticsearch:    http://localhost:9200"
echo "   Vault:            http://localhost:8200"
echo ""
echo "📚 API Documentation:"
echo "   Swagger UI:       http://localhost:8080/swagger"
echo ""
echo "🔐 Default Admin Credentials:"
echo "   Email: admin@hardlock.com"
echo "   Password: Admin123!"
echo ""
echo "💡 To stop HARDLOCK, run: docker-compose down"
echo "💡 To view logs, run: docker-compose logs -f [service-name]"
echo ""
echo "🔒 HARDLOCK is ready to secure your files!" 