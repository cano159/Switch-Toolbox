﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Octokit;
using System.Net;
using System.IO.Compression;
using System.IO;

namespace Updater
{
    class Program
    {
        static Release[] releases;

        static string execDirectory = "";
        static string folderDir = "";
        static bool foundRelease = false;

        static void Main(string[] args)
        {
            execDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            folderDir = execDirectory;

            var client = new GitHubClient(new ProductHeaderValue("ST_UpdateTool"));
            GetReleases(client).Wait();

            string[] versionInfo = File.ReadLines(Path.Combine(execDirectory, "Version.txt")).ToArray();
            string ProgramVersion = versionInfo[0];
            string CompileDate = versionInfo[1];
            string CommitInfo = versionInfo[2];

            foreach (string arg in args)
            {
                switch (arg)
                {
                    case "-d":
                    case "--download":
                        Download(CompileDate);
                        break;
                    case "-i":
                    case "--install":
                        Install();
                        break;
                    case "-b":
                    case "--boot":
                        Boot();
                        Environment.Exit(0);
                        break;

                }
            }
            Console.Read();
        }
        static void Boot()
        {
            Console.WriteLine("Booting...");

            Thread.Sleep(3000);
            System.Diagnostics.Process.Start(Path.Combine(folderDir, "Toolbox.exe"));
        }
        static void Install()
        {
            Console.WriteLine("Installing...");
            foreach (string dir in Directory.GetDirectories("master/"))
            {
                string dirName = new DirectoryInfo(dir).Name;

                if (Directory.Exists(Path.Combine(folderDir, dirName + @"\")))
                    Directory.Delete(Path.Combine(folderDir, dirName + @"\"), true);
                Directory.Move(dir, Path.Combine(folderDir, dirName + @"\"));
            }
            foreach (string file in Directory.GetFiles("master/"))
            {
                if (file.Contains("Updater.exe") || file.Contains("Updater.exe.config")
                    || file.Contains("Updater.pdb") || file.Contains("Octokit.dll"))
                    continue;
           
                if (File.Exists(Path.Combine(folderDir, Path.GetFileName(file))))
                    File.Delete(Path.Combine(folderDir, Path.GetFileName(file)));
                File.Move(file, Path.Combine(folderDir, Path.GetFileName(file)));
            }
        }
        static void Download(string CompileDate)
        {
            foreach (Release latest in releases)
            {
                Console.WriteLine("Checking Update");
                if (!foundRelease)
                {
                    if (!latest.Assets[0].UpdatedAt.ToString().Equals(CompileDate))
                    {
                        Console.WriteLine("Downloading release...");
                        bool IsDownloaded = DownloadedProgram(latest);

                        if (IsDownloaded)
                            Console.WriteLine("Downloaded update successfully!");
                        else
                            Console.WriteLine("Failed to download update!");
                    }
                }
                foundRelease = true;
            }
        }
        static bool DownloadedProgram(Release release)
        {
            return DownloadRelease("master",
                release.Assets[0].BrowserDownloadUrl,
                release.TagName,
                release.Assets[0].UpdatedAt.ToString(),
                release.TargetCommitish);
        }
        static bool DownloadRelease(string downloadName, string url, string ProgramVersion, string CompileDate, string CommitInfo)
        {
            try
            {
                using (var webClient = new WebClient())
                {
                    webClient.DownloadFile(url, downloadName + ".zip");
                }
                if (Directory.Exists(downloadName + "/"))
                    Directory.Delete(downloadName + "/", true);
                ZipFile.ExtractToDirectory(downloadName + ".zip", downloadName + "/");
                string versionTxt = Path.Combine(Path.GetFullPath(downloadName + "/"), "Version.txt");

                using (StreamWriter writer = new StreamWriter(versionTxt))
                {
                    writer.WriteLine($"{ProgramVersion}");
                    writer.WriteLine($"{CompileDate}");
                    writer.WriteLine($"{CommitInfo}");
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        static async Task GetReleases(GitHubClient client)
        {
            List<Release> Releases = new List<Release>();
            foreach (Release r in await client.Repository.Release.GetAll("KillzXGaming", "Switch-Toolbox"))
                Releases.Add(r);
            releases = Releases.ToArray();
        }
    }
}
