namespace Salix.AspNetCore.Utilities.Tests
{
    using System.Diagnostics.CodeAnalysis;
    using FluentAssertions;
    using Xunit;

    [ExcludeFromCodeCoverage]
    public class AbstractionsTests
    {
        [Fact]
        public void ApiDataValidationError_EqualsSimilar_AreEqual()
        {
            var testable1 = new ApiDataValidationError
            {
                AttemptedValue = 7,
                Message = "Testing",
                PropertyName = "Identifier"
            };

            var testable2 = new ApiDataValidationError
            {
                AttemptedValue = 7,
                Message = "Testing",
                PropertyName = "Identifier"
            };

            testable1.Equals(testable2).Should().BeTrue();
        }

        [Fact]
        public void ApiDataValidationError_EqualsSimilar_EqualOperationIsTrue()
        {
            var testable1 = new ApiDataValidationError
            {
                AttemptedValue = 7,
                Message = "Testing",
                PropertyName = "Identifier"
            };

            var testable2 = new ApiDataValidationError
            {
                AttemptedValue = 7,
                Message = "Testing",
                PropertyName = "Identifier"
            };

            var result = testable1 == testable2;
            result.Should().BeTrue();
        }

        [Fact]
        public void ApiDataValidationError_EqualsObject_AreEqual()
        {
            var testable1 = new ApiDataValidationError
            {
                AttemptedValue = 7,
                Message = "Testing",
                PropertyName = "Identifier"
            };

            var testable2 = new ApiDataValidationError
            {
                AttemptedValue = 7,
                Message = "Testing",
                PropertyName = "Identifier"
            };

            testable1.Equals((object)testable2).Should().BeTrue();
        }

        [Fact]
        public void ApiDataValidationError_EqualsSimilar_NonEqualOperationIsFalse()
        {
            var testable1 = new ApiDataValidationError
            {
                AttemptedValue = 7,
                Message = "Testing",
                PropertyName = "Identifier"
            };

            var testable2 = new ApiDataValidationError
            {
                AttemptedValue = 7,
                Message = "Testing",
                PropertyName = "Identifier"
            };

            var result = testable1 != testable2;
            result.Should().BeFalse();
        }

        [Fact]
        public void ApiDataValidationError_EqualsDiffPropName_AreNotEqual()
        {
            var testable1 = new ApiDataValidationError
            {
                AttemptedValue = 7,
                Message = "Testing",
                PropertyName = "Identifier"
            };

            var testable2 = new ApiDataValidationError
            {
                AttemptedValue = 7,
                Message = "Testing",
                PropertyName = "Label"
            };

            testable1.Equals(testable2).Should().BeFalse();
        }

        [Fact]
        public void ApiDataValidationError_EqualsDiffValue_AreNotEqual()
        {
            var testable1 = new ApiDataValidationError
            {
                AttemptedValue = 7,
                Message = "Testing",
                PropertyName = "Identifier"
            };

            var testable2 = new ApiDataValidationError
            {
                AttemptedValue = 9,
                Message = "Testing",
                PropertyName = "Identifier"
            };

            testable1.Equals(testable2).Should().BeFalse();
        }

        [Fact]
        public void ConfigurationValidationException_Message_IsCorrect()
        {
            var testable = new ConfigurationValidationException();
            testable.Message.Should().Be("There are invalid settings in configuration objects found by application builder.");
        }

        [Fact]
        public void ConfigurationValidationException_CustomMessage_IsCorrect()
        {
            var testable = new ConfigurationValidationException("Custom message");
            testable.Message.Should().Be("Custom message");
        }

        [Fact]
        public void ConfigurationValidationException_ComplexMessage_IsCorrect()
        {
            var testable = new ConfigurationValidationException("Ass", "Poop", "Eat shit and die");
            testable.Message.Should().Be(@"Settings for Ass.Poop are invalid: ""Eat shit and die"". 
Check that your configuration has been loaded correctly, and all necessary values are set in the configuration files.");
        }
    }
}