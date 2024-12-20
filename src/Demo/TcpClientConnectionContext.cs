﻿// DSC TLink - a communications library for DSC Powerseries NEO alarm panels
// Copyright (C) 2024 Brian Humlicek
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY, without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http.Features;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;

namespace DSC.TLink.Demo
{
	public class TcpClientConnectionContext : ConnectionContext
	{
		TcpClient tcpClient;
		public TcpClientConnectionContext(TcpClient tcpClient)
		{
			Transport = new NetworkStreamDuplexPipe(tcpClient.GetStream());
			this.tcpClient = tcpClient;
		}
		public override EndPoint? RemoteEndPoint { get => tcpClient?.Client?.RemoteEndPoint; set => throw new NotImplementedException(); }
		public override IDuplexPipe Transport { get; set; }
		public override string ConnectionId { get => RemoteEndPoint?.ToString() ?? "No end point"; set => throw new NotImplementedException(); }
		public override IFeatureCollection Features => throw new NotImplementedException();
		public override IDictionary<object, object?> Items { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
	}
}
