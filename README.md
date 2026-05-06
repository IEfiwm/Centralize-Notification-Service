# Notification Center (Back-End)

این ریپو یک سرویس «مرکز اعلان‌ها» است که درخواست‌های ارسال پیام را از طریق API دریافت می‌کند، در دیتابیس ثبت می‌کند و سپس از طریق RabbitMQ به یک Consumer می‌سپارد تا پیام را با Provider مناسب ارسال کند.

## اجزای پروژه

کد در مسیر `src/` به چند پروژه تقسیم شده است:

- **`CNS.Api`**: Web API (ASP.NET) برای دریافت درخواست‌ها و صف‌بندی پیام‌ها
- **`CNS.Consumer`**: Worker Service که پیام‌ها را از RabbitMQ مصرف می‌کند و ارسال را انجام می‌دهد
- **`CNS.Application`**: منطق کاربردی (Command Handlerها و Abstractionها)
- **`CNS.Infrastructure`**: پیاده‌سازی زیرساخت (EF Core/Postgres، RabbitMQ، Providerها، Security)
- **`CNS.Domain`**: مدل‌های دامنه (مثل `MessageLog`)
- **`CNS.Contracts`**: DTOها و قرارداد رویدادها (مثل `SendMessageRequested`)

## جریان کلی (Flow)

1) کلاینت به API درخواست ارسال پیام می‌دهد.  
2) API یک `RequestId` تولید می‌کند، رکورد `MessageLog` را در Postgres ذخیره می‌کند و رویداد `SendMessageRequested` را روی RabbitMQ منتشر می‌کند.  
3) Consumer پیام را از صف می‌خواند، وضعیت را به Processing تغییر می‌دهد، Provider را resolve می‌کند و ارسال را انجام می‌دهد.  
4) نتیجه در `MessageLog` (Sent/Failed) ثبت می‌شود.

## پیش‌نیازها

- **.NET SDK** مطابق TargetFramework پروژه‌ها (در حال حاضر `net10.0`)
- **PostgreSQL**
- **RabbitMQ**

## تنظیمات (Configuration)

هر دو پروژهٔ `CNS.Api` و `CNS.Consumer` از فایل‌های زیر استفاده می‌کنند:

- `appsettings.json` (پایه)
- `appsettings.{Environment}.json` (اختیاری)
- **`appsettings.local.json`** (اختیاری، برای تنظیمات لوکال/سکرت‌ها)

در `Program.cs` هر دو پروژه، `appsettings.local.json` به صورت optional اضافه شده است؛ توصیه می‌شود **سکرت‌ها فقط** در همین فایل قرار بگیرند.

### تنظیمات مهم

- **Postgres**: `ConnectionStrings:Postgres`
- **RabbitMQ**: بخش `RabbitMQ` شامل `HostName`، `Port`، `VirtualHost`، `UserName`، `Password` و `QueueName`
- **Providers**:
  - `Providers:Sms:ProviderName` تعیین می‌کند SMS با کدام Provider ارسال شود
  - `Providers:Sms:ApiKey` و `Providers:Sms:Sender` (بسته به Provider)
  - برای Providerهای خاص (مثلاً `Trez`) بخش اختصاصی زیر `Providers:Sms:Trez` وجود دارد

### نکتهٔ امنیتی

اگر در تنظیمات فعلی پروژه credential واقعی دارید، بهتر است آن‌ها را از `appsettings.json` حذف کرده و به `appsettings.local.json` منتقل کنید و مطمئن شوید این فایل در git نرود.

نمونه `appsettings.local.json` پیشنهادی:

```json
{
  "ConnectionStrings": {
    "Postgres": "Host=localhost;Port=5432;Database=notification_center;Username=postgres;Password=postgres"
  },
  "RabbitMQ": {
    "HostName": "localhost",
    "Port": 5672,
    "VirtualHost": "/",
    "UserName": "guest",
    "Password": "guest",
    "QueueName": "notification.send"
  },
  "Seed": {
    "AdminUser": "admin",
    "AdminPassword": "admin123"
  }
}
```

## اجرای پروژه

### اجرای API

از ریشه ریپو:

```bash
dotnet run --project src/CNS.Api/CNS.Api.csproj
```

آدرس‌های پیش‌فرض در حالت Development (طبق `launchSettings.json`):

- **HTTPS**: `https://localhost:7163`
- **HTTP**: `http://localhost:5226`

### اجرای Consumer

در یک ترمینال جدا:

```bash
dotnet run --project src/CNS.Consumer/CNS.Consumer.csproj
```

## احراز هویت (Basic Auth)

تمام Controllerها از `ApiControllerBase` ارث‌بری می‌کنند و `[Authorize]` فعال است؛ بنابراین همهٔ endpointها نیاز به Basic Auth دارند.

فرمت هدر:

- `Authorization: Basic base64(username:password)`

کاربر/پسورد (seed) از `Seed:AdminUser` و `Seed:AdminPassword` خوانده می‌شود.

## API

Base route فعلی: `messages`

### 1) صف‌بندی پیام (عمومی)

`POST /messages`

Body: `SendMessageRequestDto`

- `channel`: یکی از مقادیر موجود در `CNS.Contracts.Channels` مثل `sms`, `email`, `telegram`, `bale`
- `recipient`: گیرنده (مثلاً شماره موبایل)
- `subject`: اختیاری
- `body`: متن پیام
- `metadata`: اختیاری (key/value)
- `providerHint`: اختیاری (برای انتخاب Provider خاص)

Response:

```json
{ "requestId": "<id>", "status": "Queued" }
```

نمونه curl:

```bash
curl -X POST "https://localhost:7163/messages" ^
  -H "Content-Type: application/json" ^
  -H "Authorization: Basic <BASE64>" ^
  -d "{\"channel\":\"sms\",\"recipient\":\"09120000000\",\"subject\":null,\"body\":\"Hello\",\"metadata\":null,\"providerHint\":null}"
```

### 2) ارسال سریع SMS (endpoint ساده‌تر)

`POST /messages/sms/quick`

Body: `QuickSendSmsRequestDto` (حداقل فیلدها)

- `phone`: اجباری
- `message`: اجباری
- `providerHint`: اختیاری

اگر `phone` یا `message` خالی باشد `400 BadRequest` برمی‌گرداند. در غیر این صورت، پیام را با `channel=sms` صف‌بندی می‌کند.

## RabbitMQ Payload (Event Contract)

رویداد منتشرشده روی صف: `SendMessageRequested` (در `CNS.Contracts`)

شامل:
- `RequestId`
- `Channel`
- `Recipient`
- `Subject`
- `Body`
- `Metadata`
- `ProviderHint`
- `EnqueuedAtUtc`

## وضعیت پیام‌ها

ثبت وضعیت‌ها در جدول/مدل `MessageLog` انجام می‌شود و Consumer با موفق/ناموفق شدن ارسال، وضعیت را به‌روزرسانی می‌کند.

## توسعه و عیب‌یابی

- **OpenAPI**: در حالت Development، OpenAPI map می‌شود (در `CNS.Api/Program.cs`)
- **Migration/DB Init**: در شروع هر دو برنامه `DatabaseInitializer.InitializeAsync(...)` اجرا می‌شود

## Roadmap

هدف این نقشه‌راه: اضافه‌کردن Providerهای مختلف برای **SMS** و همچنین کانال‌های **Email**، **Bale**، **Slack** و **Firebase** با یک مسیر استاندارد (ثبت لاگ، صف‌بندی، ارسال، و رصد نتیجه).

### فاز 0 — آماده‌سازی پایه (پیشنهادی قبل از اضافه‌کردن کانال‌ها)

- **Contract واحد برای Providerها**: اطمینان از این‌که همه Providerها با `IMessageProvider` و `MessageContext` کار می‌کنند (بدون منطق شرطی در Consumer)
- **Resolver قابل توسعه**: انتخاب Provider بر اساس `Channel` و `ProviderHint` (پیکربندی‌محور)
- **Observability**: لاگ‌های ساخت‌یافته + Correlation با `RequestId`
- **Retries/Poison handling**: سیاست retry و مدیریت پیام‌های مشکل‌دار (در RabbitMQ، DLQ یا سیاست جایگزین)
- **Status API (اختیاری ولی کاربردی)**: endpoint برای گرفتن وضعیت `RequestId` از `MessageLog`

### فاز 1 — SMS با Providerهای مختلف

خروجی مورد انتظار: امکان انتخاب Provider SMS با `Providers:Sms:ProviderName` یا `providerHint` در درخواست.

- **Provider 1 (Sample/Mock)**: مناسب برای Development و تست
- **Provider 2 (Trez)**: تکمیل/پایدارسازی تنظیمات، خطاها و پاسخ‌ها
- **Provider 3 (Provider جدید)**: اضافه‌کردن یک Provider دیگر (مثلاً Kavenegar/Melipayamak/…)
- **Fallback Chain (پیشنهادی)**: اگر Provider اصلی fail شد، ارسال با Provider دوم (قابل کنترل با config)
- **قوانین اعتبارسنجی گیرنده**: نرمال‌سازی شماره موبایل (مثلاً E.164) و validation

### فاز 2 — Email

خروجی مورد انتظار: `channel=email` با حداقل یک Provider ایمیل.

- **Email Provider**: SMTP یا سرویس‌های API (SendGrid/Mailgun/…)
- **Template/Subject**: پشتیبانی از `Subject` و قالب‌بندی Body
- **Recipient validation**: اعتبارسنجی ایمیل + محدودیت‌های نرخ ارسال (اختیاری)

### فاز 3 — Bale Bot (ارسال پیام داخل ربات بله)

خروجی مورد انتظار: `channel=bale` با provider اختصاصی Bot.

- **Auth/Token management**: ذخیره امن توکن و refresh (در صورت نیاز)
- **شناسه گیرنده**: قرارداد `recipient` برای Bale (chatId/userId) + نرمال‌سازی
- **ارسال پیام**: پیاده‌سازی Provider با API بله

### فاز 4 — Slack

خروجی مورد انتظار: `channel=<slack>` (پیشنهاد: اضافه‌کردن مقدار `slack` به `CNS.Contracts.Channels`) و ارسال به channel/user.

- **Slack App/Webhook**: انتخاب بین Incoming Webhook یا Bot Token
- **Destination mapping**: قرارداد `recipient` (channelId/userId/webhook alias)
- **Rate limit handling**: رعایت محدودیت‌های Slack و backoff

### فاز 5 — Firebase (Push Notification)

خروجی مورد انتظار: `channel=<firebase>` (پیشنهاد: اضافه‌کردن مقدار `firebase` به `CNS.Contracts.Channels`) و ارسال push با FCM.

- **FCM credentials**: مدیریت امن سرویس‌اکانت/کلیدها
- **Recipient contract**: `recipient` = device token / topic
- **Payload**: نگاشت `Metadata` به data payload
- **Platform-specific options**: تنظیمات Android/iOS (اختیاری)

### فاز 6 — تست و استانداردسازی

- **Integration tests**: تست end-to-end برای queue + DB + provider mock
- **Contract tests**: برای Providerها (ورودی/خروجی یکسان)
- **Load/throughput**: تنظیم prefetch و concurrency برای Consumer
- **Idempotency**: جلوگیری از ارسال تکراری در retryها (در حد نیاز پروژه)

## ساختار پوشه‌ها (خلاصه)

```
src/
  CNS.Api/
  CNS.Consumer/
  CNS.Application/
  CNS.Infrastructure/
  CNS.Domain/
  CNS.Contracts/
```

