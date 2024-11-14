using Microsoft.Extensions.AI;
using OpenAI.RealtimeConversation;
using RealtimeFormApp.Support;
using System.Text;
using System.Text.Json;
using System.Text.Json.Schema;

namespace RealtimeFormApp;

public class RealtimeConversationManager<TModel>(string modelDescription, RealtimeConversationClient realtimeConversationClient, Stream micStream, Speaker speaker, Action<TModel> updateCallback, Action<string> addMessage) : IDisposable
{
    RealtimeConversationSession? session;
    string? prevModelJson;

    // Call back into the UI layer to update the data in the form
    AIFunction[] tools = [AIFunctionFactory.Create((TModel modelData) => updateCallback(modelData), "Save_ModelData")];

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var jsonSchema = JsonSerializer.Serialize(
            JsonSerializerOptions.Default.GetJsonSchemaAsNode(typeof(TModel), new() { TreatNullObliviousAsNonNullable = true }));
        var sessionOptions = new ConversationSessionOptions()
        {
            Instructions = $"""
                You are helping to edit a JSON object that represents a {modelDescription}.
                This JSON object conforms to the following schema: {jsonSchema}

                Listen to the user and collect information from them. Do not reply to them unless they explicitly
                ask for your input; just listen.
                Each time they provide information that can be added to the JSON object, add it to the existing object,
                and then call the tool to save the updated object. Don't stop updating the JSON object.
                Even if you think the information is incorrect, accept it - do not try to correct mistakes.
                After each time you have called the JSON updating tool, just reply OK.
                """,
            Voice = ConversationVoice.Alloy,
            ContentModalities = ConversationContentModalities.Text,
            TurnDetectionOptions = ConversationTurnDetectionOptions.CreateServerVoiceActivityTurnDetectionOptions(detectionThreshold: 0.4f, silenceDuration: TimeSpan.FromMilliseconds(150)),
        };

        foreach (var tool in tools)
        {
            sessionOptions.Tools.Add(tool.ToConversationFunctionTool());
        }

        addMessage("Connecting...");
        session = await realtimeConversationClient.StartConversationSessionAsync();
        await session.ConfigureSessionAsync(sessionOptions);
        var outputStringBuilder = new StringBuilder();

        await foreach (ConversationUpdate update in session.ReceiveUpdatesAsync(cancellationToken))
        {
            switch (update)
            {
                case ConversationSessionStartedUpdate:
                    addMessage("Connected");
                    _ = Task.Run(async () => await session.SendInputAudioAsync(micStream, cancellationToken));
                    break;

                case ConversationInputSpeechStartedUpdate:
                    addMessage("Speech started");
                    await speaker.ClearPlaybackAsync(); // If the user interrupts, stop talking
                    break;

                case ConversationInputSpeechFinishedUpdate:
                    addMessage("Speech finished");
                    break;

                case ConversationItemStreamingPartDeltaUpdate outputDelta:
                    // Happens each time a chunk of output is received
                    await speaker.EnqueueAsync(outputDelta.AudioBytes?.ToArray());
                    outputStringBuilder.Append(outputDelta.Text ?? outputDelta.AudioTranscript);
                    break;

                case ConversationResponseFinishedUpdate responseFinished:
                    // Happens when a "response turn" is finished
                    addMessage(outputStringBuilder.ToString());
                    outputStringBuilder.Clear();
                    break;
            }

            await HandleToolCallsAsync(update, tools);
        }
    }

    public void Dispose()
    {
        session?.Dispose();
    }

    // Called by the UI when the user manually edits the form. This lets the AI know
    // the latest state in case it needs to make further updates.
    public async Task SetModelData(TModel modelData)
    {
        if (session is not null)
        {
            var newJson = JsonSerializer.Serialize(modelData);
            if (newJson != prevModelJson)
            {
                prevModelJson = newJson;
                await session.AddItemAsync(ConversationItem.CreateUserMessage([$"The current modelData value is {newJson}. When updating this later, include all these same values if they are unchanged (or they will be overwritten with nulls)."]));
            }
        }
    }

    private async Task HandleToolCallsAsync(ConversationUpdate update, AIFunction[] tools)
    {
        switch (update)
        {
            case ConversationItemStreamingFinishedUpdate itemFinished:
                // If we need to call a tool to update the model, do so
                if (!string.IsNullOrEmpty(itemFinished.FunctionName) && await itemFinished.GetFunctionCallOutputAsync(tools) is { } output)
                {
                    await session!.AddItemAsync(output);
                }
                break;

            case ConversationResponseFinishedUpdate responseFinished:
                // If we added one or more function call results, instruct the model to respond to them
                if (responseFinished.CreatedItems.Any(item => !string.IsNullOrEmpty(item.FunctionName)))
                {
                    await session!.StartResponseAsync();
                }
                break;
        }
    }
}
