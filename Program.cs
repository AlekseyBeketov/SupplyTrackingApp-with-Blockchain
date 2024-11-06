using Blockchain_Supply_Chain_Tracking_System.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using MongoDB.Driver;
using Blockchain_Supply_Chain_Tracking_System.Services; // Для класса MongoBatchService

var builder = WebApplication.CreateBuilder(args);

// Добавление контекста базы данных
builder.Services.AddDbContext<SupplyTrackingContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Добавление сервисов для MongoDB
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("MongoDbConnection");
    return new MongoClient(connectionString);
});

builder.Services.AddScoped<MongoBatchService>();
builder.Services.AddScoped<MongoUserGroupService>();

builder.Services.AddScoped(sp =>
{
    var mongoClient = sp.GetRequiredService<IMongoClient>();
    return mongoClient.GetDatabase("SupplyTrackingDb"); // Укажите имя вашей базы данных
});

// Добавление контроллеров с представлениями
builder.Services.AddControllersWithViews();

// Добавление аутентификации с использованием куков
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.Cookie.Name = "UserLoginCookie"; // Название куки
        options.LoginPath = "/Account/Login";    // Страница для логина
        options.AccessDeniedPath = "/Account/AccessDenied"; // Страница при отказе доступа
    });

// Добавление авторизации
builder.Services.AddAuthorization(options =>
{
    // Пример политики для администратора
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("admin"));
    options.AddPolicy("VendorOnly", policy => policy.RequireRole("vendor"));
    options.AddPolicy("CarrierOnly", policy => policy.RequireRole("carrier"));
    options.AddPolicy("ClientOnly", policy => policy.RequireRole("client"));
});

var app = builder.Build();

// Обработка ошибок и маршрутизация
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Включение аутентификации и авторизации
app.UseAuthentication();
app.UseAuthorization();
app.UseDeveloperExceptionPage();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();