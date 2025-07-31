using System.Reflection;

namespace Bible.Backend.Services
{
    public sealed class DataAssembly
    {
        public static string[] GetManifestResourceNames()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var names = assembly.GetManifestResourceNames();
            return names;
        }

        public static Stream? GetManifestResourceStream(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            return assembly.GetManifestResourceStream(resourceName);
        }

        public static Assembly GetAssembly()
        {
            return Assembly.GetExecutingAssembly();
        }
    }
}