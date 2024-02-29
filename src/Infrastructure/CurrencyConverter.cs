using System.Collections.Concurrent;

using Application;

namespace Infrastructure;

public sealed class CurrencyConverter : ICurrencyConverter
{
    private static readonly Lazy<CurrencyConverter> _lazy = new(() => new CurrencyConverter());
    private readonly ConcurrentDictionary<(string from, string to), double> _conversionRates = new();

    public static CurrencyConverter Instance { get { return _lazy.Value; } }

    private CurrencyConverter() { }

    public void ClearConfiguration()
    {
        _conversionRates.Clear();
    }

    public void UpdateConfiguration(IEnumerable<Tuple<string, string, double>> rates)
    {
        foreach (var rate in rates)
        {
            _conversionRates.AddOrUpdate((rate.Item1, rate.Item2), rate.Item3, (key, oldValue) => rate.Item3);
        }
    }

    public double Convert(string fromCurrency, string toCurrency, double amount)
    {
        var path = Dijkstra(fromCurrency, toCurrency);
        if (path == null || path.Count == 1) // if 1 just contains fromCurrency
        {
            throw new InvalidOperationException($"No conversion path between {fromCurrency} and {toCurrency}.");
        }
        double conversionRate = 1.0;
        for (int i = 0; i < path.Count - 1; i++)
        {
            var rate = _conversionRates.TryGetValue((path[i], path[i + 1]), out double rateValue) ? rateValue : default;
            if (rate != default)
            {
                conversionRate *= rate;
            }
            else
            {
                rate = _conversionRates.TryGetValue((path[i + 1], path[i]), out rateValue) ? rateValue : default;
                if (rate != default)
                {
                    conversionRate /= rate;
                }
                else
                {
                    throw new InvalidOperationException($"No conversion rate between {path[i]} and {path[i + 1]}.");
                }
            }
        }
        return amount * conversionRate;
    }

    private List<string>? Dijkstra(string start, string end)
    {
        var previous = new Dictionary<string, string>();
        var distances = new Dictionary<string, int>();
        var nodes = new List<string>();

        List<string>? path = null;

        foreach (var vertex in _conversionRates.Keys.Select(x => x.from).Union(_conversionRates.Keys.Select(x => x.to)).Distinct())
        {
            if (vertex == start)
            {
                distances[vertex] = 0;
            }
            else
            {
                distances[vertex] = int.MaxValue;
            }

            nodes.Add(vertex);
        }

        while (nodes.Count != 0)
        {
            nodes.Sort((x, y) => distances[x].CompareTo(distances[y]));

            var smallest = nodes[0];
            nodes.Remove(smallest);

            if (smallest == end)
            {
                path = [];
                while (previous.ContainsKey(smallest))
                {
                    path.Add(smallest);
                    smallest = previous[smallest];
                }
                path.Add(start);
                path.Reverse();

                break;
            }

            if (distances[smallest] == int.MaxValue)
            {
                break;
            }

            foreach (var neighbor in _conversionRates.Where(x => x.Key.from == smallest))
            {
                var alt = distances[smallest] + 1; // Each conversion counts as 1, regardless of the rate
                if (alt < distances[neighbor.Key.to])
                {
                    distances[neighbor.Key.to] = alt;
                    previous[neighbor.Key.to] = smallest;
                }
            }
            foreach (var neighbor in _conversionRates.Where(x => x.Key.to == smallest)) // Check the reverse path
            {
                var alt = distances[smallest] + 1; // Each conversion counts as 1, regardless of the rate
                if (alt < distances[neighbor.Key.from])
                {
                    distances[neighbor.Key.from] = alt;
                    previous[neighbor.Key.from] = smallest;
                }
            }
        }

        return path;
    }
}
