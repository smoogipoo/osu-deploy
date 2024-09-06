// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Desktop.Deploy.Uploaders
{
    public class LinuxVelopackUploader : VelopackUploader
    {
        private readonly string channel;

        public LinuxVelopackUploader(string applicationName, string operatingSystemName, string runtimeIdentifier, string channel, string? extraArgs = null, string? stagingPath = null)
            : base(applicationName, operatingSystemName, runtimeIdentifier, channel, extraArgs, stagingPath)
        {
            this.channel = channel;
        }

        protected override void Upload(string version)
        {
            base.Upload(version);
            RenameAsset($"{Program.PackageName}-{channel}.AppImage", "osu.AppImage");
        }
    }
}
