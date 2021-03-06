﻿// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using A2v10.ProcS.Infrastructure;

namespace A2v10.ProcS
{
	public class EndpointManager : IEndpointManager, IEndpointResolver
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
			if (handlers.TryGetValue(key, out var hnd))
			{
				return hnd;
			}
			else if (factories.TryGetValue(key, out var factory))
			{
				return handlers.GetOrAdd(key, k => factory.CreateHandler());
			}
			return null;
		}
	}

	public class DefaultCallback : IEndpointHandler
	{
		private readonly IServiceBus bus;

		public DefaultCallback(IServiceBus bus)
		{
			this.bus = bus;
		}

		public Task<(String body, String type)> HandleAsync(String body, String path)
		{
			var pathes = path.Split('/');
			var cbm = new CallbackMessage(pathes[0])
			{
				Result = DynamicObjectConverters.FromJson(body)
			};
			bus.Send(cbm);
			return Task.FromResult(("", "text/plain"));
		}
	}
}
