﻿// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Threading.Tasks;
using A2v10.ProcS.Interfaces;

namespace A2v10.ProcS
{
	public class ResumeProcess : MessageBase<String>
	{
		public Guid Id { get; }
		public String Result { get; }

		public ResumeProcess(Guid id, String result) : base(null)
		{
			Id = id;
			Result = result;
		}
	}

	public class ProcessSaga : SagaBaseDispatched<String, ResumeProcess>
	{
		public static void Register()
		{
			InMemorySagaKeeper.RegisterMessageType<ResumeProcess, ProcessSaga>();
		}

		protected override async Task Handle(IHandleContext context, ResumeProcess message)
		{
			var instance = await context.LoadInstance(message.Id);
			var resumeContext = context.CreateResumeContext(instance);
			resumeContext.Result = message.Result;
			await instance.Workflow.Resume(resumeContext);
		}
	}
}
