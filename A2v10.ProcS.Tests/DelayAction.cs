﻿// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using A2v10.ProcS.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace A2v10.ProcS.Tests
{
	[TestClass]
	public class DelayAction
	{
		[TestMethod]
		public async Task SimpleRun()
		{
			var (engine, repository, bus) = ProcessEngine.CreateEngine();

			var sw = new Stopwatch();
			sw.Start();

			var instance = await engine.StartWorkflow("delay.json");
			var stm = instance.Workflow as StateMachine;

			await bus.Process(CancellationToken.None);

			sw.Stop();

			Assert.IsTrue(sw.ElapsedMilliseconds > 500);

			var ni = await repository.Get(instance.Id);

			Assert.AreEqual("S1", stm.InitialState);
			Assert.AreEqual("Delay Test", stm.Description);
			var s1 = stm.States["S1"];
			Assert.IsInstanceOfType(s1.OnEntry, typeof(DelayActivity));

			Assert.IsNull(ni.CurrentState);
		}
	}
}
