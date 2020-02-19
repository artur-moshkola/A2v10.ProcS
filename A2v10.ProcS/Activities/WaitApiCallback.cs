﻿// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Threading.Tasks;

using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS
{
	public class WaitApiCallback : IActivity
	{
		public String Tag { get; set; }

		public String CorrelationValue { get; set; }
		public String CorrelationExpression { get; set; }

		public ActivityExecutionResult Execute(IExecuteContext context)
		{
			if (context.IsContinue)
				return ActivityExecutionResult.Complete;

			var book = context.SetBookmark();

			var mess = new RegisterCallbackMessage(Tag) {
				CorrelationExpression = CorrelationExpression
			};

			var mess2 = new WaitCallbackMessage(book, Tag, CorrelationValue);

			context.SendMessagesSequence(mess, mess2);

			return ActivityExecutionResult.Idle;
		}
	}
}
