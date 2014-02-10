﻿#region usings
using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Audio;

using NAudio.Wave;
using NAudio.Wave.Asio;
using NAudio.CoreAudioApi;
using NAudio.Wave.SampleProviders;
using NAudio.Utils;


using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{
	public class AudioInSignal : AudioSignal
	{
		protected AudioEngine FEngine;
		protected int FIndex;
		public AudioInSignal(AudioEngine engine, int index)
		{
			FEngine = engine;
			FIndex = index;
		}
		
		protected override void FillBuffer(float[] buffer, int offset, int count)
		{
			Array.Copy(FEngine.InputBuffers[FIndex], offset, buffer, offset, count);
		}
	}
	
	[PluginInfo(Name = "AudioIn", Category = "Audio", Version = "Source", Help = "Outputs the audio input", AutoEvaluate = true, Tags = "record, input")]
	public class AudioInNode : IPluginEvaluate
	{
		
		[Output("Audio Out")]
		public ISpread<AudioSignal> OutBuffer;
		
		public AudioInNode()
		{
			FEngine = AudioService.Engine;
		}
		
		AudioEngine FEngine;
		bool FFirstFrame = true;
		public void Evaluate(int SpreadMax)
		{
			if(OutBuffer.SliceCount != FEngine.InputBuffers.Length)
			{
				var channels = FEngine.InputBuffers.Length;
				OutBuffer.SliceCount = channels;
				for(int i=0; i<channels; i++)
				{
					if(OutBuffer[i] != null) OutBuffer[i].Dispose();
					OutBuffer[i] = new AudioInSignal(FEngine, i);
				}
			}
		}
	}
}


