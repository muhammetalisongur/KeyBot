# KeyBot - Tuş Otomasyonu Uygulaması

**Windows için profesyonel tuş otomasyonu çözümü**

KeyBot, Windows işletim sistemi için geliştirilmiş kullanıcı dostu bir tuş otomasyonu uygulamasıdır. Oyunlar, test süreçleri ve tekrarlayan görevler için idealdir.

## Özellikler

### Tuş ve Fare Desteği
- **Özel Tuşlar**: Space, Enter, Tab, Escape, Backspace, Delete
- **Alfanumerik**: A-Z harfleri, 0-9 rakamları (çoklu klavye dili desteği)
- **Fonksiyon Tuşları**: F1-F12
- **Navigasyon**: Yön tuşları (↑, ↓, ←, →)
- **Modifier Tuşları**: Ctrl, Shift, Alt, Windows
- **NumPad**: Sayı tuş takımı tuşları
- **Fare İşlemleri**: Sol tık, sağ tık, orta tık, tekerlek yukarı/aşağı, çift tık, ekstra fare düğmeleri
- **Konum-Tabanlı Fare İşlemleri**: Belirli koordinatlarda fare işlemleri

### Gelişmiş Yakalama Sistemi
- **Gerçek Zamanlı Tuş Yakalama**: ⌨️ butonu ile tüm klavye tuşlarını anında yakala
- **Çoklu Klavye Dili Desteği**: Aktif klavye dili algılama (Windows+Space desteği)
- **Gerçek Zamanlı Fare Yakalama**: 🖱️ butonu ile tüm fare işlemlerini anında yakala
- **Konum Yakalama**: 📍 butonu ile ekran üzerindeki herhangi bir konumu yakala
- **Özel İsimlendirme**: Bilinmeyen tuş/fare işlemleri için özel isimler belirleme
- **Vazgeçme Modu**: Yakalama sırasında ESC ile iptal etme
- **Akıllı Algılama**: 100+ tuş ve fare işlemi desteği
- **Kalıcı Özel Tuşlar**: Yakalanan özel tuşlar otomatik kaydedilir

### Gelişmiş Kontroller
- **Tek İşlem Modu**: Seçilen tuş veya fare işlemini belirli aralıklarla gerçekleştir
- **Fare Modu**: Özel fare işlemi modu
- **Çoklu İşlem Modu**: Tuş ve fare işlemlerini karıştırarak özel diziler oluştur
- **Drag & Drop Sıralama**: Çoklu işlem modunda öğeleri sürükleyerek yeniden sıralama
- **Konum-Tabanlı Otomasyon**: Fare işlemlerini belirli ekran koordinatlarında gerçekleştir
- **Hassas Zamanlama**: 0.1 - 60.0 saniye arası aralık ayarı
- **Tekrar Kontrolü**: 1-1000 arası belirli tekrar veya sınırsız mod
- **Geri Sayım**: 3 saniye sesli geri sayım ile güvenli başlatma
- **Ses Efektleri**: Başlatma, durdurma ve geri sayım sesleri
- **Ayar Kalıcılığı**: Tüm ayarlar JSON formatında otomatik kaydedilir

### Kullanıcı Arayüzü
- Sezgisel ve temiz tasarım
- Gerçek zamanlı durum gösterimi
- İlerleme çubuğu
- Hand cursor ile tıklanabilir alanların belirtilmesi
- Sesli uyarılar ve geri bildirimler
- Tooltip sistemi ile kullanım ipuçları
- Görsel geri bildirim (yakalama sırasında renk değişimi)
- Canlı konum takibi (fare koordinatları)
- Akıllı buton aktivasyonu (mod bazlı)

## Sistem Gereksinimleri

- **İşletim Sistemi**: Windows 10/11
- **Framework**: .NET 8.0
- **RAM**: 100 MB minimum
- **Disk Alanı**: 50 MB

## Kurulum

### Hazır Uygulama İndir (Önerilen)
**En kolay yöntem**: [Releases](https://github.com/muhammetalisongur/KeyBot/releases) sayfasından en son sürümü indirin.

**Kurulum Adımları:**
1. `KeyBot.exe` dosyasını indirin (~150 MB)
2. İstediğiniz bir klasöre koyun (örnek: `C:\Program Files\KeyBot\`)
3. Çift tıklayarak çalıştırın
4. Tüm gereksinimler dahil - ek kurulum gerekmez

**Not**: Antivirüs yazılımı uyarı verebilir, güvenli listesine ekleyin.

### Kaynak Koddan Derleme
```powershell
git clone https://github.com/muhammetalisongur/KeyBot.git
cd KeyBot
dotnet build
dotnet run
```

### Executable Oluşturma
```powershell
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o ./publish
```

## Kullanım

### Temel Kullanım
1. **Mod Seçimi**: Mod seçin (tek işlem, fare veya çoklu işlem)
2. **İşlem Seçimi**: Tuş veya fare işlemi seçin
   - Manuel seçim yapın VEYA
   - ⌨️/🖱️ butonları ile gerçek zamanlı yakalama yapın
   - 📍 butonu ile konum yakala (fare işlemleri için)
3. **Zaman Ayarları**: Aralık ve tekrar sayısını belirleyin
4. **Başlatma**: Başlat butonuna tıklayın
   - 3 saniye sesli geri sayım başlar
   - Bu süre içinde hedef uygulamaya geçiş yapın (Alt+Tab)
   - Otomasyon otomatik olarak başlar
5. **Durdurma**: İstediğiniz zaman durdur butonuna tıklayın

### Gerçek Zamanlı Yakalama

#### Tuş Yakalama
1. ⌨️ butonuna tıklayın
2. Buton "Vazgeç" moduna geçer ve turuncu renk alır
3. Yakalamak istediğiniz klavye tuşuna basın
4. **Çoklu Dil Desteği**: Aktif klavye dili otomatik algılanır (ör: "Q (tr-TR)" veya "Q (en-US)")
5. Tuş otomatik olarak listeye eklenir ve kaydedilir
6. Bilinmeyen tuşlar için özel isim belirleyebilirsiniz
7. **İptal**: ESC tuşu ile yakalamayı iptal edebilirsiniz

#### Fare Yakalama
1. 🖱️ butonuna tıklayın
2. Buton "Vazgeç" moduna geçer ve turuncu renk alır
3. Yakalamak istediğiniz fare işlemini yapın
4. **Not**: Sadece 🖱️ butonu dışındaki alanlar yakalanır
5. İşlem otomatik olarak listeye eklenir ve kaydedilir
6. **İptal**: ESC tuşu ile yakalamayı iptal edebilirsiniz

#### Konum Yakalama (YENİ!)
1. Fare modu veya çoklu işlem modunda fare işlemi seçin
2. 📍 butonuna tıklayın (sadece fare işlemleri seçiliyken aktif)
3. Buton "Durdur" moduna geçer ve kırmızı renk alır
4. Ekran üzerinde istediğiniz konuma fareyi götürün
5. **SPACE** tuşu ile konumu kaydedin VEYA **ESC** ile iptal edin
6. Konum koordinatları gösterilir ve kaydedilir
7. Otomasyon sırasında fare işlemi bu konumda gerçekleştirilir

### Hedef Uygulama Seçimi
**Önemli**: KeyBot tuş komutlarını aktif pencereye gönderir. Bu nedenle:
- Başlat butonuna tıkladıktan sonra 3 saniye süreniz vardır
- Bu süre içinde **Alt+Tab** ile hedef uygulamaya geçin
- Veya fare ile hedef uygulamanın penceresine tıklayın
- Otomasyon seçilen uygulamada çalışmaya başlar

### Fare Modu
- Fare modunu seçin
- İstediğiniz fare işlemini seçin:
  - **Sol Tık**: Mevcut imlec konumunda veya yakalanan konumda sol tık
  - **Sağ Tık**: Mevcut imlec konumunda veya yakalanan konumda sağ tık
  - **Orta Tık**: Mevcut imlec konumunda veya yakalanan konumda orta tık
  - **Tekerlek Yukarı**: Yukarı kaydırma
  - **Tekerlek Aşağı**: Aşağı kaydırma
  - **Çift Tık**: Hızlı çift sol tık
- **Konum Yakalama**: 📍 butonu ile belirli bir ekran konumunu hedefleyin

### Çoklu İşlem Modu
- Çoklu işlem modunu seçin
- İşlem listesine tuş veya fare işlemleri ekleyin
- Her işlem için ayrı gecikme süresi ayarlayın
- **Drag & Drop**: Öğeleri fare ile sürükleyerek sırasını değiştirin
- İşlem sırasını istediğiniz gibi düzenleyin
- **Karma Diziler**: Tuş basma + konum-tabanlı fare tıklama + tekerlek kaydırma kombinasyonları oluşturun
- **Konum-Tabanlı Aksiyonlar**: Fare işlemlerini belirli koordinatlarda gerçekleştirin

### Çoklu Klavye Dili Desteği (YENİ!)
- Windows'ta aktif klavye dili otomatik algılanır
- Tuş yakalama sırasında dil bilgisi otomatik eklenir
- **Örnek**: Türkçe klavyede "Q" → "Q (tr-TR)", İngilizce klavyede "Q" → "Q (en-US)"
- **Windows+Space** ile klavye dili değiştirdikten sonra yakalanan tuşlar yeni dilde kaydedilir
- Farklı dillerdeki aynı tuşlar ayrı ayrı saklanır

### Kullanım Senaryoları
- **Oyun AFK**: Karakterin aktif kalması için (tuş veya fare)
- **Form Doldurma**: Alanlar arası otomatik geçiş
- **Test Otomasyonu**: Tekrarlayan test adımları ve UI testleri
- **Erişilebilirlik**: Fiziksel kısıtlamalarda yardımcı araç
- **Web Tarama**: Otomatik sayfa kaydırma (tekerlek)
- **Tıklama Oyunları**: Sürekli tıklama gerektiren oyunlar
- **Çoklu Dil Çalışma**: Farklı klavye dilleri arasında geçiş yaparak çalışma
- **Konum-Tabanlı Otomasyon**: Belirli ekran bölgelerinde otomatik işlemler
- **Grafik Tasarım**: Tekrarlayan araç seçimi ve konumlandırma

## Güvenlik

- Tamamen lokal çalışır, internet bağlantısı gerektirmez
- Kullanıcı verisi toplanmaz
- Windows API kullanır, sistem güvenliği korunur
- Açık kaynak kodlu, şeffaf geliştirme
- Ayarlar güvenli JSON formatında saklanır

**Uyarı**: Bazı antivirüs yazılımları false positive uyarısı verebilir. Bu durumda güvenlik yazılımınıza istisna ekleyin.

## Geliştirme

Bu proje .NET 8.0 Windows Forms teknolojisiyle geliştirilmiştir.

### Proje Yapısı
```
KeyBot/
├── Models/          # Veri modelleri (AppSettings, KeySequenceItem)
├── MainForm.cs      # Ana form logic (75+ metot, 1800+ satır)
├── Program.cs       # Uygulama başlatma
├── Resources/       # Kaynaklar
└── KeyBot_Settings.json  # Kullanıcı ayarları
```

### Teknik Özellikler
- **Çoklu Dil API**: Windows Keyboard Layout API entegrasyonu
- **Konum API**: Windows Cursor Position API
- **JSON Serialization**: Newtonsoft.Json ile ayar yönetimi
- **Drag & Drop**: Windows Forms native drag-drop desteği
- **Timer Yönetimi**: Hassas zamanlama için System.Windows.Forms.Timer
- **Event-Driven**: Asenkron event handling mimarisi

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