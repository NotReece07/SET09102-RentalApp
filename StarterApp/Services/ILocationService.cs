using Microsoft.Maui.Devices.Sensors;

namespace StarterApp.Services;

public interface ILocationService
{
    Task<Location?> GetCurrentLocationAsync(); // gets the user's current device location
}