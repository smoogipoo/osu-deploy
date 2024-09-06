// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Desktop.Deploy.Uploaders
{
    public class WindowsVelopackUploader : VelopackUploader
    {
        private readonly string channel;

        public WindowsVelopackUploader(string version, string applicationName, string operatingSystemName, string runtimeIdentifier, string channel, string? extraArgs = null,
                                       string? stagingPath = null)
            : base(version, applicationName, operatingSystemName, runtimeIdentifier, channel, extraArgs, stagingPath)
        {
            this.channel = channel;
        }

        protected override void Pack(string version)
        {
            Program.RunCommand("dotnet", $"sign {StagingPath}");
            base.Pack(version);
            Program.RunCommand("dotnet", $"sign {Program.ReleasesPath}");
        }

        protected override void Upload(string version)
        {
            base.Upload(version);
            RenameAsset($"{Program.PackageName}-{channel}-Setup.exe", "install.exe");
        }
    }
}
