using Microsoft.Maui.Devices.Sensors;

namespace StarterApp.Services;

public class LocationService : ILocationService
{
    public async Task<Location?> GetCurrentLocationAsync()
    {
        try
        {
            var request = new GeolocationRequest(
                GeolocationAccuracy.Medium,
                TimeSpan.FromSeconds(10)); // asks the device for a reasonably accurate location with a timeout

            return await Geolocation.Default.GetLocationAsync(request); // returns the current device location if available
        }
        catch
        {
            return null; // returns null if location permission is denied or location cannot be found
        }
    }
}