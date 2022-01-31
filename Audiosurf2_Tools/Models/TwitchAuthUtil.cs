using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Audiosurf2_Tools.Entities;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Timer = System.Timers.Timer;

namespace Audiosurf2_Tools.Models;

public class TwitchAuthUtil : ReactiveObject
{
    private static HttpClient _client = new ();
    private HttpListener _listener { get; set; } = new();
    private CancellationTokenSource _timeOutCTS { get; set; } = new ();
    private Timer _timeOut { get; set; } = new ();
    private DateTime _currentFullTime { get; set; } = DateTime.Now;

    public TwAuth CurrentAuth { get; set; } = new TwAuth();

    [Reactive] public TimeSpan? CurrentTime { get; set; }

    public TwitchAuthUtil()
    {
        _timeOut.Elapsed += (sender, args) =>
        {
            var diff = args.SignalTime - _currentFullTime;
            var ready = CurrentTime - diff;
            if (ready < TimeSpan.Zero)
            {
                CurrentTime = TimeSpan.Zero;
                _timeOut.Stop();
                _timeOutCTS.Cancel();
            }
            else
            {
                CurrentTime = ready;
                _currentFullTime = DateTime.Now;
            }
        };
    }

    public void TryStop()
    {
        try
        {
            _timeOut.Stop();
            _listener.Stop();
            CurrentTime = default;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    
    public async Task DoOAuthFlowAsync(string url)
    {
        _listener.Prefixes.Add("http://localhost:8888/");
        string code = "";
        try
        {
            _listener.Start();
            _timeOut.Start();
            _currentFullTime = DateTime.Now;
            CurrentTime = TimeSpan.FromMinutes(3);
            while (_listener.IsListening)
            {
                Process.Start(new ProcessStartInfo(url)
                { 
                    UseShellExecute = true, 
                    Verb = "open" 
                }); 
                var context = await Task.Run(_listener.GetContextAsync, _timeOutCTS.Token);
                var query = context?.Request.Url!.Query;
                if (!query!.Contains("error"))
                {
                    code = query.Replace("?code=", "");
                    code = code.Split('&')[0];
                    
                    string responseString = "<HTML><BODY> You can close this :)</BODY></HTML>";
                    byte[] buffer = Encoding.UTF8.GetBytes(responseString);
                    
                    context!.Response.ContentLength64 = buffer.Length;
                    Stream output = context.Response.OutputStream;
                    await output.WriteAsync(buffer,0,buffer.Length);
                    output.Close();
                    break;
                }
                
                _currentFullTime = DateTime.Now;
                CurrentTime = TimeSpan.FromMinutes(3);
            }
            _timeOut.Stop();
            if (string.IsNullOrWhiteSpace(code))
                return;
            _listener.Stop();;
            this.CurrentAuth = await GetOuthTokenAsync(code) ?? new TwAuth();
        }
        catch (Exception e)
        {
            _listener.Stop();;
            Console.WriteLine(e);
        }
    }

    public async Task<TwAuth?> GetOuthTokenAsync(string code)
    {
        //Idk about this
        using var msg = new HttpRequestMessage(HttpMethod.Post,
            "https://id.twitch.tv/oauth2/token?client_id=ff9dg7h1dibw47gvj9y2y5brqo0edt" +
            "&client_secret=" +
            $"&code={code}" +
            "&grant_type=authorization_code" +
            "&redirect_uri=http://localhost:8888");
        var resp = await _client.SendAsync(msg);
        var data = await resp.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<TwAuth>(data);
    }
}