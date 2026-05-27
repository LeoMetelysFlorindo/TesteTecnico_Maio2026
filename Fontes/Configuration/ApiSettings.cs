namespace ProductApi.Configuration;

public class ApiSettings
{
    public int DefaultPageSize { get; set; } = 10;
    public int MaxPageSize { get; set; } = 100;
    public string ApiVersion { get; set; } = "1.0";
}