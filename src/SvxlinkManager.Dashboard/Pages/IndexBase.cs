using Microsoft.AspNetCore.Components;

using SvxlinkManager.Common.Models;
using SvxlinkManager.Common.Service;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SvxlinkManager.Dashboard.Pages
{
  public class IndexBase : ComponentBase
  {
    public readonly string applicationPath = Directory.GetCurrentDirectory();

    protected override async Task OnInitializedAsync()
    {
      await base.OnInitializedAsync();

      var channel = new ChannelBase { Name = "Salon Suisse Rommand" };

      SvxLinkService.StartSvxlink(channel, pidFile: "/var/run/svxlink.pid", runAs: "root", configFile: $"{applicationPath}/SvxlinkConfig/svxlink.conf");
    }

    [Inject]
    public SvxlinkServiceBase SvxLinkService { get; set; }

    public List<Common.Models.Node> Nodes
    {
      get => SvxLinkService.Nodes.OrderBy(n => n.Name).ToList();
      set => SvxLinkService.Nodes = value;
    }
  }
}