using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.HOS.Services.Am.AppletAE.AllSystemAppletProxiesService.SystemAppletProxy;
using System.Collections.Concurrent;

namespace Ryujinx.HLE.HOS.SystemState
{
    class AppletMessageQueue
    {
        public ConcurrentQueue<AppletMessage> Messages { get; }
        public KEvent MessageEvent { get; }

        public AppletMessageQueue(Horizon system)
        {
            Messages = new ConcurrentQueue<AppletMessage>();
            MessageEvent = new KEvent(system.KernelContext);
        }
    }

    class AppletStateMgr
    {
        private readonly ConcurrentDictionary<long, AppletMessageQueue> _messageQueues;

        public FocusState FocusState { get; private set; }

        public IdDictionary AppletResourceUserIds { get; }

        public ConcurrentQueue<byte[]> AppletData { get; } = new();

        public AppletStateMgr()
        {
            _messageQueues = new ConcurrentDictionary<long, AppletMessageQueue>();

            AppletResourceUserIds = new IdDictionary();
        }

        public void SetFocus(bool isFocused)
        {
            FocusState = isFocused ? FocusState.InFocus : FocusState.OutOfFocus;


            SendMessageToAll(AppletMessage.FocusStateChanged);

            if (isFocused)
            {
                SendMessageToAll(AppletMessage.ChangeIntoForeground);
            }

            SignalAll();
        }

        public void SendMessageToAll(AppletMessage message)
        {
            foreach (var mq in _messageQueues.Values)
            {
                mq.Messages.Enqueue(message);
            }
        }

        public void SignalAll()
        {
            foreach (var mq in _messageQueues.Values)
            {
                mq.MessageEvent.ReadableEvent.Signal();
            }
        }

        public void CreateQueue(Horizon system, long pid)
        {
            _messageQueues.TryAdd(pid, new AppletMessageQueue(system));
        }

        public AppletMessageQueue GetQueue(long pid)
        {
            return _messageQueues[pid];
        }
    }
}