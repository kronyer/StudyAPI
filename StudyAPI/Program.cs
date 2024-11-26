using Microsoft.EntityFrameworkCore;
using Serilog;
using StudyAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//Log.Logger = new LoggerConfiguration()
//    .MinimumLevel.Debug()
//    .WriteTo.File("log/villaLogs.txt", rollingInterval: RollingInterval.Day)
//    .CreateLogger();
//builder.Host.UseSerilog(); Se Quiser o Serilog!

builder.Services.AddControllers(options => options.ReturnHttpNotAcceptable=true).AddNewtonsoftJson().AddXmlDataContractSerializerFormatters(); //Adicionar para o patch
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<VillaDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    //app.UseSwaggerUI(options =>
    //{
    //    options.SwaggerEndpoint("/openapi/v1.json", "StudyAPI v1");
    //});
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
