using Android.Runtime;
using Java.Lang;

namespace ShoppingList002.Platforms.Android.Kana
{
    public static class KanaConverterWrapper
    {
        public static string ToKatakana(string text)
        {
            // com.yourapp.kana.KanaConverter.getReadingKatakana(String)
            IntPtr classHandle = JNIEnv.FindClass("com/yourapp/kana/KanaConverter");
            IntPtr methodId = JNIEnv.GetStaticMethodID(classHandle, "getReadingKatakana", "(Ljava/lang/String;)Ljava/lang/String;");

            IntPtr nativeText = JNIEnv.NewString(text);
            IntPtr result = JNIEnv.CallStaticObjectMethod(classHandle, methodId, new JValue(nativeText));
            string output = JNIEnv.GetString(result, JniHandleOwnership.TransferLocalRef);

            JNIEnv.DeleteLocalRef(nativeText);
            JNIEnv.DeleteLocalRef(classHandle);

            return output;
        }
    }
}
