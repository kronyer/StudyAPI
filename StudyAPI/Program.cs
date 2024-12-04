using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using StudyAPI.Data;
using StudyAPI.Mapping;
using StudyAPI.Models;
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

builder.Services.AddControllers(opt =>
{
    opt.CacheProfiles.Add("120SecondsDuration", new CacheProfile
    {
        Duration = 120
    });
})
.AddNewtonsoftJson()
.AddXmlDataContractSerializerFormatters(); // Adicionar para o patch

builder.Services.AddSwaggerGen(opt =>
{
    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header usando o esquema Bearer.
                        \r\n\r\nInforme 'Bearer' [espaço] e então seu token.
                        \r\n\r\nExemplo: 'Bearer 12345abcdef'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    opt.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "Bearer",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
    opt.SwaggerDoc("v1", new OpenApiInfo { Title = "StudyAPI", Version = "v1.0" });
    opt.SwaggerDoc("v2", new OpenApiInfo { Title = "StudyAPI", Version = "v2.0" });
});

builder.Services.AddIdentity<VillaUser, IdentityRole>().AddEntityFrameworkStores<VillaDbContext>();


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
builder.Services.AddResponseCaching();

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
        options.SwaggerEndpoint("/swagger/v2/swagger.json", "StudyAPI v2"); // para a versão 2 . Ordem decrescente!
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "StudyAPI v1");


    });
}


app.UseHttpsRedirection();

app.UseAuthentication(); //usar antes de autorização, pois antes de autorizar um método, ée preciso autenticar
app.UseAuthorization();

app.MapControllers();

app.Run();
