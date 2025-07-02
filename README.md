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
5. **Cold Wallet Service** - Immutable backup with WORM compliance
6. **Audit Service** - Comprehensive audit logging
7. **Notification Service** - Real-time alerts and notifications

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
./start.sh

# API dokümantasyonuna erişin
open http://localhost:8080/swagger
```

### 📋 Gereksinimler

- Docker & Docker Compose
- .NET 8 SDK (geliştirme için)
- En az 4GB RAM
- 10GB boş disk alanı

## 📋 Gereksinimler

### 🛠️ Geliştirme Ortamı
- .NET 8 SDK
- Docker & Docker Compose
- Git

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
JWT_SECRET=your-super-secret-key
JWT_EXPIRY_MINUTES=15

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
- **Grafana**: http://localhost:3001 (admin/hardlock_admin)
- **Prometheus**: http://localhost:9090

## 🔐 Varsayılan Kullanıcı

- **Email**: admin@hardlock.com
- **Şifre**: Admin123!

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

### 🚧 Geliştirilme Aşamasında
- [ ] Storage Service implementasyonu
- [ ] Access Control Service implementasyonu
- [ ] Cold Wallet Service implementasyonu
- [ ] Audit Service implementasyonu
- [ ] Notification Service implementasyonu
- [ ] API Gateway implementasyonu

### 🎯 Planlanan Özellikler
- [ ] FIDO2 biyometrik doğrulama
- [ ] Quantum-safe şifreleme
- [ ] AI destekli anomali tespiti
- [ ] BLE yakınlık doğrulaması
- [ ] Cross-region replication
- [ ] WORM compliance

### 📈 Production Hazırlığı
- [ ] PostgreSQL migration
- [ ] Performance optimization
- [ ] Security audit
- [ ] Load testing
- [ ] Documentation completion 