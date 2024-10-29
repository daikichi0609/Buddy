using UnityEngine;
using System.IO;

public static class CharaDataManager
{
    private static string m_Datapath = Application.dataPath + "/Resources/TestJson.json";
    public static string Datapath
    {
        get { return m_Datapath; }
    }

    //セーブのメソッド
    public static void SaveTest(PlayerStatus data)
    {
        string jsonstr = JsonUtility.ToJson(data);//受け取ったPlayerDataをJSONに変換
        StreamWriter writer = new StreamWriter(m_Datapath, false);//初めに指定したデータの保存先を開く
        writer.WriteLine(jsonstr);//JSONデータを書き込み
        writer.Flush();//バッファをクリアする
        writer.Close();//ファイルをクローズする
    }

    public static string LoadTest(string dataPath)
    {
        StreamReader reader = new StreamReader(dataPath); //受け取ったパスのファイルを読み込む
        string datastr = reader.ReadToEnd();//ファイルの中身をすべて読み込む
        reader.Close();//ファイルを閉じる

        return datastr;//読み込んだJSONファイルをstring型に変換して返す
    }
}