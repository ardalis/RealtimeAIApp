using Azure.AI.OpenAI;
using System.ClientModel;
using RealtimeFormApp.Components;
using OpenAI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(o => o.DetailedErrors = true);

// -----------------------------------------------------------------------------------
// YOU MUST ENABLE ONE OF THE FOLLOWING (FOR EITHER OPENAI OR AZURE OPENAI)

// If using OpenAI:
Console.WriteLine("Using OpenAI directly");

// Set in User Secrets
// dotnet user-secrets init
// dotnet user-secrets set "OpenAI:Key" "<your OpenAI key>"
// dotnet user-secrets list
var openAiKey = builder.Configuration["OpenAI:Key"]!;
Console.WriteLine($"OpenAI API Key configured: {!string.IsNullOrEmpty(openAiKey)}");
var openAiClient = new OpenAIClient(openAiKey);

/*
// If using Azure OpenAI:
var endpoint = builder.Configuration["AzureOpenAI:Endpoint"]!;
var key = builder.Configuration["AzureOpenAI:Key"]!;
Console.WriteLine($"Using Azure OpenAI Endpoint: {endpoint}");
Console.WriteLine($"API Key configured: {!string.IsNullOrEmpty(key)}");

var openAiClient = new AzureOpenAIClient(
    new Uri(endpoint),
    new ApiKeyCredential(key));
*/
// -----------------------------------------------------------------------------------

var deploymentName = "gpt-4o-realtime-preview";
Console.WriteLine($"Looking for model: {deploymentName}");

var realtimeClient = openAiClient.GetRealtimeConversationClient(deploymentName);
Console.WriteLine($"RealtimeConversationClient created successfully");
builder.Services.AddSingleton(realtimeClient);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
