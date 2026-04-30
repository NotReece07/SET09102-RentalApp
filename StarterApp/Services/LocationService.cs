using Microsoft.Maui.Devices.Sensors;

namespace StarterApp.Services;

public class LocationService : ILocationService
{
    public async Task<AppLocation?> GetCurrentLocationAsync()
    {
        try
        {
            var request = new GeolocationRequest(
                GeolocationAccuracy.Medium,
                TimeSpan.FromSeconds(10)); // asks the device for a reasonably accurate location with a timeout

            var location = await Geolocation.Default.GetLocationAsync(request); // asks MAUI/Android for the current GPS location

            if (location == null)
            {
                return null; // returns null if no location was found
            }

            return new AppLocation(location.Latitude, location.Longitude); // converts the MAUI location into our own simple app location
        }
        catch
        {
            return null; // returns null if location permission is denied or location cannot be found
        }
    }
}