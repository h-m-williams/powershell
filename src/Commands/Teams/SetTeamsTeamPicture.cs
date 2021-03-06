﻿using PnP.PowerShell.Commands.Attributes;
using PnP.PowerShell.Commands.Base;
using PnP.PowerShell.Commands.Base.PipeBinds;
using PnP.PowerShell.Commands.Utilities;
using System.IO;
using System.Management.Automation;
using System.Threading.Tasks;

namespace PnP.PowerShell.Commands.Graph
{
    [Cmdlet(VerbsCommon.Set, "PnPTeamsTeamPicture")]
    [MicrosoftGraphApiPermissionCheckAttribute(MicrosoftGraphApiPermission.Group_ReadWrite_All)]
    [PnPManagementShellScopes("Group.ReadWrite.All")]

    public class SetTeamsTeamPicture : PnPGraphCmdlet
    {
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public TeamsTeamPipeBind Team;

        [Parameter(Mandatory = true)]
        public string Path;

        protected override void ExecuteCmdlet()
        {
            var groupId = Team.GetGroupId(HttpClient, AccessToken);
            if (groupId != null)
            {
                if (!System.IO.Path.IsPathRooted(Path))
                {
                    Path = System.IO.Path.Combine(SessionState.Path.CurrentFileSystemLocation.Path, Path);

                }
                if (System.IO.File.Exists(Path))
                {
                    var contentType = "";
                    var fileInfo = new FileInfo(Path);
                    switch (fileInfo.Extension)
                    {
                        case ".jpg":
                        case ".jpeg":
                            {
                                contentType = "image/jpeg";
                                break;
                            }
                        case ".png":
                            {
                                contentType = "image/png";
                                break;
                            }
                    }
                    if (string.IsNullOrEmpty(contentType))
                    {
                        throw new PSArgumentException("File is not of a supported content type (jpg/png)");
                    }
                    var byteArray = System.IO.File.ReadAllBytes(Path);
                    TeamsUtility.SetTeamPictureAsync(HttpClient, AccessToken, groupId, byteArray, contentType).GetAwaiter().GetResult();
                }
                else
                {
                    throw new PSArgumentException("File not found");
                }
            }
            else
            {
                throw new PSArgumentException("Team not found");
            }

        }
    }
}