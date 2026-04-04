using Microsoft.EntityFrameworkCore;
using StudyHelper.API.Data;
using StudyHelper.API.Repository;
using StudyHelper.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("StudyHelperDb"));

builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<ITestService, TestService>();

//builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    //app.MapOpenApi();
}

app.UseCors();

app.MapControllers();

app.Run("http://localhost:5152");