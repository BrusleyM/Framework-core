using Framework.Editor;
using NUnit.Framework;

namespace Framework.Tests
{
    public class PackageCompatibilityTests
    {
        [Test]
        public void An_installed_version_that_meets_the_requirement_has_no_problem()
        {
            Assert.That(PackageCompatibility.FindProblem("Sequencer", "Core", "1.3.0", "1.2.0"), Is.Null);
        }

        [Test]
        public void An_older_installed_version_says_which_version_is_needed_and_which_is_installed()
        {
            var problem = PackageCompatibility.FindProblem("Sequencer", "Core", "1.1.0", "1.2.0");

            Assert.That(problem, Does.Contain("Sequencer needs Core 1.2.0 or newer").And.Contain("1.1.0 is installed"));
        }

        [Test]
        public void An_unreadable_requirement_is_reported_as_a_manifest_mistake()
        {
            Assert.That(PackageCompatibility.FindProblem("Sequencer", "Core", "1.1.0", "^1.2"), Does.Contain("not a version"));
        }

        [Test]
        public void An_unreadable_installed_version_is_reported()
        {
            Assert.That(PackageCompatibility.FindProblem("Sequencer", "Core", "", "1.2.0"), Does.Contain("could not be read"));
        }
    }
}
