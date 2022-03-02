using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Serialization.Formatters;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace Microsoft.Shell
{
  public interface ISingleInstanceApp
  {
    void ProcessCommandLineArgs(string[] args);
  }

  public static class SingleInstance<TApplication> where TApplication : Application, ISingleInstanceApp
  {
    private static Mutex singleInstanceMutex;
    private static IpcServerChannel channel;
    private const string ChannelNameSuffix = "SingleInstanceIPCChannel",
                         RemoteServiceName = "SingleInstanceApplicationService";

    public static bool Initialize(string uniqueName, string[] args)
    {
      string applicationIdentifier = $"{uniqueName}_{Environment.UserName.GetHashCode():X8}",
             channelName = $"{applicationIdentifier}:{ChannelNameSuffix}";

      singleInstanceMutex = new Mutex(true, applicationIdentifier, out bool firstInstance);

      if (firstInstance)
      {
        CreateRemoteService(channelName);
        ActivateFirstInstance(args);
      }
      else
      {
        SignalFirstInstance(channelName, args);
      }

      return firstInstance;
    }

    public static void Cleanup()
    {
      if (singleInstanceMutex != null)
      {
        singleInstanceMutex.Close();
        singleInstanceMutex = null;
      }

      if (channel != null)
      {
        ChannelServices.UnregisterChannel(channel);
        channel = null;
      }
    }

    private static void CreateRemoteService(string channelName)
    {
      BinaryServerFormatterSinkProvider serverProvider = new BinaryServerFormatterSinkProvider
      {
        TypeFilterLevel = TypeFilterLevel.Full
      };
      IDictionary props = new Dictionary<string, string>
      {
        ["name"] = channelName,
        ["portName"] = channelName,
        ["exclusiveAddressUse"] = "false"
      };

      channel = new IpcServerChannel(props, serverProvider);

      ChannelServices.RegisterChannel(channel, true);

      IPCRemoteService remoteService = new IPCRemoteService();
      RemotingServices.Marshal(remoteService, RemoteServiceName);
    }

    private static void SignalFirstInstance(string channelName, string[] args)
    {
      IpcClientChannel secondInstanceChannel = new IpcClientChannel();
      ChannelServices.RegisterChannel(secondInstanceChannel, true);

      string remotingServiceUrl = $"ipc://{channelName}/{RemoteServiceName}";

      IPCRemoteService firstInstanceRemoteServiceReference = (IPCRemoteService)RemotingServices.Connect(typeof(IPCRemoteService), remotingServiceUrl);

      if (firstInstanceRemoteServiceReference != null)
        firstInstanceRemoteServiceReference.InvokeFirstInstance(args);
    }

    private static object ActivateFirstInstanceCallback(object arg)
    {
      ActivateFirstInstance(arg as string[]);
      return null;
    }

    private static void ActivateFirstInstance(string[] args)
    {
      if (Application.Current != null)
        ((TApplication)Application.Current).ProcessCommandLineArgs(args);
    }

    private class IPCRemoteService : MarshalByRefObject
    {
      public void InvokeFirstInstance(string[] args)
      {
        if (Application.Current != null)
        {
          Application.Current.Dispatcher.BeginInvoke(
            DispatcherPriority.Normal, new DispatcherOperationCallback(ActivateFirstInstanceCallback), args);
        }
      }

      public override object InitializeLifetimeService() => null;
    }
  }
}