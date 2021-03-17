using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

using SvxlinkManager.Common.Models;
using SvxlinkManager.Common.Service;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SvxlinkManager.Dashboard.Pages
{
  public class IndexBase : ComponentBase, IDisposable
  {
    protected override async Task OnInitializedAsync()
    {
      await base.OnInitializedAsync();

      SvxLinkService.NodeConnected += SvxLinkService_NodeConnected;

      SvxLinkService.NodeDisconnected += SvxLinkService_NodeDisconnected;

      SvxLinkService.NodeTx += SvxLinkService_NodeTx;

      SvxLinkService.NodeRx += SvxLinkService_NodeRx;

      SvxLinkService.Error += SvxLinkService_Error;
    }

    [Inject]
    public SvxlinkServiceBase SvxLinkService { get; set; }

    [Inject]
    public ILogger<IndexBase> Logger { get; set; }

    [Inject]
    public TelemetryClient Telemetry { get; set; }

    [Inject]
    public IJSRuntime Js { get; set; }

    public Common.Models.Node CurrentTxNode { get; set; }

    public List<Common.Models.Node> Nodes
    {
      get => SvxLinkService.Nodes.OrderBy(n => n.Name).ToList();
      set => SvxLinkService.Nodes = value;
    }

    /// <summary>Show new toast.</summary>
    /// <param name="title">Toast title</param>
    /// <param name="body">Toast body</param>
    /// <param name="type">Toast type. Accept : success, info, danger</param>
    /// <param name="autohide">Autohide, always false if type is danger</param>
    /// <param name="delay">Autohide delay in second</param>
    private async Task ShowToastAsync(string title, string body, string type, bool autohide = true, int delay = 5000) =>
      await Js.InvokeVoidAsync("addToast", Guid.NewGuid().ToString(), title, body, type, DateTime.Now.ToString("HH:mm:ss"), autohide, delay);

    protected async Task ShowInfoToastAsync(string title, string body, bool autohide = true, int delay = 5000) =>
       await ShowToastAsync(title, body, "info", autohide, delay);

    protected async Task ShowErrorToastAsync(string title, string body, bool autohide = true, int delay = 10000) =>
      await ShowToastAsync(title, body, "danger", autohide, delay);

    private async void SvxLinkService_NodeConnected(Common.Models.Node n)
    {
      try
      {
        await InvokeAsync(() => StateHasChanged());
        await ShowInfoToastAsync(n.Name, "A rejoint le salon.");
      }
      catch (Exception e)
      {
        Logger.LogError($"Impossible d'indiquer qu'un noeud a rejoint le salon. {e.Message}");
        Telemetry.TrackException(e);
      }
    }

    private async void SvxLinkService_NodeDisconnected(Common.Models.Node n)
    {
      try
      {
        await InvokeAsync(() => StateHasChanged());
        await ShowInfoToastAsync(n.Name, "A quitté le salon.");
      }
      catch (Exception e)
      {
        Logger.LogError($"Impossible d'indiquer qu'un noeus a quitté le salon. {e.Message}");
        Telemetry.TrackException(e);
      }
    }

    private void SvxLinkService_NodeTx(Common.Models.Node n)
    {
      try
      {
        CurrentTxNode = n;
        InvokeAsync(() => StateHasChanged());
      }
      catch (Exception e)
      {
        Logger.LogError($"Impossible de passer le node en TX. {e.Message}");
        Telemetry.TrackException(e);
      }
    }

    private void SvxLinkService_NodeRx(Common.Models.Node n)
    {
      try
      {
        CurrentTxNode = null;
        InvokeAsync(() => StateHasChanged());
      }
      catch (Exception e)
      {
        Logger.LogError($"Impossible de repasser le node en RX. {e.Message}");
        Telemetry.TrackException(e);
      }
    }

    private async void SvxLinkService_Error(string t, string b)
    {
      try
      {
        await ShowErrorToastAsync(t, b);
      }
      catch (Exception e)
      {
        Logger.LogError($"Impossible de mettre à jour d'afficher le toast. {e.Message}");
        Telemetry.TrackException(e);
      }
    }

    protected async override Task OnAfterRenderAsync(bool firstRender)
    {
      await base.OnAfterRenderAsync(firstRender).ConfigureAwait(false);

      await Js.InvokeVoidAsync("SetPopOver");
    }

    public void Dispose()
    {
      SvxLinkService.NodeConnected -= SvxLinkService_NodeConnected;

      SvxLinkService.NodeDisconnected -= SvxLinkService_NodeDisconnected;

      SvxLinkService.NodeTx -= SvxLinkService_NodeTx;

      SvxLinkService.NodeRx -= SvxLinkService_NodeRx;

      SvxLinkService.Error -= SvxLinkService_Error;
    }
  }
}