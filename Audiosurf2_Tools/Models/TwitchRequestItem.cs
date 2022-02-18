using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Audiosurf2_Tools.Models;

public class TwitchRequestItem : ReactiveObject
{
    private readonly Collection<TwitchRequestItem> _parentCollection;
    private readonly Collection<TwitchRequestItem> _parentPastCollection;
    
    [Reactive] public string Title { get; set; }
    [Reactive] public string Channel { get; set; }
    [Reactive] public TimeSpan Duration { get; set; }
    [Reactive] public string Location { get; set; }
    [Reactive] public string Requester { get; set; }

    public TwitchRequestItem()
    {
        
    }
    
    public TwitchRequestItem(Collection<TwitchRequestItem> parentCollection, Collection<TwitchRequestItem> parentPastCollection, string title, string channel, string location, string requester, TimeSpan duration = new TimeSpan())
    {
        _parentCollection = parentCollection;
        _parentPastCollection = parentPastCollection;
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

    public void RemoveAndCount()
    {
        _parentPastCollection.Insert(0, this);
        Remove();
    }
    
    public void MoveUp()
    {
        var index = _parentCollection.IndexOf(this);
        if (index == 0)
            return;
        var temp = _parentCollection[index];
        Remove();
        _parentCollection.Insert(index - 1, temp);
    }

    public void MoveDown()
    {
        var index = _parentCollection.IndexOf(this);
        if (index == _parentCollection.Count - 1)
            return;
        var temp = _parentCollection[index];
        _parentCollection.Insert(index + 2, temp);
        Remove();
    }
}