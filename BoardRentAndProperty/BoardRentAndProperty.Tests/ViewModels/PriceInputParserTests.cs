using BoardRentAndProperty.ViewModels;
using NUnit.Framework;

namespace BoardRentAndProperty.Tests.ViewModels
{
    [TestFixture]
    public sealed class PriceInputParserTests
    {
        [Test]
        public void TryParsePriceInput_NullString_ReturnsFalseAndZero()
        {
            bool parseSucceeded = PriceInputParser.TryParsePriceInput(null!, out double price);

            Assert.That(parseSucceeded, Is.False);
            Assert.That(price, Is.EqualTo(0));
        }

        [Test]
        public void TryParsePriceInput_OnlyWhitespace_ReturnsFalseAndZero()
        {
            bool parseSucceeded = PriceInputParser.TryParsePriceInput("   ", out double price);

            Assert.That(parseSucceeded, Is.False);
            Assert.That(price, Is.EqualTo(0));
        }

        [Test]
        public void TryParsePriceInput_WholeNumber_ParsesCorrectly()
        {
            bool parseSucceeded = PriceInputParser.TryParsePriceInput("42", out double price);

            Assert.That(parseSucceeded, Is.True);
            Assert.That(price, Is.EqualTo(42));
        }

        [Test]
        public void TryParsePriceInput_DotDecimalSeparator_ParsesCorrectly()
        {
            bool parseSucceeded = PriceInputParser.TryParsePriceInput("12.50", out double price);

            Assert.That(parseSucceeded, Is.True);
            Assert.That(price, Is.EqualTo(12.5));
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
