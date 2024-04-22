namespace TsugamerLibrary
{
    using UnityEngine;

    ///---------------------------------------------------------------------------------
    /// <summary>
    /// シングルトンベースクラス
    /// </summary>
    ///---------------------------------------------------------------------------------
    public abstract class Singleton<T> : MonoBehaviour
        where T : MonoBehaviour
    {
        private static T ms_Instance; //!< シングルトンのインスタンス

        ///---------------------------------------------------------------------------------
        /// <summary>
        /// インスタンスの取得
        /// </summary>
        ///
        /// <returns>
        /// シングルトンのインスタンス <br>
        /// </returns>
        ///---------------------------------------------------------------------------------
        public static T GetInstance()
        {
            return ms_Instance;
        }

        ///---------------------------------------------------------------------------------
        /// <summary>
        /// インスタンスのセット
        /// </summary>
        ///
        /// <returns>
        /// 無し
        /// </returns>
        ///---------------------------------------------------------------------------------
        private void SetInstance(T instance)
        {
            ms_Instance = instance;
        }


        ///---------------------------------------------------------------------------------
        /// <summary>
        /// インスタンスの削除
        /// </summary>
        ///
        /// <returns>
        /// 無し
        /// </returns>
        ///---------------------------------------------------------------------------------
        public void DestroyInstance()
        {
            if (ms_Instance != null)
            {
                Destroy(ms_Instance);
            }

            ms_Instance = null;
        }

        ///---------------------------------------------------------------------------------
        /// <summary>
        /// インスタンスの削除(EditorモードでDestroyではなくDestroyImmediateを使わねばならないケース)
        /// </summary>
        ///
        /// <returns>
        /// 無し
        /// </returns>
        ///---------------------------------------------------------------------------------
        public void DestroyImmediateInstance()
        {
            if (ms_Instance != null)
            {
                DestroyImmediate(ms_Instance);
            }

            ms_Instance = null;
        }

#if UNITY_EDITOR

        ///---------------------------------------------------------------------------------
        /// <summary>
        /// EditorでSingletonを継承したクラスを使用するときに、初期化を行うために呼ぶメソッド
        /// </summary>
        ///---------------------------------------------------------------------------------
        public void EditorAwake()
        {
            Awake();
        }
#endif

        ///---------------------------------------------------------------------------------
        /// <summary>
        /// Awake
        /// </summary>
        ///
        /// <returns>
        /// 無し
        /// </returns>
        ///---------------------------------------------------------------------------------
        protected virtual void Awake()
        {
#if DEBUG

            if (!ReferenceEquals(ms_Instance, null))
            {
                // シングルトンが破棄されていなかった場合にアサートを出す
                //DebugFunction.LogAssertionFormat("The singleton instance'{0}' was not explicitly released.", GetType().Name);
            }

#endif


            if (ms_Instance == null)
            {
                // 自分をSingletonとして登録する
                SetInstance(this as T);
            }
            else
            {
#if DEBUG
                //DebugFunction.LogAssertionFormat("The singleton '{0} ' already exist.", GetType().Name);
#endif
            }
        }
    }
}