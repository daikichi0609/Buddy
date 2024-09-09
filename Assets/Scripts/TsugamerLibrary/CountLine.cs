using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public class CountLine : EditorWindow
{
    private static readonly string PATH = "/Users/daikichi/Desktop/UnityProjects/Buddy/Assets/Scripts";

    [MenuItem("Window/LineCount")]
    public static void Init()
    {
        CountLine window = EditorWindow.GetWindow<CountLine>("CountLine");
        window.Show();
        window.Focus();
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Count All Script line"))
        {
            DirectoryInfo dir = new DirectoryInfo(PATH); // Application.dataPath
            FileInfo[] info = dir.GetFiles("*.cs", System.IO.SearchOption.AllDirectories);
            if (info == null || info.Length == 0)
            {
                return;
            }

            int allLineCount = 0;
            info.Where((x) => x.Name != "CountLine.cs").ToList().ForEach((x) =>
            {

                using (StreamReader sr = x.OpenText())
                {
                    while (sr.ReadLine() != null)
                    {
                        allLineCount++;
                    }
                }
            });

            Debug.LogFormat("Number of All Scripts line is {0}", allLineCount);

        }
    }
}