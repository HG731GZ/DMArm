using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.XInput;
using System.Threading;

namespace RobotControl
{
	public class Joystick
	{
		private Controller controller;
		private State state;
		private Gamepad gamepad;
		private int left_thumb_x, left_thumb_y, right_thumb_x, right_thumb_y;
		private Thread trd;
		public struct Button
		{
			public bool X, Y, A, B, Up, Down, Left, Right, LeftShoulder, RightShoulder;
			public void set_button(bool x, bool y, bool a, bool b, bool up, bool down, bool left, bool right,bool leftshoulder,bool rightshoulder)
			{
				X = x;
				Y = y;
				A = a;
				B = b;
				Up = up;
				Down = down;
				Left = left;
				Right = right;
				LeftShoulder = leftshoulder;
				RightShoulder = rightshoulder;
			}
		}
		private Button button;
		public Button key
		{
			get { return button; }
		}
		public Joystick()
		{
			left_thumb_x = 0;
			left_thumb_y = 0;
			right_thumb_x = 0;
			right_thumb_y = 0;
		}
		public bool is_connected
		{
			get
			{
				if (controller!=null)
				{
					return controller.IsConnected;
				}
				return false;
			}
		}
		public int left_x
		{
			get { return left_thumb_x; }
		}
		public int left_y
		{
			get { return left_thumb_y; }
		}
		public int right_x
		{
			get { return right_thumb_x; }
		}
		public int right_y
		{
			get { return right_thumb_y; }
		}
		public void connect()
		{
			if (trd != null)
			{
				trd.Abort();
				trd = null;
			}
			controller = new Controller(UserIndex.One);
			button = new Button();
			trd = new Thread(main_trd);
			trd.Name = "Get Joystick Status Thread";
			trd.IsBackground = true;
			trd.Start();
		}
		private void main_trd()
		{
			while (controller.IsConnected)
			{
				state = controller.GetState();

				// 获取左摇杆和右摇杆的值
				gamepad = state.Gamepad;
				left_thumb_x = gamepad.LeftThumbX;
				left_thumb_y = gamepad.LeftThumbY;
				right_thumb_x = gamepad.RightThumbX;
				right_thumb_y = gamepad.RightThumbY;
				button.set_button
					(gamepad.Buttons.HasFlag(GamepadButtonFlags.X),
					gamepad.Buttons.HasFlag(GamepadButtonFlags.Y),
					gamepad.Buttons.HasFlag(GamepadButtonFlags.A),
					gamepad.Buttons.HasFlag(GamepadButtonFlags.B),
					gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadUp),
					gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadDown),
					gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadLeft),
					gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadRight),
					gamepad.Buttons.HasFlag(GamepadButtonFlags.LeftShoulder),
					gamepad.Buttons.HasFlag(GamepadButtonFlags.RightShoulder));
				delayms(0.1);
			}
		}
		private static double delayms(double time)       //手动设定延迟
		{
			if (time == 0)
			{
				return 0;
			}
			System.Diagnostics.Stopwatch stopTime = new System.Diagnostics.Stopwatch();

			stopTime.Start();
			while (stopTime.Elapsed.TotalMilliseconds < time)
			{
			}
			stopTime.Stop();
			stopTime.Reset();

			return stopTime.Elapsed.TotalMilliseconds;
		}
	}
}
