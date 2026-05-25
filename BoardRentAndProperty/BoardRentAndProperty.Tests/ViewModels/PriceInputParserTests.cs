using BoardRentAndProperty.ViewModels;
using NUnit.Framework;

namespace BoardRentAndProperty.Tests.ViewModels
{
    [TestFixture]
    public sealed class PriceInputParserTests
    {
        [Test]
        public void TryParsePriceInput_NullOrWhitespaceInput_ReturnsFalseAndZero()
        {
            bool nullParseSucceeded = PriceInputParser.TryParsePriceInput(null!, out double nullPrice);
            Assert.That(nullParseSucceeded, Is.False);
            Assert.That(nullPrice, Is.EqualTo(0));

            bool whitespaceParseSucceeded = PriceInputParser.TryParsePriceInput("   ", out double whitespacePrice);
            Assert.That(whitespaceParseSucceeded, Is.False);
            Assert.That(whitespacePrice, Is.EqualTo(0));
        }

        [Test]
        public void TryParsePriceInput_NumericValues_ParseCorrectly()
        {
            bool wholeParsed = PriceInputParser.TryParsePriceInput("42", out double wholePrice);
            Assert.That(wholeParsed, Is.True);
            Assert.That(wholePrice, Is.EqualTo(42));

            bool decimalParsed = PriceInputParser.TryParsePriceInput("12.50", out double decimalPrice);
            Assert.That(decimalParsed, Is.True);
            Assert.That(decimalPrice, Is.EqualTo(12.5));
        }

        [Test]
        public void TryParsePriceInput_NonNumericText_ReturnsFalseAndZero()
        {
            bool parseSucceeded = PriceInputParser.TryParsePriceInput("banana", out double price);

            Assert.That(parseSucceeded, Is.False);
            Assert.That(price, Is.EqualTo(0));
        }
    }
}
