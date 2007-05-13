using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace HDGraph
{
    static class Program
    {
        private static bool launchForm = true;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            HDGTools.mySwitch = new TraceSwitch("traceLevelSwitch", "HDG TraceSwitch");
            try
            {
                Trace.WriteLineIf(HDGTools.mySwitch.TraceInfo, "Application started.");
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                MainForm form = new MainForm();
                ProcessCommandLineArgs(form);
                if (launchForm)
                    Application.Run(form);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
                Trace.TraceError(HDGTools.PrintError(ex));
                System.Resources.ResourceManager res = new System.Resources.ResourceManager(typeof(Program).Assembly.GetName().Name + ".Resources.ApplicationMessages", typeof(Program).Assembly);
                MessageBox.Show(res.GetString("CriticalError"),
                                res.GetString("Error"),
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Analyse and process the command lines arguments.
        /// </summary>
        /// <param name="form"></param>
        private static void ProcessCommandLineArgs(MainForm form)
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                string path = args[1]; // args[0] correspond � l'ex�cutable, args[1] au premier argument
                Trace.WriteLineIf(HDGTools.mySwitch.TraceInfo, "Path argument received: " + path);

                if (path == "/" + HDGTools.RestartAction.addToExplorerContextMenu.ToString())
                {
                    HDGTools.AddMeToExplorerContextMenu(true);
                    launchForm = false;
                }
                else if (path == "/" + HDGTools.RestartAction.removeFromExplorerContextMenu.ToString())
                {
                    HDGTools.RemoveMeFromExplorerContextMenu(true);
                    launchForm = false;
                }
                else if (path.StartsWith("/"))
                {
                    path = "";
                }
                else
                {   // chemin pour lequel lancer le scan au d�marrage
                    path = RemoveDoubleQuoteIfNecessary(path);
                    if (File.Exists(path) && Path.GetExtension(path) == ".hdg")
                    {
                        // le 1er argument est un fichier HDG � charger
                        form.LoadGraphFromFile(path);
                    }
                    else
                    {   // le 1er argument est un r�pertoire: il faut lancer le scan.
                        path = (new DirectoryInfo(path)).FullName;
                        form.comboBoxPath.Text = path;
                        form.SavePathHistory();
                        form.LaunchScanOnStartup = true;
                    }
                }
            }
            if (args.Length > 2)
            {
                string commandLineOption1 = args[2];
                ProcessArg(form, commandLineOption1);
            }
            if (args.Length > 3)
            {
                string commandLineOption2 = args[3];
                ProcessArg(form, commandLineOption2);
            }
        }

        private static void ProcessArg(MainForm form, string arg)
        {
            if (arg.StartsWith(OUTPUT_IMG_CMD_LINE_OPTION_PREFIX))
            {
                arg = arg.Substring(OUTPUT_IMG_CMD_LINE_OPTION_PREFIX.Length);
                form.OutputImgFilePath = RemoveDoubleQuoteIfNecessary(arg);
            }
            if (arg.StartsWith(OUTPUT_GRAPH_CMD_LINE_OPTION_PREFIX))
            {
                arg = arg.Substring(OUTPUT_GRAPH_CMD_LINE_OPTION_PREFIX.Length);
                form.OutputGraphFilePath = RemoveDoubleQuoteIfNecessary(arg);
            }
        }



        private const string OUTPUT_IMG_CMD_LINE_OPTION_PREFIX = "/imgOutput:";
        private const string OUTPUT_GRAPH_CMD_LINE_OPTION_PREFIX = "/graphOutput:";

        private static string RemoveDoubleQuoteIfNecessary(string path)
        {
            if (path.EndsWith("\""))
                path = path.Substring(0, path.Length - 1);
            if (path.StartsWith("\""))
                path = path.Substring(1);
            return path;
        }

    }
}