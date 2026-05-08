namespace DMArmAPP
{
	partial class Form1
	{
		/// <summary>
		/// 必需的设计器变量。
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// 清理所有正在使用的资源。
		/// </summary>
		/// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows 窗体设计器生成的代码

		/// <summary>
		/// 设计器支持所需的方法 - 不要修改
		/// 使用代码编辑器修改此方法的内容。
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.groupBox_motor_temp = new System.Windows.Forms.GroupBox();
			this.textBox_motor_temp6 = new System.Windows.Forms.TextBox();
			this.textBox_motor_temp5 = new System.Windows.Forms.TextBox();
			this.textBox_motor_temp4 = new System.Windows.Forms.TextBox();
			this.textBox_motor_temp3 = new System.Windows.Forms.TextBox();
			this.textBox_motor_temp2 = new System.Windows.Forms.TextBox();
			this.textBox_motor_temp1 = new System.Windows.Forms.TextBox();
			this.groupBox_motor_tor = new System.Windows.Forms.GroupBox();
			this.textBox_motor_tor6 = new System.Windows.Forms.TextBox();
			this.textBox_motor_tor5 = new System.Windows.Forms.TextBox();
			this.textBox_motor_tor4 = new System.Windows.Forms.TextBox();
			this.textBox_motor_tor3 = new System.Windows.Forms.TextBox();
			this.textBox_motor_tor2 = new System.Windows.Forms.TextBox();
			this.textBox_motor_tor1 = new System.Windows.Forms.TextBox();
			this.groupBox_motor_vel = new System.Windows.Forms.GroupBox();
			this.textBox_motor_vel6 = new System.Windows.Forms.TextBox();
			this.textBox_motor_vel5 = new System.Windows.Forms.TextBox();
			this.textBox_motor_vel4 = new System.Windows.Forms.TextBox();
			this.textBox_motor_vel3 = new System.Windows.Forms.TextBox();
			this.textBox_motor_vel2 = new System.Windows.Forms.TextBox();
			this.textBox_motor_vel1 = new System.Windows.Forms.TextBox();
			this.groupBox_motor_pos = new System.Windows.Forms.GroupBox();
			this.textBox_motor_pos6 = new System.Windows.Forms.TextBox();
			this.textBox_motor_pos5 = new System.Windows.Forms.TextBox();
			this.textBox_motor_pos4 = new System.Windows.Forms.TextBox();
			this.textBox_motor_pos3 = new System.Windows.Forms.TextBox();
			this.textBox_motor_pos2 = new System.Windows.Forms.TextBox();
			this.textBox_motor_pos1 = new System.Windows.Forms.TextBox();
			this.button_enable_motor = new System.Windows.Forms.Button();
			this.button_start_sending = new System.Windows.Forms.Button();
			this.button_disable_motor = new System.Windows.Forms.Button();
			this.button_open_device = new System.Windows.Forms.Button();
			this.param_updating_timer = new System.Windows.Forms.Timer(this.components);
			this.label_mode = new System.Windows.Forms.Label();
			this.groupBox_clamp = new System.Windows.Forms.GroupBox();
			this.textBox_clamp6 = new System.Windows.Forms.TextBox();
			this.textBox_clamp5 = new System.Windows.Forms.TextBox();
			this.textBox_clamp4 = new System.Windows.Forms.TextBox();
			this.textBox_clamp3 = new System.Windows.Forms.TextBox();
			this.textBox_clamp2 = new System.Windows.Forms.TextBox();
			this.textBox_clamp1 = new System.Windows.Forms.TextBox();
			this.udp_send_timer = new System.Windows.Forms.Timer(this.components);
			this.button_set_zero1 = new System.Windows.Forms.Button();
			this.groupBox_single_motor_set = new System.Windows.Forms.GroupBox();
			this.button_lock6 = new System.Windows.Forms.Button();
			this.button_set_zero6 = new System.Windows.Forms.Button();
			this.button_lock5 = new System.Windows.Forms.Button();
			this.button_set_zero5 = new System.Windows.Forms.Button();
			this.button_lock4 = new System.Windows.Forms.Button();
			this.button_set_zero4 = new System.Windows.Forms.Button();
			this.button_lock3 = new System.Windows.Forms.Button();
			this.button_set_zero3 = new System.Windows.Forms.Button();
			this.button_lock2 = new System.Windows.Forms.Button();
			this.button_set_zero2 = new System.Windows.Forms.Button();
			this.button_lock1 = new System.Windows.Forms.Button();
			this.button_set_MIT = new System.Windows.Forms.Button();
			this.button_set_PV = new System.Windows.Forms.Button();
			this.button_gravcomp_sw = new System.Windows.Forms.Button();
			this.button_clamp_sw = new System.Windows.Forms.Button();
			this.numericUpDown_clamp_torque = new System.Windows.Forms.NumericUpDown();
			this.label_clamp_torque = new System.Windows.Forms.Label();
			this.label_can_param = new System.Windows.Forms.Label();
			this.checkBox_clamp_spring = new System.Windows.Forms.CheckBox();
			this.groupBox_DH = new System.Windows.Forms.GroupBox();
			this.textBox_DH6 = new System.Windows.Forms.TextBox();
			this.textBox_DH5 = new System.Windows.Forms.TextBox();
			this.textBox_DH4 = new System.Windows.Forms.TextBox();
			this.textBox_DH3 = new System.Windows.Forms.TextBox();
			this.textBox_DH2 = new System.Windows.Forms.TextBox();
			this.textBox_DH1 = new System.Windows.Forms.TextBox();
			this.trackBar_outputX = new System.Windows.Forms.TrackBar();
			this.label_output_X = new System.Windows.Forms.Label();
			this.label_output_Y = new System.Windows.Forms.Label();
			this.trackBar_outputY = new System.Windows.Forms.TrackBar();
			this.label_output_Z = new System.Windows.Forms.Label();
			this.trackBar_outputZ = new System.Windows.Forms.TrackBar();
			this.button_start_output = new System.Windows.Forms.Button();
			this.groupBox_tool = new System.Windows.Forms.GroupBox();
			this.textBox_tool6 = new System.Windows.Forms.TextBox();
			this.textBox_tool5 = new System.Windows.Forms.TextBox();
			this.textBox_tool4 = new System.Windows.Forms.TextBox();
			this.textBox_tool3 = new System.Windows.Forms.TextBox();
			this.textBox_tool2 = new System.Windows.Forms.TextBox();
			this.textBox_tool1 = new System.Windows.Forms.TextBox();
			this.textBox_udp_ip = new System.Windows.Forms.TextBox();
			this.textBox_udp_port = new System.Windows.Forms.TextBox();
			this.button_udp_connect = new System.Windows.Forms.Button();
			this.groupBox_motor_temp.SuspendLayout();
			this.groupBox_motor_tor.SuspendLayout();
			this.groupBox_motor_vel.SuspendLayout();
			this.groupBox_motor_pos.SuspendLayout();
			this.groupBox_clamp.SuspendLayout();
			this.groupBox_single_motor_set.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown_clamp_torque)).BeginInit();
			this.groupBox_DH.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.trackBar_outputX)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBar_outputY)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBar_outputZ)).BeginInit();
			this.groupBox_tool.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox_motor_temp
			// 
			this.groupBox_motor_temp.Controls.Add(this.textBox_motor_temp6);
			this.groupBox_motor_temp.Controls.Add(this.textBox_motor_temp5);
			this.groupBox_motor_temp.Controls.Add(this.textBox_motor_temp4);
			this.groupBox_motor_temp.Controls.Add(this.textBox_motor_temp3);
			this.groupBox_motor_temp.Controls.Add(this.textBox_motor_temp2);
			this.groupBox_motor_temp.Controls.Add(this.textBox_motor_temp1);
			this.groupBox_motor_temp.Location = new System.Drawing.Point(1136, 33);
			this.groupBox_motor_temp.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.groupBox_motor_temp.Name = "groupBox_motor_temp";
			this.groupBox_motor_temp.Padding = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.groupBox_motor_temp.Size = new System.Drawing.Size(221, 413);
			this.groupBox_motor_temp.TabIndex = 40;
			this.groupBox_motor_temp.TabStop = false;
			this.groupBox_motor_temp.Text = "电机温度/错误码";
			// 
			// textBox_motor_temp6
			// 
			this.textBox_motor_temp6.Location = new System.Drawing.Point(11, 355);
			this.textBox_motor_temp6.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.textBox_motor_temp6.Name = "textBox_motor_temp6";
			this.textBox_motor_temp6.ReadOnly = true;
			this.textBox_motor_temp6.Size = new System.Drawing.Size(199, 39);
			this.textBox_motor_temp6.TabIndex = 5;
			// 
			// textBox_motor_temp5
			// 
			this.textBox_motor_temp5.Location = new System.Drawing.Point(11, 293);
			this.textBox_motor_temp5.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.textBox_motor_temp5.Name = "textBox_motor_temp5";
			this.textBox_motor_temp5.ReadOnly = true;
			this.textBox_motor_temp5.Size = new System.Drawing.Size(199, 39);
			this.textBox_motor_temp5.TabIndex = 4;
			// 
			// textBox_motor_temp4
			// 
			this.textBox_motor_temp4.Location = new System.Drawing.Point(11, 229);
			this.textBox_motor_temp4.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.textBox_motor_temp4.Name = "textBox_motor_temp4";
			this.textBox_motor_temp4.ReadOnly = true;
			this.textBox_motor_temp4.Size = new System.Drawing.Size(199, 39);
			this.textBox_motor_temp4.TabIndex = 3;
			// 
			// textBox_motor_temp3
			// 
			this.textBox_motor_temp3.Location = new System.Drawing.Point(11, 168);
			this.textBox_motor_temp3.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.textBox_motor_temp3.Name = "textBox_motor_temp3";
			this.textBox_motor_temp3.ReadOnly = true;
			this.textBox_motor_temp3.Size = new System.Drawing.Size(199, 39);
			this.textBox_motor_temp3.TabIndex = 2;
			// 
			// textBox_motor_temp2
			// 
			this.textBox_motor_temp2.Location = new System.Drawing.Point(11, 107);
			this.textBox_motor_temp2.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.textBox_motor_temp2.Name = "textBox_motor_temp2";
			this.textBox_motor_temp2.ReadOnly = true;
			this.textBox_motor_temp2.Size = new System.Drawing.Size(199, 39);
			this.textBox_motor_temp2.TabIndex = 1;
			// 
			// textBox_motor_temp1
			// 
			this.textBox_motor_temp1.BackColor = System.Drawing.SystemColors.Control;
			this.textBox_motor_temp1.ForeColor = System.Drawing.SystemColors.WindowText;
			this.textBox_motor_temp1.Location = new System.Drawing.Point(11, 43);
			this.textBox_motor_temp1.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.textBox_motor_temp1.Name = "textBox_motor_temp1";
			this.textBox_motor_temp1.ReadOnly = true;
			this.textBox_motor_temp1.Size = new System.Drawing.Size(199, 39);
			this.textBox_motor_temp1.TabIndex = 0;
			// 
			// groupBox_motor_tor
			// 
			this.groupBox_motor_tor.Controls.Add(this.textBox_motor_tor6);
			this.groupBox_motor_tor.Controls.Add(this.textBox_motor_tor5);
			this.groupBox_motor_tor.Controls.Add(this.textBox_motor_tor4);
			this.groupBox_motor_tor.Controls.Add(this.textBox_motor_tor3);
			this.groupBox_motor_tor.Controls.Add(this.textBox_motor_tor2);
			this.groupBox_motor_tor.Controls.Add(this.textBox_motor_tor1);
			this.groupBox_motor_tor.Location = new System.Drawing.Point(899, 33);
			this.groupBox_motor_tor.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.groupBox_motor_tor.Name = "groupBox_motor_tor";
			this.groupBox_motor_tor.Padding = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.groupBox_motor_tor.Size = new System.Drawing.Size(221, 413);
			this.groupBox_motor_tor.TabIndex = 39;
			this.groupBox_motor_tor.TabStop = false;
			this.groupBox_motor_tor.Text = "电机扭矩";
			// 
			// textBox_motor_tor6
			// 
			this.textBox_motor_tor6.Location = new System.Drawing.Point(11, 355);
			this.textBox_motor_tor6.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.textBox_motor_tor6.Name = "textBox_motor_tor6";
			this.textBox_motor_tor6.ReadOnly = true;
			this.textBox_motor_tor6.Size = new System.Drawing.Size(199, 39);
			this.textBox_motor_tor6.TabIndex = 5;
			// 
			// textBox_motor_tor5
			// 
			this.textBox_motor_tor5.Location = new System.Drawing.Point(11, 293);
			this.textBox_motor_tor5.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.textBox_motor_tor5.Name = "textBox_motor_tor5";
			this.textBox_motor_tor5.ReadOnly = true;
			this.textBox_motor_tor5.Size = new System.Drawing.Size(199, 39);
			this.textBox_motor_tor5.TabIndex = 4;
			// 
			// textBox_motor_tor4
			// 
			this.textBox_motor_tor4.Location = new System.Drawing.Point(11, 229);
			this.textBox_motor_tor4.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.textBox_motor_tor4.Name = "textBox_motor_tor4";
			this.textBox_motor_tor4.ReadOnly = true;
			this.textBox_motor_tor4.Size = new System.Drawing.Size(199, 39);
			this.textBox_motor_tor4.TabIndex = 3;
			// 
			// textBox_motor_tor3
			// 
			this.textBox_motor_tor3.Location = new System.Drawing.Point(11, 168);
			this.textBox_motor_tor3.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.textBox_motor_tor3.Name = "textBox_motor_tor3";
			this.textBox_motor_tor3.ReadOnly = true;
			this.textBox_motor_tor3.Size = new System.Drawing.Size(199, 39);
			this.textBox_motor_tor3.TabIndex = 2;
			// 
			// textBox_motor_tor2
			// 
			this.textBox_motor_tor2.Location = new System.Drawing.Point(11, 107);
			this.textBox_motor_tor2.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.textBox_motor_tor2.Name = "textBox_motor_tor2";
			this.textBox_motor_tor2.ReadOnly = true;
			this.textBox_motor_tor2.Size = new System.Drawing.Size(199, 39);
			this.textBox_motor_tor2.TabIndex = 1;
			// 
			// textBox_motor_tor1
			// 
			this.textBox_motor_tor1.Location = new System.Drawing.Point(11, 43);
			this.textBox_motor_tor1.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.textBox_motor_tor1.Name = "textBox_motor_tor1";
			this.textBox_motor_tor1.ReadOnly = true;
			this.textBox_motor_tor1.Size = new System.Drawing.Size(199, 39);
			this.textBox_motor_tor1.TabIndex = 0;
			// 
			// groupBox_motor_vel
			// 
			this.groupBox_motor_vel.Controls.Add(this.textBox_motor_vel6);
			this.groupBox_motor_vel.Controls.Add(this.textBox_motor_vel5);
			this.groupBox_motor_vel.Controls.Add(this.textBox_motor_vel4);
			this.groupBox_motor_vel.Controls.Add(this.textBox_motor_vel3);
			this.groupBox_motor_vel.Controls.Add(this.textBox_motor_vel2);
			this.groupBox_motor_vel.Controls.Add(this.textBox_motor_vel1);
			this.groupBox_motor_vel.Location = new System.Drawing.Point(661, 33);
			this.groupBox_motor_vel.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.groupBox_motor_vel.Name = "groupBox_motor_vel";
			this.groupBox_motor_vel.Padding = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.groupBox_motor_vel.Size = new System.Drawing.Size(221, 413);
			this.groupBox_motor_vel.TabIndex = 38;
			this.groupBox_motor_vel.TabStop = false;
			this.groupBox_motor_vel.Text = "电机速度";
			// 
			// textBox_motor_vel6
			// 
			this.textBox_motor_vel6.Location = new System.Drawing.Point(11, 355);
			this.textBox_motor_vel6.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.textBox_motor_vel6.Name = "textBox_motor_vel6";
			this.textBox_motor_vel6.ReadOnly = true;
			this.textBox_motor_vel6.Size = new System.Drawing.Size(199, 39);
			this.textBox_motor_vel6.TabIndex = 5;
			// 
			// textBox_motor_vel5
			// 
			this.textBox_motor_vel5.Location = new System.Drawing.Point(11, 293);
			this.textBox_motor_vel5.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.textBox_motor_vel5.Name = "textBox_motor_vel5";
			this.textBox_motor_vel5.ReadOnly = true;
			this.textBox_motor_vel5.Size = new System.Drawing.Size(199, 39);
			this.textBox_motor_vel5.TabIndex = 4;
			// 
			// textBox_motor_vel4
			// 
			this.textBox_motor_vel4.Location = new System.Drawing.Point(11, 229);
			this.textBox_motor_vel4.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.textBox_motor_vel4.Name = "textBox_motor_vel4";
			this.textBox_motor_vel4.ReadOnly = true;
			this.textBox_motor_vel4.Size = new System.Drawing.Size(199, 39);
			this.textBox_motor_vel4.TabIndex = 3;
			// 
			// textBox_motor_vel3
			// 
			this.textBox_motor_vel3.Location = new System.Drawing.Point(11, 168);
			this.textBox_motor_vel3.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.textBox_motor_vel3.Name = "textBox_motor_vel3";
			this.textBox_motor_vel3.ReadOnly = true;
			this.textBox_motor_vel3.Size = new System.Drawing.Size(199, 39);
			this.textBox_motor_vel3.TabIndex = 2;
			// 
			// textBox_motor_vel2
			// 
			this.textBox_motor_vel2.Location = new System.Drawing.Point(11, 107);
			this.textBox_motor_vel2.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.textBox_motor_vel2.Name = "textBox_motor_vel2";
			this.textBox_motor_vel2.ReadOnly = true;
			this.textBox_motor_vel2.Size = new System.Drawing.Size(199, 39);
			this.textBox_motor_vel2.TabIndex = 1;
			// 
			// textBox_motor_vel1
			// 
			this.textBox_motor_vel1.Location = new System.Drawing.Point(11, 43);
			this.textBox_motor_vel1.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.textBox_motor_vel1.Name = "textBox_motor_vel1";
			this.textBox_motor_vel1.ReadOnly = true;
			this.textBox_motor_vel1.Size = new System.Drawing.Size(199, 39);
			this.textBox_motor_vel1.TabIndex = 0;
			// 
			// groupBox_motor_pos
			// 
			this.groupBox_motor_pos.Controls.Add(this.textBox_motor_pos6);
			this.groupBox_motor_pos.Controls.Add(this.textBox_motor_pos5);
			this.groupBox_motor_pos.Controls.Add(this.textBox_motor_pos4);
			this.groupBox_motor_pos.Controls.Add(this.textBox_motor_pos3);
			this.groupBox_motor_pos.Controls.Add(this.textBox_motor_pos2);
			this.groupBox_motor_pos.Controls.Add(this.textBox_motor_pos1);
			this.groupBox_motor_pos.Location = new System.Drawing.Point(425, 33);
			this.groupBox_motor_pos.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.groupBox_motor_pos.Name = "groupBox_motor_pos";
			this.groupBox_motor_pos.Padding = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.groupBox_motor_pos.Size = new System.Drawing.Size(221, 413);
			this.groupBox_motor_pos.TabIndex = 37;
			this.groupBox_motor_pos.TabStop = false;
			this.groupBox_motor_pos.Text = "电机位置";
			// 
			// textBox_motor_pos6
			// 
			this.textBox_motor_pos6.Location = new System.Drawing.Point(11, 355);
			this.textBox_motor_pos6.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.textBox_motor_pos6.Name = "textBox_motor_pos6";
			this.textBox_motor_pos6.ReadOnly = true;
			this.textBox_motor_pos6.Size = new System.Drawing.Size(199, 39);
			this.textBox_motor_pos6.TabIndex = 5;
			// 
			// textBox_motor_pos5
			// 
			this.textBox_motor_pos5.Location = new System.Drawing.Point(11, 293);
			this.textBox_motor_pos5.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.textBox_motor_pos5.Name = "textBox_motor_pos5";
			this.textBox_motor_pos5.ReadOnly = true;
			this.textBox_motor_pos5.Size = new System.Drawing.Size(199, 39);
			this.textBox_motor_pos5.TabIndex = 4;
			// 
			// textBox_motor_pos4
			// 
			this.textBox_motor_pos4.Location = new System.Drawing.Point(11, 229);
			this.textBox_motor_pos4.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.textBox_motor_pos4.Name = "textBox_motor_pos4";
			this.textBox_motor_pos4.ReadOnly = true;
			this.textBox_motor_pos4.Size = new System.Drawing.Size(199, 39);
			this.textBox_motor_pos4.TabIndex = 3;
			// 
			// textBox_motor_pos3
			// 
			this.textBox_motor_pos3.Location = new System.Drawing.Point(11, 168);
			this.textBox_motor_pos3.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.textBox_motor_pos3.Name = "textBox_motor_pos3";
			this.textBox_motor_pos3.ReadOnly = true;
			this.textBox_motor_pos3.Size = new System.Drawing.Size(199, 39);
			this.textBox_motor_pos3.TabIndex = 2;
			// 
			// textBox_motor_pos2
			// 
			this.textBox_motor_pos2.Location = new System.Drawing.Point(11, 107);
			this.textBox_motor_pos2.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.textBox_motor_pos2.Name = "textBox_motor_pos2";
			this.textBox_motor_pos2.ReadOnly = true;
			this.textBox_motor_pos2.Size = new System.Drawing.Size(199, 39);
			this.textBox_motor_pos2.TabIndex = 1;
			// 
			// textBox_motor_pos1
			// 
			this.textBox_motor_pos1.Location = new System.Drawing.Point(11, 43);
			this.textBox_motor_pos1.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.textBox_motor_pos1.Name = "textBox_motor_pos1";
			this.textBox_motor_pos1.ReadOnly = true;
			this.textBox_motor_pos1.Size = new System.Drawing.Size(199, 39);
			this.textBox_motor_pos1.TabIndex = 0;
			// 
			// button_enable_motor
			// 
			this.button_enable_motor.Enabled = false;
			this.button_enable_motor.Location = new System.Drawing.Point(69, 127);
			this.button_enable_motor.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.button_enable_motor.Name = "button_enable_motor";
			this.button_enable_motor.Size = new System.Drawing.Size(149, 45);
			this.button_enable_motor.TabIndex = 44;
			this.button_enable_motor.Text = "电机使能";
			this.button_enable_motor.UseVisualStyleBackColor = true;
			this.button_enable_motor.Click += new System.EventHandler(this.button_enable_motor_Click);
			// 
			// button_start_sending
			// 
			this.button_start_sending.Enabled = false;
			this.button_start_sending.Location = new System.Drawing.Point(229, 71);
			this.button_start_sending.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.button_start_sending.Name = "button_start_sending";
			this.button_start_sending.Size = new System.Drawing.Size(149, 45);
			this.button_start_sending.TabIndex = 43;
			this.button_start_sending.Text = "开始发送";
			this.button_start_sending.UseVisualStyleBackColor = true;
			this.button_start_sending.Click += new System.EventHandler(this.button_start_sending_Click);
			// 
			// button_disable_motor
			// 
			this.button_disable_motor.Enabled = false;
			this.button_disable_motor.Location = new System.Drawing.Point(229, 127);
			this.button_disable_motor.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.button_disable_motor.Name = "button_disable_motor";
			this.button_disable_motor.Size = new System.Drawing.Size(149, 45);
			this.button_disable_motor.TabIndex = 42;
			this.button_disable_motor.Text = "电机失能";
			this.button_disable_motor.UseVisualStyleBackColor = true;
			this.button_disable_motor.Click += new System.EventHandler(this.button_disable_motor_Click);
			// 
			// button_open_device
			// 
			this.button_open_device.Location = new System.Drawing.Point(69, 71);
			this.button_open_device.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.button_open_device.Name = "button_open_device";
			this.button_open_device.Size = new System.Drawing.Size(149, 45);
			this.button_open_device.TabIndex = 41;
			this.button_open_device.Text = "开启设备";
			this.button_open_device.UseVisualStyleBackColor = true;
			this.button_open_device.Click += new System.EventHandler(this.button_open_device_Click);
			// 
			// param_updating_timer
			// 
			this.param_updating_timer.Enabled = true;
			this.param_updating_timer.Interval = 10;
			this.param_updating_timer.Tick += new System.EventHandler(this.param_updating_timer_Tick);
			// 
			// label_mode
			// 
			this.label_mode.AutoSize = true;
			this.label_mode.Font = new System.Drawing.Font("微软雅黑", 13.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.label_mode.Location = new System.Drawing.Point(12, 1037);
			this.label_mode.Name = "label_mode";
			this.label_mode.Size = new System.Drawing.Size(347, 48);
			this.label_mode.TabIndex = 45;
			this.label_mode.Text = "当前模式：MIT模式";
			// 
			// groupBox_clamp
			// 
			this.groupBox_clamp.Controls.Add(this.textBox_clamp6);
			this.groupBox_clamp.Controls.Add(this.textBox_clamp5);
			this.groupBox_clamp.Controls.Add(this.textBox_clamp4);
			this.groupBox_clamp.Controls.Add(this.textBox_clamp3);
			this.groupBox_clamp.Controls.Add(this.textBox_clamp2);
			this.groupBox_clamp.Controls.Add(this.textBox_clamp1);
			this.groupBox_clamp.Location = new System.Drawing.Point(1611, 33);
			this.groupBox_clamp.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.groupBox_clamp.Name = "groupBox_clamp";
			this.groupBox_clamp.Padding = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.groupBox_clamp.Size = new System.Drawing.Size(221, 413);
			this.groupBox_clamp.TabIndex = 38;
			this.groupBox_clamp.TabStop = false;
			this.groupBox_clamp.Text = "夹钳电机";
			// 
			// textBox_clamp6
			// 
			this.textBox_clamp6.Location = new System.Drawing.Point(11, 355);
			this.textBox_clamp6.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.textBox_clamp6.Name = "textBox_clamp6";
			this.textBox_clamp6.ReadOnly = true;
			this.textBox_clamp6.Size = new System.Drawing.Size(199, 39);
			this.textBox_clamp6.TabIndex = 5;
			this.textBox_clamp6.Text = "状态：";
			// 
			// textBox_clamp5
			// 
			this.textBox_clamp5.Location = new System.Drawing.Point(11, 293);
			this.textBox_clamp5.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.textBox_clamp5.Name = "textBox_clamp5";
			this.textBox_clamp5.ReadOnly = true;
			this.textBox_clamp5.Size = new System.Drawing.Size(199, 39);
			this.textBox_clamp5.TabIndex = 4;
			this.textBox_clamp5.Text = "温度：";
			// 
			// textBox_clamp4
			// 
			this.textBox_clamp4.Location = new System.Drawing.Point(11, 229);
			this.textBox_clamp4.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.textBox_clamp4.Name = "textBox_clamp4";
			this.textBox_clamp4.ReadOnly = true;
			this.textBox_clamp4.Size = new System.Drawing.Size(199, 39);
			this.textBox_clamp4.TabIndex = 3;
			this.textBox_clamp4.Text = "夹持力：";
			// 
			// textBox_clamp3
			// 
			this.textBox_clamp3.Location = new System.Drawing.Point(11, 168);
			this.textBox_clamp3.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.textBox_clamp3.Name = "textBox_clamp3";
			this.textBox_clamp3.ReadOnly = true;
			this.textBox_clamp3.Size = new System.Drawing.Size(199, 39);
			this.textBox_clamp3.TabIndex = 2;
			this.textBox_clamp3.Text = "力矩：";
			// 
			// textBox_clamp2
			// 
			this.textBox_clamp2.Location = new System.Drawing.Point(11, 107);
			this.textBox_clamp2.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.textBox_clamp2.Name = "textBox_clamp2";
			this.textBox_clamp2.ReadOnly = true;
			this.textBox_clamp2.Size = new System.Drawing.Size(199, 39);
			this.textBox_clamp2.TabIndex = 1;
			this.textBox_clamp2.Text = "速度：";
			// 
			// textBox_clamp1
			// 
			this.textBox_clamp1.Location = new System.Drawing.Point(11, 43);
			this.textBox_clamp1.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.textBox_clamp1.Name = "textBox_clamp1";
			this.textBox_clamp1.ReadOnly = true;
			this.textBox_clamp1.Size = new System.Drawing.Size(199, 39);
			this.textBox_clamp1.TabIndex = 0;
			this.textBox_clamp1.Text = "位置：";
			// 
			// udp_send_timer
			// 
			this.udp_send_timer.Interval = 18;
			this.udp_send_timer.Tick += new System.EventHandler(this.udp_send_timer_Tick);
			// 
			// button_set_zero1
			// 
			this.button_set_zero1.Enabled = false;
			this.button_set_zero1.Location = new System.Drawing.Point(11, 43);
			this.button_set_zero1.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.button_set_zero1.Name = "button_set_zero1";
			this.button_set_zero1.Size = new System.Drawing.Size(83, 39);
			this.button_set_zero1.TabIndex = 46;
			this.button_set_zero1.Text = "设零";
			this.button_set_zero1.UseVisualStyleBackColor = true;
			this.button_set_zero1.Click += new System.EventHandler(this.button_set_single_zero_Click);
			// 
			// groupBox_single_motor_set
			// 
			this.groupBox_single_motor_set.Controls.Add(this.button_lock6);
			this.groupBox_single_motor_set.Controls.Add(this.button_set_zero6);
			this.groupBox_single_motor_set.Controls.Add(this.button_lock5);
			this.groupBox_single_motor_set.Controls.Add(this.button_set_zero5);
			this.groupBox_single_motor_set.Controls.Add(this.button_lock4);
			this.groupBox_single_motor_set.Controls.Add(this.button_set_zero4);
			this.groupBox_single_motor_set.Controls.Add(this.button_lock3);
			this.groupBox_single_motor_set.Controls.Add(this.button_set_zero3);
			this.groupBox_single_motor_set.Controls.Add(this.button_lock2);
			this.groupBox_single_motor_set.Controls.Add(this.button_set_zero2);
			this.groupBox_single_motor_set.Controls.Add(this.button_lock1);
			this.groupBox_single_motor_set.Controls.Add(this.button_set_zero1);
			this.groupBox_single_motor_set.Location = new System.Drawing.Point(1373, 33);
			this.groupBox_single_motor_set.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.groupBox_single_motor_set.Name = "groupBox_single_motor_set";
			this.groupBox_single_motor_set.Padding = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.groupBox_single_motor_set.Size = new System.Drawing.Size(167, 413);
			this.groupBox_single_motor_set.TabIndex = 40;
			this.groupBox_single_motor_set.TabStop = false;
			this.groupBox_single_motor_set.Text = "电机设定";
			// 
			// button_lock6
			// 
			this.button_lock6.Enabled = false;
			this.button_lock6.Location = new System.Drawing.Point(104, 355);
			this.button_lock6.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.button_lock6.Name = "button_lock6";
			this.button_lock6.Size = new System.Drawing.Size(53, 39);
			this.button_lock6.TabIndex = 57;
			this.button_lock6.Text = "🔒";
			this.button_lock6.UseVisualStyleBackColor = true;
			this.button_lock6.Click += new System.EventHandler(this.button_single_lock_Click);
			// 
			// button_set_zero6
			// 
			this.button_set_zero6.Enabled = false;
			this.button_set_zero6.Location = new System.Drawing.Point(11, 355);
			this.button_set_zero6.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.button_set_zero6.Name = "button_set_zero6";
			this.button_set_zero6.Size = new System.Drawing.Size(83, 39);
			this.button_set_zero6.TabIndex = 56;
			this.button_set_zero6.Text = "设零";
			this.button_set_zero6.UseVisualStyleBackColor = true;
			this.button_set_zero6.Click += new System.EventHandler(this.button_set_single_zero_Click);
			// 
			// button_lock5
			// 
			this.button_lock5.Enabled = false;
			this.button_lock5.Location = new System.Drawing.Point(104, 293);
			this.button_lock5.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.button_lock5.Name = "button_lock5";
			this.button_lock5.Size = new System.Drawing.Size(53, 39);
			this.button_lock5.TabIndex = 55;
			this.button_lock5.Text = "🔒";
			this.button_lock5.UseVisualStyleBackColor = true;
			this.button_lock5.Click += new System.EventHandler(this.button_single_lock_Click);
			// 
			// button_set_zero5
			// 
			this.button_set_zero5.Enabled = false;
			this.button_set_zero5.Location = new System.Drawing.Point(11, 293);
			this.button_set_zero5.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.button_set_zero5.Name = "button_set_zero5";
			this.button_set_zero5.Size = new System.Drawing.Size(83, 39);
			this.button_set_zero5.TabIndex = 54;
			this.button_set_zero5.Text = "设零";
			this.button_set_zero5.UseVisualStyleBackColor = true;
			this.button_set_zero5.Click += new System.EventHandler(this.button_set_single_zero_Click);
			// 
			// button_lock4
			// 
			this.button_lock4.Enabled = false;
			this.button_lock4.Location = new System.Drawing.Point(104, 229);
			this.button_lock4.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.button_lock4.Name = "button_lock4";
			this.button_lock4.Size = new System.Drawing.Size(53, 39);
			this.button_lock4.TabIndex = 53;
			this.button_lock4.Text = "🔒";
			this.button_lock4.UseVisualStyleBackColor = true;
			this.button_lock4.Click += new System.EventHandler(this.button_single_lock_Click);
			// 
			// button_set_zero4
			// 
			this.button_set_zero4.Enabled = false;
			this.button_set_zero4.Location = new System.Drawing.Point(11, 229);
			this.button_set_zero4.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.button_set_zero4.Name = "button_set_zero4";
			this.button_set_zero4.Size = new System.Drawing.Size(83, 39);
			this.button_set_zero4.TabIndex = 52;
			this.button_set_zero4.Text = "设零";
			this.button_set_zero4.UseVisualStyleBackColor = true;
			this.button_set_zero4.Click += new System.EventHandler(this.button_set_single_zero_Click);
			// 
			// button_lock3
			// 
			this.button_lock3.Enabled = false;
			this.button_lock3.Location = new System.Drawing.Point(104, 168);
			this.button_lock3.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.button_lock3.Name = "button_lock3";
			this.button_lock3.Size = new System.Drawing.Size(53, 39);
			this.button_lock3.TabIndex = 51;
			this.button_lock3.Text = "🔒";
			this.button_lock3.UseVisualStyleBackColor = true;
			this.button_lock3.Click += new System.EventHandler(this.button_single_lock_Click);
			// 
			// button_set_zero3
			// 
			this.button_set_zero3.Enabled = false;
			this.button_set_zero3.Location = new System.Drawing.Point(11, 168);
			this.button_set_zero3.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.button_set_zero3.Name = "button_set_zero3";
			this.button_set_zero3.Size = new System.Drawing.Size(83, 39);
			this.button_set_zero3.TabIndex = 50;
			this.button_set_zero3.Text = "设零";
			this.button_set_zero3.UseVisualStyleBackColor = true;
			this.button_set_zero3.Click += new System.EventHandler(this.button_set_single_zero_Click);
			// 
			// button_lock2
			// 
			this.button_lock2.Enabled = false;
			this.button_lock2.Location = new System.Drawing.Point(104, 107);
			this.button_lock2.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.button_lock2.Name = "button_lock2";
			this.button_lock2.Size = new System.Drawing.Size(53, 39);
			this.button_lock2.TabIndex = 49;
			this.button_lock2.Text = "🔒";
			this.button_lock2.UseVisualStyleBackColor = true;
			this.button_lock2.Click += new System.EventHandler(this.button_single_lock_Click);
			// 
			// button_set_zero2
			// 
			this.button_set_zero2.Enabled = false;
			this.button_set_zero2.Location = new System.Drawing.Point(11, 107);
			this.button_set_zero2.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.button_set_zero2.Name = "button_set_zero2";
			this.button_set_zero2.Size = new System.Drawing.Size(83, 39);
			this.button_set_zero2.TabIndex = 48;
			this.button_set_zero2.Text = "设零";
			this.button_set_zero2.UseVisualStyleBackColor = true;
			this.button_set_zero2.Click += new System.EventHandler(this.button_set_single_zero_Click);
			// 
			// button_lock1
			// 
			this.button_lock1.Enabled = false;
			this.button_lock1.Location = new System.Drawing.Point(104, 43);
			this.button_lock1.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.button_lock1.Name = "button_lock1";
			this.button_lock1.Size = new System.Drawing.Size(53, 39);
			this.button_lock1.TabIndex = 47;
			this.button_lock1.Text = "🔒";
			this.button_lock1.UseVisualStyleBackColor = true;
			this.button_lock1.Click += new System.EventHandler(this.button_single_lock_Click);
			// 
			// button_set_MIT
			// 
			this.button_set_MIT.Enabled = false;
			this.button_set_MIT.Location = new System.Drawing.Point(69, 183);
			this.button_set_MIT.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.button_set_MIT.Name = "button_set_MIT";
			this.button_set_MIT.Size = new System.Drawing.Size(149, 45);
			this.button_set_MIT.TabIndex = 46;
			this.button_set_MIT.Text = "设MIT模式";
			this.button_set_MIT.UseVisualStyleBackColor = true;
			this.button_set_MIT.Click += new System.EventHandler(this.button_set_MIT_Click);
			// 
			// button_set_PV
			// 
			this.button_set_PV.Enabled = false;
			this.button_set_PV.Location = new System.Drawing.Point(229, 183);
			this.button_set_PV.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.button_set_PV.Name = "button_set_PV";
			this.button_set_PV.Size = new System.Drawing.Size(149, 45);
			this.button_set_PV.TabIndex = 47;
			this.button_set_PV.Text = "设位置模式";
			this.button_set_PV.UseVisualStyleBackColor = true;
			this.button_set_PV.Click += new System.EventHandler(this.button_set_PV_Click);
			// 
			// button_gravcomp_sw
			// 
			this.button_gravcomp_sw.Enabled = false;
			this.button_gravcomp_sw.Location = new System.Drawing.Point(69, 239);
			this.button_gravcomp_sw.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.button_gravcomp_sw.Name = "button_gravcomp_sw";
			this.button_gravcomp_sw.Size = new System.Drawing.Size(309, 45);
			this.button_gravcomp_sw.TabIndex = 48;
			this.button_gravcomp_sw.Text = "开启重力补偿";
			this.button_gravcomp_sw.UseVisualStyleBackColor = true;
			this.button_gravcomp_sw.Click += new System.EventHandler(this.button_gravcomp_sw_Click);
			// 
			// button_clamp_sw
			// 
			this.button_clamp_sw.Enabled = false;
			this.button_clamp_sw.Location = new System.Drawing.Point(69, 336);
			this.button_clamp_sw.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.button_clamp_sw.Name = "button_clamp_sw";
			this.button_clamp_sw.Size = new System.Drawing.Size(103, 45);
			this.button_clamp_sw.TabIndex = 49;
			this.button_clamp_sw.Text = "夹钳关";
			this.button_clamp_sw.UseVisualStyleBackColor = true;
			this.button_clamp_sw.Click += new System.EventHandler(this.button_clamp_sw_Click);
			// 
			// numericUpDown_clamp_torque
			// 
			this.numericUpDown_clamp_torque.DecimalPlaces = 1;
			this.numericUpDown_clamp_torque.Enabled = false;
			this.numericUpDown_clamp_torque.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
			this.numericUpDown_clamp_torque.Location = new System.Drawing.Point(299, 340);
			this.numericUpDown_clamp_torque.Maximum = new decimal(new int[] {
            2,
            0,
            0,
            0});
			this.numericUpDown_clamp_torque.Name = "numericUpDown_clamp_torque";
			this.numericUpDown_clamp_torque.Size = new System.Drawing.Size(83, 39);
			this.numericUpDown_clamp_torque.TabIndex = 50;
			this.numericUpDown_clamp_torque.Value = new decimal(new int[] {
            5,
            0,
            0,
            65536});
			this.numericUpDown_clamp_torque.ValueChanged += new System.EventHandler(this.numericUpDown_clamp_torque_ValueChanged);
			// 
			// label_clamp_torque
			// 
			this.label_clamp_torque.AutoSize = true;
			this.label_clamp_torque.Enabled = false;
			this.label_clamp_torque.Location = new System.Drawing.Point(180, 344);
			this.label_clamp_torque.Name = "label_clamp_torque";
			this.label_clamp_torque.Size = new System.Drawing.Size(116, 31);
			this.label_clamp_torque.TabIndex = 51;
			this.label_clamp_torque.Text = "夹持力矩:";
			// 
			// label_can_param
			// 
			this.label_can_param.AutoSize = true;
			this.label_can_param.Font = new System.Drawing.Font("微软雅黑", 13.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.label_can_param.Location = new System.Drawing.Point(1459, 1047);
			this.label_can_param.Name = "label_can_param";
			this.label_can_param.Size = new System.Drawing.Size(377, 48);
			this.label_can_param.TabIndex = 52;
			this.label_can_param.Text = "系统刷新率：0000Hz";
			this.label_can_param.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// checkBox_clamp_spring
			// 
			this.checkBox_clamp_spring.AutoSize = true;
			this.checkBox_clamp_spring.Location = new System.Drawing.Point(69, 293);
			this.checkBox_clamp_spring.Name = "checkBox_clamp_spring";
			this.checkBox_clamp_spring.Size = new System.Drawing.Size(190, 35);
			this.checkBox_clamp_spring.TabIndex = 53;
			this.checkBox_clamp_spring.Text = "夹钳弹簧模拟";
			this.checkBox_clamp_spring.UseVisualStyleBackColor = true;
			// 
			// groupBox_DH
			// 
			this.groupBox_DH.Controls.Add(this.textBox_DH6);
			this.groupBox_DH.Controls.Add(this.textBox_DH5);
			this.groupBox_DH.Controls.Add(this.textBox_DH4);
			this.groupBox_DH.Controls.Add(this.textBox_DH3);
			this.groupBox_DH.Controls.Add(this.textBox_DH2);
			this.groupBox_DH.Controls.Add(this.textBox_DH1);
			this.groupBox_DH.Location = new System.Drawing.Point(425, 467);
			this.groupBox_DH.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.groupBox_DH.Name = "groupBox_DH";
			this.groupBox_DH.Padding = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.groupBox_DH.Size = new System.Drawing.Size(221, 413);
			this.groupBox_DH.TabIndex = 38;
			this.groupBox_DH.TabStop = false;
			this.groupBox_DH.Text = "关节角度";
			// 
			// textBox_DH6
			// 
			this.textBox_DH6.Location = new System.Drawing.Point(11, 355);
			this.textBox_DH6.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.textBox_DH6.Name = "textBox_DH6";
			this.textBox_DH6.ReadOnly = true;
			this.textBox_DH6.Size = new System.Drawing.Size(199, 39);
			this.textBox_DH6.TabIndex = 5;
			// 
			// textBox_DH5
			// 
			this.textBox_DH5.Location = new System.Drawing.Point(11, 293);
			this.textBox_DH5.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.textBox_DH5.Name = "textBox_DH5";
			this.textBox_DH5.ReadOnly = true;
			this.textBox_DH5.Size = new System.Drawing.Size(199, 39);
			this.textBox_DH5.TabIndex = 4;
			// 
			// textBox_DH4
			// 
			this.textBox_DH4.Location = new System.Drawing.Point(11, 229);
			this.textBox_DH4.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.textBox_DH4.Name = "textBox_DH4";
			this.textBox_DH4.ReadOnly = true;
			this.textBox_DH4.Size = new System.Drawing.Size(199, 39);
			this.textBox_DH4.TabIndex = 3;
			// 
			// textBox_DH3
			// 
			this.textBox_DH3.Location = new System.Drawing.Point(11, 168);
			this.textBox_DH3.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.textBox_DH3.Name = "textBox_DH3";
			this.textBox_DH3.ReadOnly = true;
			this.textBox_DH3.Size = new System.Drawing.Size(199, 39);
			this.textBox_DH3.TabIndex = 2;
			// 
			// textBox_DH2
			// 
			this.textBox_DH2.Location = new System.Drawing.Point(11, 107);
			this.textBox_DH2.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.textBox_DH2.Name = "textBox_DH2";
			this.textBox_DH2.ReadOnly = true;
			this.textBox_DH2.Size = new System.Drawing.Size(199, 39);
			this.textBox_DH2.TabIndex = 1;
			// 
			// textBox_DH1
			// 
			this.textBox_DH1.Location = new System.Drawing.Point(11, 43);
			this.textBox_DH1.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.textBox_DH1.Name = "textBox_DH1";
			this.textBox_DH1.ReadOnly = true;
			this.textBox_DH1.Size = new System.Drawing.Size(199, 39);
			this.textBox_DH1.TabIndex = 0;
			// 
			// trackBar_outputX
			// 
			this.trackBar_outputX.Location = new System.Drawing.Point(1184, 491);
			this.trackBar_outputX.Minimum = -10;
			this.trackBar_outputX.Name = "trackBar_outputX";
			this.trackBar_outputX.Size = new System.Drawing.Size(548, 90);
			this.trackBar_outputX.TabIndex = 55;
			// 
			// label_output_X
			// 
			this.label_output_X.AutoSize = true;
			this.label_output_X.Location = new System.Drawing.Point(1739, 491);
			this.label_output_X.Name = "label_output_X";
			this.label_output_X.Size = new System.Drawing.Size(42, 31);
			this.label_output_X.TabIndex = 56;
			this.label_output_X.Text = "X: ";
			// 
			// label_output_Y
			// 
			this.label_output_Y.AutoSize = true;
			this.label_output_Y.Location = new System.Drawing.Point(1739, 573);
			this.label_output_Y.Name = "label_output_Y";
			this.label_output_Y.Size = new System.Drawing.Size(41, 31);
			this.label_output_Y.TabIndex = 58;
			this.label_output_Y.Text = "Y: ";
			// 
			// trackBar_outputY
			// 
			this.trackBar_outputY.Location = new System.Drawing.Point(1184, 573);
			this.trackBar_outputY.Minimum = -10;
			this.trackBar_outputY.Name = "trackBar_outputY";
			this.trackBar_outputY.Size = new System.Drawing.Size(548, 90);
			this.trackBar_outputY.TabIndex = 57;
			// 
			// label_output_Z
			// 
			this.label_output_Z.AutoSize = true;
			this.label_output_Z.Location = new System.Drawing.Point(1739, 659);
			this.label_output_Z.Name = "label_output_Z";
			this.label_output_Z.Size = new System.Drawing.Size(42, 31);
			this.label_output_Z.TabIndex = 60;
			this.label_output_Z.Text = "Z: ";
			// 
			// trackBar_outputZ
			// 
			this.trackBar_outputZ.Location = new System.Drawing.Point(1184, 659);
			this.trackBar_outputZ.Minimum = -10;
			this.trackBar_outputZ.Name = "trackBar_outputZ";
			this.trackBar_outputZ.Size = new System.Drawing.Size(548, 90);
			this.trackBar_outputZ.TabIndex = 59;
			// 
			// button_start_output
			// 
			this.button_start_output.Location = new System.Drawing.Point(1300, 757);
			this.button_start_output.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.button_start_output.Name = "button_start_output";
			this.button_start_output.Size = new System.Drawing.Size(309, 45);
			this.button_start_output.TabIndex = 61;
			this.button_start_output.Text = "开启力输出";
			this.button_start_output.UseVisualStyleBackColor = true;
			this.button_start_output.Click += new System.EventHandler(this.button_start_output_Click);
			// 
			// groupBox_tool
			// 
			this.groupBox_tool.Controls.Add(this.textBox_tool6);
			this.groupBox_tool.Controls.Add(this.textBox_tool5);
			this.groupBox_tool.Controls.Add(this.textBox_tool4);
			this.groupBox_tool.Controls.Add(this.textBox_tool3);
			this.groupBox_tool.Controls.Add(this.textBox_tool2);
			this.groupBox_tool.Controls.Add(this.textBox_tool1);
			this.groupBox_tool.Location = new System.Drawing.Point(661, 467);
			this.groupBox_tool.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.groupBox_tool.Name = "groupBox_tool";
			this.groupBox_tool.Padding = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.groupBox_tool.Size = new System.Drawing.Size(221, 413);
			this.groupBox_tool.TabIndex = 40;
			this.groupBox_tool.TabStop = false;
			this.groupBox_tool.Text = "末端位置/欧拉角";
			// 
			// textBox_tool6
			// 
			this.textBox_tool6.Location = new System.Drawing.Point(11, 355);
			this.textBox_tool6.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.textBox_tool6.Name = "textBox_tool6";
			this.textBox_tool6.ReadOnly = true;
			this.textBox_tool6.Size = new System.Drawing.Size(199, 39);
			this.textBox_tool6.TabIndex = 5;
			// 
			// textBox_tool5
			// 
			this.textBox_tool5.Location = new System.Drawing.Point(11, 293);
			this.textBox_tool5.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.textBox_tool5.Name = "textBox_tool5";
			this.textBox_tool5.ReadOnly = true;
			this.textBox_tool5.Size = new System.Drawing.Size(199, 39);
			this.textBox_tool5.TabIndex = 4;
			// 
			// textBox_tool4
			// 
			this.textBox_tool4.Location = new System.Drawing.Point(11, 229);
			this.textBox_tool4.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.textBox_tool4.Name = "textBox_tool4";
			this.textBox_tool4.ReadOnly = true;
			this.textBox_tool4.Size = new System.Drawing.Size(199, 39);
			this.textBox_tool4.TabIndex = 3;
			// 
			// textBox_tool3
			// 
			this.textBox_tool3.Location = new System.Drawing.Point(11, 168);
			this.textBox_tool3.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.textBox_tool3.Name = "textBox_tool3";
			this.textBox_tool3.ReadOnly = true;
			this.textBox_tool3.Size = new System.Drawing.Size(199, 39);
			this.textBox_tool3.TabIndex = 2;
			// 
			// textBox_tool2
			// 
			this.textBox_tool2.Location = new System.Drawing.Point(11, 107);
			this.textBox_tool2.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.textBox_tool2.Name = "textBox_tool2";
			this.textBox_tool2.ReadOnly = true;
			this.textBox_tool2.Size = new System.Drawing.Size(199, 39);
			this.textBox_tool2.TabIndex = 1;
			// 
			// textBox_tool1
			// 
			this.textBox_tool1.Location = new System.Drawing.Point(11, 43);
			this.textBox_tool1.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.textBox_tool1.Name = "textBox_tool1";
			this.textBox_tool1.ReadOnly = true;
			this.textBox_tool1.Size = new System.Drawing.Size(199, 39);
			this.textBox_tool1.TabIndex = 0;
			// 
			// textBox_udp_ip
			// 
			this.textBox_udp_ip.Location = new System.Drawing.Point(69, 448);
			this.textBox_udp_ip.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.textBox_udp_ip.Name = "textBox_udp_ip";
			this.textBox_udp_ip.Size = new System.Drawing.Size(199, 39);
			this.textBox_udp_ip.TabIndex = 6;
			this.textBox_udp_ip.Text = "192.168.1.88";
			// 
			// textBox_udp_port
			// 
			this.textBox_udp_port.Location = new System.Drawing.Point(280, 448);
			this.textBox_udp_port.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.textBox_udp_port.Name = "textBox_udp_port";
			this.textBox_udp_port.Size = new System.Drawing.Size(93, 39);
			this.textBox_udp_port.TabIndex = 62;
			this.textBox_udp_port.Text = "8000";
			// 
			// button_udp_connect
			// 
			this.button_udp_connect.Location = new System.Drawing.Point(69, 505);
			this.button_udp_connect.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
			this.button_udp_connect.Name = "button_udp_connect";
			this.button_udp_connect.Size = new System.Drawing.Size(309, 45);
			this.button_udp_connect.TabIndex = 63;
			this.button_udp_connect.Text = "连接UDP";
			this.button_udp_connect.UseVisualStyleBackColor = true;
			this.button_udp_connect.Click += new System.EventHandler(this.button_udp_connect_Click);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(192F, 192F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.ClientSize = new System.Drawing.Size(1859, 1104);
			this.Controls.Add(this.button_udp_connect);
			this.Controls.Add(this.textBox_udp_port);
			this.Controls.Add(this.textBox_udp_ip);
			this.Controls.Add(this.groupBox_tool);
			this.Controls.Add(this.button_start_output);
			this.Controls.Add(this.label_output_Z);
			this.Controls.Add(this.trackBar_outputZ);
			this.Controls.Add(this.label_output_Y);
			this.Controls.Add(this.trackBar_outputY);
			this.Controls.Add(this.label_output_X);
			this.Controls.Add(this.trackBar_outputX);
			this.Controls.Add(this.groupBox_DH);
			this.Controls.Add(this.checkBox_clamp_spring);
			this.Controls.Add(this.label_can_param);
			this.Controls.Add(this.label_clamp_torque);
			this.Controls.Add(this.numericUpDown_clamp_torque);
			this.Controls.Add(this.button_clamp_sw);
			this.Controls.Add(this.button_gravcomp_sw);
			this.Controls.Add(this.button_set_PV);
			this.Controls.Add(this.button_set_MIT);
			this.Controls.Add(this.groupBox_single_motor_set);
			this.Controls.Add(this.groupBox_clamp);
			this.Controls.Add(this.label_mode);
			this.Controls.Add(this.button_enable_motor);
			this.Controls.Add(this.button_start_sending);
			this.Controls.Add(this.button_disable_motor);
			this.Controls.Add(this.button_open_device);
			this.Controls.Add(this.groupBox_motor_temp);
			this.Controls.Add(this.groupBox_motor_tor);
			this.Controls.Add(this.groupBox_motor_vel);
			this.Controls.Add(this.groupBox_motor_pos);
			this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.Name = "Form1";
			this.Text = "电机调试";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
			this.groupBox_motor_temp.ResumeLayout(false);
			this.groupBox_motor_temp.PerformLayout();
			this.groupBox_motor_tor.ResumeLayout(false);
			this.groupBox_motor_tor.PerformLayout();
			this.groupBox_motor_vel.ResumeLayout(false);
			this.groupBox_motor_vel.PerformLayout();
			this.groupBox_motor_pos.ResumeLayout(false);
			this.groupBox_motor_pos.PerformLayout();
			this.groupBox_clamp.ResumeLayout(false);
			this.groupBox_clamp.PerformLayout();
			this.groupBox_single_motor_set.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown_clamp_torque)).EndInit();
			this.groupBox_DH.ResumeLayout(false);
			this.groupBox_DH.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.trackBar_outputX)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBar_outputY)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBar_outputZ)).EndInit();
			this.groupBox_tool.ResumeLayout(false);
			this.groupBox_tool.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.GroupBox groupBox_motor_temp;
		private System.Windows.Forms.TextBox textBox_motor_temp6;
		private System.Windows.Forms.TextBox textBox_motor_temp5;
		private System.Windows.Forms.TextBox textBox_motor_temp4;
		private System.Windows.Forms.TextBox textBox_motor_temp3;
		private System.Windows.Forms.TextBox textBox_motor_temp2;
		private System.Windows.Forms.TextBox textBox_motor_temp1;
		private System.Windows.Forms.GroupBox groupBox_motor_tor;
		private System.Windows.Forms.TextBox textBox_motor_tor6;
		private System.Windows.Forms.TextBox textBox_motor_tor5;
		private System.Windows.Forms.TextBox textBox_motor_tor4;
		private System.Windows.Forms.TextBox textBox_motor_tor3;
		private System.Windows.Forms.TextBox textBox_motor_tor2;
		private System.Windows.Forms.TextBox textBox_motor_tor1;
		private System.Windows.Forms.GroupBox groupBox_motor_vel;
		private System.Windows.Forms.TextBox textBox_motor_vel6;
		private System.Windows.Forms.TextBox textBox_motor_vel5;
		private System.Windows.Forms.TextBox textBox_motor_vel4;
		private System.Windows.Forms.TextBox textBox_motor_vel3;
		private System.Windows.Forms.TextBox textBox_motor_vel2;
		private System.Windows.Forms.TextBox textBox_motor_vel1;
		private System.Windows.Forms.GroupBox groupBox_motor_pos;
		private System.Windows.Forms.TextBox textBox_motor_pos6;
		private System.Windows.Forms.TextBox textBox_motor_pos5;
		private System.Windows.Forms.TextBox textBox_motor_pos4;
		private System.Windows.Forms.TextBox textBox_motor_pos3;
		private System.Windows.Forms.TextBox textBox_motor_pos2;
		private System.Windows.Forms.TextBox textBox_motor_pos1;
		private System.Windows.Forms.Button button_enable_motor;
		private System.Windows.Forms.Button button_start_sending;
		private System.Windows.Forms.Button button_disable_motor;
		private System.Windows.Forms.Button button_open_device;
		private System.Windows.Forms.Timer param_updating_timer;
		private System.Windows.Forms.Label label_mode;
		private System.Windows.Forms.GroupBox groupBox_clamp;
		private System.Windows.Forms.TextBox textBox_clamp6;
		private System.Windows.Forms.TextBox textBox_clamp5;
		private System.Windows.Forms.TextBox textBox_clamp4;
		private System.Windows.Forms.TextBox textBox_clamp3;
		private System.Windows.Forms.TextBox textBox_clamp2;
		private System.Windows.Forms.TextBox textBox_clamp1;
		public System.Windows.Forms.Timer udp_send_timer;
		private System.Windows.Forms.Button button_set_zero1;
		private System.Windows.Forms.GroupBox groupBox_single_motor_set;
		private System.Windows.Forms.Button button_lock1;
		private System.Windows.Forms.Button button_lock6;
		private System.Windows.Forms.Button button_set_zero6;
		private System.Windows.Forms.Button button_lock5;
		private System.Windows.Forms.Button button_set_zero5;
		private System.Windows.Forms.Button button_lock4;
		private System.Windows.Forms.Button button_set_zero4;
		private System.Windows.Forms.Button button_lock3;
		private System.Windows.Forms.Button button_set_zero3;
		private System.Windows.Forms.Button button_lock2;
		private System.Windows.Forms.Button button_set_zero2;
		private System.Windows.Forms.Button button_set_MIT;
		private System.Windows.Forms.Button button_set_PV;
		private System.Windows.Forms.Button button_gravcomp_sw;
		private System.Windows.Forms.Button button_clamp_sw;
		private System.Windows.Forms.NumericUpDown numericUpDown_clamp_torque;
		private System.Windows.Forms.Label label_clamp_torque;
		private System.Windows.Forms.Label label_can_param;
		private System.Windows.Forms.CheckBox checkBox_clamp_spring;
		private System.Windows.Forms.GroupBox groupBox_DH;
		private System.Windows.Forms.TextBox textBox_DH6;
		private System.Windows.Forms.TextBox textBox_DH5;
		private System.Windows.Forms.TextBox textBox_DH4;
		private System.Windows.Forms.TextBox textBox_DH3;
		private System.Windows.Forms.TextBox textBox_DH2;
		private System.Windows.Forms.TextBox textBox_DH1;
		private System.Windows.Forms.TrackBar trackBar_outputX;
		private System.Windows.Forms.Label label_output_X;
		private System.Windows.Forms.Label label_output_Y;
		private System.Windows.Forms.TrackBar trackBar_outputY;
		private System.Windows.Forms.Label label_output_Z;
		private System.Windows.Forms.TrackBar trackBar_outputZ;
		private System.Windows.Forms.Button button_start_output;
		private System.Windows.Forms.GroupBox groupBox_tool;
		private System.Windows.Forms.TextBox textBox_tool6;
		private System.Windows.Forms.TextBox textBox_tool5;
		private System.Windows.Forms.TextBox textBox_tool4;
		private System.Windows.Forms.TextBox textBox_tool3;
		private System.Windows.Forms.TextBox textBox_tool2;
		private System.Windows.Forms.TextBox textBox_tool1;
        private System.Windows.Forms.TextBox textBox_udp_ip;
        private System.Windows.Forms.TextBox textBox_udp_port;
        private System.Windows.Forms.Button button_udp_connect;
    }
}

