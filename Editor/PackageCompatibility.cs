namespace Framework.Editor
{
    public static class PackageCompatibility
    {
        public static string FindProblem(string dependentName, string dependencyName, string installedVersion, string requiredVersion)
        {
            if (!PackageVersion.TryParse(requiredVersion, out var required))
            {
                return $"{dependentName} declares '{requiredVersion}' as the {dependencyName} version it needs, which is not a version like 1.2.0.";
            }

            if (!PackageVersion.TryParse(installedVersion, out var installed))
            {
                return $"{dependentName} needs {dependencyName} {requiredVersion} or newer, but the installed version '{installedVersion}' could not be read.";
            }

            if (installed.IsAtLeast(required))
            {
                return null;
            }

            return $"{dependentName} needs {dependencyName} {requiredVersion} or newer, but {installedVersion} is installed. Update {dependencyName}.";
        }
    }
}
