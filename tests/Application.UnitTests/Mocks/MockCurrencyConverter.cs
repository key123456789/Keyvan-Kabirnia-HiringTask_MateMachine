using System.Collections.Concurrent;

namespace Application.UnitTests.Mocks;
public class MockCurrencyConverter : ICurrencyConverter
{
    private readonly ConcurrentDictionary<(string from, string to), double> _conversionRates = new();

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
        // a predictable result.
        var allCurrencies = _conversionRates.Keys.Select(x => x.from).Union(_conversionRates.Keys.Select(x => x.to)).Distinct();
        if (!(allCurrencies.Contains(toCurrency) && allCurrencies.Contains(fromCurrency)))
        {
            throw new InvalidOperationException($"No conversion rate between {fromCurrency} and {toCurrency}.");
        }
        return amount;
    }
}
