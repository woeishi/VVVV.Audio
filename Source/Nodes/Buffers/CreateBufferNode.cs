﻿#region usings
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

using NAudio.CoreAudioApi;
using NAudio.Utils;
using NAudio.Wave;
using NAudio.Wave.Asio;
using NAudio.Wave.SampleProviders;
using VVVV.Audio;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

#endregion usings

namespace VVVV.Nodes
{	
	/// <summary>
	/// Base class for nodes which work with buffers
	/// </summary>
	public abstract class BufferAudioSignal : AudioSignalInput
	{
		public BufferAudioSignal(string bufferKey)
		{
			FBufferKey = bufferKey;
			AudioService.BufferStorage.BufferSet += BufferStorage_BufferSet;
			AudioService.BufferStorage.BufferRemoved += BufferStorage_BufferRemoved;
			
			if(AudioService.BufferStorage.ContainsKey(FBufferKey))
			{
				SetBuffer(AudioService.BufferStorage[FBufferKey]);
			}
			else
			{
				SetBuffer(new float[AudioService.Engine.Settings.BufferSize]);
			}
		}

		void BufferStorage_BufferRemoved(object sender, BufferEventArgs e)
		{
			if(e.BufferName == FBufferKey)
			{
				SetBuffer(new float[AudioService.Engine.Settings.BufferSize]);
			}
		}

		void BufferStorage_BufferSet(object sender, BufferEventArgs e)
		{
			if(e.BufferName == FBufferKey)
			{
				SetBuffer(e.Buffer);
			}
		}
		
		protected void SetBuffer(float[] buffer)
		{
			FBuffer = buffer;
			FBufferSize = FBuffer.Length;
		}
	
		protected string FBufferKey;
		public string BufferKey
		{
			get
			{
				return FBufferKey;
			}
			set
			{
				if(FBufferKey != value)
				{
					FBufferKey = value;
					SetBuffer(AudioService.BufferStorage[FBufferKey]);
				}
			}
		}
		protected int FBufferSize;
		protected float[] FBuffer;
		
		protected override void FillBuffer(float[] buffer, int offset, int count)
		{
			
		}
		
		public override void Dispose()
		{
			AudioService.BufferStorage.BufferSet -= BufferStorage_BufferSet;
			AudioService.BufferStorage.BufferRemoved -= BufferStorage_BufferRemoved;
			base.Dispose();
		}
	}
	
	[PluginInfo(Name = "CreateBuffer", Category = "VAudio", Help = "Creates a buffer which can be used to write and read samples", AutoEvaluate = true, Tags = "record")]
	public class CreateBufferNode : IPluginEvaluate, IDisposable
	{
		#pragma warning disable 0649
		[Input("Buffer ID", DefaultString = "")]
		IDiffSpread<string> FNameIn;
		
		[Input("Size", DefaultValue = 1024)]
		IDiffSpread<int> FSizeIn;
	
		#pragma warning restore
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if(FNameIn.IsChanged || FSizeIn.IsChanged)
			{
				var storage = AudioService.BufferStorage;
				for (int i = 0; i < FNameIn.SliceCount; i++)
				{
					var key = FNameIn[i];
					if (!string.IsNullOrEmpty(key))
					{
						if(storage.ContainsKey(key))
						{
							if(storage[key].Length != FSizeIn[i]) //resize?
							{
								storage.SetBuffer(key, new float[FSizeIn[i]]);
							}
						}
						else
						{
							storage.SetBuffer(key, new float[FSizeIn[i]]);
						}
					}
				}
				
				UpdateEnum();
				
				//delete buffers?
				foreach (var key in storage.Keys) 
				{
					if(!FNameIn.Contains(key))
					{
						storage.RemoveBuffer(key);
					}
				}
			}
		}
		
		void UpdateEnum()
		{
			var bufferKeys = AudioService.BufferStorage.Keys.ToArray();
			
			if (bufferKeys.Length > 0)
			{
				EnumManager.UpdateEnum("AudioBufferStorageKeys", bufferKeys[0], bufferKeys);
			}
			else
			{
				bufferKeys = new string[]{"No Buffers Created yet"};
				EnumManager.UpdateEnum("AudioBufferStorageKeys", bufferKeys[0], bufferKeys);
			}
		}
		
		public void Dispose()
		{
			foreach (var key in FNameIn) 
			{
				AudioService.BufferStorage.Remove(key);
			}
			
		}
	}
}


