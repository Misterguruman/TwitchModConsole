using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TwitchModConsole;

internal static class TwitchOAuth
{
    private static readonly string _clientId = "zavjqrvio46mui0l42f0gu0mdw1t24";
    private static readonly string _redirectUri = "http://localhost:8080/redirect/";
    private static readonly string _tokenUri = "http://localhost:8080/token/";
    private static readonly string _scope = "moderator:manage:banned_users+moderator:manage:chat_messages+moderator:read:chatters+chat:edit+chat:read";

    private static readonly string _responseString = """
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
        </html>
        """;

    internal static async Task GetOAuthToken() {
        var state = Guid.NewGuid().ToString();
        var authorizationUrl = $"https://id.twitch.tv/oauth2/authorize?response_type=token&client_id={_clientId}&redirect_uri={_redirectUri}&scope={_scope}&state={state}";

        AnsiConsole.WriteLine($"Please authorize the application by visiting this URL:\n{authorizationUrl}");

        // Set up both listeners
        var redirectListener = new HttpListener();
        redirectListener.Prefixes.Add(_redirectUri);
        redirectListener.Start();

        var tokenListener = new HttpListener();
        tokenListener.Prefixes.Add(_tokenUri);
        tokenListener.Start();

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Shark)
            .StartAsync("Listening for OAuth2 response...", async ctx =>
            {
                // Handle the initial redirect from Twitch
                var redirectContextTask = redirectListener.GetContextAsync();
                var tokenContextTask = tokenListener.GetContextAsync();

                var context = await redirectContextTask;
                var response = context.Response;

                var buffer = System.Text.Encoding.UTF8.GetBytes(_responseString);
                response.ContentLength64 = buffer.Length;
                var output = response.OutputStream;
                await output.WriteAsync(buffer, 0, buffer.Length);
                output.Close();

                ctx.Status("Waiting for token response...");

                var tokenContext = await tokenContextTask;

                using var reader = new System.IO.StreamReader(tokenContext.Request.InputStream, tokenContext.Request.ContentEncoding);
                var tokenData = await reader.ReadToEndAsync();

                var tokenParams = System.Web.HttpUtility.ParseQueryString(tokenData);
                var token = tokenParams["access_token"];

                AnsiConsole.WriteLine(token is not null ? $"Access token: {token}" : "Authorization failed or access token not found.");
         
            });

        redirectListener.Stop();
        tokenListener.Stop();
    }
}
