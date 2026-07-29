using System.Linq;
using FluentAssertions;
using FWU.Exam.Management.Web.Helpers;

namespace FWU.Exam.Management.Web.Tests.Helpers;

public class PasswordGeneratorTests
{
    private const string Lowercase = "abcdefghijklmnopqrstuvwxyz";
    private const string Uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string Digits = "0123456789";
    private const string Special = "@#$!%*?&";

    [Fact]
    public void Generate_WithDefaultLength_Returns12Chars()
    {
        var password = PasswordGenerator.Generate();
        password.Should().HaveLength(12);
    }

    [Fact]
    public void Generate_WithCustomLength_ReturnsCorrectLength()
    {
        var password = PasswordGenerator.Generate(16);
        password.Should().HaveLength(16);
    }

    [Fact]
    public void Generate_WithLengthLessThan8_Returns8Chars()
    {
        var password = PasswordGenerator.Generate(4);
        password.Should().HaveLength(8);
    }

    [Fact]
    public void Generate_ContainsAtLeastOneLowercase()
    {
        var password = PasswordGenerator.Generate();
        password.Should().ContainAny(Lowercase.ToCharArray().Select(c => c.ToString()).ToArray());
    }

    [Fact]
    public void Generate_ContainsAtLeastOneUppercase()
    {
        var password = PasswordGenerator.Generate();
        password.Should().ContainAny(Uppercase.ToCharArray().Select(c => c.ToString()).ToArray());
    }

    [Fact]
    public void Generate_ContainsAtLeastOneDigit()
    {
        var password = PasswordGenerator.Generate();
        password.Should().ContainAny(Digits.ToCharArray().Select(c => c.ToString()).ToArray());
    }

    [Fact]
    public void Generate_ContainsAtLeastOneSpecial()
    {
        var password = PasswordGenerator.Generate();
        password.Should().ContainAny(Special.ToCharArray().Select(c => c.ToString()).ToArray());
    }

    [Fact]
    public void Generate_ProducesDifferentPasswords()
    {
        var pwd1 = PasswordGenerator.Generate();
        var pwd2 = PasswordGenerator.Generate();
        pwd1.Should().NotBe(pwd2);
    }

    [Fact]
    public void Generate_AllCharactersAreFromValidSet()
    {
        var allValid = Lowercase + Uppercase + Digits + Special;
        var password = PasswordGenerator.Generate(20);

        password.All(c => allValid.Contains(c)).Should().BeTrue();
    }
}
