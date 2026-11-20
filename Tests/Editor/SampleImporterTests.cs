using System.IO;
using Framework.Editor;
using NUnit.Framework;

namespace Framework.Tests
{
    public class SampleImporterTests
    {
        private string _root;

        [SetUp]
        public void SetUp()
        {
            _root = Path.Combine(Path.GetTempPath(), "sample-importer-" + System.Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_root);
        }

        [TearDown]
        public void TearDown()
        {
            Directory.Delete(_root, true);
        }

        [Test]
        public void The_package_manifest_lists_the_samples_with_their_names_and_paths()
        {
            var json = "{\"displayName\":\"Framework Core\",\"samples\":[{\"displayName\":\"Showcase\",\"description\":\"d\",\"path\":\"Samples~/Showcase\"}]}";

            var package = SampleImporter.ReadPackage(json);

            Assert.That(package.displayName, Is.EqualTo("Framework Core"));
            Assert.That(package.samples.Length, Is.EqualTo(1));
            Assert.That(package.samples[0].displayName, Is.EqualTo("Showcase"));
            Assert.That(package.samples[0].path, Is.EqualTo("Samples~/Showcase"));
        }

        [Test]
        public void A_manifest_without_samples_gives_an_empty_list()
        {
            var package = SampleImporter.ReadPackage("{\"displayName\":\"Framework Core\"}");

            Assert.That(package.samples, Is.Empty);
        }

        [Test]
        public void A_sample_goes_to_the_samples_folder_under_the_package_and_sample_names()
        {
            Assert.That(SampleImporter.DestinationFor("Framework Core", "Showcase"), Is.EqualTo("Assets/Samples/Framework Core/Showcase"));
        }

        [Test]
        public void Copying_takes_every_file_and_folder_including_the_meta_files()
        {
            var source = Path.Combine(_root, "source");
            Directory.CreateDirectory(Path.Combine(source, "Scenes"));
            File.WriteAllText(Path.Combine(source, "a.cs"), "a");
            File.WriteAllText(Path.Combine(source, "a.cs.meta"), "meta");
            File.WriteAllText(Path.Combine(source, "Scenes", "b.unity"), "b");
            var destination = Path.Combine(_root, "destination");

            SampleImporter.Copy(source, destination);

            Assert.That(File.ReadAllText(Path.Combine(destination, "a.cs")), Is.EqualTo("a"));
            Assert.That(File.Exists(Path.Combine(destination, "a.cs.meta")), Is.True);
            Assert.That(File.ReadAllText(Path.Combine(destination, "Scenes", "b.unity")), Is.EqualTo("b"));
        }

        [Test]
        public void Copying_refuses_to_overwrite_an_existing_folder()
        {
            var source = Path.Combine(_root, "source");
            var destination = Path.Combine(_root, "destination");
            Directory.CreateDirectory(source);
            Directory.CreateDirectory(destination);

            Assert.Throws<IOException>(() => SampleImporter.Copy(source, destination));
        }

        [Test]
        public void Copying_a_sample_that_does_not_exist_says_so()
        {
            Assert.Throws<DirectoryNotFoundException>(() => SampleImporter.Copy(Path.Combine(_root, "missing"), Path.Combine(_root, "destination")));
        }
    }
}
