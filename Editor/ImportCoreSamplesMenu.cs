using UnityEditor;
using UnityEngine;

namespace Framework.Editor
{
    public static class ImportCoreSamplesMenu
    {
        [MenuItem("Tools/Framework/Import core samples")]
        public static void Import()
        {
            if (EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("Stop play mode and wait for the compile to finish before importing samples.");
                return;
            }

            if (!Application.isBatchMode && !EditorUtility.DisplayDialog("Import core samples", "Copy the Framework Core samples into Assets/Samples?", "Import", "Cancel"))
            {
                return;
            }

            SampleImporter.ImportAll(SampleImporter.FindPackageRoot("Framework.Core.Editor"), "Framework Core");
        }
    }
}
