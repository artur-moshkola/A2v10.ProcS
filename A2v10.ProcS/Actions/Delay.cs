﻿
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using A2v10.ProcS.Interfaces;

namespace A2v10.ProcS
{
	public class Delay : IWorkflowAction
	{
		public String Duration { get; set; }

		public async Task<ActionResult> Execute(IExecuteContext context)
		{
			TimeSpan span = TimeSpan.Parse(Duration);
			String bookmark = Guid.NewGuid().ToString();
			await Task.Delay(span);
			return ActionResult.Idle;
		}
	}
}