﻿// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Threading.Tasks;

namespace A2v10.ProcS.Infrastructure
{
	public enum ActionResult
	{
		Success,
		Fail,
		Idle
	}

	public interface IWorkflowAction
	{
		Task<ActionResult> Execute(IExecuteContext context);
	}
}
