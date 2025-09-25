var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddHttpClient("TCMB", client =>
{
    client.BaseAddress = new Uri("https://www.tcmb.gov.tr/");
    client.Timeout = TimeSpan.FromSeconds(10);
});
builder.Services.AddControllers();




builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();




var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
