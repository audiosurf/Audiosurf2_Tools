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
    [Reactive] public TimeSpan? CurrentTime { get; set; }
    [Reactive] public string OAuthToken { get; set; }

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
        try
        {
            _listener.Start();
            _timeOut.Start();
            _currentFullTime = DateTime.Now;
            CurrentTime = TimeSpan.FromMinutes(3);
            
            Process.Start(new ProcessStartInfo(url)
            { 
                UseShellExecute = true, 
                Verb = "open" 
            }); 
            while (_listener.IsListening)
            {
                var context = await Task.Run(_listener.GetContextAsync, _timeOutCTS.Token);
                var query = context?.Request.Url!.Query;
                if (string.IsNullOrWhiteSpace(context?.Request.Url!.Query))
                {
                    string responseString = Consts.GetTokenHtml;
                    byte[] buffer = Encoding.UTF8.GetBytes(responseString);
                    
                    context!.Response.ContentLength64 = buffer.Length;
                    Stream output = context.Response.OutputStream;
                    await output.WriteAsync(buffer,0,buffer.Length);
                    output.Close();
                }
                else if (context?.Request.Url!.Query.Contains("access_token") == true)
                {
                    var raw = context?.Request.Url!.Query.Replace("?access_token=", "");
                    OAuthToken = raw!.Split('&', StringSplitOptions.RemoveEmptyEntries)[0];
                    string responseString = Consts.CloseWindowHtml;
                    byte[] buffer = Encoding.UTF8.GetBytes(responseString);
                    
                    context!.Response.ContentLength64 = buffer.Length;
                    Stream output = context.Response.OutputStream;
                    await output.WriteAsync(buffer,0,buffer.Length);
                    output.Close();
                    break;
                }
                else
                {
                    break;
                }
                
                _currentFullTime = DateTime.Now;
                CurrentTime = TimeSpan.FromMinutes(3);
            }
            _timeOut.Stop();
            _listener.Stop();
        }
        catch (Exception e)
        {
            _listener.Stop();;
            Console.WriteLine(e);
        }
    }
}