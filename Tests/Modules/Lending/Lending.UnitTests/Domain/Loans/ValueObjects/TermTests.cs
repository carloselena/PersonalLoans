using Blocks.Domain.Exceptions;
using Lending.Domain.Loans.ValueObjects;

namespace Lending.UnitTests.Domain.Loans.ValueObjects;

public class TermTests
{
    [Fact]
    public void Of_PositiveValue_ShouldCreate()
    {
        var term = Term.Of(12);
 
        Assert.Equal(12, term.Value);
    }
 
    [Fact]
    public void Of_ZeroValue_ShouldThrow()
    {
        Assert.Throws<InvalidDomainValueException>(() => Term.Of(0));
    }
 
    [Fact]
    public void Of_NegativeValue_ShouldThrow()
    {
        Assert.Throws<InvalidDomainValueException>(() => Term.Of(-1));
    }
 
    [Fact]
    public void SameValues_ShouldBeEqual()
    {
        var a = Term.Of(6);
        var b = Term.Of(6);
 
        Assert.Equal(a, b);
    }
 
    [Fact]
    public void DifferentValues_ShouldNotBeEqual()
    {
        var a = Term.Of(6);
        var b = Term.Of(12);
 
        Assert.NotEqual(a, b);
    }
}