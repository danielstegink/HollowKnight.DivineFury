using System.Reflection;

namespace DivineFury
{
    /// <summary>
    /// Stores variables and functions used by multiple files in this project
    /// </summary>
    public static class SharedData
    {
        /// <summary>
        /// Data for the save file
        /// </summary>
        public static LocalSaveData localSaveData { get; set; } = new LocalSaveData();

        public static Charms.DivineFuryCharm divineFury;

        private static DivineFury _logger = new DivineFury();

        /// <summary>
        /// Logs message to the shared mod log at AppData\LocalLow\Team Cherry\Hollow Knight\ModLog.txt
        /// </summary>
        /// <param name="message"></param>
        public static void Log(string message)
        {
            _logger.Log(message);
        }

        public static bool exaltationInstalled = false;

        /// <summary>
        /// Gets a non-static field (even a private one) from the given input class
        /// </summary>
        /// <typeparam name="I"></typeparam>
        /// <typeparam name="O"></typeparam>
        /// <param name="input"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static O GetField<I, O>(I input, string fieldName)
        {
            FieldInfo fieldInfo = input.GetType()
                                       .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            return (O)fieldInfo.GetValue(input);
        }

        /// <summary>
        /// Calls a private method from the given input class
        /// </summary>
        /// <typeparam name="I"></typeparam>
        /// <typeparam name="O"></typeparam>
        /// <param name="input"></param>
        /// <param name="fieldName"></param>
        /// <param name="parameters"></param>
        /// <param name="isStaticOrConst"></param>
        /// <returns></returns>
        public static O CallFunction<I, O>(I input, string fieldName, object[] parameters, bool isStaticOrConst = false)
        {
            BindingFlags typeFlag = BindingFlags.Instance;
            if (isStaticOrConst)
            {
                typeFlag = BindingFlags.Static;
            }

            MethodInfo methodInfo = input.GetType()
                                            .GetMethod(fieldName, BindingFlags.NonPublic | typeFlag);
            return (O)methodInfo.Invoke(input, parameters);
        }
    }
}