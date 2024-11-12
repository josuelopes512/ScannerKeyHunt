
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using ScannerKeyHunt.IoC;

namespace ScannerKeyHunt
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddInfrastructure();
            builder.Services.AddServices();
            builder.Services.AddAuthenticationAndAuthorization();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("EnableCORS", builder =>
                {
                    builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod().Build();
                });
                options.AddPolicy("AllowAllOrigins", builder =>
                {
                    builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod().Build();
                });
                options.AddPolicy(name: "MyPolicy",
                    policy =>
                    {
                        policy.WithOrigins("http://localhost:8080")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                    });
            });

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "ScannerKeyHunt",
                    Description = "Catálogo de Produtos e Categorias",
                    TermsOfService = new Uri("https://ScannerKeyHunt.net"),
                    Contact = new OpenApiContact
                    {
                        Name = "ScannerKeyHunt",
                        Email = "ScannerKeyHunt@yahoo.com",
                        Url = new Uri("https://ScannerKeyHunt.net"),
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Use ScannerKeyHunt",
                        Url = new Uri("https://ScannerKeyHunt.net"),
                    }
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Header do JWT Authorization usando o schema Bearer.Informe 'Bearer'[espaço] e a seguir o seu token. Exemplo: \"Bearer 12345abcdef\"",
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });

                //var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                //var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                //c.IncludeXmlComments(xmlPath);
            });

            builder.Services.AddHealthChecks(builder.Configuration);

            var app = builder.Build();

            builder.Services.CreateDatabase(app);

            builder.Services.CreateSeeds();

            app.UseHealthChecks("/health", new HealthCheckOptions()
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            app.UseHealthChecksUI(options =>
            {
                options.UIPath = "/dashboard";
                options.ApiPath = "/dashboard-api";
            });

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors("EnableCORS");
            app.UseCors("AllowAllOrigins");
            app.UseCors("MyPolicy");

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
