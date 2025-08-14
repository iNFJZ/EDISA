using GrpcGreeter.Services;
using Grpc.AspNetCore.Server;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();
builder.Services.AddGrpcClient<GrpcGreeter.AuthService.AuthServiceClient>(o =>
{
    o.Address = new Uri(builder.Configuration["GrpcEndpoints:AuthService"]);
});
builder.Services.AddGrpcClient<GrpcGreeter.FileService.FileServiceClient>(o =>
{
    o.Address = new Uri(builder.Configuration["GrpcEndpoints:FileService"]);
});


builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5219, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
app.MapControllers();
app.MapGrpcService<SimpleFileGrpcService>();
app.MapGrpcService<SimpleEmailGrpcService>();
app.MapGrpcService<SimpleAuthGrpcService>();

if (app.Environment.IsDevelopment())
{
    app.MapGrpcReflectionService();
}

app.MapGet("/", () => "gRPC Microservice System is running. Communication with gRPC endpoints must be made through a gRPC client.");

app.Run();



