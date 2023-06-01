using ExampleTwilioMessageSenderProject.Domain.Contract;
using ExampleTwilioMessageSenderProject.Domain.Models.Configuration;
using ExampleTwilioMessageSenderProject.Domain.Service;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddTransient<IAlertSenderService, AlertSenderService>();

var sendGridConfig = new SendGridMailConfigurations();
new ConfigureFromConfigurationOptions<SendGridMailConfigurations>(
    builder.Configuration.GetSection("SendGridMailConfigurations"))
    .Configure(sendGridConfig);
builder.Services.AddSingleton(sendGridConfig);

var twilioConfig = new TwilioConfigurations();
new ConfigureFromConfigurationOptions<TwilioConfigurations>(
    builder.Configuration.GetSection("TwilioConfigurations"))
    .Configure(twilioConfig);
builder.Services.AddSingleton(twilioConfig);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
