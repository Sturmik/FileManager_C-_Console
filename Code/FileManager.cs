using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace SimpleFileManager
{
    enum Filter { Name = 1, Size, Date, Access, Modification};

    /// <summary>
    /// User class, which saves info about the user
    /// </summary>
    [Serializable]
    internal class User
    {
        #region Variables

        public bool Admin { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public List<string> History { get; set; }

        #endregion

        #region Constructor

        public User() { History = new List<string>(); }

        #endregion

        #region Methods

        /// <summary>
        /// Loads user
        /// </summary>
        public void LoadUser(string login)
        {
            if (CheckUserLogin(login) == true)
            {
                using (FileStream fs = new FileStream(@"Data\Users\" + login, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    BinaryFormatter BF = new BinaryFormatter();
                    User loadIn = new User();
                    loadIn = (User)BF.Deserialize(fs);

                    Login = loadIn.Login;
                    Password = loadIn.Password;
                    Admin = loadIn.Admin;
                }
            }
        }

        /// <summary>
        /// Saves the user
        /// </summary>
        public void SaveUser(string login)
        {
            using (FileStream fs = new FileStream(@"Data\Users\" + login, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write))
            {
                BinaryFormatter BF = new BinaryFormatter();
                BF.Serialize(fs, this);
            }
        }

        /// <summary>
        /// Checks if users exist
        /// </summary>
        /// <returns></returns>
        public bool UserCheck()
        {
            if (!Directory.Exists(@"Data\Users"))
                Directory.CreateDirectory(@"Data\Users");

            if (Directory.GetFileSystemEntries(@"Data\Users").Length > 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Check if user exist
        /// </summary>
        /// <param name="login">User login</param>
        /// <returns></returns>
        public bool CheckUserLogin(string login)
        {
            try
            {
                Regex cleanUp = new Regex(@"\\[^\\]+$");
                List<string> checkFile = Directory.GetFiles(@"Data\Users").ToList();
                foreach (var i in checkFile)
                {
                    if (cleanUp.Match(i).Value.Remove(0,1).Equals(login))
                        return true;
                }
                return false;
            }
            catch(Exception e)
            { return false; }
        }

        #endregion
    }

    /// <summary>
    /// Simple file manager
    /// </summary>
    public static class FileManager
    {
        #region Variables

        // Our interface buttons, with which we will interact with
        private static Window baseWindow;
        private static List<Button> actionButtons;
        private static Button location;
        private static Window logicDiskWindow;
        private static List<Button> logicDiskButtons;
        private static Button conditionInfo;
        private static Window filesWindow;
        private static List<Button> filesButtons;
        private static Button fileInfo;

        // Size of the current monitor
        private static int winX, winY;

        // User's logic disks
        private static List<string> logicDisks;
        // User's files
        private static List<string> files;
        // Current location stores the path, which user has made 
        private static List<string> currentLocation;
        // Results of finding function
        private static List<string> findResults;
        // String, which helps in finding of files or directories
        private static string[] analyzeString;
        // Analyzes files
        private static FileInfo analyzeF;
        // Analyzes directories
        private static DirectoryInfo analyzeD;

        // Timer, which shows how long the finding procedure was lasting
        private static DateTime timer;
        // History of the file manager approach
        private static List<string> history;
        // Statistic of the most viewed files and dictionaries
        private static Dictionary<string, int> statistic;

        // Checks if buffer isn't empty
        private static bool bufferInside;
        // For copy or move methods
        private static string buffer;

        // User
        private static User user;

        #endregion

        #region Constructor

        static FileManager()
        {
            logicDisks = new List<string>();
            files = new List<string>();
            currentLocation = new List<string>();
            findResults = new List<string>();
            timer = new DateTime();
            history = new List<string>();
            statistic = new Dictionary<string, int>();
            LoadStatistic();
        }

        #endregion

        #region Methods

        // Calculate method for various tasks
        #region CalculateMethods

        private static double ByteInGigaByteConvert(double bytes)
        {
            return bytes * 9.3132257461548 * Math.Pow(10, -10);
        }

        private static double ByteInMegaByteConvert(double bytes)
        {
            return bytes * 9.5367431640625 * Math.Pow(10, -7);
        }

        #endregion

        #region Interface

        // Methods, which help to setup interface for user
        // It adapts to the screen resoulution
        #region Interface_Settings_Methods / Interactions

        /// <summary>
        /// Forms active panel of interface
        /// </summary>
        /// <param name="user_admin">True - user | False - admin</param>
        /// <returns></returns>
        private static List<Button> FormingActionPanel(bool user_admin, ref int posX, ref int posY, int actionButtonSizeX, int actionButtonSizeY)
        {
            List<Button> actionButtons = new List<Button>();

            actionButtons.Add(SetUpActionButton(ref posX, ref posY, actionButtonSizeX, actionButtonSizeY, ConsoleColor.Yellow, ConsoleColor.White, "F1-Enter", "  Open"));
            actionButtons.Add(SetUpActionButton(ref posX, ref posY, actionButtonSizeX, actionButtonSizeY, ConsoleColor.Yellow, ConsoleColor.White, " F2-ESC", "  Back"));
            actionButtons.Add(SetUpActionButton(ref posX, ref posY, actionButtonSizeX, actionButtonSizeY, ConsoleColor.Yellow, ConsoleColor.White, "   F3", "  Find"));
            if (user_admin == false)
            {
                actionButtons.Add(SetUpActionButton(ref posX, ref posY, actionButtonSizeX, actionButtonSizeY, ConsoleColor.Yellow, ConsoleColor.White, "   F4", "  Copy"));
                actionButtons.Add(SetUpActionButton(ref posX, ref posY, actionButtonSizeX, actionButtonSizeY, ConsoleColor.Yellow, ConsoleColor.White, "   F5", "  Move"));
                actionButtons.Add(SetUpActionButton(ref posX, ref posY, actionButtonSizeX + 1, actionButtonSizeY, ConsoleColor.Yellow, ConsoleColor.White, "   F6", " Delete"));
                actionButtons.Add(SetUpActionButton(ref posX, ref posY, actionButtonSizeX, actionButtonSizeY, ConsoleColor.Yellow, ConsoleColor.White, "   F7", " Rename"));
            }
            actionButtons.Add(SetUpActionButton(ref posX, ref posY, actionButtonSizeX + 5, actionButtonSizeY, ConsoleColor.Red, ConsoleColor.White, "    F8", " Most viewed"));
            actionButtons.Add(SetUpActionButton(ref posX, ref posY, actionButtonSizeX + 1, actionButtonSizeY, ConsoleColor.Red, ConsoleColor.White, "   F9", " History"));
            actionButtons.Add(SetUpActionButton(ref posX, ref posY, actionButtonSizeX + 5, actionButtonSizeY, ConsoleColor.Red, ConsoleColor.White, "    F10", " Change user"));
            actionButtons.Add(SetUpActionButton(ref posX, ref posY, actionButtonSizeX, actionButtonSizeY, ConsoleColor.Green, ConsoleColor.White, "    ^", "   Up"));
            actionButtons.Add(SetUpActionButton(ref posX, ref posY, actionButtonSizeX, actionButtonSizeY, ConsoleColor.Green, ConsoleColor.White, "    V", "  Down"));
            actionButtons.Add(SetUpActionButton(ref posX, ref posY, actionButtonSizeX + 3, actionButtonSizeY, ConsoleColor.Green, ConsoleColor.White, "   <-", " Page back"));
            actionButtons.Add(SetUpActionButton(ref posX, ref posY, actionButtonSizeX + 6, actionButtonSizeY, ConsoleColor.Green, ConsoleColor.White, "   ->", " Page forward"));

            return actionButtons;
        }

        /// <summary>
        /// Adapts action button for screen resolution
        /// </summary>
        /// <returns>ActionButton</returns>
        private static Button SetUpActionButton(ref int posX, ref int posY, int actionButtonSizeX, int actionButtonSizeY, ConsoleColor outter, ConsoleColor inside, params string[] text)
        {
            if (posX >= winX - actionButtonSizeX)
            {
                posX = 2;
                posY += actionButtonSizeY + 2;
            }

            Button actionButton = new Button(posX, posY, actionButtonSizeX, actionButtonSizeY, outter, inside, true, text);
            posX += actionButtonSizeX + 5;
            return actionButton;
        }

        /// <summary>
        /// Adapts logic disk buttons for logic disk window space
        /// </summary>
        /// <returns></returns>
        private static List<Button> FormingChooseButton(ref int posX, ref int posY, int width, int height, ConsoleColor outter, ConsoleColor inside, int buttonsQuant)
        {
            List<Button> buttonsToForm = new List<Button>();

            for (int i = 0; i < buttonsQuant; i++)
            {
                buttonsToForm.Add(new Button(posX + 2, posY + 1, width, height, outter, inside));
                posY += 4 + height - 2;
            }

            buttonsToForm.Add(new Button(buttonsToForm[0].X + buttonsToForm[0].Width / 2 - 5, posY + 1, 5, 1, ConsoleColor.Red, ConsoleColor.White));

            return buttonsToForm;
        }

        /// <summary>
        /// Interface for work
        /// </summary>
        /// <param name="user_admin">True - User functions | False - Admin functions</param>
        private static void SetUpUserInterface(bool user_admin = true)
        {
            // Adapting buttons for window size

            // Size of the main window X
            int baseWinSizeX = winX - 1;

            // Size of the action buttons
            int actionButtonSizeX = baseWinSizeX / (winX / 10);
            int actionButtonSizeY = 2;

            // Position of buttons
            int posX = 2;
            int posY = 1;

            // Forming action panel
            actionButtons = FormingActionPanel(user_admin, ref posX, ref posY, actionButtonSizeX, actionButtonSizeY);

            posX = 2;
            posY += actionButtonSizeY * 2;


            // Forming location panel
            location = new Button(posX, posY, winX - 5, 1, ConsoleColor.White, ConsoleColor.Yellow, true);

            posY += 3;

            // Forming logic disk window

            logicDiskWindow = new Window(posX, posY, winX / 4, 23, ConsoleColor.White);

            logicDiskButtons = FormingChooseButton(ref posX, ref posY, logicDiskWindow.Width - 4, 3, ConsoleColor.Black, ConsoleColor.White, 4);

            // Forming condition info window

            conditionInfo = new Button(logicDiskWindow.X, logicDiskWindow.Y + logicDiskWindow.Height + 2, logicDiskWindow.Width, 1, ConsoleColor.Green, ConsoleColor.White);

            // Forming files window

            // Number of file buttons (adapts to size of the window)
            int numFiles = Console.LargestWindowHeight / 5;

            filesWindow = new Window(logicDiskWindow.X + logicDiskWindow.Width + 1, logicDiskWindow.Y, winX - logicDiskWindow.Width - 6, numFiles * 3 + 3, ConsoleColor.White);

            posX = filesWindow.X;
            posY = filesWindow.Y;

            filesButtons = FormingChooseButton(ref posX, ref posY, filesWindow.Width - 4, 1, ConsoleColor.Black, ConsoleColor.White, numFiles);


            int posForinfo = filesWindow.Y + filesWindow.Height >= conditionInfo.Y + conditionInfo.Height ? filesWindow.Y + filesWindow.Height : conditionInfo.Y + conditionInfo.Height;
            posForinfo += 2;

            // Forming file info window
            fileInfo = new Button(logicDiskWindow.X, posForinfo, location.Width, 1, ConsoleColor.White, ConsoleColor.Yellow);


            // Forming base window
            baseWindow = new Window(0, 0, baseWinSizeX, fileInfo.Y + 2, ConsoleColor.Blue);
        }

        /// <summary>
        /// Shows the interface
        /// </summary>
        private static void ShowInterface()
        {
            baseWindow.ShowWindow();
            foreach (Button act in actionButtons)
                act.ShowButton();
            location.ShowButton();
            logicDiskWindow.ShowWindow();
            foreach (Button logAct in logicDiskButtons)
                logAct.ShowButton();
            conditionInfo.ShowButton();
            filesWindow.ShowWindow();
            foreach (Button fileAct in filesButtons)
                fileAct.ShowButton();
            fileInfo.ShowButton();
        }

        /// <summary>
        /// Shows all buttons from the list
        /// </summary>
        /// <param name="which">List of buttons</param>
        private static void ShowButtons(List<Button> which)
        {
            // Showing the buttons
            foreach (Button button in which)
            {
                button.ShowButton();
            }
        }

        /// <summary>
        /// Changes state of all buttons in the list to unpressed
        /// </summary>
        /// <param name="which">List Of Buttons</param>
        private static void ReleaseButtonList(List<Button> which)
        {
            foreach (Button toRelease in which)
                toRelease.Pressed = false;
        }

        /// <summary>
        /// Finds all pressed buttons and set them in unpressed mode
        /// </summary>
        /// <param name="which">List Of Buttons</param>
        private static void ReleaseAndShowButton(List<Button> which)
        {
            foreach (Button toDisable in which)
            {
                if (toDisable.Pressed == true)
                {
                    toDisable.Pressed = false;
                    toDisable.ShowButton();
                }
            }
        }

        /// <summary>
        /// Cleans text in all buttons in the list
        /// </summary>
        /// <param name="which">List Of Buttons</param>
        private static void CleanButtonsText(List<Button> which)
        {
            foreach (Button toClean in which)
            {
                toClean.ClearInnerText();
            }
        }

        /// <summary>
        /// Filling logic disk buttons with text
        /// </summary>
        private static void LogicDiskButtonsShow(ref int selectorLog, ref int page)
        {
            if (selectorLog >= logicDisks.Count)
                selectorLog = logicDisks.Count - 1;
            else
                if (selectorLog < 0)
                selectorLog = 0;

            // Reading size of the hard drive
            DriveInfo readSize;

            // Checking page
            int minIn = selectorLog / (logicDiskButtons.Count - 1);

            // Checking if we need to refresh the page
            bool refreshCheck = false;

            if (minIn != page)
            {
                refreshCheck = true;
                page = minIn;

                CleanButtonsText(logicDiskButtons);
            }

            // Filling page button with number
            logicDiskButtons[logicDiskButtons.Count - 1].ChangeInnerText(false, $"{minIn + 1}");

            minIn *= logicDiskButtons.Count - 1;

            // Finding out which button to spotlight as pressed
            int buttonToPress = selectorLog - minIn;

            // Pressing the button
            logicDiskButtons[buttonToPress].Pressed = true;
            logicDiskButtons[buttonToPress].ShowButton();

            try
            {
                // Filling buttons with text
                for (int i = 0; i < logicDiskButtons.Count - 1; i++)
                {
                    readSize = new DriveInfo(logicDisks[minIn]);
                    logicDiskButtons[i].ChangeInnerText(false, $"Disk: {logicDisks[minIn]}",
                        $"Avaible space: {(int)ByteInGigaByteConvert(readSize.AvailableFreeSpace)} GB | {(int)ByteInMegaByteConvert(readSize.AvailableFreeSpace)} MB | {readSize.AvailableFreeSpace} B",
                        $"All space: {(int)ByteInGigaByteConvert(readSize.TotalSize)} GB | {(int)ByteInMegaByteConvert(readSize.TotalSize)} MB | {readSize.TotalSize} B");
                    minIn++;
                }
            }
            catch (Exception e) { }

            if (refreshCheck == true)
            {
                // Showing the buttons
                ShowButtons(logicDiskButtons);
            }
        }

        /// <summary>
        /// Updates action buttos and fills them with text
        /// </summary>
        private static void ActionButtonsShow(ref int selector, ref int page, List<Button> buttons, params string[] text)
        {
            if (text.Length == 0)
                return;
            if (selector >= text.Length)
                selector = text.Length - 1;
            else
                if (selector < 0)
                selector = 0;

            // Checking page
            int minIn = selector / (buttons.Count - 1);

            // Checking if we need to refresh the page
            bool refreshCheck = false;

            if (minIn != page)
            {
                refreshCheck = true;
                page = minIn;

                CleanButtonsText(buttons);
            }

            // Filling page button with number
            buttons[buttons.Count - 1].ChangeInnerText(false, $"{minIn + 1}");

            minIn *= buttons.Count - 1;

            // Finding out which button to spotlight as pressed
            int buttonToPress = selector - minIn;

            // Pressing the button
            buttons[buttonToPress].Pressed = true;
            buttons[buttonToPress].ShowButton();

            string[] splitter;

            try
            {
                // Filling buttons with text
                for (int i = 0; i < buttons.Count - 1; i++)
                {
                    splitter = text[minIn].Split('\\');
                    buttons[i].ChangeInnerText(false, splitter[splitter.Length - 1]);
                    minIn++;
                }
            }
            catch (Exception e) { }

            if (refreshCheck == true)
            {
                ShowButtons(buttons);
            }
        }

        /// <summary>
        /// Updates location window
        /// </summary>
        private static void UpdateLocation()
        {
            if (currentLocation.Count > 0)
            {
                location.ChangeInnerText(true, currentLocation[currentLocation.Count - 1]);
                location.ShowButton();
            }
        }

        /// <summary>
        /// Updates info window
        /// </summary>
        private static void UpdateInfoWindow(int selector)
        {
            if (files.Count > 0)
            {
                if (File.Exists(files[selector]))
                {
                    FileInfo analyze = new FileInfo(files[selector]);
                    fileInfo.ChangeInnerText(false, $"Size: {(int)ByteInGigaByteConvert(analyze.Length)} GB " +
                        $"{(int)ByteInMegaByteConvert(analyze.Length)} MB " +
                        $" | ReadOnly? - {analyze.IsReadOnly}" +
                        $" | Last access time: {analyze.LastAccessTime.ToShortDateString()}" +
                        $" | Last write time: {analyze.LastWriteTime.ToShortDateString()}" +
                        $" | Creation time: {analyze.CreationTime.ToShortDateString()}");
                }
                else
                    if (Directory.Exists(files[selector]))
                {
                    DirectoryInfo analyze = new DirectoryInfo(files[selector]);
                    fileInfo.ChangeInnerText(false, 
                        $"Last access time: {analyze.LastAccessTime.ToShortDateString()}" +
                        $" | Last write time: {analyze.LastWriteTime.ToShortDateString()}" +
                        $" | Creation time: {analyze.CreationTime.ToShortDateString()}");
                }
                else
                    fileInfo.ClearInnerText();

                fileInfo.ShowButton();
            }
        }

        /// <summary>
        /// Error message
        /// </summary>
        private static void ErrorMessage(string mess)
        {
            Regex cleanError = new Regex(@"[^\.]+\.");
            Button Error = new Button(filesWindow.Width / 3 + filesWindow.X, filesWindow.Height / 3 + filesWindow.Y, filesWindow.Width / 3, 3, ConsoleColor.Black, ConsoleColor.Black, false, "", $"{cleanError.Match(mess)}");
            Error.Pressed = true;
            Error.ShowButton();
            Console.ReadKey(true);
            filesWindow.ShowWindow();
        }

        /// <summary>
        /// Updated condition window
        /// </summary>
        private static void UpdateConditionWindow(string message)
        {
            conditionInfo.ChangeInnerText(false, message);
            conditionInfo.ShowButton();
        }

        /// <summary>
        /// Allows you to enter the data with strict format
        /// </summary>
        /// <returns></returns>
        private static string EnterData(int posX, int posY, int limit, bool wordsAndDigits_Digits, bool cypherText = false, bool updateCondWind = true)
        {
            Console.SetCursorPosition(posX, posY);

            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Black;
            string result = "";
            int count = 0;

            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);

                if (wordsAndDigits_Digits == false)
                    if (char.IsLetter(key.KeyChar))
                        continue;

                switch (key.Key)
                {
                    case ConsoleKey.Backspace:
                        if (result.Length > 0)
                        {
                            if (count != 0)
                            {
                                result = result.Remove(result.Length - 1, 1);
                                Console.Write("\b \b");
                                count--;
                            }
                        }
                        break;
                    case ConsoleKey.Enter:
                        Console.ResetColor();
                        result = result.Trim();
                        if (updateCondWind == true)
                             UpdateConditionWindow("WAITING FOR THE PROCEDURE TO END");
                        return result;
                    default:
                        if (char.IsDigit(key.KeyChar) || char.IsLetter(key.KeyChar))
                        {
                            if (count < limit)
                            {
                                if (cypherText == false)
                                    Console.Write(key.KeyChar);
                                else
                                    Console.Write("*");
                                result += key.KeyChar;
                                count++;
                            }
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static ConsoleKey ConfirmationWindow(string messageInside)
        {
            Button DeleteCheck = new Button(filesWindow.Width / 3 + filesWindow.X, filesWindow.Height / 3 + filesWindow.Y, 40, 3, ConsoleColor.Black, ConsoleColor.Black, true, "", messageInside);
            DeleteCheck.Pressed = true;
            DeleteCheck.ShowButton();

            ConsoleKey key = Console.ReadKey(true).Key;

            switch (key)
            {
                case ConsoleKey.Y:
                    return ConsoleKey.Y;
                default:
                    return ConsoleKey.N;
            }
        }

        #endregion

        // Menu interaction
        #region Menu

        /// <summary>
        /// Loads statistic
        /// </summary>
        private static void LoadStatistic()
        {
            if (Directory.Exists("Data"))
            {
                XmlDocument statDoc = new XmlDocument();

                string key = "";
                string value = "";

                if (File.Exists(@"Data\Statistic"))
                {
                    statDoc.Load(@"Data\Statistic");
                    XmlNodeList statXlist = statDoc.GetElementsByTagName("Info");

                    foreach (XmlNode ls in statXlist)
                    {
                        key = ls["Key"].InnerText;
                        value = ls["Value"].InnerText;
                        statistic.Add(key, int.Parse(value));
                    }
                }
            }
        }

        /// <summary>
        /// Save statistic
        /// </summary>
        private static void SaveStatistic()
        {
            if (!Directory.Exists("Data"))
                Directory.CreateDirectory("Data");

            XmlTextWriter writer = null;

            try
            {
                writer = new XmlTextWriter(@"Data\Statistic", Encoding.Unicode);

                writer.Formatting = Formatting.Indented;
                writer.WriteStartDocument();
                writer.WriteStartElement("Statistic");

                string[] keys = statistic.Keys.ToArray();

                foreach (string i in keys)
                {
                    writer.WriteStartElement("Info");

                    writer.WriteElementString("Key", i);
                    writer.WriteElementString("Value", statistic[i].ToString());

                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
            catch (Exception e) { }
            finally { if (writer != null) writer.Close(); }
        }

        /// <summary>
        /// Method, which helps us to find name. Works on recursion mechanic.
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="toSearchFor">Name</param>
        private static void FindNames(string[] path, string toSearchFor)
        {
            for (int i = 0; i < path.Length; i++)
            {
                try
                {
                    if (Directory.Exists(path[i]))
                        if (!File.Exists(path[i]))
                            FindNames(Directory.GetFileSystemEntries(path[i]), toSearchFor);
                    analyzeString = path[i].Split('\\');

                    analyzeString = analyzeString[analyzeString.Length - 1].Split('.');

                    if (analyzeString[0].ToUpper().Contains(toSearchFor.ToUpper()))
                        findResults.Add(path[i]);
                }
                catch (Exception e) { } // We are using try/catch to avoid mistakes, when pc doesn't allow us to work with folders or files
            }
        }

        /// <summary>
        /// Method, which helps us to find file with specific number of bytes. Works on recursion mechanic.
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="toSearchFor">Number of bytes</param>
        private static void FindByte(string[] path, string toSearchFor)
        {
            for (int i = 0; i < path.Length; i++)
            {
                try
                {
                    if (Directory.Exists(path[i]))
                        if (!File.Exists(path[i]))
                            FindByte(Directory.GetFileSystemEntries(path[i]), toSearchFor);

                    int.TryParse(toSearchFor, out int res);

                    analyzeF = new FileInfo(path[i]);

                    if (analyzeF.Length == res)
                        findResults.Add(path[i]);
                }
                catch (Exception e) { }
            }
        }

        /// <summary>
        /// Method, which helps us to find file with specific date. Works on recursion mechanic
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="toSearchFor">Date</param>
        private static void FindDate(string[] path, string toSearchFor)
        {
            for (int i = 0; i < path.Length; i++)
            {
                try
                {
                    if (Directory.Exists(path[i]))
                        if (!File.Exists(path[i]))
                            FindDate(Directory.GetFileSystemEntries(path[i]), toSearchFor);

                    if (File.Exists(path[i]))
                    {
                        analyzeF = new FileInfo(path[i]);
                        if (analyzeF.CreationTime.ToShortDateString() == toSearchFor)
                            findResults.Add(path[i]);
                    }
                    if (Directory.Exists(path[i]))
                    {
                        analyzeD = new DirectoryInfo(path[i]);
                        if (analyzeD.CreationTime.ToShortDateString() == toSearchFor)
                            findResults.Add(path[i]);
                    }
                }
                catch (Exception e) { }
            }
        }

        /// <summary>
        /// Method, which helps us to find file with specific access (Read or ReadWrite). Works on recursion mechanic
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="toSearchFor">Access ( 1. Read | 2. Write )</param>
        private static void FindAccess(string[] path, bool toSearchFor)
        {
            for (int i = 0; i < path.Length; i++)
            {
                try
                {
                    if (Directory.Exists(path[i]))
                        if (!File.Exists(path[i]))
                            FindAccess(Directory.GetFileSystemEntries(path[i]), toSearchFor);

                    if (File.Exists(path[i]))
                    {
                        analyzeF = new FileInfo(path[i]);
                        if (analyzeF.IsReadOnly == toSearchFor)
                            findResults.Add(path[i]);
                    }
                }
                catch (Exception e) { }
            }
        }

        /// <summary>
        /// Method, which helps us to find file with specific date of modification. Works on recursion mechanic
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="toSearchFor">Date to search</param>
        private static void FindModificationDate(string[] path, string toSearchFor)
        {
            for (int i = 0; i < path.Length; i++)
            {
                try
                {
                    if (Directory.Exists(path[i]))
                        if (!File.Exists(path[i]))
                            FindModificationDate(Directory.GetFileSystemEntries(path[i]), toSearchFor);

                    if (File.Exists(path[i]))
                    {
                        analyzeF = new FileInfo(path[i]);
                        if (analyzeF.LastAccessTime.ToShortDateString() == toSearchFor)
                            findResults.Add(path[i]);
                    }
                    if (Directory.Exists(path[i]))
                    {
                        analyzeD = new DirectoryInfo(path[i]);
                        if (analyzeD.LastAccessTime.ToShortDateString() == toSearchFor)
                            findResults.Add(path[i]);
                    }
                }
                catch (Exception e) { }
            }
        }

        /// <summary>
        /// Finding files by
        /// </summary>
        /// <param name="requir">Which requirment</param>
        /// <param name="startFrom">Start from</param>
        private static void FindFilesBy(Filter requir, string lookFor)
        {
            findResults.Clear();
            string[] toSearch;
            if (currentLocation.Count == 0)
                toSearch = Directory.GetLogicalDrives();
            else
                toSearch = Directory.GetFileSystemEntries(currentLocation[currentLocation.Count - 1]);

            switch (requir)
            {
                case Filter.Name:
                    FindNames(toSearch, lookFor);
                    break;
                case Filter.Size:
                    FindByte(toSearch, lookFor);
                    break;
                case Filter.Date:
                    FindDate(toSearch, lookFor);
                    break;
                case Filter.Access:
                    if (lookFor == "1")
                         FindAccess(toSearch, true);
                    if (lookFor == "2")
                          FindAccess(toSearch, false);
                    break;
                case Filter.Modification:
                    FindModificationDate(toSearch, lookFor);
                    break;
            }

            files = findResults;
        }

        /// <summary>
        /// Shows Up all files founded by specific requirment
        /// </summary>
        private static void FindFiles()
        {
            Button whatToFind = new Button(filesWindow.X + filesWindow.Width / 3, filesWindow.Y + filesWindow.Height / 3, 50, 6, ConsoleColor.Black, ConsoleColor.Red, false,
                "Find files-directories by: ", "1. Name", "2. Size(bytes)", "3. Date of creation (xx.xx.xxxx)", "4. ReadOnly (true/false)", "5. Last access (xx.xx.xxxx)");

            whatToFind.Pressed = true;
            whatToFind.ShowButton();

            Button enter = new Button(whatToFind.X, whatToFind.Y + whatToFind.Height + 5, 50, 2, ConsoleColor.Black, ConsoleColor.Black);
            enter.Pressed = true;
            conditionInfo.Pressed = true;

            timer = DateTime.Now;

            conditionInfo.Pressed = true;

            ConsoleKey key = Console.ReadKey(true).Key;

            switch (key)
            { 
                case ConsoleKey.D1:
                    enter.ChangeInnerText(true, "Enter the name to find: ");
                    enter.ShowButton();
                    FindFilesBy(Filter.Name , EnterData(enter.X + 1, enter.Y + 2, 40, true));
                    break;
                case ConsoleKey.D2:
                    enter.ChangeInnerText(true, "Enter the size to find: ");
                    enter.ShowButton();
                    UpdateConditionWindow("Waiting for size enter...");
                    FindFilesBy(Filter.Size, EnterData(enter.X + 1, enter.Y + 2, 40, false));
                    break;
                case ConsoleKey.D3:
                case ConsoleKey.D5:
                    enter.ChangeInnerText(true, "Enter the date to find: ");
                    enter.ShowButton();
                    UpdateConditionWindow("Enter day...");
                    string formDate = EnterData(enter.X + 1, enter.Y + 2, 2, false);
                    if (formDate.Length == 1)
                        formDate = formDate.Insert(0, "0");
                    UpdateConditionWindow("Enter month...");
                    formDate += "." + EnterData(enter.X + formDate.Length + 2, enter.Y + 2, 2, false);
                    if (formDate.Length == 4)
                        formDate = formDate.Insert(3, "0");
                    UpdateConditionWindow("Enter year...");
                    formDate += "." + EnterData(enter.X + formDate.Length + 2, enter.Y + 2, 4, false);

                    if (key == ConsoleKey.D3)
                        FindFilesBy(Filter.Date, formDate);
                    else
                        FindFilesBy(Filter.Modification, formDate);
                    break;
                case ConsoleKey.D4:
                    UpdateConditionWindow("Enter your choice...");
                    enter.ChangeInnerText(true, "Enter | 1 - ReadOnly | 2 - ReadWrite |");
                    enter.ShowButton();
                    string choice = EnterData(enter.X + 1, enter.Y + 2, 1, false);
                    FindFilesBy(Filter.Access, choice);
                    break;
            }

            ReleaseAndShowButton(filesButtons);
            currentLocation.Add("Searching Results:");
            conditionInfo.Pressed = false;
            UpdateConditionWindow($"Searching lasted: {DateTime.Now.TimeOfDay - timer.TimeOfDay}, results quant: {findResults.Count}");
        }

        /// <summary>
        /// Changes name of the object
        /// </summary>
        private static void RenameObject(string path)
        {
            string newName;
            string workWith;
            string sendTo;

            Button enter = new Button(filesWindow.X + filesWindow.Width / 3, filesWindow.Y + filesWindow.Y / 2, 50, 3, ConsoleColor.Black, ConsoleColor.Black, true, " Enter new name");
            enter.Pressed = true;
            enter.ShowButton();
          
            newName = EnterData(enter.X + 1, enter.Y + 2, enter.Width - 5, true);

            UpdateConditionWindow("  Files");

            sendTo = currentLocation[currentLocation.Count - 1];

            if (currentLocation.Count != 1)
                sendTo += "\\";

            if (File.Exists(path))
            {
                FileInfo read = new FileInfo(path);
                workWith = read.Extension;

                File.Move(path, sendTo + newName + workWith);
            }
            else
                if (Directory.Exists(path))
            {
                CopyFileOrDirTo(path, sendTo + newName);
                Directory.Delete(path, true);
            }

            filesWindow.ShowWindow();
        }

        /// <summary>
        /// Deletes file or directory
        /// </summary>
        /// <param name="path">Path to file or directory</param>
        private static void DeleteFileOrDir(string path)
        {
            if (File.Exists(path))
                File.Delete(path);
            if (Directory.Exists(path))
                Directory.Delete(path,true);
        }

        /// <summary>
        /// Function which copies file or directory
        /// </summary>
        /// <param name="path">Copy what</param>
        private static void CopyOrMoveFileOrDir(string path, bool copy_move)
        {
            Regex checkForDelete = new Regex(@"\\\w+\.\w+$");

            if (bufferInside == false)
            {
                buffer = path;
                bufferInside = true;
            }
            else
            {
                string temp = currentLocation[currentLocation.Count - 1];
                string[] toCheck = buffer.Split('\\');

                if (currentLocation.Count > 1)
                    temp += '\\';
                temp += toCheck[toCheck.Length - 1];


                // Exception for times, when user copy or moves the file in the same place, where it has been
                if (buffer == temp)
                {
                    buffer = "";
                    bufferInside = false;
                    return;
                } 

                if (File.Exists(temp))
                {
                    if (ConfirmationWindow("File already exist. Change? Y/N") == ConsoleKey.Y)
                        File.Delete(temp);
                    else
                        return;
                }
               
                if (Directory.Exists(temp))
                {
                    if (ConfirmationWindow("Folder already exist. Change? Y/N") == ConsoleKey.Y)
                        Directory.Delete(temp, true);
                    else
                        return;
                }

                if (copy_move == true)
                    CopyFileOrDirTo(buffer, temp);
                else
                {
                    if (File.Exists(buffer))
                        File.Move(buffer, temp);
                    if (Directory.Exists(buffer))
                        Directory.Move(buffer, temp);
                }

                buffer = "";
                bufferInside = false;
            }
        }

        /// <summary>
        /// Recursive method, which help us to copy file or folder with everything inside to another folder
        /// </summary>
        private static bool CopyFileOrDirTo(string from, string to)
        {
            if (File.Exists(from))
            {
                File.Copy(from, to, true);
                return true;
            }
            else
            if (Directory.Exists(from))
            {
                Directory.CreateDirectory(to);
                string[] fileToCopy = Directory.GetFileSystemEntries(from);
                string[] fileNames;
                for (int i = 0; i < fileToCopy.Length; i++)
                {
                    fileNames = fileToCopy[i].Split('\\');
                    CopyFileOrDirTo(fileToCopy[i], to + "\\" + fileNames[fileNames.Length - 1]);
                }
                return true;
            }

            return false;
        }

        /// <summary>
        /// Deletes file 
        /// </summary>
        private static void MenuDeleteFile(int selector)
        {
            if (ConfirmationWindow("Delete the file Y/N") == ConsoleKey.Y)
            {
                DeleteFileOrDir(files[selector]);
                files.RemoveAt(selector);
            }
        }

        /// <summary>
        /// Launching the file manager
        /// </summary>
        public static void EnterMenu()
        {
            // Here we change console settings to adapt them to our user

            winX = Console.LargestWindowWidth - 5;
            winY = Console.LargestWindowHeight - 5;

            Console.SetWindowSize(winX, winY);
            Console.Title = "SimpleFileManager";

            Menu();
        }

        /// <summary>
        /// Menu to work with
        /// </summary>
        private static void Menu()
        {
            Button choice = new Button(winX / 3, winY / 3, 50, 2, ConsoleColor.Black, ConsoleColor.Black, true, "                 --- Menu ---", " 1. Log In | 2. Registration | 3. Exit ");
            choice.Pressed = true; 
            user = new User();
            while (true)
            {

                // If the program launches for the first time, it will initiate admin registration

                if (user.UserCheck() == false)
                {
                    Register(false);
                    if (user.UserCheck() == false)
                        return;
                }

                // First menu to interact with. 
                // - You can log in or register new user
                // - If it is first time using this programm, it will allow you to make admin account

                Console.Clear();

                choice.ShowButton();

                switch (Console.ReadKey(true).Key)
                {
                    case ConsoleKey.D1:
                        Console.Clear();
                        if (LogIn() == true)
                        {
                            history = user.History;
                            Console.SetWindowSize(winX, winY);
                            SetUpUserInterface(user.Admin);
                            ShowInterface();
                            UserControl(user.Admin);
                            user.SaveUser(user.Login);
                        }
                        break;
                    case ConsoleKey.D2:
                        Console.Clear();
                        Register(true);
                        break;
                    case ConsoleKey.D3:
                        return;
                }
            }
        }

        /// <summary>
        /// Log-In
        /// </summary>
        /// <returns></returns>
        private static bool LogIn()
        {
            Button registerWindow = new Button(winX / 3, winY / 3, 50, 3, ConsoleColor.Black, ConsoleColor.Black, false, "Log-In (Enter 0 to exit)", "Login: ", "Password: ");

            registerWindow.Pressed = true;

            string log;
            string password;

            do
            {
                registerWindow.ShowButton();
                log = EnterData(registerWindow.X + registerWindow.InnerText[1].Length + 1, registerWindow.Y + 2, 30, true, false, false);
                Console.SetCursorPosition(registerWindow.X, registerWindow.Y + 4);
                if (log.Length == 1 && log == "0")
                    return false;
                if (user.CheckUserLogin(log) == false)
                { 
                    Console.Write("User doesn't exist");
                    Console.ReadKey(true);
                }
            } while (user.CheckUserLogin(log) != true);

            user.LoadUser(log);
            registerWindow.ChangeInnerText(true, "Log-In (Enter 0 to exit)", "Login: " + log, "Password: ");

            do
            {
                registerWindow.ShowButton();
                password = EnterData(registerWindow.X + registerWindow.InnerText[2].Length + 1, registerWindow.Y + 3, 25, true, true, false);
                Console.SetCursorPosition(registerWindow.X, registerWindow.Y + 4);
                if (password.Length == 1 && password == "0")
                    return false;
                if (password != user.Password)
                {
                    Console.Write("Password doesn't match");
                    Console.ReadKey(true);
                }
            } while (!password.Equals(user.Password, StringComparison.Ordinal));

            return true;
        }

        /// <summary>
        /// Registration
        /// </summary>
        private static void Register(bool user_admin)
        {
            string userType;

            if (user_admin == true)
                userType = "User";
            else
                userType = "Admin";

            Button registerWindow = new Button(winX / 3, winY / 3, 50, 3, ConsoleColor.Black, ConsoleColor.Black, false, userType + "Registration (Enter 0 to exit)", "Login: ", "Password: ");

            registerWindow.Pressed = true;

            string log;
            string password;

            do
            {
                registerWindow.ShowButton();
                log = EnterData(registerWindow.X + registerWindow.InnerText[1].Length + 1, registerWindow.Y + 2, 30, true, false, false);
                Console.SetCursorPosition(registerWindow.X, registerWindow.Y + 4);
                if (log.Length == 1 && log == "0")
                    return;
                if (log.Length < 6)
                { 
                    Console.Write("THERE MUST BE AT LEAST 6 SYMBOLS");
                    Console.ReadKey(true);
                }
            } while (log.Length < 6);

            registerWindow.ChangeInnerText(true, userType + "Registration (Enter 0 to exit)", "Login: " + log, "Password: ");

            do
            {
                registerWindow.ShowButton();
                password = EnterData(registerWindow.X + registerWindow.InnerText[2].Length + 1, registerWindow.Y + 3, 25, true, false, false);
                Console.SetCursorPosition(registerWindow.X, registerWindow.Y + 4);
                if (password.Length == 1 && password == "0")
                    return;
                if (password.Length < 6)
                {
                    Console.Write("THERE MUST BE AT LEAST 6 SYMBOLS");
                    Console.ReadKey(true);
                }
            } while (password.Length < 6);

            user.Login = log;
            user.Password = password;
            user.Admin = user_admin;
            user.SaveUser(log);
        }

        /// <summary>
        /// Interface control
        /// </summary>
        /// <param name="user_admin">True - user | False - admin</param>
        private static void UserControl(bool user_admin)
        {
            // Getting logic disks
            logicDisks = Directory.GetLogicalDrives().ToList();

            // *Selector is a varibale, which stays for giving you the actual info, which button shows
            // (for example button with index 3 can provide you with info from List<string>[selector]

            // Selector for logic disks                                                 
            int selectorLog = 0;
            // Pages are used to check, if the page has changed to avoid 
            int logPage = -1;

            // Selector for files 
            int selectorFile = 0;
            int filePage = -1;

            // Boolean variable, which will check, where we are at the moment
            bool logW_filW = true;

            if (logW_filW == true)
                UpdateConditionWindow("  Logic Disks");
            else
                UpdateConditionWindow("  Files");

            while (true)
            {
                ActionButtonsShow(ref selectorFile, ref filePage, filesButtons, files.ToArray());
                LogicDiskButtonsShow(ref selectorLog, ref logPage);
                UpdateLocation();
                UpdateInfoWindow(selectorFile);

                try
                {
                    Console.SetCursorPosition(filesButtons[selectorFile].X, filesButtons[selectorFile].Y);
                }
                catch(Exception e ) { }

                try
                {
                    if (ConsoleKey.TryParse(Console.ReadKey(true).Key.ToString(), out ConsoleKey choice))
                    {
                        switch (choice)
                        {
                            case ConsoleKey.F1:
                            case ConsoleKey.Enter:
                                if (logW_filW == true)
                                {
                                    files = Directory.GetFileSystemEntries(logicDisks[selectorLog]).ToList();
                                    currentLocation.Add(logicDisks[selectorLog]);
                                    filePage = -1;
                                    logW_filW = false;
                                    UpdateConditionWindow("  Files");
                                }
                                else
                                {
                                    try
                                    {
                                        if (files.Count > 0)
                                        {
                                            ReleaseButtonList(filesButtons);
                                            currentLocation.Add(files[selectorFile]);
                                            files = Directory.GetFileSystemEntries(files[selectorFile]).ToList();
                                            if (currentLocation.Count > 1)
                                                history.Insert(0, currentLocation[currentLocation.Count - 1]);
                                            CleanButtonsText(filesButtons);
                                            ShowButtons(filesButtons);
                                            filePage = -1;
                                            selectorFile = 0;
                                        }

                                    }
                                    catch (Exception e)
                                    {
                                        if (File.Exists(files[selectorFile]))
                                            Process.Start(files[selectorFile]);
                                        if (currentLocation.Count > 1)
                                            history.Insert(0, currentLocation[currentLocation.Count - 1]);
                                        currentLocation.Remove(currentLocation[currentLocation.Count - 1]);
                                    }

                                    if (statistic.ContainsKey(history[0]))
                                        statistic[history[0]] += 1;
                                    else
                                        statistic.Add(history[0], 1);

                                    SaveStatistic();
                                }
                                break;

                            case ConsoleKey.F2:
                            case ConsoleKey.Escape:
                                {
                                    try
                                    {
                                        if (logW_filW == false)
                                        {
                                            if (currentLocation.Count > 1)
                                            {
                                                currentLocation.Remove(currentLocation[currentLocation.Count - 1]);
                                                files = Directory.GetFileSystemEntries(currentLocation[currentLocation.Count - 1]).ToList();
                                                ReleaseButtonList(filesButtons);
                                                filePage = -1;
                                            }
                                            else
                                            {
                                                currentLocation.Remove(currentLocation[currentLocation.Count - 1]);

                                                UpdateConditionWindow(" Logic disks");

                                                files.Clear();

                                                location.ClearInnerText();
                                                location.ShowButton();
                                                CleanButtonsText(filesButtons);
                                                ReleaseButtonList(filesButtons);
                                                ShowButtons(filesButtons);

                                                fileInfo.ClearInnerText();
                                                fileInfo.ShowButton();

                                                logW_filW = true;
                                                filePage = -1;
                                                logPage = -1;
                                            }
                                        }
                                    }
                                    catch (Exception e) { }
                                }
                                break;

                            case ConsoleKey.F3:
                                if (logW_filW == true)
                                    logW_filW = false;
                                FindFiles();
                                filePage = -1;
                                filesWindow.ShowWindow();
                                if (files.Count == 0)
                                    goto case ConsoleKey.F2;
                                break;

                            case ConsoleKey.F4:
                            case ConsoleKey.F5:
                            case ConsoleKey.D0:
                                if (user_admin == false)
                                {
                                    if (choice == ConsoleKey.D0)
                                    {
                                        bufferInside = false;

                                        ReleaseAndShowButton(actionButtons);
                                        conditionInfo.Pressed = false;
                                        UpdateConditionWindow($" Files");
                                        break;
                                    }

                                    if (bufferInside == false)
                                    {
                                        if (choice == ConsoleKey.F4)
                                            CopyOrMoveFileOrDir(files[selectorFile], true);
                                        else
                                            CopyOrMoveFileOrDir(files[selectorFile], false);
                                    }
                                    else
                                    if (bufferInside == true)
                                    {
                                        if (choice == ConsoleKey.F4)
                                            CopyOrMoveFileOrDir("", true);
                                        else
                                            CopyOrMoveFileOrDir("", false);
                                    }

                                    if (bufferInside == true)
                                    {
                                        ReleaseAndShowButton(actionButtons);
                                        filePage = -1;

                                        actionButtons[3].Pressed = true;
                                        actionButtons[4].Pressed = true;

                                        actionButtons[3].ShowButton();
                                        actionButtons[4].ShowButton();

                                        conditionInfo.Pressed = true;
                                        UpdateConditionWindow($"TO RELEASE THE FILE, PRESS 0 | FILE: {buffer}");
                                    }
                                    else
                                    {
                                        ReleaseAndShowButton(actionButtons);
                                        conditionInfo.Pressed = false;
                                        UpdateConditionWindow("  Files");
                                        files = Directory.GetFileSystemEntries(currentLocation[currentLocation.Count - 1]).ToList();
                                        filePage = -1;
                                    }
                                }
                                break;

                            case ConsoleKey.F6:
                                if (user_admin == false)
                                {
                                    if (logW_filW == false)
                                    {
                                        MenuDeleteFile(selectorFile);
                                        filesWindow.ShowWindow();
                                        ReleaseButtonList(filesButtons);
                                        filePage = -1;
                                    }
                                }
                                break;

                            case ConsoleKey.F7:
                                if (user_admin == false)
                                {
                                    if (logW_filW == false)
                                    {
                                        if (File.Exists(files[selectorFile]) || Directory.Exists(files[selectorFile]))
                                        {
                                            RenameObject(files[selectorFile]);
                                            filePage = -1;
                                            files = Directory.GetFileSystemEntries(currentLocation[currentLocation.Count - 1]).ToList();
                                        }
                                    }
                                }
                                break;

                            case ConsoleKey.F8:
                                if (statistic.Count > 0)
                                {
                                    if (logW_filW == true)
                                        logW_filW = false;
                                    currentLocation.Add("Statistic");
                                    files.Clear();

                                    var sortStat = statistic.OrderByDescending(t => t.Value).Select(t => t.Key);
                                       
                                    files.AddRange(sortStat);
                                    ReleaseAndShowButton(filesButtons);
                                    filePage = -1;
                                }
                                break;

                            case ConsoleKey.F9:
                                if (history.Count > 0)
                                {
                                    if (logW_filW == true)
                                        logW_filW = false;

                                    currentLocation.Add("History");
                                    files.Clear();

                                    files.AddRange(history);
                                    ReleaseAndShowButton(filesButtons);
                                    filePage = -1;
                                }
                                break;

                            case ConsoleKey.F10:
                                files.Clear();
                                bufferInside = false;

                                ReleaseAndShowButton(actionButtons);
                                conditionInfo.Pressed = false;
                                UpdateConditionWindow($" Files");
   
                                CleanButtonsText(filesButtons);
                                ShowButtons(filesButtons);

                                return;

                            case ConsoleKey.UpArrow:
                                if (logW_filW == true)
                                {
                                    ReleaseAndShowButton(logicDiskButtons);
                                    selectorLog--;
                                }
                                else
                                {
                                    ReleaseAndShowButton(filesButtons);
                                    selectorFile--;
                                }
                                break;

                            case ConsoleKey.DownArrow:
                                if (logW_filW == true)
                                {
                                    ReleaseAndShowButton(logicDiskButtons);
                                    selectorLog++;
                                }
                                else
                                {
                                    ReleaseAndShowButton(filesButtons);
                                    selectorFile++;
                                }
                                break;

                            case ConsoleKey.LeftArrow:
                                if (logW_filW == true)
                                {
                                    ReleaseAndShowButton(logicDiskButtons);
                                    selectorLog -= logicDiskButtons.Count - 1;
                                }
                                else
                                {
                                    ReleaseAndShowButton(filesButtons);
                                    selectorFile -= filesButtons.Count - 1;
                                }
                                break;

                            case ConsoleKey.RightArrow:
                                if (logW_filW == true)
                                {
                                    ReleaseAndShowButton(logicDiskButtons);
                                    selectorLog += logicDiskButtons.Count - 1;
                                }
                                else
                                {
                                    ReleaseAndShowButton(filesButtons);
                                    selectorFile += filesButtons.Count - 1;
                                }
                                break;
                        }
                    }
                }
                catch(Exception e)
                {
                    ErrorMessage(e.Message);
                    filePage = -1;
                }
            } 
        }

        #endregion

        #endregion

        #endregion
    }
}
