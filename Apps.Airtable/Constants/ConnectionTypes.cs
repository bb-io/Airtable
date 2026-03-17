namespace Apps.Airtable.Constants;

public static class ConnectionTypes
{
    public const string OAuth2 = "OAuth2";
    public const string PersonalAccessToken = "Personal access token";

    public static string[] SupportedConnectionTypes = [OAuth2, PersonalAccessToken];
}
