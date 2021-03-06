﻿// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;

namespace A2v10.ProcS.Infrastructure
{
	public abstract class MessageBase<CorrelationT> : IMessage where CorrelationT : IEquatable<CorrelationT>
	{
		public MessageBase(CorrelationT correlationId)
		{
			CorrelationId = new CorrelationId<CorrelationT>(correlationId);
		}
		
		public CorrelationId<CorrelationT> CorrelationId { get; }

		ICorrelationId IMessage.CorrelationId => CorrelationId;

		public virtual void Restore(IDynamicObject store, IResourceWrapper wrapper)
		{

		}

		public virtual void Store(IDynamicObject storage, IResourceWrapper wrapper)
		{

		}

		public IDynamicObject Store(IResourceWrapper wrapper)
		{
			var obj = new DynamicObject();
			obj.Set("correlationId", CorrelationId.Value);
			Store(obj, wrapper);
			return obj;
		}
	}
}
