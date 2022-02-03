using System;
using System.Collections.Generic;
using System.Diagnostics;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Audiosurf2_Tools.Models;

public class TwitchRequestItem : ReactiveObject
{
    private readonly ICollection<TwitchRequestItem> _parentCollection;
    
    [Reactive] public string Title { get; set; }
    [Reactive] public string Channel { get; set; }
    [Reactive] public TimeSpan Duration { get; set; }
    [Reactive] public string Location { get; set; }
    [Reactive] public string Requester { get; set; }

    public TwitchRequestItem()
    {
        
    }
    
    public TwitchRequestItem(ICollection<TwitchRequestItem> parentCollection, string title, string channel, string location, string requester, TimeSpan duration = new TimeSpan())
    {
        _parentCollection = parentCollection;
        Title = title;
        Channel = channel;
        Duration = duration;
        Location = location;
        Requester = requester;
    }

    public void OpenLocation()
    {
        Process.Start(new ProcessStartInfo(Location)
        { 
            UseShellExecute = true, 
            Verb = "open" 
        }); 
    }

    public void Remove()
    {
        _parentCollection.Remove(this);
    }
}