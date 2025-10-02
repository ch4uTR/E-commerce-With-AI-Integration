# 🛒 ASP.NET Core E-Ticaret Sitesi

Kullanıcıların ürünleri sepete veya favorilere ekleyebildiği, yorum paylaşabildiği;  
admin tarafında ise **PowerBI benzeri bir dashboard** ile KPI’ların takip edilebildiği,  
**Türkiye haritası (SVG)** üzerinden il bazlı detayların ve grafiklerin görüntülenebildiği,  
**FastAPI servisi** entegrasyonu ile yorum yönetimi ve ürün açıklama özetleme gibi işlemleri otomatikleştiren modern bir e-ticaret uygulaması.

---

## 🚀 Özellikler

### 👤 Kullanıcı Tarafı
- ✅ Kullanıcı kaydı ve kimlik doğrulama (Identity)  
- 🔍 Ürün arama, filtreleme ve sıralama  
- 🛒 Sepet ve favori listesi yönetimi  
- 💳 Döviz kurlarıyla ödeme desteği (TCMB SOAP & REST servis entegrasyonu)  
- 📝 Yorum yapma ve inceleme ekleme  
- 🎟 İndirim kuponu desteği  
- 📦 Sipariş takibi (Pending, Processing, Delivered, vb.)  

### 🛠 Admin Tarafı
- 📊 KPI takip dashboardu (PowerBI kıvamında görseller)  
- 🗺 Türkiye haritası üzerinden il bazlı satış ve kullanıcı analizleri  
- 🤖 FastAPI servisi ile:
  - Yorumları otomatik sınıflandırma & onaylama  
  - Onaylı yorumlardan ürün açıklamalarına özet çıkarma  
- 🔔 SignalR ile gerçek zamanlı bildirimler (örneğin, yorum sınıflandırma tamamlandığında)  

---

## 🛠 Kullanılan Teknolojiler
- **Backend:** ASP.NET Core (.NET 9), EF Core, SQL Server  
- **Kimlik Doğrulama:** ASP.NET Core Identity (User & Admin rolleri)  
- **Frontend:** Server-side rendering + SPA mantığı (JavaScript Fetch API)  
- **Gerçek Zamanlı İletişim:** SignalR  
- **Servisler:**  
  - SOAP & REST servisleri (TCMB döviz kurları entegrasyonu)  
  - FastAPI (yorum sınıflandırma ve özetleme servisi)  
