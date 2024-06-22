
using System.Net;
using Spectre.Console;

namespace TwitchModConsole;

public static class Program {
    private static readonly string clientId = "zavjqrvio46mui0l42f0gu0mdw1t24";
    private static readonly string redirectUri = "http://localhost:8080/redirect/";
    private static readonly string tokenUri = "http://localhost:8080/token/";
    private static readonly string scope = "moderator:manage:banned_users+moderator:manage:chat_messages+moderator:read:chatters+chat:edit+chat:read";

    static async Task Main(string[] args)
    {
        var state = Guid.NewGuid().ToString();
        var authorizationUrl = $"https://id.twitch.tv/oauth2/authorize?response_type=token&client_id={clientId}&redirect_uri={redirectUri}&scope={scope}&state={state}";
        
        Console.WriteLine($"Please authorize the application by visiting this URL:\n{authorizationUrl}");

        // Set up both listeners
        var redirectListener = new HttpListener();
        redirectListener.Prefixes.Add(redirectUri);
        redirectListener.Start();

        var tokenListener = new HttpListener();
        tokenListener.Prefixes.Add(tokenUri);
        tokenListener.Start();

        Console.WriteLine("Listening for OAuth2 response...");

        // Handle the initial redirect from Twitch
        var redirectContextTask = redirectListener.GetContextAsync();
        var tokenContextTask = tokenListener.GetContextAsync();

        var context = await redirectContextTask;
        var response = context.Response;

        var responseString = @"
        <html>
            <body>
                Authorization successful. You can close this window.
                <script type='text/javascript'>
                    var fragment = window.location.hash.substring(1);
                    var params = new URLSearchParams(fragment);
                    var token = params.get('access_token');
                    fetch('http://localhost:8080/token', {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/x-www-form-urlencoded'
                        },
                        body: 'access_token=' + token
                    }).then(response => {
                        window.close();
                    });
                </script>
            </body>
        </html>";

        var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
        response.ContentLength64 = buffer.Length;
        var output = response.OutputStream;
        await output.WriteAsync(buffer, 0, buffer.Length);
        output.Close();

        // Wait for the token POST request
        var tokenContext = await tokenContextTask;

        using var reader = new System.IO.StreamReader(tokenContext.Request.InputStream, tokenContext.Request.ContentEncoding);
        var tokenData = await reader.ReadToEndAsync();

        var tokenParams = System.Web.HttpUtility.ParseQueryString(tokenData);
        var token = tokenParams["access_token"];

        Console.WriteLine(token is not null ? $"Access token: {token}" : "Authorization failed or access token not found.");

        // Stop both listeners
        redirectListener.Stop();
        tokenListener.Stop();
    }
}
