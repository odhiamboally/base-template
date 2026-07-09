using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        var handler = new HttpClientHandler { ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true };
        using var client = new HttpClient(handler);
        client.DefaultRequestHeaders.Add("Stripe-Signature", "t=123,v1=abc");
        var content = new StringContent("{"test": "data"}", Encoding.UTF8, "application/json");
        var response = await client.PostAsync("https://localhost:7049/api/v1/shared/payments/stripe/webhook", content);
        Console.WriteLine($"StatusCode: {response.StatusCode}");
        var body = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Body: {body}");
    }
}
