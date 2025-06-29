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

        public static Charms.DivineFuryCharm divineFury = new Charms.DivineFuryCharm();

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
    }
}