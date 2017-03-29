using SharpDX.DirectInput;
using System;
using System.Runtime.InteropServices;

namespace IIDXCON_ADAPTER
{
    class Program
    {
        static void Main(string[] args)
        {
            DirectInput directInput = new DirectInput();
            Joystick device;
            Guid keyboardGuid = Guid.Empty;

            // Search keybord(IIDX Premium controller)
            foreach (var g in directInput.GetDevices(DeviceType.Joystick, DeviceEnumerationFlags.AllDevices))
            {
                if (g.ProductGuid.ToString() == "80481ccf-0000-0000-0000-504944564944")
                {
                    keyboardGuid = g.InstanceGuid;
                    break;
                }
            }

            // Premium controller no connected -> exit
            if (keyboardGuid == Guid.Empty)
            {
                Console.WriteLine("Premium controller no connected");
                System.Threading.Thread.Sleep(5000);
                return;
            }

            // Initialize.
            device = new Joystick(directInput, keyboardGuid);

            // Start monitoring Scratch imput.
            device.Acquire();

            int x = device.GetCurrentState().X;

            ulong counter = 1;

            // Is Scratching flag.
            bool isActive = false;

            // is right Scratching.
            bool isRight = false;

            // Monitoring. (infinite loop)
            while (true)
            {
                // Get imput.
                var state = device.GetCurrentState();
                int stateX = state.X;
                if (x != stateX)
                {
                    bool nowRight = false;
                    if (x < stateX)
                    {
                        nowRight = true;
                        if ((stateX - x) > (65535 - stateX + x))
                        {
                            nowRight = false;
                        }
                    }
                    else if (x > stateX)
                    {
                        nowRight = false;
                        if ((x - stateX) > ((stateX + 65535) - x))
                        {
                            nowRight = true;
                        }
                    }


                    if (isActive && !(isRight == nowRight))
                    {
                        // Reversal Scratching.

                        if (isRight)
                        {
                            keybd_event(LEFT_SHIFT, 0, 2u, (UIntPtr)0uL);
                            keybd_event(LEFT_CTRL, 0, 0u, (UIntPtr)0uL);

                            Console.WriteLine("Left start");
                        }
                        else
                        {
                            keybd_event(LEFT_CTRL, 0, 2u, (UIntPtr)0uL);
                            keybd_event(LEFT_SHIFT, 0, 0u, (UIntPtr)0uL);

                            Console.WriteLine("Right start");
                        }

                        isRight = nowRight;
                        Console.WriteLine("Change!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!1");

                    }
                    else if (!isActive)
                    {
                        // Start Scratching.

                        if (nowRight)
                        {
                            keybd_event(LEFT_SHIFT, 0, 0u, (UIntPtr)0uL);
                            Console.WriteLine("Right start");
                        }
                        else
                        {
                            keybd_event(LEFT_CTRL, 0, 0u, (UIntPtr)0uL);
                            Console.WriteLine("Left start");
                        }

                        isActive = true;

                        isRight = nowRight;
                    }

                    counter = 0;
                    x = stateX;
                }

                // counter > 500000 ... Stop Scratching.
                if (counter > 500000 && isActive)
                {
                    if (isRight)
                    {
                        keybd_event(LEFT_SHIFT, 0, 2u, (UIntPtr)0uL);
                    }
                    else
                    {
                        keybd_event(LEFT_CTRL, 0, 2u, (UIntPtr)0uL);
                    }

                    isActive = false;
                    counter = 0;
                }

                if (counter == ulong.MaxValue)
                {
                    counter = 0;
                }

                counter++;
            }
        }

        [DllImport("user32.dll")]
        public static extern uint keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        private static byte LEFT_SHIFT = 0xA0;
        private static byte LEFT_CTRL = 0xA2;

    }
}