using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Runtime;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var credentials = new BasicAWSCredentials("AKIAYIUISDZE76YWMMWB", "sEcqD5d9x5d/4NTYdg+00bknTXbI/PhP2nvaYXgy");
var config = new AmazonDynamoDBConfig()
{
    RegionEndpoint = Amazon.RegionEndpoint.USEast1
};
var client = new AmazonDynamoDBClient(credentials,config);
builder.Services.AddSingleton<IAmazonDynamoDB>(client);
builder.Services.AddSingleton<DynamoDBContext, DynamoDBContext>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("nuevaPolitica", app =>
    {
        app.AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("nuevaPolitica");
app.UseAuthorization();

app.MapControllers();

app.Run();
