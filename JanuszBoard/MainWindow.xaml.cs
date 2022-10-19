using System.Windows;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using XInputDotNetPure;
using System.Linq;
using System.Windows.Input;

namespace JanuszBoard
{
    public partial class MainWindow : Window
    {
        TcpClient client = new TcpClient();
        Thread thread;
        bool running = false;
        ButtonState[] oldStates = Enumerable.Repeat(ButtonState.Released, 10).ToArray();
        float[] oldAxes = Enumerable.Repeat(0f, 6).ToArray();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Connbutt_Click(object sender, RoutedEventArgs e)
        {
            client.Connect("192.168.4.1", 2137);
            connbutt.IsEnabled = false;
            startbutt.IsEnabled = true;
        }

        private void Startbutt_Click(object sender, RoutedEventArgs e)
        {
            startbutt.Content = running ? "Start" : "Stop";
            startbutt.IsEnabled = false;
            MemoryStream memoryStream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(memoryStream);
            writer.Write((byte)0x11);
            writer.Write((byte)0x20);
            writer.Write((byte)(running ? 0x04 : 0x03));
            writer.Write((byte)0x00);
            client.GetStream().Write(memoryStream.ToArray());
            running = !running;
            thread = new Thread(new ThreadStart(Thr));
            if (running) thread.Start();
            startbutt.IsEnabled = true;
        }

        

        private void Thr()
        {
            while (running)
            {
                GamePadState state = GamePad.GetState(PlayerIndex.One);
                var states = new[] { state.Buttons.A, state.Buttons.B, state.Buttons.X, state.Buttons.Y, state.Buttons.Start, state.Buttons.Back, state.Buttons.LeftStick, state.Buttons.RightStick, state.Buttons.LeftShoulder, state.Buttons.RightShoulder };
                for (int i = 0; i < 10; i++)
                {
                    if (oldStates[i] != states[i])
                    {
                        MemoryStream memoryStream = new MemoryStream();
                        BinaryWriter writer = new BinaryWriter(memoryStream);
                        writer.Write((byte)0x11);
                        writer.Write((byte)0x01);
                        writer.Write((byte)i);
                        writer.Write(states[i] == ButtonState.Pressed);
                        client.GetStream().Write(memoryStream.ToArray());
                    }
                }
                oldStates = states;
                var axes = new[] { state.ThumbSticks.Left.X, state.ThumbSticks.Left.Y, state.ThumbSticks.Right.X, state.ThumbSticks.Right.Y, state.Triggers.Left, state.Triggers.Right };
                for (int i = 0; i < 6; i++)
                {
                    if (oldAxes[i] != axes[i])
                    {
                        MemoryStream memoryStream = new MemoryStream();
                        BinaryWriter writer = new BinaryWriter(memoryStream);
                        writer.Write((byte)0x11);
                        writer.Write((byte)0x02);
                        writer.Write((byte)i);
                        writer.Write((byte)((int)axes[i] * 127.5 + 127.5));
                        client.GetStream().Write(memoryStream.ToArray());
                    }
                }
                oldAxes = axes;

                /*MemoryStream ms = new MemoryStream();
                BinaryWriter wr = new BinaryWriter(ms);
                wr.Write((byte)0x11);
                wr.Write((byte)0x21);
                wr.Write((byte)0x37);
                wr.Write((byte)0x69);
                client.GetStream().Write(ms.ToArray());*/
            }
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter && running)
            {
                startbutt.Content = "Start";
                startbutt.IsEnabled = false;
                MemoryStream memoryStream = new MemoryStream();
                BinaryWriter writer = new BinaryWriter(memoryStream);
                writer.Write((byte)0x11);
                writer.Write((byte)0x20);
                writer.Write((byte)0x04);
                writer.Write((byte)0x00);
                client.GetStream().Write(memoryStream.ToArray());
                running = false;
                thread = new Thread(new ThreadStart(Thr));
                startbutt.IsEnabled = true;
            }
        }
    }
}
