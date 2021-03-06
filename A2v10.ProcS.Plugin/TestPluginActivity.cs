﻿// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Threading.Tasks;
using A2v10.ProcS.Infrastructure;

[assembly: ProcSPlugin("com.a2v10.procs.test")]

namespace A2v10.ProcS.Plugin
{
	[ResourceKey("com.a2v10.procs.test:" + nameof(TestPluginActivity))]
	public class TestPluginActivity : IActivity
	{
		public Int32 CorrelationId { get; set; }

		public ActivityExecutionResult Execute(IExecuteContext context)
		{
			if (context.IsContinue)
				return ActivityExecutionResult.Complete;

			var corrId = new CorrelationId<Int32>(42);
			context.SaveInstance();
			context.SendMessage(new TaskPluginActionMessage(context.Instance.Id, corrId));

			return ActivityExecutionResult.Idle;
		}
	}
}
