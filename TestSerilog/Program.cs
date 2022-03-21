using Serilog;
using TestSerilog.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<ITestService, TestService>();
builder.Services.AddScoped<ILogQueryService, LogQueryService>();

var serilogConfig = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration);

builder.Services.AddLogging(config => config.AddSerilog(serilogConfig.CreateLogger()));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
