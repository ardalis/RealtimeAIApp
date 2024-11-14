## Experiment with a realtime conversational editor

### Prerequisite

First use Azure OpenAI Studio to deploy a `gpt-4o-realtime-preview` model to Azure OpenAI. **Make sure the deployment name is actually `gpt-4o-realtime-preview`**, or if not, go and edit the following line in `Program.cs` to put in your deployment name:

```cs
var realtimeClient = openAiClient.GetRealtimeConversationClient("gpt-4o-realtime-preview");
```

Also make sure you're on .NET 9 (`dotnet --version` should confirm this).

### How to run

Set the Azure OpenAI resource endpoint and API key:

```
cd RealtimeFormApp
dotnet user-secrets set "AzureOpenAI:Endpoint" https://YOUR-RESOURCE.openai.azure.com/
dotnet user-secrets set "AzureOpenAI:Key" abcdef...
```

Now you can run it:

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

## How it works

There's an object model defined in `CarDescriptor.cs`. This is formatted as a JSON schema in the prompt, so the AI knows what kinds of data and edits are allowed.

The AI is told there's a JSON document that it should update based on whatever the user asks. This JSON document is two-way bound to the UI, so any changes made by the AI are shown in the UI, and any changes made by the user are visible to the AI.

Note that `RealtimeConversationManager` is generically typed so the same logic there should work for editing other data schemas.
