using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using FluentFTP;
using System.Diagnostics;
using Microsoft.Win32;



namespace CopyBuild
{
    class Program
    {
        public static FtpClient client;

        public static string localPathForCopy = "";
        public static string serverPath = "10.43.88.206";
        public static bool download7zip = false;

        static void Main(string[] args)
        {
            localPathForCopy = Directory.GetCurrentDirectory();

            client = new FtpClient("10.43.88.206");
            client.Credentials = new NetworkCredential("qtp", "qtpdev");
            client.Connect();

            Console.WriteLine("Input target build number:");
            string targetBuild = Console.ReadLine();
            Console.WriteLine("**********************************Loading**********************************");

            string targetFolderToCopy = CheckBuildExist(targetBuild);

            


            if (!string.IsNullOrEmpty(targetFolderToCopy))
            {
                Stopwatch timer = new Stopwatch();
                timer.Start();

                DownloadDirectory(targetFolderToCopy);

                Console.WriteLine("\n*****************************Download Completed!*****************************");
                timer.Stop();
                Console.WriteLine("Time consumed:" + timer.Elapsed.ToString().Split('.')[0]);

                string zipPath = CheckFor7zip();
                //Console.WriteLine("7z: " + zipPath);
                if (!String.IsNullOrEmpty(zipPath) && download7zip)
                {
                    Console.WriteLine("\n7-Zip installation detected, proceed to extract file...");
                    UnzipDVD(zipPath, targetFolderToCopy);

                    /* Input to select unzip or not
                    Console.WriteLine("7-Zip installation detected, press Enter to unzip, Esc to exit");

                    ConsoleKey key = Console.ReadKey().Key;
                    while (key != ConsoleKey.Enter && key != ConsoleKey.Escape)
                    {
                        key = Console.ReadKey().Key;
                    }

                    if (key == ConsoleKey.Enter)
                    {
                        UnzipDVD(zipPath, targetFolderToCopy);                     
                    }
                    else if (key == ConsoleKey.Escape)
                    { }
                    */
                }
            }
            else
                Console.WriteLine("No Build Found!");

            Console.WriteLine("\nPress Any Key to exit");

            Console.ReadKey();
        }

        private static string GetNameFromFtpDetail(string input)
        {
            string name;
            int start = input.IndexOf('>');
            int end = input.LastIndexOf('<');
            name = input.Substring(start + 1, end - start - 1);
            return name;
        }

        private static string CheckBuildExist(string targetBuild)
        {
            List<string> matches = new List<string>();

            foreach (string file in client.GetNameListing("Builds"))
            {
                if (file.Contains(targetBuild))
                    matches.Add(file);
            }

            if (matches.Count == 1)
                return matches[0];
            else if (matches.Count == 0)
            {
                Console.WriteLine("Target Build not exist on Server");
                return string.Empty;
            }
            else
            {
                Console.WriteLine();
                for (int index = 1; index<= matches.Count; index++)
                {
                    Console.WriteLine(index + "." + matches[index - 1]);
                }
                Console.WriteLine("Multiple Match found, please input number to select:");
                string selection = Console.ReadLine();
                while (Convert.ToInt32(selection) > matches.Count)
                {
                    Console.WriteLine("Invalid input, please retry.");
                    selection = Console.ReadLine();
                }
                return matches[Convert.ToInt32(selection) - 1];
            }



            #region oldCode
            //return @"ftp://10.43.88.206/Builds";

            //string url = @"ftp://10.43.88.206/Builds";
            //FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(url);
            //ftpRequest.Credentials = new NetworkCredential("qtp", "qtpdev");
            //ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;

            //FtpWebResponse ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();

            //StreamReader streamReader = new StreamReader(ftpResponse.GetResponseStream());

            //List<string> directories = new List<string>();

            //string line = streamReader.ReadLine();
            //while (!string.IsNullOrEmpty(line))
            //{
            //    directories.Add(line);
            //    line = streamReader.ReadLine();
            //}

            //streamReader.Close();

            //List<string> matches = new List<string>();
            //foreach (string str in directories)
            //{
            //    if (str.Contains(targetBuild))
            //        matches.Add(str);
            //}

            //string folderName = "";
            //if (matches.Count == 1)
            //    folderName = GetNameFromFtpDetail(matches[0]);
            //else
            //    return null;

            //localPathForCopy += folderName;
            #endregion oldCode
        }

        private static async Task<bool> DownloadFtpFile(string url)
        {
            Console.WriteLine("Downloading file: " + url);
            bool x = client.DownloadFile(url.Replace("/Builds", localPathForCopy), url);
            return x;

            #region oldCode
            //FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(url);
            //ftpRequest.Credentials = new NetworkCredential("qtp", "qtpdev");
            //ftpRequest.Method = WebRequestMethods.Ftp.DownloadFile;

            //FtpWebResponse ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
            //Stream stream = ftpResponse.GetResponseStream();

            ////StreamReader reader = new StreamReader(stream);
            ////Console.WriteLine(reader.ReadToEnd());

            //Console.Write("Copying: " + url);
            //string filePath = url.Replace("ftp://10.43.88.206/Builds/", @"E:\");
            //FileStream fs = new FileStream(filePath, FileMode.Create);
            //int Length = 1024000;
            //Byte[] buffer = new Byte[Length];
            //int bytesRead = 0;
            //while (true)
            //{
            //    Console.Write('.');
            //    bytesRead = stream.Read(buffer, 0, Length);

            //    if (bytesRead == 0)
            //        break;

            //    fs.Write(buffer, 0, bytesRead);
            //}
            //fs.Close();
            //ftpResponse.Close();
            //Console.WriteLine("Done!");
            #endregion
        }

        private static async void DownloadDirectory(string url)
        {
            foreach (FtpListItem item in client.GetListing(url))
            {
                if (item.Type == FtpFileSystemObjectType.Directory)
                {
                    if (item.Name == "DVD")
                    {
                        Console.WriteLine("DVD Folder detected, select to download: 1.7-Zip (5-10 min)   2.DVD (30-40 min)");
                        if (Console.ReadLine() == "1")
                        {
                            download7zip = true;
                            continue;
                        }
                    }

                    string localFolder = url.Replace("/Builds", localPathForCopy) + @"\" + item.Name;
                    if (!Directory.Exists(localFolder))
                    {
                        Directory.CreateDirectory(localFolder);
                        //Console.WriteLine("Creating directory: " + localFolder);
                    }

                    string subFolder = url + @"\" + item.Name;
                    DownloadDirectory(subFolder);
                }
                else if (item.Type == FtpFileSystemObjectType.File)
                {
                    if (download7zip && item.Name.Contains("7z") || !download7zip)
                    {
                        string file = url + @"\" + item.Name;
                        Console.Write("Downloading file: " + file.Remove(0, 7));
                        bool inProgress = false;

                        inProgress = client.DownloadFile(file.Replace("/Builds", localPathForCopy), file);

                        if (inProgress)
                            Console.WriteLine("...Done!");
                    }
                    else
                        continue;
                }
            }

            #region oldCode
            //FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(url);
            //ftpRequest.Credentials = new NetworkCredential("qtp", "qtpdev");
            //ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;

            //FtpWebResponse ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();

            //StreamReader streamReader = new StreamReader(ftpResponse.GetResponseStream());

            //List<string> dir = new List<string>();

            //string line = streamReader.ReadLine();
            //while (!string.IsNullOrEmpty(line))
            //{
            //    dir.Add(line);
            //    line = streamReader.ReadLine();
            //}

            //streamReader.Close();

            //bool isHeadEnd = true;
            //foreach (string str in dir)
            //{
            //    if (str.Contains("PRE"))
            //    {
            //        isHeadEnd = !isHeadEnd;
            //    }

            //    if (isHeadEnd)
            //        continue;

            //    if (str.Contains("Directory"))
            //    {
            //        string subFolderName = GetNameFromFtpDetail(str);
            //        if (subFolderName == "DVD")
            //        {
            //            Console.WriteLine("DVD Folder detected, select to download: 1.DVD   2.7Zip");
            //            if (Console.ReadLine() == "2")
            //                continue;
            //        }
            //        string localFolder = (url + subFolderName).Replace("ftp://10.43.88.206/Builds/", @"E:\");
            //        if (!Directory.Exists(localFolder))
            //            Directory.CreateDirectory(localFolder);

            //         DownloadDirectory(url + subFolderName + "/");
            //    }
            //    else if (str.Contains("HREF"))
            //    {
            //        string fileName = GetNameFromFtpDetail(str);
            //         DownloadFileAsync(url + fileName);
            //    }
            //    else
            //        continue;
            //}

            #endregion
        }

        private static void UnzipDVD(string zipPath, string targetFolderToCopy)
        {
            zipPath = zipPath + "7z.exe";

            ProcessStartInfo psi = new ProcessStartInfo();
            psi.WindowStyle = ProcessWindowStyle.Normal;
            psi.FileName = zipPath;
            string zipFolderTarget = targetFolderToCopy.Replace("/Builds", "E:\\");
            string zipFilePath = zipFolderTarget + "\\DVD.7z.001";
            psi.Arguments = "x " + zipFilePath + " -o" + zipFolderTarget;

            Process pro = Process.Start(psi);
            pro.WaitForExit();
            Console.WriteLine("Unzip done!");
        }

        private static string CheckFor7zip()
        {
            string displayName;

            string registryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            RegistryKey key = Registry.LocalMachine.OpenSubKey(registryKey);
            if (key != null)
            {
                foreach (RegistryKey subkey in key.GetSubKeyNames().Select(keyName => key.OpenSubKey(keyName)))
                {
                    displayName = subkey.GetValue("DisplayName") as string;
                    if (displayName != null && displayName.Contains("7-Zip"))
                    {
                        return subkey.GetValue("UninstallString").ToString().Substring(1).Replace("Uninstall.exe","&").Split('&')[0];
                    }
                }
                key.Close();
            }

            registryKey = @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
            key = Registry.LocalMachine.OpenSubKey(registryKey);
            if (key != null)
            {
                foreach (RegistryKey subkey in key.GetSubKeyNames().Select(keyName => key.OpenSubKey(keyName)))
                {
                    displayName = subkey.GetValue("DisplayName") as string;
                    if (displayName != null && displayName.Contains("7-Zip"))
                    {
                        return subkey.GetValue("UninstallString").ToString().Substring(1).Replace("Uninstall.exe", "").Split('&')[0];                       
                    }
                }
                key.Close();
            }
            return String.Empty;
        }
    }
}
