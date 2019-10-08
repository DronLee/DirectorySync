using System;
using System.IO;

namespace XUnitTestProject.Infrastructure
{
    public static class TestFilesManager
    {
        public static string TestFilesDirectory => Path.Combine(Environment.CurrentDirectory, "TestFiles");

        public static string SettingsDirectory => Path.Combine(TestFilesDirectory, "Settings");
    }
}