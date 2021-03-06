﻿// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using A2v10.ProcS.Infrastructure;
using Microsoft.Extensions.Logging;

namespace A2v10.ProcS
{
	public class ServiceBusItem : IServiceBusItem
	{
		public ServiceBusItem(IMessage message)
		{
			Message = message;
			Next = Array.Empty<ServiceBusItem>();
			After = null;
		}

		public ServiceBusItem(IMessage message, DateTime after)
			: this(message)
		{
			After = after;
		}

		public ServiceBusItem(IMessage message, IEnumerable<ServiceBusItem> next)
			: this(message)
		{
			if (next != null)
				Next = next.ToArray();
		}

		public IMessage Message { get; private set; }
		public IServiceBusItem[] Next { get; private set; }
		public DateTime? After { get; private set; }
	}

	public abstract class ServiceBusBase : IServiceBus
	{
		private readonly ISagaKeeper _sagaKeeper;
		private readonly ITaskManager _taskManager;
		private readonly ILogger _logger;
		private readonly INotifyManager _notifyManager;

		protected ServiceBusBase(ITaskManager taskManager, ISagaKeeper sagaKeeper, ILogger logger, INotifyManager notifyManager)
		{
			_taskManager = taskManager;
			_notifyManager = notifyManager;
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_sagaKeeper = sagaKeeper;
		}

		protected abstract void SignalUpdate();

		protected abstract void SignalFail(Exception e);

		private IPromise SendInternal(IServiceBusItem item)
		{
			async Task task()
			{
				await _sagaKeeper.SendMessage(item);
			}
			return _taskManager.AddTask(task);
		}

		protected void Send(IServiceBusItem item)
		{
			SendInternal(item).Done(SignalUpdate).Catch(SignalFail);
		}

		protected void Send(IEnumerable<IServiceBusItem> items)
		{
			var pa = new PromiseAggregator(items.Select(itm => SendInternal(itm)));
			_taskManager.AddTask(pa.Aggregate).Done(SignalUpdate);
		}

		public void Send(IEnumerable<IMessage> messages)
		{
			Send(messages.Select(m => new ServiceBusItem(m)));
		}

		public void Send(IMessage message)
		{
			Send(new ServiceBusItem(message));
		}

		public void SendAfter(DateTime after, IMessage message)
		{
			Send(new ServiceBusItem(message, after));
		}

		private static ServiceBusItem GetSequenceItem(IEnumerator<IMessage> en)
		{
			if (en.MoveNext())
			{
				var curr = en.Current;
				var after = GetSequenceItem(en);
				if (after == null) return new ServiceBusItem(curr);
				return new ServiceBusItem(curr, new ServiceBusItem[] { after });
			}
			return null;
		}

		public void SendSequence(IEnumerable<IMessage> messages)
		{
			var en = messages.GetEnumerator();
			Send(GetSequenceItem(en));
		}

		public Task Process()
		{
			return Process(CancellationToken.None);
		}

		public async Task Process(CancellationToken token)
		{
			while (!token.IsCancellationRequested)
			{
				try
				{
					var sk = await _sagaKeeper.PickSaga();
					if (!sk.Available)
						break;
					ProcessItem(sk);
				}
				catch (Exception e)
				{
					_logger.LogCritical(e, "Erorr while picking saga");
				}
			}
		}

		private void ProcessItem(PickedSaga item)
		{
			async Task task()
			{
				try
				{
					var saga = item.Saga;
					var hc = new HandleContext(this, _logger, _notifyManager);
					await saga.Handle(hc, item.ServiceBusItem.Message);
					Send(item.ServiceBusItem.Next);
					await _sagaKeeper.ReleaseSaga(item);
				}
				catch (Exception e)
				{
					_logger.LogError(e, nameof(ProcessItem));
					await _sagaKeeper.FailSaga(item, e);
				}
			}
			_taskManager.AddTask(task).Catch(SignalFail);
		}
	}

	public class ServiceBus : ServiceBusBase
	{
		public ServiceBus(ITaskManager taskManager, ISagaKeeper sagaKeeper, ILogger logger, INotifyManager notifyManager)
			: base(taskManager, sagaKeeper, logger, notifyManager)
		{
			
		}

		protected override void SignalUpdate()
		{
			mre.Set();
		}

		protected override void SignalFail(Exception e)
		{
			exception = e;
			failed = true;
			running = false;
			SignalUpdate();
		}

		private readonly ManualResetEvent mre = new ManualResetEvent(false);

		private Exception exception = null;
		private volatile Boolean running = false;
		private volatile Boolean failed = false;

		public void Stop()
		{
			running = false;
			SignalUpdate();
		}

		public void Start()
		{
			lock (this)
			{
				running = true;
				while (running)
				{
					Process(CancellationToken.None).Wait();
					mre.Reset();
					mre.WaitOne(50);
				}
				if (failed)
				{
					throw exception ?? new Exception("Error while ServiceBus processing");
				}
			}
		}
	}

	public class ServiceBusAsync : ServiceBusBase
	{
		public ServiceBusAsync(ITaskManager taskManager, ISagaKeeper sagaKeeper, ILogger logger, INotifyManager notifyManager)
			: base(taskManager, sagaKeeper, logger, notifyManager)
		{

		}

		protected override void SignalUpdate()
		{
			rwl.AcquireReaderLock(10);
			try
			{
				ts?.TrySetResult(true);
			}
			finally
			{
				rwl.ReleaseReaderLock();
			}
		}

		protected override void SignalFail(Exception e)
		{
			rwl.AcquireReaderLock(10);
			try
			{
				ts.SetException(e);
			}
			finally
			{
				rwl.ReleaseReaderLock();
			}
		}

		private readonly ReaderWriterLock rwl = new ReaderWriterLock();
		private volatile TaskCompletionSource<Boolean> ts = null;

		public async Task Run(CancellationToken token)
		{
			while (!token.IsCancellationRequested)
			{
				await Process(token);
				rwl.AcquireWriterLock(0);
				try
				{
					ts = new TaskCompletionSource<Boolean>(TaskCreationOptions.RunContinuationsAsynchronously);
				}
				finally
				{
					rwl.ReleaseWriterLock();
				}
				await Task.WhenAny(ts.Task, Task.Delay(500, token));
			}
		}
	}
}
