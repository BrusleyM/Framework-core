import pathlib
import sys
import tempfile
import unittest

sys.path.insert(0, str(pathlib.Path(__file__).resolve().parent))

import check_test_results
import create_host_project


class HostProjectTests(unittest.TestCase):
    def test_the_manifest_points_at_the_package_and_makes_its_tests_runnable(self):
        manifest = create_host_project.build_manifest("../..", "17.3.0", "1.6.0")

        self.assertEqual("file:../..", manifest["dependencies"]["com.vishnu.framework.core"])
        self.assertEqual("17.3.0", manifest["dependencies"]["com.unity.render-pipelines.universal"])
        self.assertEqual(["com.vishnu.framework.core"], manifest["testables"])

    def test_creating_the_project_writes_the_manifest_and_the_editor_version(self):
        with tempfile.TemporaryDirectory() as folder:
            package = pathlib.Path(folder) / "package"
            project = package / ".host"
            package.mkdir()

            create_host_project.create_host_project(project, package, "6000.3.9f1", "17.3.0", "1.6.0")

            self.assertEqual("m_EditorVersion: 6000.3.9f1\n", (project / "ProjectSettings" / "ProjectVersion.txt").read_text())
            self.assertIn("file:../..", (project / "Packages" / "manifest.json").read_text())
            self.assertTrue((project / "Assets").is_dir())


class TestResultsTests(unittest.TestCase):
    def write_results(self, folder, name, total, failed):
        path = pathlib.Path(folder) / name
        path.write_text(f'<test-run total="{total}" failed="{failed}" passed="{total - failed}"></test-run>')
        return path

    def test_results_with_tests_and_no_failures_pass(self):
        with tempfile.TemporaryDirectory() as folder:
            files = [self.write_results(folder, "editmode-results.xml", 80, 0)]

            self.assertEqual([], check_test_results.find_problems(files))

    def test_a_run_with_failures_is_reported_by_name(self):
        with tempfile.TemporaryDirectory() as folder:
            files = [self.write_results(folder, "playmode-results.xml", 5, 2)]

            self.assertEqual(["playmode-results.xml: 2 of 5 tests failed"], check_test_results.find_problems(files))

    def test_a_run_where_no_tests_ran_is_reported(self):
        with tempfile.TemporaryDirectory() as folder:
            files = [self.write_results(folder, "editmode-results.xml", 0, 0)]

            self.assertEqual(["editmode-results.xml: no tests ran"], check_test_results.find_problems(files))

    def test_a_result_file_that_is_not_valid_xml_is_reported_instead_of_crashing(self):
        with tempfile.TemporaryDirectory() as folder:
            broken = pathlib.Path(folder) / "editmode-results.xml"
            broken.write_text("<test-run")

            self.assertEqual(["editmode-results.xml: could not be read as test results"], check_test_results.find_problems([broken]))

    def test_missing_result_files_are_reported(self):
        self.assertEqual(["No test result files were found."], check_test_results.find_problems([]))


if __name__ == "__main__":
    unittest.main()
