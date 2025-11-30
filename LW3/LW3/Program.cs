using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

public class Latest
{
    public string result { get; set; } = string.Empty;
    public string base_code { get; set; } = string.Empty;
    public string time_last_update_utc { get; set; } = string.Empty;
    public Dictionary<string, decimal> rates { get; set; } = new();
}

public class Pair
{
    public string result { get; set; } = string.Empty;
    public string base_code { get; set; } = string.Empty;
    public string target_code { get; set; } = string.Empty;
    public decimal conversion_rate { get; set; }
}

class Program
{
    private static readonly HttpClient _http = new()
    {
        BaseAddress = new Uri("https://open.er-api.com/v6/"),
        Timeout = TimeSpan.FromSeconds(15)
    };

    static async Task Main()
    {
        try
        {
            Console.WriteLine("Запит до ExchangeRate API...");

            var resp = await _http.GetAsync("latest/USD");
            Console.WriteLine($"Request URL: {resp.RequestMessage?.RequestUri}");
            Console.WriteLine($"Status: {(int)resp.StatusCode} {resp.StatusCode}");
            resp.EnsureSuccessStatusCode();

            var json = await resp.Content.ReadAsStringAsync();

            var pretty = JsonSerializer.Serialize(
                JsonSerializer.Deserialize<object>(json)!,
                new JsonSerializerOptions { WriteIndented = true }
            );
            Console.WriteLine("Raw JSON (pretty):");
            Console.WriteLine(pretty);

            var latest = JsonSerializer.Deserialize<Latest>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (latest is not null)
            {
                Console.WriteLine("\n=== Результат парсингу ===");
                Console.WriteLine($"Result: {latest.result}");
                Console.WriteLine($"Base: {latest.base_code}");
                Console.WriteLine($"Updated: {latest.time_last_update_utc}");

                foreach (var code in new[] { "UAH", "EUR", "PLN" })
                {
                    if (latest.rates.TryGetValue(code, out var rate))
                        Console.WriteLine($"USD → {code}: {rate}");
                }
                Console.WriteLine($"Всього валют: {latest.rates.Count}");
            }

            var pairResp = await _http.GetAsync("pair/USD/UAH");
            pairResp.EnsureSuccessStatusCode();
            var pairJson = await pairResp.Content.ReadAsStringAsync();
            var pair = JsonSerializer.Deserialize<Pair>(pairJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (pair?.result == "success")
                Console.WriteLine($"\nUSD → UAH (pair): {pair.conversion_rate}");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Помилка: " + ex.Message);
        }
    }
}
