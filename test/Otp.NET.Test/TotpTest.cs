﻿using System;
using System.Text;
using Moq;
using NUnit.Framework;

namespace OtpNet.Test;

[TestFixture]
public class TotpTest
{
    private const string Rfc6238SecretSha1 = "12345678901234567890";
    private const string Rfc6238SecretSha256 = "12345678901234567890123456789012";
    private const string Rfc6238SecretSha512 = "1234567890123456789012345678901234567890123456789012345678901234";

    [TestCase(Rfc6238SecretSha1, OtpHashMode.Sha1, 59, "94287082")]
    [TestCase(Rfc6238SecretSha256, OtpHashMode.Sha256, 59, "46119246")]
    [TestCase(Rfc6238SecretSha512, OtpHashMode.Sha512, 59, "90693936")]
    [TestCase(Rfc6238SecretSha1, OtpHashMode.Sha1, 1111111109, "07081804")]
    [TestCase(Rfc6238SecretSha256, OtpHashMode.Sha256, 1111111109, "68084774")]
    [TestCase(Rfc6238SecretSha512, OtpHashMode.Sha512, 1111111109, "25091201")]
    [TestCase(Rfc6238SecretSha1, OtpHashMode.Sha1, 1111111111, "14050471")]
    [TestCase(Rfc6238SecretSha256, OtpHashMode.Sha256, 1111111111, "67062674")]
    [TestCase(Rfc6238SecretSha512, OtpHashMode.Sha512, 1111111111, "99943326")]
    [TestCase(Rfc6238SecretSha1, OtpHashMode.Sha1, 1234567890, "89005924")]
    [TestCase(Rfc6238SecretSha256, OtpHashMode.Sha256, 1234567890, "91819424")]
    [TestCase(Rfc6238SecretSha512, OtpHashMode.Sha512, 1234567890, "93441116")]
    [TestCase(Rfc6238SecretSha1, OtpHashMode.Sha1, 2000000000, "69279037")]
    [TestCase(Rfc6238SecretSha256, OtpHashMode.Sha256, 2000000000, "90698825")]
    [TestCase(Rfc6238SecretSha512, OtpHashMode.Sha512, 2000000000, "38618901")]
    [TestCase(Rfc6238SecretSha1, OtpHashMode.Sha1, 20000000000, "65353130")]
    [TestCase(Rfc6238SecretSha256, OtpHashMode.Sha256, 20000000000, "77737706")]
    [TestCase(Rfc6238SecretSha512, OtpHashMode.Sha512, 20000000000, "47863826")]
    public void ComputeTOTPTest(string secret, OtpHashMode hash, long timestamp, string expectedOtp)
    {
        var otpCalc = new Totp(Encoding.UTF8.GetBytes(secret), 30, hash, expectedOtp.Length);
        var time = DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
        var otp = otpCalc.ComputeTotp(time);
        Assert.That(otp, Is.EqualTo(expectedOtp));
    }

    [Test]
    public void ContructorWithKeyProviderTest()
    {
        // Mock a key provider which always returns an all-zero HMAC (causing an all-zero OTP)
        var keyMock = new Mock<IKeyProvider>();
        keyMock.Setup(key => key.ComputeHmac(It.Is<OtpHashMode>(m => m == OtpHashMode.Sha1), It.IsAny<byte[]>())).Returns(new byte[20]);

        var otp = new Totp(keyMock.Object, 30, OtpHashMode.Sha1, 6);
        Assert.That(otp.ComputeTotp(), Is.EqualTo("000000"));
    }
}
