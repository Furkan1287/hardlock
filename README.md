# 🔒 HARDLOCK - Gelişmiş Dosya Güvenlik Platformu

> ⚠️ **Geliştirilme Aşamasında** ⚠️
> 
> Bu proje aktif olarak geliştirilmektedir. Production kullanımı için henüz hazır değildir.

HARDLOCK, finansal sektör standartlarında güvenlik sağlayan, mikroservis tabanlı gelişmiş dosya şifreleme ve depolama platformudur. Kullanıcıların dosyalarını güvenli bir şekilde şifreleyip saklamalarını, coğrafi kısıtlamalar uygulamalarını ve hatta darknet üzerinde yedekleme yapmalarını sağlar.

## 🏗️ Mimari

### Core Services
1. **Identity Service** - JWT Authentication & Authorization
2. **File Encryption Service** - Client-side AES-256-GCM encryption
3. **File Storage Service** - S3/Blob storage with cold wallet backup
4. **Access Control Service** - Multi-user access management
5. **Audit Service** - Comprehensive audit logging
6. **Notification Service** - Real-time alerts and notifications
7. **API Gateway** - Reverse proxy and routing

### Advanced Security Features
- 🔐 **Dual-Layer Encryption**: File-level + Storage-level encryption
- 💥 **Self-Destruct Mechanism**: Auto-destruct after failed attempts
- 🌍 **Geographic Locking**: Location-based access control
- 🧬 **Biometric Key Derivation**: FIDO2 integration
- ⚡ **Quantum-Safe Encryption**: Post-quantum cryptography
- 🕸️ **Darknet Backup**: Distributed storage via Tor network
- 🎯 **Honey Pot Files**: Attack detection and prevention
- ⏰ **Timelock Decryption**: Time-based access control
- 🔍 **File Hash Verification**: SHA-256/SHA-512 integrity checking

## 🚀 Hızlı Başlangıç

```bash
# Repository'yi klonlayın
git clone https://github.com/Furkan1287/hardlock.git
cd hardlock

# Tüm servisleri başlatın
docker-compose up -d

# Frontend'e erişin
open http://localhost:3000

# API dokümantasyonuna erişin
open http://localhost:8080/swagger
```

### 📋 Gereksinimler

- Docker & Docker Compose
- .NET 8 SDK (geliştirme için)
- En az 4GB RAM
- 10GB boş disk alanı

## 🛠️ Geliştirme Ortamı

### Kurulum Adımları

1. **Repository'yi klonlayın:**
   ```bash
   git clone https://github.com/Furkan1287/hardlock.git
   cd hardlock
   ```

2. **Docker Compose ile başlatın:**
   ```bash
   docker-compose up -d
   ```

3. **Servislerin durumunu kontrol edin:**
   ```bash
   docker-compose ps
   ```

4. **Logları kontrol edin:**
   ```bash
   docker-compose logs -f [service-name]
   ```

### 🔧 Geliştirme İçin Gereksinimler
- .NET 8 SDK
- Docker & Docker Compose
- Git
- Node.js 18+ (frontend geliştirme için)

### 🗄️ Veritabanı
- **Geliştirme**: SQLite (otomatik kurulum)
- **Production**: PostgreSQL 15+ (planlanıyor)

### 🔧 Diğer Servisler
- Redis 7+ (cache ve session yönetimi)
- Hashicorp Vault (anahtar yönetimi)
- Tor Browser (darknet erişimi testi için)
- IPFS (Docker setup'ında dahil)

## 🔧 Konfigürasyon

Environment değişkenleri her servis için `.env` dosyalarında yapılandırılır. Ana konfigürasyonlar:

```env
# Veritabanı (Geliştirme)
DATABASE_CONNECTION_STRING=Data Source=hardlock.db

# JWT
JWT__Secret=your-super-secret-key-here
JWT__Issuer=hardlock
JWT__Audience=hardlock-users
JWT__ExpiryMinutes=15

# Depolama (Production için)
AWS_S3_BUCKET=hardlock-files
AWS_GLACIER_VAULT=hardlock-backup

# Güvenlik
MAX_LOGIN_ATTEMPTS=3
SELF_DESTRUCT_ENABLED=true
```

## 🛡️ Güvenlik Özellikleri

### 📁 Dosya Şifreleme
- **Client-side şifreleme** AES-256-GCM ile
- **Sharded encryption** - dosyalar 4KB parçalara bölünür
- **Dosya başına benzersiz salt** - PBKDF2-HMAC-SHA256 anahtar türetme
- **Quantum-safe algoritmalar** - CRYSTALS-Kyber & CRYSTALS-Dilithium

### 🔐 Erişim Kontrolü
- **Çok faktörlü kimlik doğrulama** FIDO2 ile
- **Coğrafi kısıtlama** - GPS tabanlı erişim kontrolü
- **Yakınlık doğrulaması** - BLE tabanlı cihaz doğrulama
- **Davranış analizi** - AI destekli anomali tespiti

### 💾 Yedekleme & Kurtarma
- **Soğuk cüzdan depolama** - AWS Glacier değiştirilemez yedekleme
- **Darknet yedekleme** - Tor ağı üzerinde IPFS ile dağıtık depolama
- **Cross-region replication** - Felaket kurtarma
- **WORM compliance** - Write Once Read Many
- **Distributed Hash Table** - P2P dosya keşfi

## 📊 İzleme & Monitoring

- **Prometheus** - Metrik toplama
- **Grafana** - Gerçek zamanlı dashboard'lar
- **ELK Stack** - Log toplama
- **Jaeger** - Distributed tracing

## 🔍 API Dokümantasyonu

Kapsamlı API dokümantasyonu her servis için `/swagger` endpoint'inde mevcuttur.

## 🚀 Servis URL'leri

Proje başlatıldıktan sonra aşağıdaki URL'lere erişebilirsiniz:

- **Frontend**: http://localhost:3000
- **API Gateway**: http://localhost:8080
- **Identity Service**: http://localhost:5001
- **Encryption Service**: http://localhost:5002
- **Storage Service**: http://localhost:5003
- **Access Control Service**: http://localhost:5004
- **Audit Service**: http://localhost:5005
- **Notification Service**: http://localhost:5006
- **Grafana**: http://localhost:3001 (admin/hardlock_admin)
- **Prometheus**: http://localhost:9090

## 🔐 Varsayılan Kullanıcı

- **Email**: admin@hardlock.com
- **Şifre**: Admin123!

## 🐛 Sorun Giderme

### Yaygın Sorunlar ve Çözümleri

1. **Port Çakışması:**
   ```bash
   # Kullanılan portları kontrol edin
   lsof -i :3000
   lsof -i :8080
   
   # Servisleri durdurun ve yeniden başlatın
   docker-compose down
   docker-compose up -d
   ```

2. **JWT Secret Hatası:**
   - `appsettings.json` dosyasının doğru konumda olduğundan emin olun
   - Environment değişkenlerinin doğru ayarlandığını kontrol edin

3. **Frontend Bağlantı Sorunu:**
   ```bash
   # Frontend container'ını yeniden başlatın
   docker-compose restart frontend
   
   # Logları kontrol edin
   docker-compose logs frontend
   ```

4. **Build Hataları:**
   ```bash
   # Cache'i temizleyin ve yeniden build edin
   docker-compose down -v
   docker-compose build --no-cache
   docker-compose up -d
   ```

## 📝 Lisans

MIT License - detaylar için LICENSE dosyasına bakın.

## 🤝 Katkıda Bulunma

1. Repository'yi fork edin
2. Feature branch oluşturun
3. Değişiklikleri commit edin
4. Branch'e push edin
5. Pull Request oluşturun

## 🆘 Destek

Destek ve sorular için:
- 📧 Email: support@hardlock.com
- 💬 Discord: [HARDLOCK Community](https://discord.gg/hardlock)
- 📖 Dokümantasyon: [docs.hardlock.com](https://docs.hardlock.com)

## 🔮 Gelecek Planları

### ✅ Tamamlanan Servisler
- [x] Identity Service implementasyonu
- [x] Storage Service implementasyonu
- [x] Access Control Service implementasyonu
- [x] Audit Service implementasyonu
- [x] Notification Service implementasyonu
- [x] Encryption Service implementasyonu
- [x] API Gateway implementasyonu
- [x] Frontend React uygulaması
- [x] Docker Compose konfigürasyonu
- [x] Monitoring (Prometheus + Grafana)

### 🚧 Geliştirilme Aşamasında
- [ ] Cold Wallet Service implementasyonu
- [ ] Frontend UI/UX iyileştirmeleri
- [ ] API dokümantasyonu tamamlama

### 🎯 Planlanan Özellikler
- [ ] FIDO2 biyometrik doğrulama
- [ ] Quantum-safe şifreleme
- [ ] AI destekli anomali tespiti
- [ ] BLE yakınlık doğrulaması
- [ ] Cross-region replication
- [ ] WORM compliance
- [ ] Darknet backup implementasyonu
- [ ] Geographic locking
- [ ] Self-destruct mechanism

### 📈 Production Hazırlığı
- [ ] PostgreSQL migration
- [ ] Performance optimization
- [ ] Security audit
- [ ] Load testing
- [ ] Documentation completion
- [ ] CI/CD pipeline
- [ ] Kubernetes deployment 