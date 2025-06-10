# KeyBot - Tuş Otomasyonu Uygulaması

**Windows için profesyonel tuş otomasyonu çözümü**

KeyBot, Windows işletim sistemi için geliştirilmiş kullanıcı dostu bir tuş otomasyonu uygulamasıdır. Oyunlar, test süreçleri ve tekrarlayan görevler için idealdir.

## Özellikler

### Tuş Desteği
- **Özel Tuşlar**: Space, Enter, Tab, Escape, Backspace, Delete
- **Alfanumerik**: A-Z harfleri, 0-9 rakamları
- **Fonksiyon Tuşları**: F1-F12
- **Navigasyon**: Yön tuşları (↑, ↓, ←, →)

### Gelişmiş Kontroller
- **Tek Tuş Modu**: Seçilen tuşu belirli aralıklarla bas
- **Çoklu Tuş Modu**: Tuş dizisi oluştur ve her tuş için farklı gecikme ayarla
- **Hassas Zamanlama**: 0.1 - 60.0 saniye arası aralık ayarı
- **Tekrar Kontrolü**: 1-1000 arası belirli tekrar veya sınırsız mod
- **Ayar Kalıcılığı**: Tüm ayarlar otomatik kaydedilir

### Kullanıcı Arayüzü
- Sezgisel ve temiz tasarım
- Gerçek zamanlı durum gösterimi
- İlerleme çubuğu
- Hand cursor ile tıklanabilir alanların belirtilmesi

## Sistem Gereksinimleri

- **İşletim Sistemi**: Windows 10/11
- **Framework**: .NET 8.0
- **RAM**: 100 MB minimum
- **Disk Alanı**: 50 MB

## Kurulum

### Kaynak Koddan Derleme
```powershell
git clone https://github.com/muhammetalisongur/KeyBot.git
cd KeyBot
dotnet build
dotnet run
```

### Executable Oluşturma
```powershell
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```

## Kullanım

### Temel Kullanım
1. **Tuş Seçimi**: Mod seçin (tek tuş veya çoklu tuş)
2. **Zaman Ayarları**: Aralık ve tekrar sayısını belirleyin
3. **Başlatma**: Başlat butonuna tıklayın
4. **Durdurma**: İstediğiniz zaman durdur butonuna tıklayın

### Çoklu Tuş Modu
- Çoklu tuş modunu seçin
- Tuş listesine tuş ekleyin
- Her tuş için ayrı gecikme süresi ayarlayın
- Tuş sırasını istediğiniz gibi düzenleyin

### Kullanım Senaryoları
- **Oyun AFK**: Karakterin aktif kalması için
- **Form Doldurma**: Alanlar arası otomatik geçiş
- **Test Otomasyonu**: Tekrarlayan test adımları
- **Erişilebilirlik**: Fiziksel kısıtlamalarda yardımcı araç

## Güvenlik

- Tamamen lokal çalışır, internet bağlantısı gerektirmez
- Kullanıcı verisi toplanmaz
- Windows API kullanır, sistem güvenliği korunur
- Açık kaynak kodlu, şeffaf geliştirme

**Uyarı**: Bazı antivirüs yazılımları false positive uyarısı verebilir. Bu durumda güvenlik yazılımınıza istisna ekleyin.

## Geliştirme

Bu proje .NET 8.0 Windows Forms teknolojisiyle geliştirilmiştir.

### Proje Yapısı
```
KeyBot/
├── Models/          # Veri modelleri
├── MainForm.cs      # Ana form logic
├── Program.cs       # Uygulama başlatma
└── Resources/       # Kaynaklar
```

### Katkıda Bulunma
1. Projeyi fork yapın
2. Feature branch oluşturun (`git checkout -b feature/yeni-ozellik`)
3. Değişikliklerinizi commit edin
4. Branch'inizi push edin
5. Pull request gönderin

## Lisans

Bu proje MIT Lisansı altında lisanslanmıştır.

## İletişim

- **Hata Bildirimi**: [GitHub Issues](https://github.com/muhammetalisongur/KeyBot/issues)
- **Geliştirici**: Muhammet Ali Songur
- **Website**: [muhammetalisongur.com](https://muhammetalisongur.com) 