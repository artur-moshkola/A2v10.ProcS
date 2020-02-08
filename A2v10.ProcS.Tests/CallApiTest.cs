﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A2v10.ProcS.Tests
{
	[TestClass]
	public class CallApiTest
	{
		[TestMethod]
		public async Task OpenWeatherApi()
		{
			var (engine, _, bus) = ProcessEngine.CreateEngine();

			var prms = new DynamicObject();
			prms.Set("city", "London");
			var instance = await engine.StartWorkflow(new Identity("callapi/openweather.json"), prms);

			await bus.Run();

			Assert.AreEqual(7.0, instance.GetResult().Eval<Double>("temp"));
			Assert.AreEqual("London", instance.GetResult().Eval<String>("city"));

			Assert.AreEqual("End", instance.CurrentState);
		}
	}
}
