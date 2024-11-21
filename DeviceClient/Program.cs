// See https://aka.ms/new-console-template for more information

using DeviceClient;

Console.WriteLine("Booting up connection to the DeviceProxy...");

var SSETunneler = new SSETunneler(new HttpClient(){BaseAddress = new Uri("http://localhost:5164")});
await SSETunneler.Start("http://localhost:5004/push-channel/1", "http://localhost:5004/response/1");

Console.WriteLine("Press any key to exit...");
Console.ReadKey();