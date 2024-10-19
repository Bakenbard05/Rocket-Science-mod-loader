using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace RocketScienceModLoader
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        public Int32 GetProcessId(String proc)
        {
            Process[] procList;
            procList = Process.GetProcessesByName(proc);
            return procList[0].Id;
        }
        public static void InjectDLL(string procName, string dllPath)
        {
            int exitCode;
            ProcessStartInfo processInfo;
            Process process;
            processInfo = new ProcessStartInfo("Inject.exe", $"l \"{procName}.exe\" {dllPath}");
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.Verb = "runas";
            // *** Redirect the output ***
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;
            process = Process.Start(processInfo);
            process.WaitForExit();
            // *** Read the streams ***
            // Warning: This approach can lead to deadlocks, see Edit #2
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            exitCode = process.ExitCode;

            Console.WriteLine(error);
            Console.WriteLine(output);
            process.Close();
        }
        public static void DeinjectDLL(string processName, string dllName)
        {
            int exitCode;
            ProcessStartInfo processInfo;
            Process process;
            processInfo = new ProcessStartInfo("Inject.exe", $"u {processName} {dllName}");
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.Verb = "runas";
            // *** Redirect the output ***
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;
            process = Process.Start(processInfo);
            process.WaitForExit();
            // *** Read the streams ***
            // Warning: This approach can lead to deadlocks, see Edit #2
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            exitCode = process.ExitCode;
            Console.WriteLine(error);
            Console.WriteLine(output);
            process.Close();
        }
        
        public void CompileMod(string name)
        {
            // Создаёт dll загрузчика
            // Creates dll of loader
            string t1 = "// dllmain.cpp : Defines the entry point for the DLL application.\r\n#include \"pch.h\"\r\n#include <windows.h>\r\n#include <iostream>\r\n#include <mono/jit/jit.h>\r\n#include <fstream>\r\n#include <iostream>\r\n#include <mono/metadata/metadata.h>\r\n#include <mono/metadata/assembly.h>\r\n#include <mono/metadata/image.h>\r\n#include <mono/metadata/object.h>\r\n#include <mono/metadata/class.h>\r\n#include <vector>\r\n";
            string t = $"#define ASSEMBLY_PATH \"/{name}.dll\"\r\n#define FILE_NAME \"{name}.dll\"\r\n";
            string loader_text = File.ReadAllText(".\\LoaderFiles\\loader\\dllmain.cpp");
            string loade_code = t1 + t + loader_text;
            File.WriteAllText(".\\LoaderFiles\\loader\\code.cpp", loade_code);
            var processInfo = new ProcessStartInfo("cmd.exe", "/c " + "call LoaderFiles/x64/build.bat");
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;

            var process = Process.Start(processInfo);

            process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
                Console.WriteLine("output>>" + e.Data);
            process.BeginOutputReadLine();

            process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
                Console.WriteLine("error>>" + e.Data);
            process.BeginErrorReadLine();

            process.WaitForExit();

            Console.WriteLine("ExitCode: {0}", process.ExitCode);
            process.Close();
        }
        string modName = "";

        private void InjectButton_Click(object sender, RoutedEventArgs e)
        {
            modName = ModName.Text;
            String strProcessName = "Rocket Science";
            try
            {
                Int32 ProcId = GetProcessId(strProcessName);
                if(ProcId >= 0)
                {
                    if(File.Exists($"./{modName}Loader.dll"))
                    {
                        File.Delete($"./{modName}Loader.dll");
                    }
                    CompileMod(modName);
                    File.Move("code.dll", $"./{modName}Loader.dll");
                    FileInfo f = new FileInfo($"{modName}Loader.dll");
                    Console.WriteLine(f.FullName.Replace(@"\", "/"));
                    InjectDLL(strProcessName, f.FullName.Replace(@"\", "/"));
                }
            } catch(IndexOutOfRangeException ex)
            {
                MessageBox.Show("Something wrong... Are you sure that game is running?");
            }
        }

        private void ModName_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        private void CheckProcess_Click(object sender, RoutedEventArgs e)
        {
            String strProcessName = "Rocket Science";
            try
            {
                Process proc = Process.GetProcessesByName(strProcessName)[0];
                foreach (ProcessModule mod in proc.Modules)
                {
                    Console.WriteLine(mod.ModuleName);
                }
            }
            catch (IndexOutOfRangeException ex)
            {
                MessageBox.Show("Something wrong... Are you sure that game is running?");
            }
        }

        private void DetachButton_Click(object sender, RoutedEventArgs e)
        {
            modName = ModName.Text;
            DeinjectDLL("\"Rocket Science.exe\"", $"\"{modName}Loader.dll\"");
            DeinjectDLL("\"Rocket Science.exe\"", $"\"{modName}.dll\"");
        }
    }
}
