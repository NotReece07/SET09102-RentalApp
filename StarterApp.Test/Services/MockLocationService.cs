using StarterApp.Services;

namespace StarterApp.Test.Services;

public class MockLocationService : ILocationService
{
    private readonly AppLocation? _location; // stores the fake location returned during tests

    public MockLocationService(AppLocation? location)
    {
        _location = location; // lets each test decide whether location is found or missing
    }

    public Task<AppLocation?> GetCurrentLocationAsync()
    {
        return Task.FromResult(_location); // returns the fake location instead of using real GPS
    }
}