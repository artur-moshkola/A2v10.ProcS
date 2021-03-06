﻿// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using A2v10.ProcS.Infrastructure;
using Microsoft.Extensions.Logging;

namespace A2v10.ProcS
{
	public class StateMachine : IWorkflowDefinition
	{
		public String Description { get; set; }
		public String InitialState { get; set; }

		public Dictionary<String, State> States { get; set; }


		public Dictionary<String, IActivity> EventHandlers { get; set; }

		private IIdentity _identity;

		public IIdentity GetIdentity() { return _identity; }
		public void SetIdentity(IIdentity identity) { _identity = identity; }

		public Task Run(IExecuteContext context)
		{
			context.Logger.LogInformation($"Workflow.Start:  ProcessId:'{_identity.ProcessId}', Version:'{_identity.Version}', InstanceId='{context.Instance.Id}'");
			return Execute(context);
		}

		public Task<ExecuteResult> Execute(IExecuteContext context)
		{
			var instance = context.Instance;
			if (States == null || States.Count == 0)
			{
				instance.IsComplete = true;
				return Task.FromResult(ExecuteResult.Complete);
			}
			if (String.IsNullOrEmpty(instance.CurrentState))
			{
				if (String.IsNullOrEmpty(InitialState))
					instance.CurrentState = States.First(x => true).Key;
				else
					instance.CurrentState = InitialState;
			}
			return DoContinue(instance, context);
		}

		void FireEvent(IExecuteContext context, StateMachineEvent evt)
		{
			if (EventHandlers == null)
				return;
			if (EventHandlers.TryGetValue(evt.ToString(), out IActivity activity))
			{
				activity.Execute(context);
			}
		}

		private async Task<ExecuteResult> DoContinue(IInstance instance, IExecuteContext context)
		{ 
			while (true)
			{
				if (instance.CurrentState == null)
				{
					context.Instance.IsComplete = true;
					await context.SaveInstance();
					FireEvent(context, StateMachineEvent.InstanceSaved);
					context.ProcessComplete(context.Bookmark);
					return ExecuteResult.Complete;
				}
				if (States.TryGetValue(instance.CurrentState, out State state))
				{
					var result = state.Execute(context);
					if (result == ActivityExecutionResult.Idle)
					{
						await context.SaveInstance();
						FireEvent(context, StateMachineEvent.InstanceSaved);
						return ExecuteResult.Idle;
					}
				}
			}
		}

		public Task Continue(IExecuteContext context)
		{
			context.Logger.LogInformation($"Workflow.Continue: InstanceId='{context.Instance.Id}'");
			return DoContinue(context.Instance, context);
		}

		#region IStorable
		
		public IDynamicObject Store(IResourceWrapper wrapper)
		{
			var stmStore = new DynamicObject();
			foreach (var stx in States)
			{
				var stateStore = stx.Value.Store(wrapper);
				if (!stateStore.IsEmpty)
					stmStore.Set(stx.Key, stateStore);
			}
			return stmStore;
		}

		public void Restore(IDynamicObject store, IResourceWrapper wrapper)
		{
			foreach (var stx in States)
			{
				var obj = store.GetDynamicObject(stx.Key);
				if (obj != null)
					stx.Value.Restore(obj, wrapper);
			}
		}
		#endregion
	}
}
