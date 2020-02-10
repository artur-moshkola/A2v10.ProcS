﻿// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS
{
	public class EndointManager : IEndpointManager, IEndpointResolver
	{
		private readonly Dictionary<String, IEndpointHandlerFactory> factories = new Dictionary<String, IEndpointHandlerFactory>();
		private readonly ConcurrentDictionary<String, IEndpointHandler> handlers = new ConcurrentDictionary<String, IEndpointHandler>();

		public void RegisterEndpoint(String key, IEndpointHandler handler)
		{
			handlers.AddOrUpdate(key, handler, (k, h) => handler);
		}

		public void RegisterEndpoint(String key, IEndpointHandlerFactory factory)
		{
			factories.Add(key, factory);
		}

		public IEndpointHandler GetHandler(String key)
		{
			return handlers.GetOrAdd(key, k => factories[k].CreateHandler());
		}
	}
}
