using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using DotNetEnv; 

internal class Program
{
    static async Task Main(string[] args)
    {

        // Load environment variables from .env file
        Env.Load(@"./../../../.env");

        // Retrieve environment variables
        var modelId = Environment.GetEnvironmentVariable("OPENAI_MODEL_ID") 
            ?? throw new InvalidOperationException("OPENAI_MODEL_ID not found in environment variables.");
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") 
            ?? throw new InvalidOperationException("OPENAI_API_KEY not found in environment variables.");
        
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("Hello! I am The Magic Cauldron's AI Chatbot! How can I help you today?");

        var builder = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(modelId, apiKey);

        builder.Plugins.AddFromType<BookingsPlugin>();
        builder.Plugins.AddFromType<RestaurantInfoPlugin>();

        builder.Services.AddLogging(configure => configure.AddConsole());
        builder.Services.AddLogging(configure => configure.SetMinimumLevel(LogLevel.Information));

        Kernel kernel = builder.Build();

        // Enable planning / automatic function calling
        OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
        };
        // Create chat history and system prompt/
        var history =
        new ChatHistory(@"You are professional customer service agent that handles bookings for a restaurant. 
        Make sure you ask the user to provide all the required parameters when adding bookings, feedback, checking availability, etc.
        The current year is 2024.");

        // Get chat completion service
        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        Console.ForegroundColor = ConsoleColor.Green;

        // Start the conversation
        Console.Write("User > ");

        string? userInput;
        while ((userInput = Console.ReadLine()) != null)
        {
            // Add user input
            history.AddUserMessage(userInput);

            // Revert console colour to original colour
            Console.ForegroundColor = ConsoleColor.White;

            // Get the response from the AI
            var result = await chatCompletionService.GetChatMessageContentAsync(
                history,
                executionSettings: openAIPromptExecutionSettings,
                kernel: kernel
            );
            
            // Print the results
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("Assistant > " + result);

            // Add the message from the agent to the chat history
            history.AddAssistantMessage(result.Content ?? string.Empty);

            // Get user input again
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("User > ");
        }
    }
}