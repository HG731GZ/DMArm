using RobotControl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DMArmDLL
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //StreamWriter logfile = new StreamWriter("D:\\" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".txt");
            USBCAN CAN = new USBCAN();
            for (int i=0;i<6;i++)
            {
                CAN.motors[i].id_offset = CAN.motors[i].id + 0x100;
            }
            Robot robot_v = new Robot();
            Robot robot_r = new Robot();
            TCP_Server server = new TCP_Server("127.0.0.1", 11451);
            Joystick joystick = new Joystick();
            joystick.connect();
            server.start_tcp_server();
            CAN.open_device();
            CAN.init_double_can();
            CAN.start_double_can();
            CAN.enable_all();
            CAN.start_can_thread();
            double[] q1 = new double[6] { -90, -90, 220, 0, -35, 0 };
            double[] q2 = new double[6] { -90, 180, -95, 0, 0, 0 };
            double[] q3 = new double[6] { -90, 180, -95, 0, 93, 0 };
            for (int i = 0; i < 6; i++)
            {
                q1[i] = q1[i] * Math.PI / 180;
                q2[i] = q2[i] * Math.PI / 180;
                q3[i] = q3[i] * Math.PI / 180;
            }

            float[] motor_angle = robot_v.dh2motor(CAN.motors, q1);
            if (motor_angle != null)
            {
                Console.WriteLine("Clear!");
                for (int j = 0; j < 6; j++)
                {
                    CAN.motors[j].position_set = motor_angle[j];
                    CAN.motors[j].velocity_set = 0.5f;
                    CAN.motors[j].set_PV();
                }
            }
            robot_v.Angle = q1;
            robot_v.set_robot();
            
            while(true)
            {
                double[] dh = robot_r.motor2dh(CAN.motors);
                robot_r.Angle = dh;
                Console.WriteLine((dh[0] * 180f / Math.PI).ToString("0.00").PadLeft(10)
                    + (dh[1] * 180f / Math.PI).ToString("0.00").PadLeft(10)
                    + (dh[2] * 180f / Math.PI).ToString("0.00").PadLeft(10)
                    + (dh[3] * 180f / Math.PI).ToString("0.00").PadLeft(10)
                    + (dh[4] * 180f / Math.PI).ToString("0.00").PadLeft(10)
                    + (dh[5] * 180f / Math.PI).ToString("0.00").PadLeft(10)
                    + CAN.can_fps[0].ToString(".0").PadLeft(8));

                if (joystick.key.LeftShoulder)
                {
                    robot_v.move_world(-(double)(joystick.left_x) / (Math.Pow(2, 24)), 'x');
                    robot_v.move_world(-(double)(joystick.left_y) / (Math.Pow(2, 24)), 'y');
                    robot_v.move_world((double)(joystick.right_y) / (Math.Pow(2, 24)), 'z');

                    motor_angle = robot_v.dh2motor(CAN.motors, robot_v.Angle);
                    
                    if (motor_angle != null)
                    {
                        Console.WriteLine((double)(joystick.right_y) / (Math.Pow(2, 21)));
                        for (int j = 0; j < 6; j++)
                        {
                            CAN.motors[j].position_set = motor_angle[j];
                            CAN.motors[j].velocity_set = 5f;
                            CAN.motors[j].set_PV();
                        }
                    }
                    else
                    {
                        robot_v.Angle = robot_v.motor2dh(CAN.motors);
                        robot_v.set_robot();
                    }
                    //string str = "";
                    //for (int j = 0; j < 6; j++)
                    //{
                    //    str = str + robot_v.Angle[j].ToString() + " ";
                    //}
                    //for (int j = 0; j < 6; j++)
                    //{
                    //    str = str + dh[j].ToString() + " ";
                    //}
                    //logfile.WriteLine(str);
                }

                if (joystick.key.B)
                {
                    robot_v.Angle = q1;
                    robot_v.set_robot();
                    motor_angle = robot_v.dh2motor(CAN.motors, q1);
                    if (motor_angle != null)
                    {
                        Console.WriteLine("Clear!");
                        for (int j = 0; j < 6; j++)
                        {
                            CAN.motors[j].position_set = motor_angle[j];
                            CAN.motors[j].velocity_set = 0.5f;
                            CAN.motors[j].set_PV();
                        }
                    }

                }

                USBCAN.delayms(20);
            }
            
        }
    }
}
