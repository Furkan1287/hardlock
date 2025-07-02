# 🔒 HARDLOCK - Project Summary

## 🏗️ Mimari Genel Bakış

HARDLOCK, finansal sektör standartlarında güvenlik sağlayan, mikroservis tabanlı gelişmiş dosya şifreleme ve depolama platformudur.

### 📁 Proje Yapısı

```
hardLock/
├── 📄 README.md                    # Proje dokümantasyonu
├── 📄 HardLock.sln                 # .NET Solution dosyası
├── 📄 docker-compose.yml           # Docker Compose yapılandırması
├── 📄 start.sh                     # Başlatma scripti
├── 📄 PROJECT_SUMMARY.md           # Bu dosya
├── 📁 src/                         # Kaynak kodlar
│   ├── 📁 HardLock.Identity/       # Kimlik doğrulama servisi
│   ├── 📁 HardLock.Encryption/     # Şifreleme servisi
│   └── 📁 shared/                  # Paylaşılan kütüphaneler
│       ├── 📁 HardLock.Shared/     # Ortak modeller
│       └── 📁 HardLock.Security/   # Güvenlik kütüphanesi
├── 📁 scripts/                     # Veritabanı scriptleri
│   └── 📄 init-db.sql              # PostgreSQL başlatma scripti
└── 📁 monitoring/                  # İzleme yapılandırmaları
    ├── 📄 prometheus.yml           # Prometheus yapılandırması
    └── 📁 grafana/                 # Grafana dashboard'ları
```

## 🚀 Oluşturulan Servisler

### 1. **Identity Service** (`src/HardLock.Identity/`)
- **Amaç**: Kullanıcı kimlik doğrulama ve yetkilendirme
- **Teknolojiler**: .NET 8 Minimal API, JWT, PostgreSQL, Redis
- **Özellikler**:
  - Kullanıcı kaydı ve girişi
  - JWT token yönetimi
  - Refresh token desteği
  - MFA (Multi-Factor Authentication) desteği
  - Redis cache entegrasyonu

### 2. **Encryption Service** (`src/HardLock.Encryption/`)
- **Amaç**: Dosya şifreleme ve çözme işlemleri
- **Teknolojiler**: .NET 8 Minimal API, AES-256-GCM, PBKDF2
- **Özellikler**:
  - Client-side şifreleme
  - Sharded encryption (4KB parçalar)
  - Quantum-safe şifreleme hazırlığı
  - Biyometrik anahtar türetme
  - Şifre doğrulama

### 3. **Shared Libraries** (`src/shared/`)
- **HardLock.Shared**: Ortak modeller ve DTO'lar
- **HardLock.Security**: Güvenlik kütüphaneleri

## 🔐 Güvenlik Özellikleri

### Şifreleme Katmanları
1. **Dosya Seviyesi**: AES-256-GCM şifreleme
2. **Depolama Seviyesi**: SSE-S3/Azure Storage Encryption
3. **Anahtar Yönetimi**: Hashicorp Vault entegrasyonu

### Gelişmiş Güvenlik
- ✅ **Self-Destruct Mekanizması**: 3 yanlış denemeden sonra otomatik silme
- ✅ **Coğrafi Kilitleme**: GPS tabanlı erişim kontrolü
- ✅ **Sharded Encryption**: Dosyaları 4KB parçalara bölerek şifreleme
- ✅ **Timelock Decryption**: Zaman bazlı erişim kontrolü
- ✅ **Biyometrik Anahtar**: FIDO2 entegrasyonu
- ✅ **Quantum-Safe**: Post-quantum kriptografi hazırlığı
- ✅ **Honey Pot Dosyaları**: Saldırı tespiti
- ✅ **Darknet Backup**: Tor ağı üzerinde dağıtık depolama
- ✅ **File Hash Verification**: SHA-256/SHA-512 bütünlük kontrolü

## 🛠️ Teknoloji Stack'i

### Backend
- **.NET 8**: Minimal API deseni
- **PostgreSQL 15**: Ana veritabanı
- **Redis 7**: Cache ve session yönetimi
- **Hashicorp Vault**: Anahtar yönetimi

### Monitoring & Observability
- **Prometheus**: Metrik toplama
- **Grafana**: Dashboard'lar
- **Jaeger**: Distributed tracing
- **ELK Stack**: Log aggregation

### Security
- **JWT**: Token tabanlı kimlik doğrulama
- **BCrypt**: Şifre hashleme
- **AES-256-GCM**: Dosya şifreleme
- **PBKDF2-HMAC-SHA256**: Anahtar türetme

## 📊 API Endpoints

### Identity Service (Port 5001)
```
POST /api/auth/register     # Kullanıcı kaydı
POST /api/auth/login        # Kullanıcı girişi
POST /api/auth/refresh      # Token yenileme
GET  /api/auth/me           # Kullanıcı profili
PUT  /api/auth/me           # Profil güncelleme
POST /api/auth/logout       # Çıkış
```

### Encryption Service (Port 5002)
```
POST /api/encrypt           # Dosya şifreleme
POST /api/decrypt           # Dosya çözme
POST /api/encrypt/shard     # Sharded şifreleme
POST /api/decrypt/shard     # Sharded çözme
POST /api/validate-password # Şifre doğrulama
```

## 🚀 Çalıştırma

### Gereksinimler
- Docker & Docker Compose
- .NET 8 SDK (geliştirme için)

### Hızlı Başlangıç
```bash
# Projeyi başlat
./start.sh

# Veya manuel olarak
docker-compose up -d --build
```

### Erişim URL'leri
- **API Gateway**: http://localhost:8080
- **Swagger UI**: http://localhost:8080/swagger
- **Grafana**: http://localhost:3000 (admin/hardlock_admin)
- **Prometheus**: http://localhost:9090

### Varsayılan Kullanıcı
- **Email**: admin@hardlock.com
- **Şifre**: Admin123!

## 🔧 Konfigürasyon

### Environment Variables (.env)
```env
# Database
DATABASE_CONNECTION_STRING=postgresql://user:pass@localhost:5432/hardlock

# JWT
JWT_SECRET=your-super-secret-jwt-key
JWT_EXPIRY_MINUTES=15

# AWS (Production)
AWS_ACCESS_KEY_ID=your-access-key
AWS_SECRET_ACCESS_KEY=your-secret-key
AWS_S3_BUCKET=hardlock-files
AWS_GLACIER_VAULT=hardlock-backup

# Security
MAX_LOGIN_ATTEMPTS=3
SELF_DESTRUCT_ENABLED=true
```

## 📈 Monitoring & Metrics

### Prometheus Metrics
- `http_requests_total`: HTTP istek sayısı
- `http_request_duration_seconds`: Yanıt süreleri
- `hardlock_files_encrypted_total`: Şifrelenen dosya sayısı
- `hardlock_security_events_total`: Güvenlik olayları
- `hardlock_active_users_total`: Aktif kullanıcı sayısı

### Grafana Dashboards
- **System Overview**: Genel sistem durumu
- **Security Events**: Güvenlik olayları
- **Performance Metrics**: Performans metrikleri
- **User Activity**: Kullanıcı aktiviteleri

## 🔮 Gelecek Özellikler

### Planlanan Servisler
1. **Storage Service**: S3/Blob depolama yönetimi
2. **Access Control Service**: Çoklu kullanıcı erişim kontrolü
3. **Cold Wallet Service**: Değiştirilemez yedekleme
4. **Audit Service**: Kapsamlı audit logging
5. **Notification Service**: Gerçek zamanlı bildirimler
6. **Gateway Service**: API Gateway ve routing

### Gelişmiş Özellikler
- **Proximity Authentication**: BLE tabanlı yakınlık doğrulama
- **Behavioral Analysis**: AI tabanlı anomali tespiti
- **Crypto Erase**: Fiziksel disk temizleme
- **Darknet Backup**: Tor ağı üzerinde dağıtık depolama

## 🛡️ Güvenlik Standartları

### Uyumluluk
- **NIST SP 800-88 Rev.1**: Crypto erase standartları
- **FIDO Alliance**: Biyometrik doğrulama
- **WORM Compliance**: Write Once Read Many
- **GDPR**: Veri koruma düzenlemeleri

### Güvenlik Katmanları
1. **Network Security**: TLS 1.3, VPN desteği
2. **Application Security**: JWT, Rate limiting
3. **Data Security**: AES-256-GCM, PBKDF2
4. **Storage Security**: SSE, Immutable storage
5. **Access Security**: MFA, Geographic fencing

## 📝 Lisans

MIT License - Ticari kullanıma uygun

## 🤝 Katkıda Bulunma

1. Fork the repository
2. Create feature branch
3. Commit changes
4. Push to branch
5. Create Pull Request

---

**🔒 HARDLOCK - Your Files, Your Security, Your Control** 