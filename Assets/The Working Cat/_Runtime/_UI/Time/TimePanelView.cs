// Assets/TheWorkingCat/Runtime/UI/Time/TimePanelView.cs
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TheWorkingCat.Simulation;

namespace TheWorkingCat.UI.Time
{
	public sealed class TimePanelView : MonoBehaviour
	{
		[Header("Refs")]
		[SerializeField] private SimulationRunner runner;

		[Header("Display")]
		[SerializeField] private TextMeshProUGUI infoText;

		[Header("Controls")]
		[SerializeField] private Slider tickRateSlider;   // 1..240
		[SerializeField] private Slider speedSlider;      // 0..20+
		[SerializeField] private Slider maxTicksSlider;   // 1..64
		[SerializeField] private Button playPauseButton;
		[SerializeField] private Button stepButton;

		private readonly StringBuilder _sb = new StringBuilder(512);
		private bool _uiWired;

		private CanvasGroup _group;


		private void OnEnable()
		{
			_group = GetComponent<CanvasGroup>();
			WireUIOnce();
			RefreshSlidersFromRunner();
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.T))
			{
				if (_group)
				{
					bool show = _group.alpha == 0f;
					_group.alpha = show ? 1f : 0f;
					_group.interactable = show;
					_group.blocksRaycasts = show;
				}
				else
				{
					gameObject.SetActive(!gameObject.activeSelf);
				}
			}

			if (runner == null || infoText == null) return;
			var c = runner.Clock;

			_sb.Clear();
			_sb.AppendLine("<b>TIME</b>");
			_sb.AppendLine($"State: {(c.IsPaused ? "Paused" : "Running")}");
			_sb.AppendLine();
			_sb.AppendLine($"Ticks: {c.TickCount:n0}");
			_sb.AppendLine($"Tick Rate: {c.TargetTicksPerSecond} /s");
			_sb.AppendLine($"Speed Multiplier: x{c.SpeedMultiplier:0.00}");
			_sb.AppendLine($"Max Ticks / Frame: {c.MaxTicksPerFrame}");
			_sb.AppendLine();
			_sb.AppendLine($"Ticks This Frame: {runner.LastFrameTicksExecuted}");
			_sb.AppendLine($"UPS (measured): {c.MeasuredUPS:0.0}");
			_sb.AppendLine($"FPS (measured): {runner.MeasuredFPS:0.0}");
			_sb.AppendLine($"Frame dt: {runner.LastFrameDeltaTime * 1000f:0.0} ms");
			_sb.AppendLine($"Accumulator: {c.AccumulatorSeconds:0.000} s");

			infoText.text = _sb.ToString();

			if (stepButton)
				stepButton.interactable = c.IsPaused;

			if (playPauseButton && playPauseButton.TryGetComponent(out TextMeshProUGUI btnLabel))
				btnLabel.text = c.IsPaused ? "Play" : "Pause";
		}

		private void WireUIOnce()
		{
			if (_uiWired || runner == null) return;

			if (tickRateSlider)
				tickRateSlider.onValueChanged.AddListener(v => runner.SetTicksPerSecond(v));

			if (speedSlider)
				speedSlider.onValueChanged.AddListener(v => runner.SetSpeedMultiplier(v));

			if (maxTicksSlider)
				maxTicksSlider.onValueChanged.AddListener(v => runner.SetMaxTicksPerFrame(v));

			if (playPauseButton)
				playPauseButton.onClick.AddListener(() => runner.TogglePause());

			if (stepButton)
				stepButton.onClick.AddListener(() =>
				{
					if (runner.Clock.IsPaused)
						runner.Step();
				});

			_uiWired = true;
		}

		private void RefreshSlidersFromRunner()
		{
			if (runner == null) return;
			var c = runner.Clock;

			if (tickRateSlider)
				tickRateSlider.SetValueWithoutNotify(c.TargetTicksPerSecond);

			if (speedSlider)
				speedSlider.SetValueWithoutNotify(c.SpeedMultiplier);

			if (maxTicksSlider)
				maxTicksSlider.SetValueWithoutNotify(c.MaxTicksPerFrame);
		}
	}
}
