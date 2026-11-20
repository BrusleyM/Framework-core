using Framework.Editor;
using NUnit.Framework;

namespace Framework.Tests
{
    public class PackageVersionTests
    {
        [TestCase("1.2.3")]
        [TestCase("v1.2.3")]
        [TestCase(" 1.2.3 ")]
        [TestCase("1.2.3-preview.1")]
        [TestCase("1.2.3+build.5")]
        public void A_semantic_version_is_read(string text)
        {
            Assert.That(PackageVersion.TryParse(text, out var version), Is.True);
            Assert.That(version.Major, Is.EqualTo(1));
            Assert.That(version.Minor, Is.EqualTo(2));
            Assert.That(version.Patch, Is.EqualTo(3));
        }

        [TestCase("")]
        [TestCase(null)]
        [TestCase("1.2")]
        [TestCase("one.two.three")]
        [TestCase("^1.2.3")]
        public void Text_that_is_not_a_semantic_version_is_refused(string text)
        {
            Assert.That(PackageVersion.TryParse(text, out _), Is.False);
        }

        [TestCase("1.2.0", "1.2.0", true)]
        [TestCase("1.10.0", "1.9.0", true)]
        [TestCase("2.0.0", "1.9.9", true)]
        [TestCase("1.1.9", "1.2.0", false)]
        [TestCase("1.2.0-preview.1", "1.2.0", false)]
        [TestCase("1.2.0", "1.2.0-preview.1", true)]
        [TestCase("1.2.0-preview.2", "1.2.0-preview.1", true)]
        public void A_version_is_at_least_another_by_number_with_a_pre_release_below_its_release(string installed, string required, bool expected)
        {
            PackageVersion.TryParse(installed, out var installedVersion);
            PackageVersion.TryParse(required, out var requiredVersion);

            Assert.That(installedVersion.IsAtLeast(requiredVersion), Is.EqualTo(expected));
        }
    }
}
