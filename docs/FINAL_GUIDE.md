# CorePOS — الدليل الشامل النهائي
## نظام إدارة المبيعات الاحترافي — Core Tech

**الإصدار:** 1.0.0  
**التاريخ:** 2024  
**المطور:** Core Tech — جوهرة مول، العاشر من رمضان

---

## 📊 ملخص المشروع الكامل

| Phase | الاسم | الحالة | الملفات |
|-------|-------|--------|---------|
| 1 | تحليل المتطلبات (SRS) | ✅ مكتمل | `CorePOS_Phase1_MASTER_SPEC.md` |
| 2 | تصميم قاعدة البيانات | ✅ مكتمل | `CorePOS_Phase2_DatabaseDesign.md` |
| 3 | معمارية الحل | ✅ مكتمل | `CorePOS_Phase3_Architecture.md` |
| 4 | هيكل الـ Solution | ✅ مكتمل | `CorePOS_Phase4_VS_Solution.md` |
| 5 | SQL Scripts | ✅ مكتمل | `CorePOS_Phase5_SQL_Scripts.zip` |
| 6 | Domain Layer | ✅ مكتمل | داخل `Phase6_7_8.zip` |
| 7 | Application Layer (CQRS) | ✅ مكتمل | `CorePOS_Phase7_ApplicationLayer.zip` |
| 8 | Infrastructure Layer | ✅ مكتمل | `CorePOS_Phase8_Infrastructure.zip` |
| 9 | WinForms UI | ✅ مكتمل | `CorePOS_Phase9_WinForms.zip` |
| 10 | Reports & Printing | ✅ مكتمل | `CorePOS_Phase10_Reports_Printing.zip` |
| 11 | Backup & Restore | ✅ مكتمل | داخل هذا الملف |
| 12 | License Module | ✅ مكتمل | داخل هذا الملف |

---

## 🏗️ معمارية المشروع

```
CorePOS.sln
├── src/
│   ├── CorePOS.Domain/           ← Phase 6: Entities, Enums, Interfaces
│   ├── CorePOS.Application/      ← Phase 7: CQRS, Commands, Queries, DTOs
│   ├── CorePOS.Infrastructure/   ← Phase 8-12: EF Core, Repositories, Services
│   ├── CorePOS.WinForms/         ← Phase 9-12: UI, Forms, Controls
│   └── CorePOS.Shared/           ← Common utilities
├── tools/
│   └── LicenseGenerator/         ← Phase 12: Activation code generator
└── sql/
    └── *.sql                      ← Phase 5 + 11 + 12 scripts
```

---

## 🛠️ التقنيات المستخدمة

| الطبقة | التقنية |
|--------|---------|
| UI | WinForms .NET 8, Arabic RTL |
| Architecture | Clean Architecture + CQRS + MediatR |
| ORM | Entity Framework Core 8 |
| Database | SQL Server Express/Standard |
| Reports | FastReport.OpenSource |
| Security | BCrypt, AES-256, HMAC-SHA256 |
| Logging | Serilog → File |
| DI | Microsoft.Extensions.DependencyInjection |
| Background | IHostedService (Auto Backup) |
| Hardware | ESC/POS (Thermal Printer) + WinAPI (Cash Drawer) |

---

## 📋 خطوات البناء والتشغيل

### الخطوة 1 — المتطلبات
```
✅ Visual Studio 2022 (v17.8+)
✅ .NET 8 SDK
✅ SQL Server 2019+ (Express مجاني)
✅ SQL Server Management Studio
```

### الخطوة 2 — تجهيز قاعدة البيانات
```sql
-- 1. افتح SSMS واتصل بالسيرفر
-- 2. شغّل الملفات بالترتيب:
01_CreateDatabase.sql       -- إنشاء قاعدة البيانات
02_DDL_GroupA_Security.sql  -- جداول الأمان والمستخدمين
03_DDL_GroupB_MasterData.sql -- جداول البيانات الأساسية
04_DDL_GroupC_People.sql    -- العملاء والموردين والموظفين
05_DDL_GroupG_Finance.sql   -- الخزنة والمالية
06_DDL_GroupD_Sales.sql     -- المبيعات
07_DDL_Groups_E_F.sql       -- المخزون والمشتريات
08_StoredProcedures.sql     -- الإجراءات المخزنة
09_Views.sql                -- الـ Views
10_SeedData.sql             -- البيانات الأساسية (دور Admin)
Phase11_BackupLog.sql       -- جدول سجل النسخ الاحتياطية
Phase12_License.sql         -- جدول الترخيص
```

### الخطوة 3 — فتح الـ Solution
```
1. افتح CorePOS.sln في Visual Studio
2. تأكد من references:
   WinForms → Application → Domain
   Infrastructure → Application → Domain
3. Build → Restore NuGet Packages
```

### الخطوة 4 — إعداد appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=CorePOS;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

### الخطوة 5 — تشغيل EF Migrations (إذا لم تستخدم SQL scripts)
```bash
cd src/CorePOS.Infrastructure
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### الخطوة 6 — Build & Run
```bash
cd src/CorePOS.WinForms
dotnet build
dotnet run
```

### الخطوة 7 — بيانات الدخول الافتراضية
```
اسم المستخدم: admin
كلمة المرور:  Admin@123
```

---

## 📦 NuGet Packages الكاملة

### CorePOS.Infrastructure
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools"     Version="8.0.0" />
<PackageReference Include="FastReport.OpenSource"                   Version="2024.1.0" />
<PackageReference Include="FastReport.OpenSource.Export.PdfSimple"  Version="2024.1.0" />
<PackageReference Include="FastReport.OpenSource.Export.OoXML"      Version="2024.1.0" />
<PackageReference Include="BCrypt.Net-Next"                         Version="4.0.3" />
<PackageReference Include="ZXing.Net"                               Version="0.16.9" />
<PackageReference Include="MediatR"                                 Version="12.2.0" />
<PackageReference Include="Microsoft.Extensions.Caching.Memory"     Version="8.0.0" />
<PackageReference Include="Serilog"                                 Version="3.1.1" />
```

### CorePOS.WinForms
```xml
<PackageReference Include="FastReport.OpenSource"                   Version="2024.1.0" />
<PackageReference Include="Microsoft.Extensions.Hosting"            Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
<PackageReference Include="MediatR"                                 Version="12.2.0" />
<PackageReference Include="Serilog.Extensions.Hosting"             Version="8.0.0" />
<PackageReference Include="Serilog.Sinks.File"                     Version="6.0.0" />
```

---

## 🖥️ الشاشات الكاملة (15 شاشة + 25 dialog)

| الشاشة | الوصف |
|--------|--------|
| LoginForm | تسجيل الدخول |
| MainForm | الشاشة الرئيسية (Sidebar + TopBar) |
| DashboardForm | لوحة التحكم (KPIs + آخر الفواتير) |
| **POSForm** | **شاشة الكاشير الرئيسية** |
| SalesListForm | فواتير المبيعات |
| PurchasesListForm | فواتير المشتريات |
| InventoryForm | المخزون (جرد + تحويلات + تسويات) |
| FinanceForm | الخزنة والمالية |
| CustomersForm | العملاء |
| SuppliersForm | الموردين |
| ProductsForm | الأصناف |
| EmployeesForm | الموظفين |
| ReportsFormV2 | 14 تقرير متكامل |
| SettingsForm | الإعدادات |
| BackupForm | النسخ الاحتياطي والاستعادة |
| LicenseForm | معلومات وتفعيل الترخيص |

---

## 🖨️ الطباعة والتقارير

### أحجام الطباعة المدعومة
| الحجم | الوصف | الاستخدام |
|-------|--------|-----------|
| 58mm | طابعة حرارية صغيرة | محلات صغيرة |
| 80mm | طابعة حرارية عريضة | الاستخدام الأكثر شيوعاً |
| A5 | نصف ورقة | فواتير متوسطة |
| A4 | ورقة كاملة | فواتير رسمية |

### التقارير المتاحة (14 تقرير)
```
📈 مبيعات الفترة       📅 مبيعات اليوم
💹 الأرباح             👥 مديونية العملاء
📋 كشف حساب عميل      🚚 مستحقات الموردين
📋 كشف حساب مورد      📦 المخزون الحالي
⚠  الأصناف الناقصة    🐌 الأصناف الراكدة
💸 المصروفات           🏧 حركة الخزنة
⏰ تقرير الوردية       👤 أداء الكاشيرين
```

---

## 🔒 نظام الترخيص

### للعميل
1. يشغّل البرنامج → يظهر 7 أيام تجريبية
2. يتواصل مع Core Tech ويرسل **معرّف الجهاز**
3. يستلم **كود التفعيل** ويدخله في شاشة الترخيص
4. البرنامج يعمل للمدة المتفق عليها

### لـ Core Tech (توليد الكود)
```bash
cd tools/LicenseGenerator
dotnet run -- "MACHINE_ID" "Standard" 365 "اسم العميل"
```

---

## 💾 النسخ الاحتياطي

### يدوي
- من شاشة النسخ الاحتياطي → نسخ يدوي
- اختيار المجلد + تشفير اختياري + رفع Google Drive

### تلقائي
| النوع | الجدول الافتراضي |
|-------|-----------------|
| يومي | كل يوم 11 مساءً |
| أسبوعي | كل جمعة 10 مساءً |
| شهري | أول الشهر 1 صباحاً |

### Google Drive
```bash
# تثبيت rclone
winget install Rclone.Rclone

# إعداد Google Drive
rclone config
# اسم الـ remote: gdrive

# اختبار
rclone ls gdrive:
```

---

## ⌨️ اختصارات لوحة المفاتيح (شاشة الكاشير)

| المفتاح | الوظيفة |
|---------|---------|
| F10 | إتمام الدفع |
| F8 | تعليق الفاتورة |
| F9 | استرجاع فاتورة معلقة |
| F5 | تحديد عميل |
| Enter (في حقل الباركود) | بحث فوري / إضافة صنف |
| Delete (في جدول الكارت) | حذف الصنف المحدد |
| Esc | مسح حقل البحث |

---

## 🔧 معالجة المشكلات الشائعة

### مشكلة: البرنامج لا يفتح
```
✓ تأكد من تثبيت .NET 8 Runtime
✓ تحقق من appsettings.json
✓ تحقق من اتصال SQL Server
✓ راجع ملف logs/corepos-*.log
```

### مشكلة: الطابعة لا تعمل
```
✓ تأكد من اسم الطابعة في الإعدادات
✓ شغّل اختبار الطباعة من الإعدادات
✓ تحقق من تثبيت درايفر الطابعة
✓ جرب الـ GDI fallback (تلقائي)
```

### مشكلة: فشل النسخ الاحتياطي
```
✓ تأكد من صلاحيات الكتابة في المجلد
✓ تحقق من مساحة القرص
✓ راجع ملف logs للأخطاء
✓ تأكد من صلاحية حساب SQL Server
```

---

## 📞 الدعم الفني

**Core Tech**  
📍 جوهرة مول، العاشر من رمضان، محافظة الشرقية  
🛠️ خدمات: كاميرات مراقبة، شبكات، أنظمة كاشير، ERP

---

*CorePOS v1.0 — All Phases Complete ✅*  
*Core Tech © 2024 — Commercial License Required*
