﻿// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace A2v10.ProcS.Infrastructure
{
	public interface IHandleContext 
	{
		Task<IInstance> LoadInstance(Guid id);

		void SendMessage(IMessage message);
		void SendMessageAfter(DateTime after, IMessage message);
		void SendMessagesSequence(params IMessage[] messages);

		IScriptContext ScriptContext { get; }
		ILogger Logger { get; }

		IExecuteContext CreateExecuteContext(IInstance instance, String bookmark = null, IDynamicObject result = null);
		Task<IInstance> StartProcess(String processId, Guid parentId, IDynamicObject data = null);

		void ContinueProcess(Guid id, String bookmark, IDynamicObject result);
		void ContinueProcess(Guid id, String bookmark, String json);

		void ResumeBookmark(Guid id, IDynamicObject result);
	}

	public interface IExecuteContext : IHandleContext
	{
		IInstance Instance { get; }
		Boolean IsContinue { get; set; }
		String Bookmark { get; set; }
		IDynamicObject Result { get; set; }

		Task SaveInstance();

		String Resolve(String source);
		T EvaluateScript<T>(String expression);
		void ExecuteScript(String code);

		void ProcessComplete(String bookmark);

		Guid SetBookmark();
	}
}
