## Experiment with a realtime conversational editor

Demo video:
[![Demo video](https://github.com/user-attachments/assets/66459cef-8e86-497b-ad8f-78fca30f5443)](https://youtu.be/H4BEF-VSDXQ)

### Prerequisite

You need either an OpenAI or Azure OpenAI account that has access to a `gpt-4o-realtime-preview` model. Also make sure you're on .NET 9 (`dotnet --version` should confirm this).

#### To use OpenAI:

In `Program.cs`, uncomment the following lines, and comment out the equivalent Azure OpenAI ones:

```cs
var openAiClient = new OpenAIClient(
    builder.Configuration["OpenAI:Key"]!);
```

Then use the command line/terminal to add your OpenAI API key to the .NET user secrets feature:

```
dotnet user-secrets set "OpenAI:Key" abcdef...
```

#### To use Azure OpenAI:

First use Azure OpenAI Studio to deploy a `gpt-4o-realtime-preview` model to Azure OpenAI. **Make sure the deployment name is actually `gpt-4o-realtime-preview`**, or if not, go and edit the following line in `Program.cs` to put in your deployment name:

```cs
var realtimeClient = openAiClient.GetRealtimeConversationClient("gpt-4o-realtime-preview");
```

Then use the command line/terminal to set the Azure OpenAI resource endpoint and API key:

```
cd RealtimeFormApp
dotnet user-secrets set "AzureOpenAI:Endpoint" https://YOUR-RESOURCE.openai.azure.com/
dotnet user-secrets set "AzureOpenAI:Key" abcdef...
```

### How to run

Run from Visual Studio or VS Code with Ctrl+F5 or from the console:

```
dotnet run
```

This should automatically open a browser at http://localhost:5174/. The browser will ask permission to access your mic; be sure to grant it.

You can now click the microphone icon to connect and then describe a vehicle to list for sale, or ask for edits to the content of the form.

### Getting voice output

You might prefer not to hear the voice output since you can read the text overlay faster than listen to speech, but if you do want to try hearing audio output, go into `RealtimeConversationManager.cs` and comment out this line:

```cs
ContentModalities = ConversationContentModalities.Text,
```

Now it will revert to the default modalities, which are text+speech.

If you want more of a two-way conversation, consider updating the prompt in `RealtimeConversationManager.cs` to remove the phrase `Do not reply to them unless they explicitly ask for your input; just listen`. Now it will be more inclined to respond in detail instead of just replying "OK".

### Troubleshooting

If you click the microphone and it says "Connecting..." but *doesn't* then say "Connected", you may be hitting a rate limit. `gpt-4o-realtime-preview` currently allows at most 10 connections/minute, or one connection every 6 seconds. So if you only just reloaded the page, wait a few seconds before reloading and trying again. There isn't a lot of error handling in this prototype.

## How it works

There's an object model defined in `CarDescriptor.cs`. This is formatted as a JSON schema in the prompt, so the AI knows what kinds of data and edits are allowed.

The AI is told there's a JSON document that it should update based on whatever the user asks. This JSON document is two-way bound to the UI, so any changes made by the AI are shown in the UI, and any changes made by the user are visible to the AI.

Note that `RealtimeConversationManager` is generically typed so the same logic there should work for editing other data schemas.
