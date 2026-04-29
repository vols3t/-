// using Microsoft.EntityFrameworkCore;
// using StudyHelper.API.Data;
// using StudyHelper.API.Repository;
// using StudyHelper.API.Services;
//
// var builder = WebApplication.CreateBuilder(args);
//
// builder.Services.AddControllers()
//     .AddJsonOptions(options =>
//     {
//         options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
//     });
//
// // builder.Services.AddCors(options =>
// // {
// //     options.AddDefaultPolicy(policy =>
// //     {
// //         policy.AllowAnyOrigin()
// //             .AllowAnyHeader()
// //             .AllowAnyMethod();
// //     });
// // });
//
// // В блоке конфигурации сервисов:
// builder.Services.AddCors(options =>
// {
//     options.AddPolicy("AllowAll", policy =>
//     {
//         policy.AllowAnyOrigin()
//             .AllowAnyHeader()
//             .AllowAnyMethod();
//     });
// });
//
// if (builder.Environment.IsProduction())
// {
//     var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
//     builder.Services.AddDbContext<ApplicationDbContext>(options =>
//         options.UseNpgsql(connectionString));
// }
// else
// {
//     builder.Services.AddDbContext<ApplicationDbContext>(options =>
//         options.UseInMemoryDatabase("StudyHelperDb"));
// }
//
// builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
// builder.Services.AddScoped<ITestService, TestService>();
//
// //builder.Services.AddOpenApi();
//
// // var app = builder.Build();
// //
// // if (app.Environment.IsDevelopment())
// // {
// //     //app.MapOpenApi();
// // }
// //
// // // app.UseCors();
// // app.UseCors("AllowAll"); // Обязательно передай имя политики
// //
// //
// // app.MapControllers();
// //
// // app.Use(async (context, next) =>
// // {
// //     context.Response.Headers.Add("Content-Security-Policy",
// //         "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline';");
// //     await next();
// // });
// //
// // app.Run();
//
// var app = builder.Build();
//
// app.UseDefaultFiles(); 
//
// app.UseStaticFiles(); 
//
// app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
//
// app.MapControllers();
//
// app.Run();

using Microsoft.EntityFrameworkCore;
using StudyHelper.API.Data;
using StudyHelper.API.Repository;
using StudyHelper.API.Services;

var builder = WebApplication.CreateBuilder(args);

var frontendUrl = builder.Configuration["FrontendUrl"] ?? "http://localhost:63343";

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(frontendUrl) 
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

if (builder.Environment.IsProduction())
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
}
else
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseInMemoryDatabase("StudyHelperDb"));
}

builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<ITestService, TestService>();

var app = builder.Build();

app.UseCors("AllowFrontend"); 

app.MapControllers();

app.Run();