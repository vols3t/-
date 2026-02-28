var builder = WebApplication.CreateBuilder(args);

// Добавляем контроллеры
builder.Services.AddControllers();

// Добавляем CORS (чтобы фронтенд мог обращаться)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://127.0.0.1:5500", "http://localhost:5500")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Добавляем OpenAPI (документация)
builder.Services.AddOpenApi();

var app = builder.Build();

// Настраиваем pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Включаем CORS
app.UseCors("AllowFrontend");

// Маппим контроллеры (все API эндпоинты будут в контроллерах)
app.MapControllers();

app.Run();