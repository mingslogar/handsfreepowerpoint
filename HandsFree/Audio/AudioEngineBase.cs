using System;
using System.Speech.Recognition;

namespace HandsFree.Audio
{
	public abstract class AudioEngineBase
	{
		#region Constructors

		public AudioEngineBase()
		{
			engine = new SpeechRecognitionEngine();

			engine.SpeechDetected += engine_SpeechDetected;
			engine.SpeechRecognitionRejected += engine_SpeechRecognitionRejected;
		}

		#endregion

		#region Public Methods

		public void StartListening()
		{
			engine.SetInputToDefaultAudioDevice();
			engine.RecognizeAsync(RecognizeMode.Multiple);
		}

		public void StopListening()
		{
			engine.RecognizeAsyncCancel();
		}

		#endregion

		#region Protected Fields

		protected SpeechRecognitionEngine engine;

		#endregion

		#region Protected Methods
		
		protected void engine_SpeechRecognitionRejected(object sender, SpeechRecognitionRejectedEventArgs e)
		{
			RaiseCommandRejected(this, EventArgs.Empty);
		}

		protected void engine_SpeechDetected(object sender, SpeechDetectedEventArgs e)
		{
			RaiseSpeechDetected(this, EventArgs.Empty);
		}

		#endregion

		#region Events

		public delegate void CommandExecutedEventHandler(object sender, EventArgs e);

		public event CommandExecutedEventHandler CommandExecuted;

		protected void RaiseCommandExecuted(object sender, EventArgs e)
		{
			if (CommandExecuted != null)
				CommandExecuted(sender, e);
		}

		public delegate void SpeechDetectedEventHandler(object sender, EventArgs e);

		public event SpeechDetectedEventHandler SpeechDetected;

		protected void RaiseSpeechDetected(object sender, EventArgs e)
		{
			if (SpeechDetected != null)
				SpeechDetected(sender, e);
		}

		public delegate void CommandRejectedEventHandler(object sender, EventArgs e);

		public event CommandRejectedEventHandler CommandRejected;

		protected void RaiseCommandRejected(object sender, EventArgs e)
		{
			if (CommandRejected != null)
				CommandRejected(sender, e);
		}

		#endregion
	}
}
