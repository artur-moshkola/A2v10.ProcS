﻿// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;

namespace A2v10.ProcS.Infrastructure
{
	public interface IScriptEngine
	{
		IScriptContext CreateContext();
	}

	public interface IScriptContext : IDisposable
	{
		IScriptContext NewContext();

		T Eval<T>(String expression);
		void Execute(String code);
		void SetValue(String name, IDynamicObject value);
		T GetValueFromObject<T>(IDynamicObject obj, String expression);
	}
}
