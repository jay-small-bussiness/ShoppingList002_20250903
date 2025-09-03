#if ANDROID
using Android.Runtime;

namespace ShoppingList002.Platforms.Android
{
    public static class AndroidKanaBridge
    {
        static readonly IntPtr s_class;
        static readonly IntPtr s_getReading;
        static readonly IntPtr s_setUserDict;

        static AndroidKanaBridge()
        {
            IntPtr localRef = JNIEnv.FindClass("com/yourapp/kana/KanaConverter");
            s_class = JNIEnv.NewGlobalRef(localRef);

            s_getReading = JNIEnv.GetStaticMethodID(
                s_class,
                "getReadingKatakana",
                "(Ljava/lang/String;)Ljava/lang/String;");

            s_setUserDict = JNIEnv.GetStaticMethodID(
                s_class,
                "setUserDictionary",
                "(Ljava/lang/String;)V");
        }

        /// <summary>
        /// CSV本文をJavaに渡してユーザー辞書を設定
        /// </summary>
        public static void SetUserDictionary(string csvContent)
        {
            using var jstr = new Java.Lang.String(csvContent ?? "");
            JNIEnv.CallStaticVoidMethod(s_class, s_setUserDict, new JValue(jstr.Handle));
        }

        public static string ToKatakana(string text)
        {
            IntPtr jText = IntPtr.Zero;
            try
            {
                jText = JNIEnv.NewString(text ?? string.Empty);
                IntPtr ret = JNIEnv.CallStaticObjectMethod(
                    s_class,
                    s_getReading,
                    new JValue(jText));

                return JNIEnv.GetString(ret, JniHandleOwnership.TransferLocalRef) ?? string.Empty;
            }
            finally
            {
                if (jText != IntPtr.Zero) JNIEnv.DeleteLocalRef(jText);
            }
        }
    }
}
#endif


//#if ANDROID
//using Android.Runtime;
//using System.Text;

//namespace ShoppingList002.Platforms.Android
//{
//    public static class AndroidKanaBridge
//    {
//        // ★ クラスとメソッドIDを静的にキャッシュして、解放しない
//        //static readonly IntPtr s_class;
//        static readonly IntPtr s_method;
//        static IntPtr s_class, s_getReading, s_setUserDict;

//        static AndroidKanaBridge()
//        {
//            IntPtr localRef = JNIEnv.FindClass("com/yourapp/kana/KanaConverter");
//            s_class = JNIEnv.NewGlobalRef(localRef);

//            s_getReading = JNIEnv.GetStaticMethodID(s_class,
//                "getReadingKatakana", "(Ljava/lang/String;)Ljava/lang/String;");

//            s_setUserDict = JNIEnv.GetStaticMethodID(s_class,
//                "setUserDictionary", "(Ljava/lang/String;)V");
//        }


//        //public static void SetUserDictionary(string csv)
//        //{
//        //    using var jstr = new Java.Lang.String(csv ?? "");
//        //    JNIEnv.CallStaticVoidMethod(s_class, s_setUserDict, new JValue(jstr.Handle));
//        //}
//        public static void SetUserDictionary(string csvContent)
//        {
//            using var jstr = new Java.Lang.String(csvContent);
//            JNIEnv.CallStaticVoidMethod(s_class, s_setUserDict, new JValue(jstr.Handle));
//        }

//        //public static void SetUserDictionary(string csv)
//        //{
//        //    // 一時ファイルにUTF-8 (BOMなし) で書き出す
//        //    var tmpPath = System.IO.Path.Combine(
//        //        System.IO.Path.GetTempPath(), "userdict.csv");
//        //    System.IO.File.WriteAllText(tmpPath, csv, new System.Text.UTF8Encoding(false));

//        //    using var jstr = new Java.Lang.String(tmpPath);
//        //    JNIEnv.CallStaticVoidMethod(s_class, s_setUserDict, new JValue(jstr.Handle));
//        //}

//        public static string ToKatakana(string text)
//        {
//            IntPtr jText = IntPtr.Zero;
//            try
//            {
//                jText = JNIEnv.NewString(text ?? string.Empty);
//                // ★ s_method じゃなく s_getReading を使う
//                IntPtr ret = JNIEnv.CallStaticObjectMethod(
//                    s_class,
//                    s_getReading,
//                    new JValue(jText));
//                // 結果は TransferLocalRef で Xamarin 側が delete してくれる
//                return JNIEnv.GetString(ret, JniHandleOwnership.TransferLocalRef) ?? string.Empty;
//            }
//            catch (System.Exception ex)
//            {
//                Console.WriteLine("AndroidKanaBridge", $"toKatakana failed: {ex.ToString}");
//                return text ?? string.Empty;
//            }
//            finally
//            {
//                if (jText != IntPtr.Zero) JNIEnv.DeleteLocalRef(jText); // ★ ここだけ消す
//                // DO NOT: JNIEnv.DeleteLocalRef(s_class);
//            }
//        }
//    }
//}
//#endif
