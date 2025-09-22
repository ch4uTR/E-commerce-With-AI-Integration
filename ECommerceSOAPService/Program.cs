using ECommerceSOAPService.Services;
using SoapCore;



var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSoapCore();
builder.Services.AddSingleton<ICurrencyService, CurrencyService>();



var app = builder.Build();
app.UseHttpsRedirection();
app.UseHttpsRedirection();


app.UseSoapEndpoint<ICurrencyService>("/CurrencyService.svc", new SoapEncoderOptions());


app.MapGet("/", () => "Hello World!");

app.Run();
