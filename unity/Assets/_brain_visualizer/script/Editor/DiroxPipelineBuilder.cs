using UnityEditor;

namespace dirox.emotiv.editor
{
    public class DiroxPipelineBuilder
    {
        [MenuItem ("Dirox/Build All")]
        public static void BuildEmotiv ()
        {
            string path = EditorUtility.SaveFolderPanel ("Choose Location of Built Game", "", "");
            string[] levels = new string[]{ "Assets/ipad.unity" };
            buildWindows32Bit (path, levels);
            buildOSXUniversal (path, levels);
        }

        [MenuItem ("Dirox/Build Windows 32 bit")]
        public static void BuildEmotivWindows32 ()
        {
            string path = EditorUtility.SaveFolderPanel ("Choose Location of Built Game", "", "");
            string[] levels = new string[]{ "Assets/ipad.unity" };
            buildWindows32Bit (path, levels);
        }

        [MenuItem ("Dirox/Build OSX Universal")]
        public static void BuildEmotivOSXUniversal ()
        {
            string path = EditorUtility.SaveFolderPanel ("Choose Location of Built Game", "", "");
            string[] levels = new string[]{ "Assets/ipad.unity" };
            buildOSXUniversal (path, levels);
        }



        private static void buildWindows32Bit (string path, string[] levels)
        {
            // build Windows
            BuildPipeline.BuildPlayer (levels, path + "/BrainViz_Win/brainviz.exe", BuildTarget.StandaloneWindows, BuildOptions.None);
            // copy dlls

            string[] dlls = System.IO.Directory.GetFiles (path + "/BrainViz_Win/brainviz_Data/Plugins", "*");
            foreach (string file in dlls) {
                System.IO.File.Move (file, path + "/BrainViz_Win/brainviz_Data/Mono/" + System.IO.Path.GetFileName (file));
            }
        }

        private static void buildOSXUniversal (string path, string[] levels)
        {
            BuildPipeline.BuildPlayer (levels, path + "/BrainViz_OSX/brainviz.app", BuildTarget.StandaloneOSX, BuildOptions.None);
            // copy dlls
            string[] libs = System.IO.Directory.GetFiles ("Assets/Plugins", "*.dylib");
            foreach (string file in libs) {
              System.IO.File.Copy (file, path + "/BrainViz_OSX/brainviz.app/Contents/Frameworks/MonoEmbedRuntime/osx/" + System.IO.Path.GetFileName (file));
            }
        }
    }
}