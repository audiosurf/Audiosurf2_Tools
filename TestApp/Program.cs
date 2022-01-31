// See https://aka.ms/new-console-template for more information


using System.Net;

var a = (1UL << 56) | (1UL << 52) | (1UL << 32) | 99938558;
Console.WriteLine(a);
Console.WriteLine("A");

var listener = new HttpListener();
listener.Prefixes.Add("http://localhost:8888/");
listener.Start();

while (true)
{
    var cont = await listener.GetContextAsync();
    Console.WriteLine(cont.Request.Url);
    Console.WriteLine("wait");
}
