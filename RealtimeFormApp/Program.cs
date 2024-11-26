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
/*
// If using OpenAI:
var openAiClient = new OpenAIClient(
    builder.Configuration["OpenAI:Key"]!);
*/

// If using Azure OpenAI:
var openAiClient = new AzureOpenAIClient(
    new Uri(builder.Configuration["AzureOpenAI:Endpoint"]!),
    new ApiKeyCredential(builder.Configuration["AzureOpenAI:Key"]!));
// -----------------------------------------------------------------------------------

var realtimeClient = openAiClient.GetRealtimeConversationClient("gpt-4o-realtime-preview");
builder.Services.AddSingleton(realtimeClient);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
