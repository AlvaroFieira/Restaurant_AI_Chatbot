using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Data.SqlClient;
using DotNetEnv; 

public sealed class BookingsPlugin
{

    private readonly string _connectionString;

    // Constructor to load environment variables and initialize the connection string
    public BookingsPlugin()
    {
        // Load environment variables from .env file
        Env.Load(@"./../../../.env");

        _connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION") 
            ?? throw new InvalidOperationException("DATABASE_CONNECTION not found in environment variables.");
    }

    [KernelFunction]
    [Description(@"Determines the next available booking slot given the 
    number of guests and date to start looking from. The restaurant only accepts bookings at 18:00 and 20:00.")]
    public async Task<DateTime?> GetNextAvailableDateAsync(
        [Description("The date to start searching from")] string date,
        [Description("The number of guests")] string guests
    )
    {
        if (!DateTime.TryParse(date, out var startDate))
        {
            throw new ArgumentException("Invalid date format", nameof(date));
        }

        if (!int.TryParse(guests, out var numberOfGuests))
        {
            throw new ArgumentException("Invalid guests number format", nameof(guests));
        }

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            var query = @"SELECT TOP 1 date 
                        FROM ChatBot.Availability 
                        WHERE 
                            date >= @date 
                            AND (max_party_size - current_booked) > @guests 
                        ORDER BY date ASC";

            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@date", startDate);
                command.Parameters.AddWithValue("@guests", numberOfGuests);

                // Log the query with parameters
                var loggedQuery = query;
                foreach (SqlParameter param in command.Parameters)
                {
                    var value = param.Value is DateTime dateTime
                        ? dateTime.ToString("yyyy-MM-dd")  // Format dates
                        : param.Value.ToString();
                    loggedQuery = loggedQuery.Replace(param.ParameterName, value);
                }
                Console.WriteLine($"Executing SQL: {loggedQuery}");

                var result = await command.ExecuteScalarAsync();
                return result != null ? (DateTime?)Convert.ToDateTime(result) : null;
            }
        }
    }


    [KernelFunction]
    [Description(@"Allows the user to check if a date, time, and number of guests are available 
    (assume 2024 if no year is provided).")]
    public async Task<string> CheckAvailabilityAsync(
        [Description("The date of the appointment")] string date,
        [Description("The time of the appointment")] string time,
        [Description("The number of guests")] string guests
    )
    {
        // Validate and convert inputs
        if (!DateTime.TryParse(date, out var appointmentDate))
        {
            throw new ArgumentException("Invalid date format", nameof(date));
        }

        if (!TimeSpan.TryParse(time, out var appointmentTime))
        {
            throw new ArgumentException("Invalid time format", nameof(time));
        }

        if (!int.TryParse(guests, out var numberOfGuests))
        {
            throw new ArgumentException("Invalid guests number format", nameof(guests));
        }

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            // Define the availability query
            var availabilityQuery = @"SELECT * 
                                    FROM ChatBot.Availability 
                                    WHERE 
                                        date = @date 
                                        AND time = @time
                                        AND @guests < (max_party_size - current_booked)";

            using (var checkCommand = new SqlCommand(availabilityQuery, connection))
            {
                checkCommand.Parameters.AddWithValue("@date", appointmentDate);
                checkCommand.Parameters.AddWithValue("@time", appointmentTime);
                checkCommand.Parameters.AddWithValue("@guests", numberOfGuests);

                // Log the query with parameters
                var loggedQuery = availabilityQuery;
                foreach (SqlParameter param in checkCommand.Parameters)
                {
                    var value = param.Value is DateTime dateTime
                        ? dateTime.ToString("yyyy-MM-dd")  // Format dates
                        : param.Value.ToString();
                    loggedQuery = loggedQuery.Replace(param.ParameterName, value);
                }
                Console.WriteLine($"Executing SQL: {loggedQuery}");

                using (var reader = await checkCommand.ExecuteReaderAsync())
                {
                    if (!reader.HasRows)
                    {
                        return $"There is no availability on {appointmentDate:yyyy-MM-dd} at {appointmentTime}.";
                    }
                    else
                    {
                        return $"There is availability on {appointmentDate:yyyy-MM-dd} at {appointmentTime}.";
                    }
                }
            }
        }
    }

    [KernelFunction]
    [Description(@"Allows the user to make a booking if the date, time, and number of guests are available (assume 2024 if no year is provided).")]
    public async Task<string> BookAppointmentAsync(
        [Description("The date of the appointment")] string date,
        [Description("The time of the appointment")] string time,
        [Description("The name of the user")] string name,
        [Description("The email of the user")] string email,
        [Description("The phone number of the user")] string phone,
        [Description("The number of guests")] string guests
    )
    {
        // Validate and parse inputs
        if (!DateTime.TryParse(date, out var bookingDate))
        {
            throw new ArgumentException("Invalid date format", nameof(date));
        }

        if (!TimeSpan.TryParse(time, out var bookingTime))
        {
            throw new ArgumentException("Invalid time format", nameof(time));
        }

        if (!int.TryParse(guests, out var partySize))
        {
            throw new ArgumentException("Invalid guests number format", nameof(guests));
        }

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            // Book the appointment
            var bookingQuery = @"INSERT INTO ChatBot.Bookings 
                                    (customer_name, customer_email, customer_phone, booking_date, booking_time, party_size, status)
                                VALUES 
                                    (@name, @email, @phone, @date, @time, @partySize, 'confirmed')";

            using (var bookingCommand = new SqlCommand(bookingQuery, connection))
            {
                bookingCommand.Parameters.AddWithValue("@name", name);
                bookingCommand.Parameters.AddWithValue("@email", email);
                bookingCommand.Parameters.AddWithValue("@phone", phone);
                bookingCommand.Parameters.AddWithValue("@date", bookingDate);
                bookingCommand.Parameters.AddWithValue("@time", bookingTime);
                bookingCommand.Parameters.AddWithValue("@partySize", partySize);
                bookingCommand.Parameters.AddWithValue("@status", "confirmed");

                // Log the SQL query
                var loggedBookingQuery = bookingQuery;
                foreach (SqlParameter param in bookingCommand.Parameters)
                {
                    var value = param.Value is DateTime dt ? dt.ToString("yyyy-MM-dd") : param.Value.ToString();
                    loggedBookingQuery = loggedBookingQuery.Replace(param.ParameterName, value);
                }
                Console.WriteLine($"Executing SQL for Booking: {loggedBookingQuery}");

                await bookingCommand.ExecuteNonQueryAsync();
            }

            // Update availability
            var updateAvailabilityQuery = @"UPDATE ChatBot.Availability 
                                            SET current_booked = current_booked + @partySize 
                                            WHERE date = @date AND time = @time";

            using (var updateCommand = new SqlCommand(updateAvailabilityQuery, connection))
            {
                updateCommand.Parameters.AddWithValue("@date", bookingDate);
                updateCommand.Parameters.AddWithValue("@time", bookingTime);
                updateCommand.Parameters.AddWithValue("@partySize", partySize);

                 // Log the SQL query
                var loggedUpdateQuery = updateAvailabilityQuery;
                foreach (SqlParameter param in updateCommand.Parameters)
                {
                    var value = param.Value is DateTime dt ? dt.ToString("yyyy-MM-dd") : param.Value.ToString();
                    loggedUpdateQuery = loggedUpdateQuery.Replace(param.ParameterName, value);
                }
                Console.WriteLine($"Executing SQL for Availability Update: {loggedUpdateQuery}");

                await updateCommand.ExecuteNonQueryAsync();
            }

            return $"Appointment booked for {bookingDate:yyyy-MM-dd} under {name}. An email will be sent to {email}.";
        }
    }


    [KernelFunction]
    [Description("Cancels a booking given the name, date, and time of the booking (assume 2024 if no year is provided).")]
    public async Task<string> CancelBookingAsync(
        [Description("The date of the appointment")] string date,
        [Description("The time of the appointment")] string time,
        [Description("The name of the user")] string name
    )
    {
        // Validate and parse inputs
        if (!DateTime.TryParse(date, out var appointmentDate))
        {
            throw new ArgumentException("Invalid date format", nameof(date));
        }

        if (!TimeSpan.TryParse(time, out var appointmentTime))
        {
            throw new ArgumentException("Invalid time format", nameof(time));
        }

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            // Check if the booking exists
            var bookingQuery = @"SELECT * 
                                FROM ChatBot.Bookings 
                                WHERE 
                                    LOWER(customer_name) = LOWER(@name) 
                                    AND booking_date = @date 
                                    AND booking_time = @time 
                                    AND status = 'confirmed'";

            using (var checkCommand = new SqlCommand(bookingQuery, connection))
            {
                checkCommand.Parameters.AddWithValue("@name", name);
                checkCommand.Parameters.AddWithValue("@date", appointmentDate);
                checkCommand.Parameters.AddWithValue("@time", appointmentTime);

                // Log the SQL query
                var loggedCheckQuery = bookingQuery;
                foreach (SqlParameter param in checkCommand.Parameters)
                {
                    var value = param.Value is DateTime dt ? dt.ToString("yyyy-MM-dd") : param.Value.ToString();
                    loggedCheckQuery = loggedCheckQuery.Replace(param.ParameterName, value);
                }
                Console.WriteLine($"Executing SQL for Booking Check: {loggedCheckQuery}");

                using (var reader = await checkCommand.ExecuteReaderAsync())
                {
                    if (!reader.HasRows)
                    {
                        return $"No confirmed booking found for {appointmentDate:yyyy-MM-dd} at {appointmentTime} under {name}.";
                    }
                }
            }

            // Cancel the booking
            var cancelQuery = @"UPDATE ChatBot.Bookings 
                                SET status = 'canceled' 
                                WHERE 
                                    LOWER(customer_name) = LOWER(@name) 
                                    AND booking_date = @date 
                                    AND booking_time = @time 
                                    AND status = 'confirmed'";

            using (var cancelCommand = new SqlCommand(cancelQuery, connection))
            {
                cancelCommand.Parameters.AddWithValue("@name", name);
                cancelCommand.Parameters.AddWithValue("@date", appointmentDate);
                cancelCommand.Parameters.AddWithValue("@time", appointmentTime);

                // Log the SQL query
                var loggedCancelQuery = cancelQuery;
                foreach (SqlParameter param in cancelCommand.Parameters)
                {
                    var value = param.Value is DateTime dt ? dt.ToString("yyyy-MM-dd") : param.Value.ToString();
                    loggedCancelQuery = loggedCancelQuery.Replace(param.ParameterName, value);
                }
                Console.WriteLine($"Executing SQL for Booking Cancel: {loggedCancelQuery}");

                await cancelCommand.ExecuteNonQueryAsync();
            }

            return $"Booking for {appointmentDate:yyyy-MM-dd} at {appointmentTime} under {name} has been successfully canceled.";
        }
    }

}
