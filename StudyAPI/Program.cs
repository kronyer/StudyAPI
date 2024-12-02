using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using StudyAPI.Data;
using StudyAPI.Mapping;
using StudyAPI.Repository;
using StudyAPI.Repository.IRepository;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//Log.Logger = new LoggerConfiguration()
//    .MinimumLevel.Debug()
//    .WriteTo.File("log/villaLogs.txt", rollingInterval: RollingInterval.Day)
//    .CreateLogger();
//builder.Host.UseSerilog(); Se Quiser o Serilog!

builder.Services.AddControllers().AddNewtonsoftJson().AddXmlDataContractSerializerFormatters(); //Adicionar para o patch
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddSwaggerGen(opt =>
{
    opt.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Auth header using the Bearer scheme \r\n\r\n " +
        "Enter 'Bearer' [space] and then your token in the input below. \r\n\r\n" +
        "Example: \"Bearer 1111111\"",
        Name = "Auth",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Scheme = "Bearer"
    });
    opt.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>() // Add this line to provide the required 'value' parameter
        }
    });
    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "StudyAPI", Version = "v1.0" }); // Documentação para o select de versai
    opt.SwaggerDoc("v2", new OpenApiInfo { Title = "StudyAPI", Version = "v2.0" }); // Documentação para o select de versai
});

builder.Services.AddApiVersioning(opt =>
{
    opt.AssumeDefaultVersionWhenUnspecified = true;
    opt.DefaultApiVersion = new ApiVersion(1, 0);
    opt.ReportApiVersions = true; // para retornar os tipos suportados de versão no header de response
}

); //Versionamento

builder.Services.AddVersionedApiExplorer(opt =>
{
    opt.GroupNameFormat = "'v'VVV";
    opt.SubstituteApiVersionInUrl = true; // para substituir a versão na url para a default (v1 no caso), para usar outras versões =>
}); //Versionamento



builder.Services.AddDbContext<VillaDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IVillaRepository, VillaRepository>();
builder.Services.AddScoped<IVillaNumberRepository, VillaNumberRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddAutoMapper(typeof(MappingProfile));

var key = builder.Configuration.GetValue<string>("ApiSettings:Secret");
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
        ValidateIssuer = false,
        ValidateAudience = false,
    };
}); //Perfil de autenticação


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "StudyAPI v1");
        options.SwaggerEndpoint("/swagger/v2/swagger.json", "StudyAPI v2"); // para a versão 2 


    });
}


app.UseHttpsRedirection();

app.UseAuthentication(); //usar antes de autorização, pois antes de autorizar um método, ée preciso autenticar
app.UseAuthorization();

app.MapControllers();

app.Run();
