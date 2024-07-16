using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddSingleton<X509Certificate2>(ctx =>
{
    IConfiguration config = ctx.GetRequiredService<IConfiguration>();
    string caminhoCertificado = config["Certificate:Path"];
    string senhaCertificado = config["Certificate:Password"];


    return new X509Certificate2(caminhoCertificado, senhaCertificado);
});

builder.Services.AddControllers();

var app = builder.Build();

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
