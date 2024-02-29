using Application.UnitTests.Mocks;

namespace Application.UnitTests;

[TestFixture]
public class ApplicationUnitTests
{
    private ICurrencyConverter _converter;

    [SetUp]
    public void SetUp()
    {
        _converter = new MockCurrencyConverter();
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
        // MockCurrencyConverter returns the same amount
        Assert.That(result, Is.EqualTo(10).Within(0.001));
    }

    [Test]
    public void TestIndirectConversion()
    {
        double result = _converter.Convert("CAD", "EUR", 10);
        // MockCurrencyConverter returns the same amount
        Assert.That(result, Is.EqualTo(10).Within(0.001));
    }

    [Test]
    public void TestNonExistentConversion()
    {
        Assert.Throws<InvalidOperationException>(() => _converter.Convert("CAD", "JPY", 10));
    }
}
