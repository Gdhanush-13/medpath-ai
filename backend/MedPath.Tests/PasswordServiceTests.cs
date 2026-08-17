using MedPath.Infrastructure;

namespace MedPath.Tests;

public sealed class PasswordServiceTests
{
    [Fact]
    public void Hashes_are_verifiable_but_not_plaintext()
    {
        var service = new PasswordService();
        var hash = service.Hash("correct horse battery staple");

        Assert.NotEqual("correct horse battery staple", hash);
        Assert.True(service.Verify("correct horse battery staple", hash));
        Assert.False(service.Verify("wrong password", hash));
    }

    [Fact]
    public void Malformed_hashes_fail_closed()
    {
        var service = new PasswordService();

        Assert.False(service.Verify("anything", "not-a-password-hash"));
    }
}
