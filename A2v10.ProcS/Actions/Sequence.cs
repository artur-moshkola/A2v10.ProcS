﻿
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using A2v10.ProcS.Interfaces;

namespace A2v10.ProcS.Actions
{
	public class Sequence : IWorkflowAction
	{
		public List<IWorkflowAction> Actions { get; set; }

		public async Task<ActionResult> Execute(IExecuteContext context)
		{
			if (Actions == null || Actions.Count == 0)
				throw new ArgumentOutOfRangeException("There are no actions in sequence");
			foreach (var a in Actions)
			{
				var result = await a.Execute(context);
			}
			return ActionResult.Idle;
		}
	}
}