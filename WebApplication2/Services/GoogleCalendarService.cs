using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using WebApplication2.Models;

public class GoogleCalendarService
{
    private readonly string[] Scopes = { CalendarService.Scope.Calendar };
    private readonly string ApplicationName = "Gym";

    public async Task AddEventToGoogleCalendarAsync(BookingOrder bookingOrder, Gym gym, string sportName)
    {
        UserCredential credential;

        using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
        {
            string credPath = "GoogleTokensOAuth";
            credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.FromStream(stream).Secrets,
                Scopes,
                "user",
                CancellationToken.None,
                new FileDataStore(credPath, true));
        }

        var service = new CalendarService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName,
        });

        EventsResource.InsertRequest request = service.Events.Insert(new Event()
        {
            Summary = $"Gym ({sportName}).",
            Description = $"Booking in {gym.name}!",
            Location = gym.adress,
            Start = new EventDateTime()
            {
                DateTimeDateTimeOffset = bookingOrder.BookingDate.AddHours(bookingOrder.BookingHour),
            },
            End = new EventDateTime()
            {
                DateTimeDateTimeOffset = bookingOrder.BookingDate.AddHours(bookingOrder.BookingHour + 1),
            }
        }, "primary");

        await request.ExecuteAsync();
    }
}