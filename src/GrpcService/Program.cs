using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Sentry.AspNetCore.Grpc;

using GrpcService;
using GrpcService.Services;

var builder = WebApplication.CreateBuilder(args);

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the container.

builder.Configuration
    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
    .AddJsonFile("appsettings.json", false, true);

builder.Services.AddGrpc();
builder.Services.AddSingleton<NotificationProvider>();
builder.Services.AddSingleton<PushNotificationProvider>();

builder.WebHost.UseSentry(sentryBuilder =>
    sentryBuilder.AddGrpc()
);

var app = builder.Build();


FirebaseApp.Create(new AppOptions()
{
    Credential = GoogleCredential.FromFile("firebase-service-cred.json"),
});

#if !DEBUG
app.Urls.Add("http://*:8080");
#endif

// Configure the HTTP request pipeline.
app.MapGrpcService<RunIntoMeService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.UseSentryTracing();

app.Run();
