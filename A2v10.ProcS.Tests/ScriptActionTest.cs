﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Threading.Tasks;
using A2v10.ProcS.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace A2v10.ProcS.Tests
{
	[TestClass]
	public class ScriptActionTest
	{
		IWorkflowEngine CreateEngine()
		{
			var storage = new FakeStorage();
			var keeper = new InMemorySagaKeeper();
			var scriptEngine = new ScriptEngine();
			var bus = new ServiceBus(keeper, storage, scriptEngine);

			return new WorkflowEngine(storage, storage, bus, scriptEngine);
		}

		[TestMethod]
		public async Task SimpleCounter()
		{
			var engine = CreateEngine();

			var data = new DynamicObject();
			data.Set("counter", 10);
			var instance = await engine.StartWorkflow(new Identity("scripts/counter.json"), data);
			var prms = instance.GetParameters();

			Assert.AreEqual("End", instance.CurrentState);
			Assert.AreEqual(-1, prms.Eval<Int32>("counter"));
		}

		[TestMethod]
		public async Task SimpleResult()
		{
			var engine = CreateEngine();

			var instance = await engine.StartWorkflow(new Identity("scripts/result.json"));

			Assert.AreEqual("S1", instance.CurrentState);

			var r = instance.GetResult();

			Assert.AreEqual(42, r.Eval<Int32>("value"));
			Assert.AreEqual("x", r.Eval<String>("array[0].x"));
			Assert.AreEqual("y", r.Eval<String>("array[1].y"));
		}
	}
}