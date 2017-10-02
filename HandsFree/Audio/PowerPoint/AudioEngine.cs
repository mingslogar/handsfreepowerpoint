using HandsFree.Keyboard;
using HandsFree.Win32;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Speech.Recognition;
using System.Windows.Forms;

namespace HandsFree.Audio.PowerPoint
{
	public class AudioEngine : AudioEngineBase
	{
		#region Constructors

		public AudioEngine()
		{
			CreateEngine();
		}

		#endregion
		
		#region Private Methods

		private void CreateEngine()
		{
			engine.LoadGrammar(Grammar());
			engine.SpeechRecognized += engine_SpeechRecognized;
		}

		private void engine_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
		{
			ReadOnlyCollection<RecognizedWordUnit> words = e.Result.Words;

			bool successful = true;

			switch (words[1].Text.ToLower())
			{
				case "start":
				case "begin":
				case "run":
					successful = SendKeystrokeToPowerPoint(KeyCodes.F5);
					break;

				case "exit":
					successful = SendKeystrokeToPowerPoint(KeyCodes.Escape);
					break;

				default:
					switch (words[2].Text.ToLower())
					{
						case "next":
						case "forward":
							successful = SendKeystrokeToPowerPoint(KeyCodes.RightArrow);
							break;

						case "previous":
						case "back":
							successful = SendKeystrokeToPowerPoint(KeyCodes.LeftArrow);
							break;

						case "start":
						case "beginning":
						case "home":
						case "first":
							successful = SendKeystrokeToPowerPoint(KeyCodes.Home);
							break;

						case "end":
						case "last":
							successful = SendKeystrokeToPowerPoint(KeyCodes.End);
							break;

						case "black":
							successful = SendKeystrokeToPowerPoint("B");
							break;

						case "white":
							successful = SendKeystrokeToPowerPoint("W");
							break;

						default:
							successful = false;
							break;
					}
					break;
			}

			if (successful)
				RaiseCommandExecuted(this, EventArgs.Empty);
			else
				RaiseCommandRejected(this, EventArgs.Empty);
		}

		private Grammar Grammar()
		{
			//
			// Default prompt
			//
			GrammarBuilder triggerPhrase = new GrammarBuilder("Okay");

			Choices keyWordOptions = new Choices(new string[]{
				"start", "begin", "run",
				"exit"
			});

			//
			// Go to
			//
			GrammarBuilder goTo = new GrammarBuilder();

			Choices goToOptions = new Choices(new string[] {
				"go",
				"show"
			});

			goTo.Append(goToOptions);

			Choices goToKeywords = new Choices(new string[] {
				"next", "forward",
				"previous", "back", 
				"start", "beginning", "home", "first",
				"end", "last",
				"black", "white"
			});

			goTo.Append(goToKeywords);

			keyWordOptions.Add(goTo);

			triggerPhrase.Append(keyWordOptions);

			return new Grammar(triggerPhrase);
		}

		private bool SendKeystrokeToPowerPoint(string sequence)
		{
			Process[] powerPoint = Process.GetProcessesByName("powerpnt");

			if (powerPoint.Length == 0)
				return false;

			if (powerPoint[0] != null)
			{
				if (!NativeMethods.SetForegroundWindow(powerPoint[0].MainWindowHandle))
					return false;

				SendKeys.SendWait(sequence);
				return true;
			}

			return false;
		}

		#endregion
	}
}
