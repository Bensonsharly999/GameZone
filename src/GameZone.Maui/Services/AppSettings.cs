namespace GameZone.Maui.Services;

public class AppSettings
{
    private const string UrlKey = "api_base_url";

    public string ApiBaseUrl
    {
        get => Preferences.Default.Get(UrlKey, DeviceInfo.Current.Platform == DevicePlatform.WinUI
            ? "http://127.0.0.1:5088"
            : "http://10.0.2.2:5088");
        set => Preferences.Default.Set(UrlKey, value.Trim().TrimEnd('/'));
    }
}
