using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Text;
using DotNetEnv; 

public sealed class RestaurantInfoPlugin
{
    private readonly string _connectionString;

    // Constructor to load environment variables and initialize the connection string
    public RestaurantInfoPlugin()
    {
        // Load environment variables from .env file
        Env.Load(@"./../../../.env");

        _connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION") 
            ?? throw new InvalidOperationException("DATABASE_CONNECTION not found in environment variables.");
    }

    // Fetches all restaurant attributes and their values.
    [KernelFunction]
    [Description(@"Fetches all attributes and values of the restaurant, such as email address, phone number, address, description.")]
    public async Task<string> GetRestaurantDetailsAsync()
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            var query = @"SELECT attribute_field, attribute_value 
                          FROM ChatBot.RestaurantAttributes";

            using (var command = new SqlCommand(query, connection))
            using (var reader = await command.ExecuteReaderAsync())
            {
                var detailsMessage = new StringBuilder("Here are the restaurant details:\n");

                while (await reader.ReadAsync())
                {
                    detailsMessage.AppendLine($"{reader["attribute_field"]}: {reader["attribute_value"]}");
                }

                return detailsMessage.Length > 0 ? detailsMessage.ToString() : "Sorry, restaurant details are currently unavailable.";
            }
        }
    }

    // Fetches menu details
    [KernelFunction]
    [Description(@"Fetches menu items and details, including dietary tags.")]
    public async Task<string> GetMenuAsync()
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            var query = @"SELECT item_name, description, price, dietary_tags 
                          FROM ChatBot.Menu";

            using (var command = new SqlCommand(query, connection))
            using (var reader = await command.ExecuteReaderAsync())
            {
                var sb = new StringBuilder("Here is our menu:\n");
                
                while (await reader.ReadAsync())
                {
                    sb.AppendLine($"{reader["item_name"]} - ${reader["price"]}\n{reader["description"]} " +
                                  $"(Dietary options: {reader["dietary_tags"]})\n");
                }

                return sb.Length > 0 ? sb.ToString() : "The menu is currently unavailable.";
            }
        }
    }

    // Records customer feedback
    [KernelFunction]
    [Description(@"Records customer feedback given the customer email address and name.")]
    public async Task<string> SubmitFeedbackAsync(
        [Description("The name of the user")] string name,
        [Description("The email of the user")] string email,
        [Description("Feedback message")] string message
    )
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            var query = @"INSERT INTO ChatBot.Feedback (customer_name, customer_email, message, responded)
                          VALUES (@name, @customer_email, @message, 0)";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@name", name);
                command.Parameters.AddWithValue("@customer_email", email);
                command.Parameters.AddWithValue("@message", message);

                // Log the SQL query with parameter values
                var loggedQuery = query;
                foreach (SqlParameter param in command.Parameters)
                {
                    var value = param.Value.ToString();
                    loggedQuery = loggedQuery.Replace(param.ParameterName, $"'{value}'");
                }
                Console.WriteLine($"Executing SQL for Feedback Submission: {loggedQuery}");

                await command.ExecuteNonQueryAsync();
            }

            return "Thank you for your feedback! Our team will get back to you if necessary.";
        }
    }
}
