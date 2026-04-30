namespace StarterApp.Services;

public class AppLocation
{
    public double Latitude { get; set; } // stores the latitude value
    public double Longitude { get; set; } // stores the longitude value

    public AppLocation(double latitude, double longitude)
    {
        Latitude = latitude; // stores the latitude passed in
        Longitude = longitude; // stores the longitude passed in
    }
}

public interface ILocationService
{
    Task<AppLocation?> GetCurrentLocationAsync(); // gets the user's current device location
}