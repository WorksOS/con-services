using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TagFiles.Common;
using VSS.Common.Abstractions.Configuration;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using VSS.Productivity3D.Productivity3D.Models.Compaction.ResultHandling;
using VSS.WebApi.Common;

namespace MegalodonSvc
{
  public class TagFileDispatchSvc : IHostedService
  {
    private readonly ILogger<TagFileDispatchSvc> _log;
    private readonly ITPaaSApplicationAuthentication _authn;
    private readonly IConfigurationStore _config;
    private readonly IProductivity3dV2ProxyCompaction _serviceProxy;
//    private Timer _timer;
    private FileSystemWatcher fileSystemWatcher;
    private readonly string _path;

    public TagFileDispatchSvc(ITPaaSApplicationAuthentication authn, ILoggerFactory logFactory, IConfigurationStore config, IProductivity3dV2ProxyCompaction serviceProxy)
    {
      _log = logFactory.CreateLogger<TagFileDispatchSvc>();
      _authn = authn;
      _config = config;
      _serviceProxy = serviceProxy;
      _path = Path.Combine(_config.GetValueString("InstallFolder"), TagConstants.TAGFILE_FOLDER);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
      MonitorDirectory(_path);
      return Task.CompletedTask;
    }


    public Task StopAsync(CancellationToken cancellationToken)
    {
      if (fileSystemWatcher != null)
      {
        fileSystemWatcher.EnableRaisingEvents = false;
      }
      return Task.CompletedTask;
    }

    private Task ScanAndUpload()
    {
      return Task.WhenAll(GetFilenames().Select(f => UploadFile(f).ContinueWith(t => {
        if (!t.IsFaulted)
        {
          if (t.Result.Code == 0)
          {
            _log.LogInformation($"Deleting file {f}");
            File.Delete(f);
          }
        }
        else
        {
          _log.LogInformation($"Can't submit file {f}");
        }
      })));
    }



    private Task<TagFileDirectSubmissionResult> UploadFile(string filename)
    {
      _log.LogInformation($"Uploading file {filename}");
      try
      {
        return _serviceProxy.SendTagfileDirect(filename, _authn.CustomHeaders());
      }
      catch (Exception e)
      {
        _log.LogWarning(e, $"Can't submit file {filename}");
      }

      return null;
    }

    private IEnumerable<string> GetFilenames() => Directory.GetFiles(_path, "*.tag").Where(f => File.GetCreationTimeUtc(f) > DateTime.UtcNow.AddMinutes(TagConstants.NEGATIVE_MINUTES_AGED_TAGFILES));

    private void MonitorDirectory(string path)

    {
      fileSystemWatcher = new FileSystemWatcher();
      fileSystemWatcher.Path = path;
      fileSystemWatcher.Created += async (s, e) => await FileSystemWatcher_Created(s,e);
      fileSystemWatcher.EnableRaisingEvents = true;

    }

    private Task FileSystemWatcher_Created(object sender, FileSystemEventArgs e)
    {
      return ScanAndUpload();
    }
  }
}
