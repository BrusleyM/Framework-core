using UnityEditor;

namespace Framework.Editor
{
    public static class ImportCoreSamplesMenu
    {
        [MenuItem("Tools/Framework/Import core samples")]
        public static void Import()
        {
            SampleImporter.ImportAll(SampleImporter.FindPackageRoot("Framework.Core.Editor"), "Framework Core");
        }
    }
}
