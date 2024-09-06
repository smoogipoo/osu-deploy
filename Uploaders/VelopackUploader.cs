// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Net.Http;
using osu.Framework.IO.Network;

namespace osu.Desktop.Deploy.Uploaders
{
    public class VelopackUploader : Uploader
    {
        protected readonly string ApplicationName;
        protected readonly string OperatingSystemName;
        protected readonly string RuntimeIdentifier;
        protected readonly string Channel;
        protected readonly string? ExtraArgs;
        protected readonly string StagingPath;

        public VelopackUploader(string version, string applicationName, string operatingSystemName, string runtimeIdentifier, string channel, string? extraArgs = null, string? stagingPath = null)
            : base(version)
        {
            ApplicationName = applicationName;
            OperatingSystemName = operatingSystemName;
            RuntimeIdentifier = runtimeIdentifier;
            Channel = channel;
            ExtraArgs = extraArgs;
            StagingPath = stagingPath ?? Program.StagingPath;
        }

        public override void RestoreBuild()
        {
            if (Program.CanGitHub)
            {
                Program.RunCommand("dotnet", $"vpk download github"
                                             + $" --repoUrl=\"{Program.GitHubRepoUrl}\""
                                             + $" --token=\"{Program.GitHubAccessToken}\""
                                             + $" --channel=\"{Channel}\""
                                             + $" --outputDir=\"{Program.ReleasesPath}\"",
                    throwIfNonZero: false,
                    useSolutionPath: false);
            }
        }

        public sealed override void PublishBuild(string version)
        {
            Pack(version);

            if (Program.CanGitHub && Program.GitHubUpload)
                Upload(version);
        }

        protected virtual void Pack(string version)
        {
            Program.RunCommand("dotnet", $"vpk [{OperatingSystemName}] pack"
                                         + $" --packTitle=\"osu!\""
                                         + $" --packId=\"{Program.PackageName}\""
                                         + $" --packVersion=\"{version}\""
                                         + $" --runtime=\"{RuntimeIdentifier}\""
                                         + $" --outputDir=\"{Program.ReleasesPath}\""
                                         + $" --mainExe=\"{ApplicationName}\""
                                         + $" --packDir=\"{StagingPath}\""
                                         + $" --channel=\"{Channel}\""
                                         + $" {ExtraArgs}",
                useSolutionPath: false);
        }

        protected virtual void Upload(string version)
        {
            if (Program.CanGitHub && Program.GitHubUpload)
            {
                Program.RunCommand("dotnet", $"vpk upload github"
                                             + $" --repoUrl=\"{Program.GitHubRepoUrl}\""
                                             + $" --token=\"{Program.GitHubAccessToken}\""
                                             + $" --outputDir=\"{Program.ReleasesPath}\""
                                             + $" --tag=\"{version}\""
                                             + $" --releaseName=\"{version}\""
                                             + $" --merge"
                                             + $" --channel=\"{Channel}\"",
                    useSolutionPath: false);
            }
        }

        protected void RenameAsset(string fromName, string toName)
        {
            if (!Program.CanGitHub || !Program.GitHubUpload)
                return;

            Logger.Write($"Renaming asset '{fromName}' to '{toName}'");

            GitHubRelease targetRelease = Program.GetLastGithubRelease(true)
                                          ?? throw new Exception("Release not found.");

            GitHubAsset asset = targetRelease.Assets.SingleOrDefault(a => a.Name == fromName)
                                ?? throw new Exception($"Asset '{fromName}' not found in the release.");

            var req = new WebRequest(asset.Url)
            {
                Method = HttpMethod.Patch,
            };

            req.AddRaw(
                $$"""
                  { "name": "{{toName}}" }
                  """);

            req.AuthenticatedBlockingPerform();
        }
    }
}
