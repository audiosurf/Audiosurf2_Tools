using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Audiosurf2_Tools.Entities;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Audiosurf2_Tools.Models;

public class Globals
{
    public static Dictionary<string, object> GlobalEntites { get; set; }

    static Globals()
    {
        GlobalEntites = new Dictionary<string, object>();
    }
    
    public static T? TryGetGlobal<T>(string key)
    {
        if (GlobalEntites.ContainsKey(key))
            if (GlobalEntites[key] is T itm)
                return itm;

        return default;
    }
}