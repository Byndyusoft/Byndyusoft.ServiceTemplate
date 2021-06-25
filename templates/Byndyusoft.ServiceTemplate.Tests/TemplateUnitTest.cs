namespace Byndyusoft.ServiceTemplate.Tests
{
    using FluentAssertions;
    using Xunit;

    public class TemplateUnitTest
    {
        [Fact]
        public void Test()
        {
            const bool trueVariable = true;
            trueVariable.Should().BeTrue();
        }
    }
}