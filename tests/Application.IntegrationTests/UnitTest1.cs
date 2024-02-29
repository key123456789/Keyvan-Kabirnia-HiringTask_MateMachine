using Infrastructure;

namespace Application.IntegrationTests;

[TestFixture]
public class ApplicationIntegrationTests
{
    private CurrencyConverter _converter;

    [SetUp]
    public void SetUp()
    {
        _converter = CurrencyConverter.Instance;
        _converter.ClearConfiguration();
        _converter.UpdateConfiguration(
        [
            new Tuple<string, string, double>("USD", "CAD", 1.34),
            new Tuple<string, string, double>("CAD", "GBP", 0.58),
            new Tuple<string, string, double>("USD", "EUR", 0.86)
        ]);
    }

    [Test]
    public void TestDirectConversion()
    {
        double result = _converter.Convert("USD", "EUR", 10);
        Assert.That(result, Is.EqualTo(8.6).Within(0.001));
    }

    [Test]
    public void TestIndirectConversion()
    {
        double result = _converter.Convert("CAD", "EUR", 10);
        Assert.That(result, Is.EqualTo(6.418).Within(0.001));
    }

    [Test]
    public void TestNonExistentFrom()
    {
        Assert.Throws<InvalidOperationException>(() => _converter.Convert("JPY", "CAD", 10));
    }

    [Test]
    public void TestNonExistentTo()
    {
        Assert.Throws<InvalidOperationException>(() => _converter.Convert("CAD", "JPY", 10));
    }
}
